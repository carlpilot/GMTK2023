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

    Vector3 lastSeenDeerPos;

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

    [Header("Chase Params")]
    [Tooltip("The time needed of not seeing a deer to abort a chase and return to alert")]
    public float chaseGiveUpTime = 3f;
    [Tooltip("The speed of the hunter when chasing")]
    public float chaseSpeed = 2f;


    void Awake()
    {
        deer = GameObject.FindWithTag("Deer");
        agent = GetComponent<NavMeshAgent>();
    }
    
    void Start() {
        StartCoroutine(RandomSearch());
    }

    void Update() {
        var mat = transform.GetChild(0).GetComponent<Renderer>().material;
        switch (state)
        {
            case 1:
                mat.color = Color.green;
                break;
            case 2:
                mat.color = Color.blue;
                break;
            case 3:
                mat.color = Color.yellow;
                break;
            case 4:
                mat.color = Color.cyan;
                break;
            case 5:
                mat.color = Color.magenta;
                break;
            case 6:
                mat.color = Color.red;
                break;
            case 7:
                mat.color = Color.black;
                break;
        }
    }

    bool DeerInView() {
        // Spherecast towards teh deer - the bigger the sphere the easier it is for the deer to hide
        RaycastHit hit;
        if (Physics.SphereCast(transform.position, 1f, deer.transform.position - transform.position, out hit, 100f))
        {
            if (hasTaggedParent(hit.collider.gameObject, "Deer"))
            {
                lastSeenDeerPos = deer.transform.position;
                return true;
            }
        }
        return false;
    }

    IEnumerator RandomSearch(){
        state = 1;
        yield return null;
        agent.speed = idleSpeed;
        var idleSwitchStateTimer = Random.Range(0f, idleWanderToHideTime);
        agent.isStopped = false;
        agent.SetDestination(deer.transform.position + new Vector3(Random.Range(-idleRange, idleRange), 0, Random.Range(-idleRange, idleRange)));
        while (true){
            idleSwitchStateTimer -= Time.deltaTime;

            // Check if the deer is in sight
            if (DeerInView()) {
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
                agent.SetDestination(deer.transform.position + new Vector3(Random.Range(-idleRange, idleRange), 0, Random.Range(-idleRange, idleRange)));
            }
            yield return null;
        }
    }

    IEnumerator RandomHide(){
        state = 2;
        yield return null;
        agent.speed = idleSpeed;
        var idleSwitchStateTimer = Random.Range(0f, idleHideToWanderTime);
        var closestBush = FindClosestBush();
        // Set destination to closest bush
        agent.isStopped = false;
        agent.SetDestination(closestBush.transform.position);
        while (true){
            idleSwitchStateTimer -= Time.deltaTime;

            // Check if the deer is in sight
            if (DeerInView()) {
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
            }
            yield return null;
        }
    }

    IEnumerator Alerted(){
        state = 3;
        yield return null;
        var alertedForTimer = 0f;
        agent.isStopped = true;
        // Crouch
        while (true){
            alertedForTimer += Time.deltaTime;
            // Have we been alerted for too long?
            if (alertedForTimer > alertToActiveTime){
                if (DeerInView()){
                     // If we are in a bush, switch to sniping. Also do this with a small random chance so the hunter is just lying on the ground
                    // Else, switch to chasing
                    var closestBush = FindClosestBush();
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
                if (!DeerInView()){
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
        // Crawl towards the deers position
        // If we are within range, switch to snipe
        // If we havent seen the deer for a while, switch to alert

        var lastSeenDeer = 0f;
        while (true){
            // Check if the deer is in sight
            if (DeerInView()) {
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
        while (true){
            // Check if the deer is in sight
            if (DeerInView()) {
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
        while (true){
            // Check if the deer is in sight
            if (DeerInView()) {
                // If it is, take an accurate shot every n seconds
                lastSeenDeer = 0f;
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
        while (true){
            agent.SetDestination(deer.transform.position);
            // Check if the deer is in sight
            if (DeerInView()) {
                // If it is, take an accurate shot every n seconds
                lastSeenDeer = 0f;
            }
            // Else, check if we have not been able to see the deer for a certain amount of time
            else{
                lastSeenDeer += Time.deltaTime;
                // If it has been too long, switch to alerted
                if (lastSeenDeer > chaseGiveUpTime){
                    StartCoroutine(Alerted());
                    yield break;
                }
                // else, do nothing
            }
            yield return null;
        }
    }

    GameObject FindClosestBush(){
        // Find the closest gameobject with the tag "Bush"
        GameObject closestBush = null;
        foreach (GameObject bush in GameObject.FindGameObjectsWithTag("Bush"))
        {
            if (closestBush == null)
            {
                closestBush = bush;
            }
            else if (Vector3.Distance(transform.position, bush.transform.position) < Vector3.Distance(transform.position, closestBush.transform.position))
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

}
