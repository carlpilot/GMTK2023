using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Spawn : MonoBehaviour
{

    public DeerMovement deer;
    public BabyDeer babyDeer;

    // Start is called before the first frame update
    void Start()
    {
        deer = FindObjectOfType<DeerMovement>();
        babyDeer = FindObjectOfType<BabyDeer>();
    }

    // Update is called once per frame
    void Update()
    {
        if ((babyDeer.gameObject.transform.position - transform.position).magnitude < 4 ) {
            if (babyDeer.hasEscaped()) {
                CurrentGameManager cgm = FindObjectOfType<CurrentGameManager>();
                babyDeer.Reset();
                cgm.CompleteDeerDay();
            }
        }
    }

}
