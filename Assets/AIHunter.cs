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

    Vector3 lastSeenDeerPos;

    void Awake()
    {
        deer = GameObject.FindWithTag("Deer");
        agent = GetComponent<NavMeshAgent>();
    }
    
    void Start() {
        StartCoroutine(RandomSearch());
    }

    void Update()
    {
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
        var idleSwitchStateTimer = Random.Range(5f, 30f);
        agent.isStopped = false;
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
        var idleSwitchStateTimer = Random.Range(5f, 30f);
        GameObject closestBush = FindClosestBush();
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
            if (alertedForTimer > 3f){
                if (DeerInView()){
                     // If we are in a bush, switch to sniping. Also do this with a small random chance so the hunter is just lying on the ground
                    // Else, switch to chasing
                    StartCoroutine(Sniping());
                    yield break;
                }
            } 
            if (alertedForTimer > 6f){
                if (!DeerInView()){
                    StartCoroutine(Tracking());
                    yield break;
                }
            }
            yield return null;
        }

    }

    IEnumerator Tracking(){
        state = 4;
        yield return null;
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
                if (lastSeenDeer > 3f){
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
                if (lastSeenDeer > 3f){
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
