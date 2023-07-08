using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Scary : MonoBehaviour
{

    public float range = 100.0f;

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKey("space")) {
            scare();
        }
    }

    void scare() {
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, range);
        foreach (Collider c in hitColliders) {
            if (c.gameObject.GetComponent<DeerMovement>() != null) {
                c.gameObject.GetComponent<DeerMovement>().flee(transform.position);
            }
        }
    }
}
