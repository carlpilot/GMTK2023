using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WayPoint : MonoBehaviour
{

    private DeerMovement leader;
    public int radius = 5;

    // Start is called before the first frame update
    void Start()
    {
        GetComponent<MeshRenderer>().enabled = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (leader != null) {
            if (Vector3.Distance(transform.position, leader.transform.position) < 1) {
                leader.setNewTarget(this);
            }
        }
    }

    public void setActive(bool active) {
        GetComponent<MeshRenderer>().enabled = active;
    }

}
