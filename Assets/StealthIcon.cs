using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StealthIcon : MonoBehaviour
{
    public Image eye;

    AIHunter hunter;

    void Awake()
    {
        
    }
    
    void Start()
    {
        
    }
    
    void Update()
    {
        hunter = GameObject.FindWithTag("AIHunter").GetComponent<AIHunter>();
        if (hunter){
            eye.gameObject.SetActive(true);
            switch(hunter.currentStealthLevel) {
                case 0:
                    eye.gameObject.SetActive(false);
                    break;
                case 1:
                    eye.color = new Color(1, 1, 1, 0.5f);
                    break;
                case 2:
                    eye.color = new Color(1, 1, 1, 0.75f);
                    break;
                case -1:
                    eye.color = new Color(1, 0, 0, 0.75f);
                    break;
            }
        } else{
            eye.gameObject.SetActive(true);
        }
    }
}
