using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.UI;

public class LobbyWaitingToStartUI : MonoBehaviour{

    [SerializeField] private Text LobbyNameText;
    [SerializeField] private Text LobbyCodeText;

    [SerializeField] private Transform playerNamesContainer;
    [SerializeField] private Transform playerNameItemPrefab;

    private Lobby joinedLobby;

    private void Start() {
        TankGameMultiplayer.Instance.OnPlayerDataNetworkListChanged += TankGameMultiplayer_OnPlayerDataNetworkListChanged;

        joinedLobby = TankGameLobby.Instance.GetLobby();

        LobbyNameText.text = joinedLobby.Name;
        LobbyCodeText.text = joinedLobby.LobbyCode.ToString();

        RefreshPlayerList();
    }

    private void TankGameMultiplayer_OnPlayerDataNetworkListChanged(object sender, EventArgs e){
        RefreshPlayerList();
    }

    private void RefreshPlayerList(){
        foreach(Transform child in playerNamesContainer){
            Destroy(child.gameObject);
        }

        foreach(ulong clientId in TankGameMultiplayer.Instance.GetClientIdsList()){
            Transform playerNameItem = Instantiate(playerNameItemPrefab, playerNamesContainer);
            playerNameItem.GetComponent<PlayerNameItem>().SetText(TankGameMultiplayer.Instance.GetPlayerNameFromClientId(clientId), TankGameMultiplayer.Instance.GetColorFromId((int) clientId));
        }
    }

    private void OnDestroy() {
        TankGameMultiplayer.Instance.OnPlayerDataNetworkListChanged -= TankGameMultiplayer_OnPlayerDataNetworkListChanged;
    }
}
