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

    int state = 1;

    // The one and only player deer
    GameObject deer;

    NavMeshAgent agent;

    public float idleRange = 50f;

    void Awake()
    {
        deer = GameObject.FindWithTag("Deer");
        agent = GetComponent<NavMeshAgent>();
    }
    
    void Start() {
    }

    bool DeerInView() {
        return false; // Unfortunately, in a tragic turn of events, the hunter lost his sight in a hunting accident
    }

    IEnumerator RandomSearch(){
        var idleSwitchStateTimer = Random.Range(5f, 30f);
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
        var idleSwitchStateTimer = Random.Range(5f, 30f);
        GameObject closestBush = null;
        // Set destination to closest bush
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
            }
            yield return null;
        }
    }

    IEnumerator Alerted(){
        var alertedForTimer = 0f;
        // Crouch
        while (true){
            alertedForTimer += Time.deltaTime;
            // Have we been alerted for too long?
            if (alertedForTimer > 10f){
                StartCoroutine(Tracking());
                yield break;
            }
            // If we can see the player
            else if (DeerInView()) {
                // If we are in a bush, switch to sniping. Also do this with a small random chance so the hunter is just lying on the ground
                // Else, switch to chasing
                StartCoroutine(Sniping());
                yield break;
            }
            yield return null;
        }

    }

    IEnumerator Tracking(){
        Vector3 lastDeerPosition = deer.transform.position;
        agent.SetDestination(lastDeerPosition);
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
        Debug.Log("Sniping");
        StartCoroutine(RandomSearch());
        yield break;
        // Check if the deer is in sight
            // If it is, take an accurate shot every n seconds
        // Else, check if we have not been able to see the deer for a certain amount of time
            // If it has been too long, switch to alerted
            // else, do nothing
    }

    IEnumerator Chasing(){
        Debug.Log("Chasing");
        StartCoroutine(RandomSearch());
        yield break;
        // Run after the deer
        // Check if the deer is in sight
            // If it is, take a shot every n seconds
        // Else, check if we have not been able to see the deer for a certain amount of time
            // If it has been too long, switch to alerted
            // else, do nothing
    }

}
