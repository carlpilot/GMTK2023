using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class DeerMovement : MonoBehaviour
{
    private NavMeshAgent agent;
    private WayPoint target;
    private float speed = 1.5f;
    private float wanderSpeed = 0.5f;

    public enum State { IDLE, FOLLOW, WANDER, FLEE };
    private State state = State.IDLE;

    // Start is called before the first frame update
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    // Update is called once per frame
    void Update()
    {
        if (target != null) {
            if (Vector3.Distance(target.transform.position, transform.position) < target.radius) {
                state = State.IDLE;
            }
        }
        if (state == State.IDLE) {
            if (Random.value < 0.001) {
                state = State.WANDER;
                Vector3 randomPoint = transform.position + (Random.insideUnitSphere * 5);
                NavMeshHit hit;
                NavMesh.SamplePosition(randomPoint, out hit, 10, 1);
                agent.destination = hit.position;
            }
        }
        if (state == State.WANDER) {
            agent.speed = wanderSpeed;
            if (agent.remainingDistance < 1) {
                state = State.IDLE;
            }
        }
        if (state == State.FOLLOW) {
            agent.speed = speed;
        }
    }

    public void setNewTarget(WayPoint newTarget) {
        if (target != null) target.setActive(false);
        newTarget.setActive(true);
        target = newTarget;
        state = State.FOLLOW;

        // get a random point within the target's radius
        Vector3 randomPoint = target.transform.position + (Random.insideUnitSphere * target.radius);
        randomPoint.y = target.transform.position.y;

        StartCoroutine(setOff(randomPoint));        
    }

    public void setState(State newState) {
        state = newState;
    }

    public State getState() {
        return state;
    }

    IEnumerator setOff(Vector3 point) {
        yield return new WaitForSeconds(Random.Range(0, 3));
        agent.destination = point;
    }
}
