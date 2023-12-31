using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

// This is the manager for a current game. It does not do scene switching or whatever
public class CurrentGameManager : MonoBehaviour
{
    [Header("Time of day stuff")]
    [Tooltip("The time taken in seconds for 24 hours to pass")]
    public float dayDuration = 60f;
    [Tooltip("The current game time in days. 1 means 24 hours")]
    public float gameTime;
    public int currentDay = 0;

    [Header("Deer/Player stuff")]
    [Tooltip("Is the player a deer. If not, the player is a human")]
    public bool isDeer;
    public GameObject aiDeerPrefab;
    public GameObject aiHunterPrefab;

    [Header("Switching animation")]
    public float maxBloom = 20f;
    public float maxThreshold = 0.5f;
    public float animationTime = 1f;

    [Header("Sun nonsense")]
    public float sunAxisAngle = 45f;

    [Header("Night Time Visuals")]
    public AnimationCurve fogDensityCurve;
    public Gradient fogColorCurve;
    public Gradient sunColorCurve;

    [Header("Night and Day Music")]
    public AudioSource dayMusic;
    public AudioSource nightMusic;

    GameObject deer;
    GameObject hunter;
    GameObject babyDeer;
    Spawn spawnObj;

    public Material deerMat1;
    public Material deerMat2;
    public Material deerMat3;

    Bloom b;

    bool isSwitchingStates = false;

    List<GameObject> currentAI = new List<GameObject>();

    public static int lastReachedDay = 1;
    public static bool lastWasDeer = false;

    bool hasShownHunterMessages = false;
    bool hasShownDeerMessages = false;


    void Awake()
    {
        gameTime = 0f;
        isDeer = false;

        GetComponent<Volume>().profile.TryGet(out b);

        b.intensity.value = 0f;
        b.threshold.value = maxThreshold;
    }
    
    void Start()
    {
        deer = GameObject.FindWithTag("Deer");
        hunter = GameObject.FindWithTag("Hunter");
        babyDeer = GameObject.FindWithTag("BabyDeer");
        spawnObj = FindObjectOfType<Spawn>();

        deer.SetActive(false);
        hunter.SetActive(false);
        babyDeer.SetActive(false);

        StartCoroutine(SwitchStates(false, true, 1));
    }
    
    void Update()
    {
        lastReachedDay = currentDay;
        lastWasDeer = isDeer;
        gameTime +=Time.deltaTime / dayDuration;
        gameTime =  Mathf.Min(gameTime, (isDeer ? 0.75f : 0.25f));
        var gt = gameTime;

        Vector3 sunDirectionMorning = Quaternion.Euler(0, sunAxisAngle, 0) * new Vector3(0, 0, 1);
        Vector3 sunDirectionNow = Quaternion.Euler(0, 0, -gt*360f) * sunDirectionMorning;
        GameObject.FindWithTag("Sun").transform.rotation = Quaternion.LookRotation(sunDirectionNow, Vector3.up);

        RenderSettings.fogDensity = fogDensityCurve.Evaluate(gt);
        RenderSettings.fogColor = fogColorCurve.Evaluate(gt);
        GameObject.FindWithTag("Sun").GetComponent<Light>().color = sunColorCurve.Evaluate(gt);

        if (isSwitchingStates) return;
    }

    void SendHunterControlsMessage () {
        if (hasShownHunterMessages) return;
        HintMessage.ShowMessage ("WASD to move, hold Shift to sneak");
        hasShownHunterMessages = true;
    }

    void SendDeerControlsMessage () {
        if (hasShownDeerMessages) return;
        HintMessage.ShowMessage ("Hold Shift to sneak, Ctrl to sprint");
        hasShownDeerMessages = true;
    }

    public void ShootDeer(){
        // We are a hunter and have shot the deer. Switch to deer. Also set to night time
        print("Switching to deer from hunter");
        if (isSwitchingStates) return;
        StartCoroutine(SwitchStates(true));
    }

    public void ShootPlayerDeer(){
        // We are a hunter and have shot the deer. Switch to deer. Also set to night time
        print("Player Dead");
        if (isSwitchingStates) return;
        //deer.SetActive(false);
        SceneManager.LoadScene(2);
    }
    public void CompleteDeerDay(){
        if (isSwitchingStates) return;
        StartCoroutine(SwitchStates(false));
    }

    public void enableDayMusic(){
        dayMusic.Play();
        nightMusic.Stop();
    }

    public void enableNightMusic(){
        dayMusic.Stop();
        nightMusic.Play();
    }

