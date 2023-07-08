using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class DeerMovement : MonoBehaviour
{
    private NavMeshAgent agent;
    private WayPoint target;
    private float speedLower = 1.5f;
    private float speedUpper = 3.0f;
    private float wanderSpeedLower = 0.2f;
    private float wanderSpeedUpper = 0.7f;
    private float fleeSpeedLower = 8.0f;
    private float fleeSpeedUpper = 12.0f;
    private Vector3 dest;

    public enum State { IDLE, FOLLOW, WANDER, FLEE };
    public State state = State.IDLE;
    public float timeFleeing = 0;

    // Start is called before the first frame update
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    // Update is called once per frame
    void Update()
    {
        if (target != null && state == State.FOLLOW) {
            if ((transform.position - dest).magnitude <= 2) {
                idle();
            }
        }
        if (state == State.IDLE) {
            if (Random.value < 0.001) {
                wander();
            }
            timeFleeing = 0;
        }
        if (state == State.WANDER) {
            if ((transform.position - dest).magnitude <= 1) {
                idle();
            }
        }
        if (state == State.FOLLOW) {
            if ((transform.position - dest).magnitude <= 1) {
                idle();
            }
        }
        if (state == State.FLEE) {
            if ((transform.position - dest).magnitude <= 2) {
                idle();
                print("chillin");
            }
            timeFleeing += Time.deltaTime;
            if (timeFleeing > 10) {
                idle();
            }
        }
    }

    public State getState() {
        return state;
    }

    public void flee(Vector3 point) {
        state = State.FLEE;

        // This might be broken: Looks like if the distance is too large, the deer won't run and can't exit FLEE state
        Vector3 destination = (transform.position - point).normalized * Random.Range(40, 60);
        // boundaries are +/- 250
        destination.x = Mathf.Clamp(destination.x, -250, 250);
        destination.z = Mathf.Clamp(destination.z, -250, 250);
        agent.destination = destination;
        dest = destination;
        print("distance to destination: " + (transform.position - dest).magnitude);
        agent.speed = Random.Range(fleeSpeedLower, fleeSpeedUpper);
        print("FUCKFUCKFUCKFUCKFUCK");
    }

    public void idle() {
        state = State.IDLE;

        agent.speed = Random.Range(wanderSpeedLower, wanderSpeedUpper);
    }

    public void wander() {
        state = State.WANDER;

        Vector3 randomPoint = transform.position + (Random.insideUnitSphere * 5);
        NavMeshHit hit;
        NavMesh.SamplePosition(randomPoint, out hit, 10, 1);
        agent.destination = hit.position;
        dest = hit.position;
        agent.speed = Random.Range(wanderSpeedLower, wanderSpeedUpper);
    }

    public void follow(WayPoint newTarget) {
        state = State.FOLLOW;

        if (target != null) target.setActive(false);
        newTarget.setActive(true);
        target = newTarget;
        // get a random point within the target's radius
        Vector3 randomPoint = target.transform.position + (Random.insideUnitSphere * target.radius);
        randomPoint.y = target.transform.position.y;
        StartCoroutine(setOff(randomPoint));   
        dest = randomPoint;
        agent.speed = Random.Range(speedLower, speedUpper);
        print("Follow the leader leader leader follow the leader");
    }

    IEnumerator setOff(Vector3 point) {
        yield return new WaitForSeconds(Random.Range(0, 3));
        agent.destination = point;
    }
}
