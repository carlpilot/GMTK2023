using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

// This is the manager for a current game. It does not do scene switching or whatever
public class CurrentGameManager : MonoBehaviour
{
    [Header("Time of day stuff")]
    [Tooltip("The time taken in seconds for 24 hours to pass")]
    public float dayDuration = 60f;
    [Tooltip("The current game time in days. 1 means 24 hours")]
    public float gameTime;

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

    GameObject deer;
    GameObject hunter;

    Bloom b;

    bool isSwitchingStates = false;

    List<GameObject> currentAI = new List<GameObject>();


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

        deer.SetActive(false);
        hunter.SetActive(false);

        StartCoroutine(SwitchStates(false, true));
    }
    
    void Update()
    {
        gameTime += Time.deltaTime / dayDuration;

        float gt = Mathf.Repeat(gameTime, 1f);
        Vector3 sunDirectionMorning = Quaternion.Euler(0, sunAxisAngle, 0) * new Vector3(0, 0, 1);
        Vector3 sunDirectionNow = Quaternion.Euler(0, 0, -gt*360f) * sunDirectionMorning;
        GameObject.FindWithTag("Sun").transform.rotation = Quaternion.LookRotation(sunDirectionNow, Vector3.up);

        if (isSwitchingStates) return;

        if  (isDeer) {
            if (inCorrectState()) {
                // We are a deer and it is night time
            } else{
                // We are a deer and survived the night. Switch to hunter
                print("Switching to hunter from deer");
                StartCoroutine(SwitchStates(false));
            }
        } else {
            if (inCorrectState()) {
                // We are a hunter and it is day time
            } else{
                // We are a hunter but ran out of time. End the game
                print("Game Over - ran out of time as hunter");
                hunter.SetActive(false);
            }
        }
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
        deer.SetActive(false);
    }

    IEnumerator SwitchStates(bool toDeer, bool startWhite = false){
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

        foreach (var ai in currentAI){
            Destroy(ai);
        }
        currentAI.Clear();

        if (!toDeer){
            // Switching to hunter
            isDeer = false;
            deer.SetActive(false);
            hunter.SetActive(true);
            for (int i = 0; i < 5; i++){
                var deer = Instantiate(aiDeerPrefab, new Vector3(Random.Range(-50, 50f), 0f, Random.Range(-50, 50f)), Quaternion.identity);
                currentAI.Add(deer);
            }
            // TODO: move the hunter to a random location
        } else {
            // Switching to deer
            isDeer = true;
            hunter.SetActive(false);
            deer.SetActive(true);
            gameTime = Mathf.Floor(gameTime) + 0.5f;
            for (int i = 0; i < 10; i++){
                var hunter = Instantiate(aiHunterPrefab, new Vector3(Random.Range(-50, 50f), 0f, Random.Range(-50, 50f)), Quaternion.identity);
                currentAI.Add(hunter);
            }
            // TODO: move the deer to a random location
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
        float gt = Mathf.Repeat(gameTime, 1f);
        if (isDeer)
        {
            return (gt >= 0.5f && gt < 1f);
        }
        else
        {
            return (gt >= 0f && gt < 0.5f);
        }
    }
}
