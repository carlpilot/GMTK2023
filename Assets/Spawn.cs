using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Spawn : MonoBehaviour
{

    public BabyDeer babyDeer;

    // Start is called before the first frame update
    void Start()
    {
        babyDeer = FindObjectOfType<BabyDeer>();
    }

    // Update is called once per frame
    void Update()
    {
        if ((babyDeer.gameObject.transform.position - transform.position).magnitude < 5 ) {
            if (babyDeer.hasEscaped()) {
                CurrentGameManager cgm = FindObjectOfType<CurrentGameManager>();
                babyDeer.Reset();
                cgm.CompleteDeerDay();
            }
        }
    }

}
