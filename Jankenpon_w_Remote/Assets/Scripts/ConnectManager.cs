using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using TMPro;
using UnityEngine.SceneManagement;

public class ConnectManager : MonoBehaviourPunCallbacks
{
    [SerializeField] TMP_InputField usernameInput;
    [SerializeField] TMP_Text feedbackText;

    private void Start() {
        feedbackText.text = "";
    }

    public void ClickQuit()
    {
        Application.Quit();
    }

    public void ClickConnect()
    {
        feedbackText.text = "";

        if(usernameInput.text.Length < 3){
            feedbackText.text = "Username min 3 Characters";
            return;
        }

        PhotonNetwork.NickName = usernameInput.text;
        PhotonNetwork.AutomaticallySyncScene = true;

        PhotonNetwork.ConnectUsingSettings();
        feedbackText.text = "Connecting...";
    }

    public override void OnConnectedToMaster()
    {
        Debug.Log("Connected to master");
        feedbackText.text = "Connected to Master";
        StartCoroutine(LoadLevelAfterConnectedAndReady());
    }

    IEnumerator LoadLevelAfterConnectedAndReady()
    {
        while(PhotonNetwork.IsConnectedAndReady == false)
            yield return null;
        
        SceneManager.LoadScene("Lobby");
    }
}
