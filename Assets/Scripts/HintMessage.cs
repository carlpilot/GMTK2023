using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class HintMessage : MonoBehaviour {

    public static HintMessage inst;
    public TMP_Text messageText;

    public float duration = 5.0f;
    float timer = 0.0f;

    private void Awake () {
        inst = this;
    }

    private void Update () {
        timer -= Time.deltaTime;
        if(timer < 0) messageText.color = new Color (1.0f, 1.0f, 1.0f, 1.0f - Mathf.Clamp01 (-timer));
    }

    public void showMessage (string text) => showMessage (text, 0);

    public void showMessage (string text, float ovrdDuration) {
        messageText.text = text;
        messageText.color = Color.white;
        timer = ovrdDuration != 0 ? ovrdDuration : duration;
    }

    public static void ShowMessage (string text) => inst.showMessage (text);
    public static void ShowMessage (string text, float ovrdDuration) => inst.showMessage (text, ovrdDuration);
}
