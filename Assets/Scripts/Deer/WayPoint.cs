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

    public void setActive(bool active) {
        GetComponent<MeshRenderer>().enabled = active;
    }

}
