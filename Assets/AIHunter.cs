using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class AIHunter : MonoBehaviour
{
    // States of the AI:
    // 1. Random Search - the hunter has no idea where the deer is but slowly moves towards it
    // 2. Random wait - the hunter has no idea where the deer is and is waiting for it to come to him whilst hiding
    // 3. Alerted - the hunter has seen the deer and is hiding whilst looking for the deer
    // 4. Tracking - the hunter was alerted but now it walks to the most recent location
    // 5. Sniping - the hunter has seen the deer it is in a bush, so takes slow accurate shots
    // 6. Chasing - the hunter has seen the deer and is chasing it whilst taking faster but innaccurate shots
    // 7. Crawling - the hunter is in the open and is moving slowly towards the deer without shooting. After it gets within range, it will start sniping

    int state = 1;

    // The one and only player deer
    GameObject deer;

    NavMeshAgent agent;
    public Animator anim;


    [Header("Bullets")]
    public GameObject bulletPrefab;
    List<GameObject> bullets = new List<GameObject>();
    public float bulletSpeed = 10f;
    public LayerMask canBlockSightLayerMask;

    [Header("Idle Params")]
    [Tooltip("The range around the player that the hunters idle around")]
    public float idleRange = 50f;
    [Tooltip("The maximum time it takes for a hunter to switch from idle wander to idle hide")]
    public float idleWanderToHideTime = 30f;
    [Tooltip("The maximum time it takes for a hunter to switch from idle hide to idle wander")]
    public float idleHideToWanderTime = 60f;
    [Tooltip("The speed of the hunter when in idle")]
    public float idleSpeed = 1f;

    [Header("Alert Params")]
    [Tooltip("The time taken for an alert hunter to start actively hunting a visible deer")]
    public float alertToActiveTime = 3f;
    [Tooltip("The time taken for an alert hunter to return to idle when there is no visible deer")]
    public float alertToIdleTime = 6f;
    [Tooltip("The probability of a hunter to crawl towards the deer instead of running and gunning")]
    public float crawlOverChaseProbability = 0.5f;

    [Header("Crawl Params")]
    [Tooltip("The time needed of not seeing a deer to abort a crawl and return to alert")]
    public float crawlGiveUpTime = 3f;
    [Tooltip("The time taken for a crawling hunter to start sniping at a visible deer")]
    public float crawlSniperRange = 10f;
    [Tooltip("The speed of the hunter when crawling")]
    public float crawlSpeed = 0.5f;

    [Header("Tracking Params")]
    public float trackingSpeed = 0.75f;

    [Header("Snipe Params")]
    [Tooltip("The time needed of not seeing a deer to abort a snipe and return to alert")]
    public float snipeGiveUpTime = 3f;
    [Tooltip("The time taken between sniper shots")]
    public float snipeShotTime = 5f;
    [Tooltip("The spread of the sniper shots")]
    public float snipeSpread = 5f;

    [Header("Chase Params")]
    [Tooltip("The time needed of not seeing a deer to abort a chase and return to alert")]
    public float chaseGiveUpTime = 3f;
    [Tooltip("The speed of the hunter when chasing")]
    public float chaseSpeed = 2f;
    [Tooltip("The time taken between chase shots")]
    public float chaseShotTime = 2;
    [Tooltip("The spread of the chase shots")]
    public float chaseSpread = 20f;
    public float minChaseDist = 3f;

    [Header("Noticing Deer")]
    public float idleNoticeMaxTime = 10f;
    public float alertNoticeMaxTime = 3f;
    public float activeNoticeMaxTime = 1f;
    public float bushVisibilityMultiplier = 0.2f;
    public float crouchVisibilityMultiplier = 0.5f;
    public float sprintVisibilityMultiplier = 2f;

    public float maxDeerNoticeDist = 75f;

    [Header("Sound")]
    public AudioSource rustlingSFX;
    public AudioSource gunshotSFX;
    public AudioSource runningSFX;
    public AudioSource walkingSFX;
    public AudioSource reloadSFX;
    private Heartbeat heartbeatManager;

    private float cooldown = 3f;
    private float cooldownTimer = 2.0f;
    private float noiseRange = 40f;

    bool canSeeDeer = false;
    Vector3 lastSeenDeerPos;
    float currentNoticeMaxTime = 1f;
    float noticeTimer = 0f;

    public int currentStealthLevel = 0;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        heartbeatManager = GameObject.Find("HeartbeatSFX").GetComponent<Heartbeat>();
    }
    
    void Start() {
        deer = GameObject.FindWithTag("Deer");

        StartCoroutine(RandomSearch());
    }

    void SetNoticeMaxTime(float nmt){
        currentNoticeMaxTime = nmt;
        ResetNoticeTimer();
    }

    void ResetNoticeTimer(){
        noticeTimer = Random.Range(currentNoticeMaxTime/2f, currentNoticeMaxTime);
    }

    void Update() {
        if (!deer) deer = GameObject.FindWithTag("Deer");

        /*float distance = (deer.transform.position - transform.position).magnitude;
        float volume = 0;
        if (distance <= noiseRange) {
            volume = 1 - distance / noiseRange;
        }
        runningSFX.volume = volume;*/

        if(agent.isStopped) transform.LookAt(deer.transform);

        // Deer viewing logic
        {
            float visibilityMultiplier = 1f;
            // If the player is in a bush, they are harder to see
            var closestPlayerBush = FindClosestBush(deer.transform.position);
            if ((closestPlayerBush.transform.position - deer.transform.position).magnitude < 2f) { visibilityMultiplier *= bushVisibilityMultiplier; currentStealthLevel ++;}
            if (deer.GetComponent<PlayerMovement>().isCrouching) {visibilityMultiplier *= crouchVisibilityMultiplier; currentStealthLevel ++;}
            if (deer.GetComponent<PlayerMovement>().isSprinting) {visibilityMultiplier *= sprintVisibilityMultiplier; currentStealthLevel --;}

            noticeTimer -= Time.deltaTime*visibilityMultiplier;
            RaycastHit hit;
            var start = transform.position + new Vector3(0, 1f, 0);
            var end = deer.transform.position + new Vector3(0, 1f, 0);
            var dir = (end-start).normalized;
            var deerInView = false;
            if (Physics.Raycast(start, dir, out hit, maxDeerNoticeDist, canBlockSightLayerMask) && hasTaggedParent(hit.collider.gameObject, "Deer")) deerInView = true;

            if (canSeeDeer){
                // We can already see the deer, so hiding in bushes is not really any use
                canSeeDeer = deerInView;
                if(canSeeDeer) Debug.DrawLine(start, end, Color.red);
                else Debug.DrawLine(start, end, Color.green);
                ResetNoticeTimer();
            } else {
                // We havent noticed the deer yet, so even if we look straight at it we still might not see it
                canSeeDeer = deerInView && noticeTimer <= 0f;
                if(canSeeDeer) {
                    Debug.DrawLine(start, end, Color.red);
                    print("This ran!");
                }
                else if (deerInView) Debug.DrawLine(start, end, Color.magenta);
                else Debug.DrawLine(start, end, Color.cyan);

                if (noticeTimer <= 0f) ResetNoticeTimer();
            }
            if (canSeeDeer) lastSeenDeerPos = deer.transform.position;
        }

        cooldownTimer -= Time.deltaTime;
        if (cooldownTimer < 0f) cooldownTimer = 0f;
    }

    IEnumerator RandomSearch(){
        state = 1;
        yield return null;
        agent.speed = idleSpeed;
        rustlingSFX.Stop();
        walkingSFX.Play();
        runningSFX.Stop();
        var idleSwitchStateTimer = Random.Range(0f, idleWanderToHideTime);
        agent.isStopped = false;
        agent.SetDestination(new Vector3(Random.Range(-idleRange, idleRange), 0, Random.Range(-idleRange, idleRange)));
        anim.SetInteger("animState", 3); // Walk
        SetNoticeMaxTime(idleNoticeMaxTime);
        while (true){
            idleSwitchStateTimer -= Time.deltaTime;

            // Check if the deer is in sight
            if (canSeeDeer) {
                StartCoroutine(Alerted());
                yield break;
            }
            // If not, check if we should randomly switch to random wait
            else if (idleSwitchStateTimer <= 0f)
            {
                StartCoroutine(RandomHide());
                yield break;
            }
            // If not, check if we should randomly change idle desitnation
            else if (agent.remainingDistance <= 0.1f)
            {
                agent.SetDestination(new Vector3(Random.Range(-idleRange, idleRange), 0, Random.Range(-idleRange, idleRange)));
            }
            yield return null;
        }
    }

    IEnumerator RandomHide(){
        state = 2;
        yield return null;
        agent.speed = idleSpeed;
        rustlingSFX.Stop();
        walkingSFX.Play();
        runningSFX.Stop();
        var idleSwitchStateTimer = Random.Range(0f, idleHideToWanderTime);
        var closestBush = FindClosestBush(transform.position);
        anim.SetInteger("animState", 3); // Walk
        // Set destination to closest bush
        agent.isStopped = false;
        agent.SetDestination(closestBush.transform.position);
        SetNoticeMaxTime(idleNoticeMaxTime);
        while (true){
            idleSwitchStateTimer -= Time.deltaTime;

            // Check if the deer is in sight
            if (canSeeDeer) {
                StartCoroutine(Alerted());
                yield break;
            }
            // If not, check if we should randomly switch to random wait
            else if (idleSwitchStateTimer <= 0f)
            {
                StartCoroutine(RandomSearch());
                yield break;
            }
            // Check if we have reached the bush yet
            else if (agent.remainingDistance <= 0.1f)
            {
                // Crouch
                agent.isStopped = true;
                runningSFX.Stop();
                anim.SetInteger("animState", 1); // Snipe
            }
            yield return null;
        }
    }

    IEnumerator Alerted(){
        state = 3;
        yield return null;
        var alertedForTimer = 0f;
        agent.isStopped = true;
        rustlingSFX.Stop();
        runningSFX.Stop();
        anim.SetInteger("animState", 0); // Look
        // Crouch
        SetNoticeMaxTime(alertNoticeMaxTime);
        while (true){
            alertedForTimer += Time.deltaTime;
            // Have we been alerted for too long?
            if (canSeeDeer){
                heartbeatManager.Alert(this);
            }
            if (alertedForTimer > alertToActiveTime){
                if (canSeeDeer){
                     // If we are in a bush, switch to sniping. Also do this with a small random chance so the hunter is just lying on the ground
                    // Else, switch to chasing
                    var closestBush = FindClosestBush(transform.position);
                    if ((transform.position - closestBush.transform.position).magnitude < 1f) {
                        StartCoroutine(Sniping());
                        yield break;
                    } 
                    else if (Random.Range(0f, 1f) < crawlOverChaseProbability){
                        StartCoroutine(Crawling());
                        yield break;
                    }
                    else {
                        StartCoroutine(Chasing());
                        yield break;
                    }
                }
            } 
            if (alertedForTimer > alertToIdleTime){
                if (!canSeeDeer){
                    heartbeatManager.UnAlert(this);
                    StartCoroutine(Tracking());
                    yield break;
                }
            }
            yield return null;
        }

    }

    IEnumerator Crawling(){
        state = 7;
        yield return null;
        agent.speed = crawlSpeed;
        agent.isStopped = false;
        agent.SetDestination(lastSeenDeerPos);
        anim.SetInteger("animState", 4); // Crawl
        rustlingSFX.Play();
        runningSFX.Stop();
        walkingSFX.Stop();
        SetNoticeMaxTime(alertNoticeMaxTime);
        // Crawl towards the deers position
        // If we are within range, switch to snipe
        // If we havent seen the deer for a while, switch to alert

        var lastSeenDeer = 0f;
        while (true){
            // Check if the deer is in sight
            if (canSeeDeer) {
                heartbeatManager.Alert(this);
                lastSeenDeer = 0f;
                if ((transform.position - deer.transform.position).magnitude <= crawlSniperRange){
                    // Snipe
                    StartCoroutine(Sniping());
                    yield break;
                }
                else{
                    agent.SetDestination(lastSeenDeerPos);
                }
            }
            // Else, check if we have not been able to see the deer for a certain amount of time
            else{
                lastSeenDeer += Time.deltaTime;
                // If it has been too long, switch to alerted
                if (lastSeenDeer > crawlGiveUpTime){
                    StartCoroutine(Alerted());
                    yield break;
                }
                // else, do nothing
            }
            yield return null;
        }
    }

    IEnumerator Tracking(){
        state = 4;
        yield return null;
        agent.speed = trackingSpeed;
        agent.isStopped = false;
        agent.SetDestination(lastSeenDeerPos);
        anim.SetInteger("animState", 3); // Walk
        rustlingSFX.Stop();
        walkingSFX.Play();
        runningSFX.Stop();
        SetNoticeMaxTime(alertNoticeMaxTime);
        while (true){
            // Check if the deer is in sight
            if (canSeeDeer) {
                StartCoroutine(Alerted());
                yield break;
            }
            // else if we are at the last known location, switch to random search
            else if (agent.remainingDistance <= 0.1f)
            {
                StartCoroutine(RandomSearch());
                yield break;
            }
            yield return null;
        }
    }

    IEnumerator Sniping(){
        state=5;
        yield return null;
        agent.isStopped = true;
        var lastSeenDeer = 0f;
        var lastShot = 0f;
        anim.SetInteger("animState", 1); // Snipe
        rustlingSFX.Stop();
        runningSFX.Stop();
        walkingSFX.Stop();
        reloadSFX.Play();
        SetNoticeMaxTime(activeNoticeMaxTime);
        while (true){
            lastShot += Time.deltaTime;
            // Check if the deer is in sight
            if (canSeeDeer) {
                // If it is, take an accurate shot every n seconds
                lastSeenDeer = 0f;
                if (lastShot > snipeShotTime && cooldownTimer <= 0f){
                    lastShot = 0f;
                    StartCoroutine(shootBullet(deer.transform.position+Vector3.up*1f, snipeSpread));
                }
            }
            // Else, check if we have not been able to see the deer for a certain amount of time
            else{
                lastSeenDeer += Time.deltaTime;
                // If it has been too long, switch to alerted
                if (lastSeenDeer > snipeGiveUpTime){
                    StartCoroutine(Alerted());
                    yield break;
                }
                // else, do nothing
            }
            yield return null;
        }
    }

    IEnumerator Chasing(){
        state=6;
        yield return null;
        agent.speed = chaseSpeed;
        agent.isStopped = false;
        var lastSeenDeer = 0f;
        var lastShot = 0f;
        rustlingSFX.Stop();
        runningSFX.Play();
        walkingSFX.Stop();
        anim.SetInteger("animState", 2); // Chase
        SetNoticeMaxTime(activeNoticeMaxTime);
        while (true){
            agent.isStopped = (transform.position - deer.transform.position).magnitude <= minChaseDist;
            lastShot += Time.deltaTime;
            agent.SetDestination(deer.transform.position);
            lastSeenDeerPos = deer.transform.position;
            // Check if the deer is in sight
            if (canSeeDeer) {
                // If it is, take an accurate shot every n seconds
                lastSeenDeer = 0f;
                if (lastShot > chaseShotTime && cooldownTimer <= 0f){
                    lastShot = 0f;
                    StartCoroutine(shootBullet(deer.transform.position+Vector3.up*1f, chaseSpread));
                }
            }
            // Else, check if we have not been able to see the deer for a certain amount of time
            else{
                lastSeenDeer += Time.deltaTime;
                // If it has been too long, switch to alerted
                if (lastSeenDeer > chaseGiveUpTime){
                    StartCoroutine(Tracking());
                    yield break;
                }
                // else, do nothing
            }
            yield return null;
        }
    }

    GameObject FindClosestBush(Vector3 at){
        // Find the closest gameobject with the tag "Bush"
        GameObject closestBush = null;
        foreach (GameObject bush in GameObject.FindGameObjectsWithTag("Bush"))
        {
            if (closestBush == null)
            {
                closestBush = bush;
            }
            else if (Vector3.Distance(at, bush.transform.position) < Vector3.Distance(at, closestBush.transform.position))
            {
                closestBush = bush;
            }
        }
        return closestBush;
    }

    bool hasTaggedParent(GameObject obj, string tag)
    {
        if (obj.tag == tag)
        {
            return true;
        }
        else if (obj.transform.parent != null)
        {
            return hasTaggedParent(obj.transform.parent.gameObject, tag);
        }
        else
        {
            return false;
        }
    }

    IEnumerator shootBullet(Vector3 at, float spread){
        var from = transform.position + Vector3.up * 1.5f;
        var dir = Quaternion.LookRotation((at-from).normalized);
        dir = Quaternion.Euler(dir.eulerAngles + new Vector3(Random.Range(-spread, spread), Random.Range(-spread, spread), 0f));
        var bullet = Instantiate(bulletPrefab, from, dir);
        bullets.Add(bullet);
        gunshotSFX.Play();
        cooldownTimer = cooldown;

        StartCoroutine(ReloadSounds());

        var timer = 0f;
        bool justCollided = false;
        while (timer < 10f)
        {
            bullet.transform.position += bullet.transform.forward * bulletSpeed * Time.deltaTime;
            timer += Time.deltaTime;

            if (justCollided) break;

            // Check for bullet collisions using raycasts
            RaycastHit hit;
            if (Physics.Raycast(bullet.transform.position, bullet.transform.forward, out hit, bulletSpeed * Time.deltaTime))
            {
                if (hasTaggedParent(hit.collider.gameObject, "Deer"))
                {
                    // Hit the deer
                    Debug.Log("Hit the player deer");
                    GameObject.Find("GameManager").GetComponent<CurrentGameManager>().ShootPlayerDeer();
                }
                justCollided = true;
            }

            yield return null;
        }
        if (bullets.Contains(bullet))
        {
            bullets.Remove(bullet);
            Destroy(bullet);
        }
    }

    IEnumerator ReloadSounds()
    {
        yield return new WaitForSeconds(2f);
        reloadSFX.Play();
    }

}
