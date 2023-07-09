using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class DeathMessage : MonoBehaviour
{
    public GameObject deathDeer;
    public GameObject deathHunter;
    public TextMeshProUGUI text;
    public Image fader;

    float faderTimer = 0f;
    void Awake()
    {
        
    }
    
    void Start()
    {
        deathDeer.SetActive(CurrentGameManager.lastWasDeer);
        deathHunter.SetActive(!CurrentGameManager.lastWasDeer);
        text.text = "day " + CurrentGameManager.lastReachedDay.ToString();
    }
    
    void Update()
    {
        faderTimer += Time.deltaTime;
        fader.color = new Color(0, 0, 0, Mathf.Lerp(0, 1, faderTimer/5));
        if (faderTimer > 5f) SceneManager.LoadScene(0);
    }
}
