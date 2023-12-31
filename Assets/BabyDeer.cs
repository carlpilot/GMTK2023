using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;

public class BabyDeer : MonoBehaviour
{

    NavMeshAgent agent;

    public GameObject target;

    private bool escaped = false;
    private Vector3 starting;

    // Start is called before the first frame update
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.enabled = false;

        starting = new Vector3(transform.position.x, transform.position.y, transform.position.z);
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

            if ((transform.position).magnitude > 220) {
                Destroy(gameObject);
                SceneManager.LoadScene(0);
            }
        }
    }

    public void Escape() {
        escaped = true;
        StartCoroutine(startFollowing());
    }

    IEnumerator startFollowing() {
        yield return new WaitForSeconds(0.5f);

        // set position to target position, 2 meters away 
        transform.position = target.transform.position + new Vector3(3, 0, 0);

        yield return null;

        agent.enabled = true;
        agent.SetDestination(target.transform.position);
    }

    public bool hasEscaped() {
        return escaped;
    }

    public void Reset() {
        escaped = false;
        agent.enabled = false;
        transform.position = starting;
    }
}
