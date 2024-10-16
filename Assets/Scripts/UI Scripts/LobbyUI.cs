using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.UI;

public class LobbyUI : MonoBehaviour{

    [SerializeField] private TMP_InputField joinCodeInputField;
    [SerializeField] private Button CreateLobbyButton;
    [SerializeField] private Button QuickJoinButton;
    [SerializeField] private TMP_InputField lobbyNameInputField;
    [SerializeField] private Toggle isPrivateToggle;
    [SerializeField] private Button JoinLobbyByCodeButton;
    [SerializeField] private TMP_InputField playerNameInputField;
    [SerializeField] private Button mainMenuButton;
    [SerializeField] private Transform lobbyContainer;
    [SerializeField] private Transform lobbyTemplate;
    

    private void Awake() {
        mainMenuButton.onClick.AddListener(() => {
            TankGameLobby.Instance.LeaveLobby();
            Loader.Load(Loader.Scene.MainMenuScene);
        });
        CreateLobbyButton.onClick.AddListener(() => {
            TankGameLobby.Instance.CreateLobby(lobbyNameInputField.text, isPrivateToggle.isOn);
        });
        QuickJoinButton.onClick.AddListener(() => {
            TankGameLobby.Instance.QuickJoin();
        });
        JoinLobbyByCodeButton.onClick.AddListener(() => {
            TankGameLobby.Instance.JoinLobbyByCode(joinCodeInputField.text);
        });

        lobbyNameInputField.onValueChanged.AddListener((string value) => {
            if (value.Length == 0) {
                CreateLobbyButton.interactable = false;
            } else {
                CreateLobbyButton.interactable = true;
            }
        });

        lobbyTemplate.gameObject.SetActive(false);
    }

    private void Start() {
        playerNameInputField.text = TankGameMultiplayer.Instance.GetPlayerName();
        playerNameInputField.onValueChanged.AddListener((string value) =>{
            TankGameMultiplayer.Instance.SetPlayerName(value);
        });

        TankGameLobby.Instance.OnLobbyListChanged += TankGameLobby_OnLobbyListChanged;

        UpdateLobbyList(new List<Lobby>());
    }

    private void TankGameLobby_OnLobbyListChanged(object sender, TankGameLobby.OnLobbyListChangedEventArgs e)
    {
        UpdateLobbyList(e.lobbyList);
    }

    private void UpdateLobbyList(List<Lobby> lobbyList){
        foreach(Transform child in lobbyContainer){
            if (child == lobbyTemplate){
                continue;
            }
            Destroy(child.gameObject);
        }

        foreach(Lobby lobby in lobbyList){
            Transform lobbyTransform = Instantiate(lobbyTemplate, lobbyContainer);
            lobbyTransform.gameObject.SetActive(true);
            lobbyTransform.GetComponent<LobbyListSingleUI>().SetLobby(lobby);
            
        }
    }

    private void OnDestroy() {
        TankGameLobby.Instance.OnLobbyListChanged -= TankGameLobby_OnLobbyListChanged;
    }
}
