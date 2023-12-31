using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using TMPro;

public class VersionChecker : MonoBehaviour {

    public static string path = "https://raw.githubusercontent.com/carlpilot/GMTK2023/main/version.txt";
    public static string downloadLink = "https://carlpilot.itch.io/";

    public static string CurrentVersion;

    public GameObject newVersionNotification;
    public TMP_Text newVersionMessage;
    public TMP_Text versionDisplay;
    public TMP_Text staticVersionDisplay;

    public bool isPromptOpen { get; private set; } = false;

    private void Awake () {
        CurrentVersion = Application.version;
        staticVersionDisplay.text = CurrentVersion;
    }

    private void Start () {
        print ("Version checker active for version " + CurrentVersion);
        StartCoroutine (RequestVersion ());
    }

    IEnumerator RequestVersion () {
        using (UnityWebRequest uwr = UnityWebRequest.Get (path)) {
            yield return uwr.SendWebRequest ();

            switch (uwr.result) {
                case UnityWebRequest.Result.Success:
                    CheckVersion (uwr.downloadHandler.text);
                    break;
                default:
                    Debug.LogError (uwr.result);
                    Debug.LogError (uwr.error);
                    break;
            }
        }
    }

    public void CheckVersion (string data) {
        string[] lines = data.Split (new char[] { '\n' }, 3);

        if (lines[0] != CurrentVersion) {
            Debug.Log ("Update needed: version available: (" + lines[0] + ") vs current version: (" + CurrentVersion + ")");
            OpenPrompt (lines);
        } else {
            Debug.Log ("Up to date");
        }
    }

    public void OpenDownloadPage () {
        Application.OpenURL (downloadLink);
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit (); 
        #endif
    }

    public void OpenPrompt (string[] lines) {
        print (lines[1]);
        isPromptOpen = true;
        newVersionNotification.SetActive (true);
        string updateText = lines[1];
        newVersionMessage.text = updateText;
        versionDisplay.text = string.Format ("Currently: {0}\nAvailable: {1}", CurrentVersion, lines[0]);
    }

    public void ClosePrompt () {
        isPromptOpen = false;
        newVersionNotification.SetActive (false);
    }
}
