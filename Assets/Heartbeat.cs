using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Heartbeat : MonoBehaviour
{

    private AudioSource heartbeatSFX;
    private AIHunter nearestHunter;

    // Start is called before the first frame update
    void Start()
    {
        heartbeatSFX = GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
        if (nearestHunter != null) {
            float distance = (nearestHunter.transform.position - transform.position).magnitude;
            float volume = 1 - distance / 100;
            //print("volume: " + volume);
            heartbeatSFX.volume = volume;
        } else {
            heartbeatSFX.volume = 0;
        }
    }

    public void Alert(AIHunter hunter) {
        //print("alert");
        if (nearestHunter == hunter) {
            return;
        }
        if (nearestHunter == null) {
            nearestHunter = hunter;
            return;
        }
        float distance  = (hunter.transform.position - transform.position).magnitude;
        float distanceToNearestHunter = (nearestHunter.transform.position - transform.position).magnitude;
        if (distance < distanceToNearestHunter) {
            nearestHunter = hunter;
        }
    }

    public void UnAlert(AIHunter hunter) {
        if (hunter == nearestHunter) {
            nearestHunter = null;
        }
    }
}
