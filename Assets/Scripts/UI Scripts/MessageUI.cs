using System;
using UnityEngine;
using UnityEngine.UI;

public class MessageUI : MonoBehaviour {
    
    [SerializeField] private Button closeButton;
    [SerializeField] private Text messageText;
    [SerializeField] private Transform container;

    private void Awake() {
        closeButton.onClick.AddListener(Hide);
        Hide();
    }

    private void Start() {
        TankGameLobby.Instance.OnCreateLobbyFailed += Instance_OnCreateLobbyFailed;
        TankGameLobby.Instance.OnCreateLobbyStarted += Instance_OnCreateLobbyStarted;
        TankGameLobby.Instance.OnJoinWithCodeFailed += Instance_OnJoinWithCodeFailed;
        TankGameLobby.Instance.OnJoinWithCodeStarted += Instance_OnJoinWithCodeStarted;
        TankGameLobby.Instance.OnQuickJoinFailed += Instance_OnQuickJoinFailed;
        TankGameLobby.Instance.OnQuickJoinStarted += Instance_OnQuickJoinStarted;
    }

    private void Instance_OnQuickJoinStarted(object sender, EventArgs e)
    {
        SetMessage("Joining lobby...");
    }

    private void Instance_OnQuickJoinFailed(object sender, EventArgs e)
    {
        Debug.Log("whatsup");
        SetMessage("Failed to join lobby \n No lobby found");
    }

    private void Instance_OnJoinWithCodeStarted(object sender, EventArgs e)
    {
        SetMessage("Joining lobby...");
    }

    private void Instance_OnJoinWithCodeFailed(object sender, EventArgs e)
    {
        SetMessage("Failed to join lobby \n Lobby code could not be found");
    }

    private void Instance_OnCreateLobbyStarted(object sender, EventArgs e)
    {
        SetMessage("Creating lobby...");
    }

    private void Instance_OnCreateLobbyFailed(object sender, EventArgs e)
    {
        SetMessage("Failed to create lobby");
    }

    private void SetMessage(string message) {
        Show();
        messageText.text = message;
    }

    private void Show(){
        container.gameObject.SetActive(true);
    }

    private void Hide(){
        container.gameObject.SetActive(false);
    }        
}
