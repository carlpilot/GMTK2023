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
    }

    void checkIdle() {
        int total = 0;
        foreach (DeerMovement d in deer) {
            if (d.getState() == DeerMovement.State.IDLE || d.getState() == DeerMovement.State.WANDER) {
                total ++;
            }
        }
        if (total == deer.Length) {
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
            d.setState(DeerMovement.State.IDLE);
        }
        yield return new WaitForSeconds(Random.Range(10, 30));
        foreach (DeerMovement d in deer) {
            d.setNewTarget(target);
        }
        chillin = false;
    }
}
