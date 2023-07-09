using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BearTrap : MonoBehaviour {

    public GameObject jaw1, jaw2;

    bool isOpen = true;

    public float closeAngle;
    public float closeTime;

    IEnumerator Close () {
        int closeFrames = Mathf.RoundToInt (closeTime / Time.deltaTime);
        for (int i = 0; i < closeFrames; i++) {
            jaw1.transform.Rotate (Vector3.left * closeAngle / closeFrames);
            jaw2.transform.Rotate (Vector3.left * closeAngle / closeFrames);
            yield return new WaitForEndOfFrame ();
        }
    }

    private void OnTriggerEnter (Collider other) {
        if (isOpen && (other.tag == "Deer" || other.gameObject.layer == 6 || other.tag == "Hunter" || other.gameObject.layer == 7) && other.GetComponent<PlayerMovement> () != null) {
            StartCoroutine (Close ());
            other.GetComponent<PlayerMovement> ().isTrapped = true;
            isOpen = false;
            HintMessage.ShowMessage ("Trapped! Press SPACE repeatedly to try to free yourself");
        }
    }
}
