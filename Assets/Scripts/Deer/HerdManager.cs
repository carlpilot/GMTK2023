using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HerdManager : MonoBehaviour
{

    public WayPoint[] waypoints;

    private DeerMovement[] deer;
    private int waypointIndex = 0;
    private WayPoint target;
    private bool chillin = false;


    // Start is called before the first frame update
    void Start()
    {
        deer = FindObjectsOfType<DeerMovement>();
    }

    // Update is called once per frame
    void Update()
    {
        if (!chillin) {
            checkIdle();
        }
        int idle = 0;
        int flee = 0;
        int wander = 0;
        int follow = 0;
        foreach (DeerMovement d in deer) {
            if (d.getState() == DeerMovement.State.IDLE) idle ++;
            if (d.getState() == DeerMovement.State.FLEE) flee ++;
            if (d.getState() == DeerMovement.State.WANDER) wander ++;
            if (d.getState() == DeerMovement.State.FOLLOW) follow ++;
        }
        print("flee: " + flee + "idle: " + idle + " wander: " + wander + " follow: " + follow);
    }

    void checkIdle() {
        int total = 0;
        int totalNotFleeing = 0;
        foreach (DeerMovement d in deer) {
            if (d.getState() == DeerMovement.State.IDLE || d.getState() == DeerMovement.State.WANDER) {
                total ++;
            }
            if (d.getState() != DeerMovement.State.FLEE) {
                totalNotFleeing ++;
            }
        }
        if (total == totalNotFleeing) {
            StartCoroutine(chill());
        }
    }

    IEnumerator chill() {
        chillin = true;
        if (target) target.setActive(false);
        waypointIndex = (waypointIndex + 1) % waypoints.Length;
        target = waypoints[waypointIndex];
        target.setActive(true);
        foreach (DeerMovement d in deer) {
            if (d.getState() != DeerMovement.State.FLEE) {
                d.idle();
            }
        }
        yield return new WaitForSeconds(5);
        foreach (DeerMovement d in deer) {
            if (d.getState() != DeerMovement.State.FLEE) {
                d.follow(target);
            }
        }
        chillin = false;
    }
}
