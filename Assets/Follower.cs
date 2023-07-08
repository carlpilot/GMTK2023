using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Follower : MonoBehaviour
{

    public float followDistance = 2f;
    public float followSpeed = 1f;
    public float followRotationSpeed = 1f;
    public float avoidanceStrength = 20f;
    public float alignmentStrength = 8f;

    private NavMeshAgent agent;

    // Start is called before the first frame update
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    // Update is called once per frame
    void Update()
    {
        // boid-like behaviour
        Vector3 followVector = Vector3.zero;
        Vector3 awayVector = Vector3.zero;
        Vector3 heading = Vector3.zero;

        foreach (Transform child in transform.parent) {
            // if child is not this object
            if (child != transform)
            {   
                followVector += child.position;
                awayVector -= child.position;
                heading += child.forward;
            }
        }
        followVector /= transform.parent.childCount;
        followVector -= transform.position;
        awayVector /= transform.parent.childCount;
        awayVector = awayVector.normalized;
        heading /= transform.parent.childCount;
        heading = heading.normalized;
        
        Vector3 direction = (transform.forward + followVector + (awayVector * avoidanceStrength) + (heading * alignmentStrength)).normalized;
        agent.destination = transform.position + (direction * followDistance);
    }
}
