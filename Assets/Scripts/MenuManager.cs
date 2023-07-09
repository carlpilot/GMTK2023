using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour {

    public static void SwitchScene (int scene) {
        SceneManager.LoadScene (scene);
    }

    public static void Quit () {
        Application.Quit ();
    }
}
