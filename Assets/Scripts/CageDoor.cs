using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CageDoor : MonoBehaviour {

    CurrentGameManager cgm;

    public Transform playerDeerCamera;

    bool hasShownTooltip = false;

    private void Awake () {
        cgm = FindObjectOfType<CurrentGameManager> ();
    }

    private void Update () {
        RaycastHit hit;
        if(cgm.isDeer && Physics.Raycast(playerDeerCamera.position, playerDeerCamera.forward, out hit, 3.0f)) {
            if(hit.collider.gameObject == this.gameObject) {
                if (!hasShownTooltip) HintMessage.ShowMessage ("Press SPACE to open the cage");
            }
        }
    }
}
