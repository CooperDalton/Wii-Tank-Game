using System.Collections;
using System.Collections.Generic;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.UI;

public class LobbyListSingleUI : MonoBehaviour{
    
    private Lobby lobby;

    [SerializeField] private Button joinLobbyButton;
    [SerializeField] private Text lobbyNameText;

    private void Awake() {
        joinLobbyButton.onClick.AddListener(() => {
            TankGameLobby.Instance.JoinLobbyById(lobby.Id);
        });
    }

    public void SetLobby(Lobby lobby){
        this.lobby = lobby;
        lobbyNameText.text = lobby.Name;
    }
}
