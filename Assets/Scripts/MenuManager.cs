using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour {

    public GameObject LoadingScreen;
    public RectTransform LoadingCircle;

    private void Update () {
        LoadingCircle.transform.Rotate (Vector3.forward * -360.0f * Time.deltaTime);
    }

    public static void SwitchScene (int scene) {
        SceneManager.LoadScene (scene);
    }

    public void SwitchSceneAsync (int scene) {
        LoadingScreen.SetActive (true);
        AsyncOperation a = SceneManager.LoadSceneAsync (scene);
        a.allowSceneActivation = true;
    }

    public static void Quit () {
        Application.Quit ();
    }
}
