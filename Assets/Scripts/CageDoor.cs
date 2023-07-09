using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CageDoor : MonoBehaviour {

    CurrentGameManager cgm;

    public GameObject babyDeer;

    public Transform playerDeerCamera;
    public float openAngle = 110.0f;
    public float openTime = 1.0f;
    public LayerMask layerMask;

    bool isOpen = false;

    private void Awake () {
        cgm = FindObjectOfType<CurrentGameManager> ();
        babyDeer = GameObject.FindWithTag("BabyDeer");
    }

    private void Update () {
        RaycastHit hit;
        if (cgm.isDeer && Physics.Raycast (playerDeerCamera.position, playerDeerCamera.forward, out hit, 5.0f, layerMask)) {
            if (hit.collider.gameObject == this.gameObject) {
                if(!isOpen) HintMessage.ShowMessage ("Press SPACE to open the cage", 0.1f);
                if (Input.GetKeyDown (KeyCode.Space) && !isOpen) {
                    StartCoroutine (Open ());
                }
            }
        }
    }

    IEnumerator Open () {
        isOpen = true;
        int closeFrames = Mathf.RoundToInt (openTime / Time.deltaTime);
        for (int i = 0; i < closeFrames; i++) {
            transform.Rotate (Vector3.left * openAngle / closeFrames);
            yield return new WaitForEndOfFrame ();
            babyDeer.GetComponent<BabyDeer>().Escape();
        }
    }

    public void Reset () {
        if(isOpen) transform.Rotate (Vector3.right * openAngle);
        isOpen = false;
    }
}