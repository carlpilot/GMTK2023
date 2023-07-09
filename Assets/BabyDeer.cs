using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class BabyDeer : MonoBehaviour
{

    NavMeshAgent agent;
    Animator animator;

    public GameObject target;

    private bool escaped = false;

    // Start is called before the first frame update
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponentInChildren<Animator>();

        agent.enabled = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (escaped) {
            if ((transform.position - target.transform.position).magnitude < 4) {
                agent.SetDestination(transform.position);
            } else {
                agent.SetDestination(target.transform.position);
            }
        }
    }

    public void Escape() {
        escaped = true;
        StartCoroutine(startFollowing());
    }

    IEnumerator startFollowing() {
        animator.Play("deerescape");
        yield return new WaitForSeconds(1.1f);
        agent.enabled = true;
        agent.SetDestination(target.transform.position);
    }
}