    IEnumerator SwitchStates(bool toDeer, bool startWhite = false, int forceDay = -1){
        print("Switching States");
        isSwitchingStates = true;
        if (!startWhite){
            for (float i = 0; i <= animationTime; i += Time.deltaTime){
                float t = i / animationTime;
                b.intensity.value = Mathf.Lerp(0f, maxBloom, t);
                b.threshold.value = Mathf.Lerp(maxThreshold, 0f, t);
                yield return null;
            }
        } else{
            b.intensity.value = maxBloom;
            b.threshold.value = 0f;
        }

        foreach (BearTrap b in FindObjectsOfType<BearTrap> ()) b.Reset ();
        FindObjectOfType<CageDoor> ().Reset ();

        foreach (var ai in currentAI){
            Destroy(ai);
        }
        currentAI.Clear();

        if (!toDeer){
            WayPoint[] waypoints = gameObject.GetComponent<HerdManager>().Start2();
            WayPoint randomWP = waypoints[Random.Range(0, waypoints.Length)];

            // Switching to hunter
            isDeer = false;
            deer.SetActive(false);
            hunter.SetActive(true);
            babyDeer.SetActive(false);
            spawnObj.gameObject.SetActive(false);
            currentDay++;
            if (forceDay != -1) currentDay = forceDay;
            gameTime = 0f;
            for (int i = 0; i < CalcNumDeerToSpawn(); i++){
                var deer = Instantiate(aiDeerPrefab, new Vector3(randomWP.transform.position.x + Random.Range(-15, 15f), 0f, randomWP.transform.position.z + Random.Range(-15, 15f)), Quaternion.identity);
                currentAI.Add(deer);
                deer.transform.parent = transform;

                WorldGenerator wg = FindObjectOfType<WorldGenerator>();
                deer.transform.position = new Vector3(deer.transform.position.x, wg.GetWorldHeight(deer.transform.position.x, deer.transform.position.z), deer.transform.position.z);

                float v = Random.value;
                if (v < 0.3f) {
                    deer.GetComponentInChildren<Renderer>().material = deerMat1;
                } else if (v <= 0.7f) {
                    deer.GetComponentInChildren<Renderer>().material = deerMat2;
                } else {
                    deer.GetComponentInChildren<Renderer>().material = deerMat3;
                    transform.localScale = new Vector3(0.6f, 0.6f, 0.6f);
                }
            }
            //

            // TODO: move the hunter to a random location

            enableDayMusic();

            if (!hasShownHunterMessages) {
                HintMessage.ShowMessage ("You are the hunter, patrol the woods around your lodge");
                Invoke ("SendHunterControlsMessage", 6.0f);
            }
        } else {
            // Switching to deer
            if (forceDay != -1) currentDay = forceDay;
            WayPoint[] waypoints = gameObject.GetComponent<HerdManager>().Start2();
            WayPoint randomWP = waypoints[Random.Range(0, waypoints.Length)];

            isDeer = true;
            hunter.SetActive(false);
            deer.SetActive(true);
            babyDeer.SetActive(true);
            gameTime = 0.5f;
            
            WorldGenerator wg = FindObjectOfType<WorldGenerator>();
            Vector3 spawn = new Vector3(randomWP.transform.position.x + Random.Range(-15, 15f), 0f, randomWP.transform.position.z + Random.Range(-15, 15f));
            spawn = new Vector3(spawn.x, wg.GetWorldHeight(spawn.x, spawn.z), spawn.z);
            spawnObj.transform.position = spawn;
            deer.GetComponent<PlayerMovement>().SetSpawn(spawn);
            spawnObj.gameObject.SetActive(true);

            print("Adding hunters");
            for (int i = 0; i < CalcNumHunterToSpawn(); i++){
                float x = 0;
                while (x < 10 && x > -10) x = Random.Range(-20, 20);
                float z = 0;
                while (z < 10 && z > -10) z = Random.Range(-20, 20);
                var hunter = Instantiate(aiHunterPrefab, new Vector3(x, 0f, z), Quaternion.identity);
                currentAI.Add(hunter);
            }
            // TODO: move the deer to a random location

            enableNightMusic();

            if (!hasShownDeerMessages) {
                HintMessage.ShowMessage ("Now you are the deer - find your way to the hunting lodge and free the baby deer from the cage, but watch out for the hunters!", 10);
                Invoke ("SendDeerControlsMessage", 11);
            }
        }

        for (float i = 0; i <= animationTime; i += Time.deltaTime){
            float t = i / animationTime;
            b.intensity.value = Mathf.Lerp(maxBloom, 0f, t);
            b.threshold.value = Mathf.Lerp(0f, maxThreshold, t);
            yield return null;
        }

        b.intensity.value = 0;
        b.threshold.value = maxThreshold;
        isSwitchingStates = false;
    }

    bool inCorrectState()
    {
        float gt = gameTime;
        if (isDeer)
        {
            return (gt >= 0.5f && gt < 1f);
        }
        else
        {
            return (gt >= 0f && gt < 0.5f);
        }
    }

    public int CalcNumDeerToSpawn(){
        switch (currentDay){
            case 0:
                return 15;
            case 1:
                return 12;
            case 3:
                return 10;
            default:
                return 8;
        }
    }

    public int CalcNumHunterToSpawn(){
        print("CurrentDay" + currentDay);
        switch (currentDay){
            case 1:
                return 2;
            case 2:
                return 3;
            case 3:
                return 5;
            case 4:
                return 6;
            case 5:
                return 7;
            default:
                return 8;
        }
    }
}
