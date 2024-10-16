using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Services.Core;
using Unity.Services.Authentication;
using System;
using Unity.Services.Lobbies.Models;
using Unity.Services.Lobbies;
using Mono.CSharp;
using UnityEngine.SceneManagement;
using Unity.Services.Relay;
using UnityEngine.Rendering;
using Unity.Services.Relay.Models;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Networking.Transport.Relay;
using Unity.Netcode.Transports.UTP;

public class TankGameLobby : MonoBehaviour{

    public const int MAX_PLAYER_AMOUNT = 4;
    private const string KEY_RELAY_JOIN_CODE = "RelayJoinCode";

    public static TankGameLobby Instance { get; private set; }

    public event EventHandler OnCreateLobbyStarted;
    public event EventHandler OnCreateLobbyFailed;
    public event EventHandler OnJoinWithCodeStarted;
    public event EventHandler OnJoinWithCodeFailed;
    public event EventHandler OnQuickJoinStarted;
    public event EventHandler OnQuickJoinFailed; 
    public event EventHandler<OnLobbyListChangedEventArgs> OnLobbyListChanged;
    public class OnLobbyListChangedEventArgs : EventArgs{
        public List<Lobby> lobbyList;
    }

    private Lobby joinedLobby;

    private float heartBeatTimer;
    private float listLobbiesTimer;

    private void Awake(){
        Instance = this;

        DontDestroyOnLoad(gameObject);

        InitializeUnityAuthentication();
    }

    private async void InitializeUnityAuthentication(){
        try {
            if (UnityServices.State != ServicesInitializationState.Initialized){
                InitializationOptions initializationOptions = new InitializationOptions();
                initializationOptions.SetProfile(UnityEngine.Random.Range(0, 10000).ToString());

                await UnityServices.InitializeAsync(initializationOptions);

                await AuthenticationService.Instance.SignInAnonymouslyAsync();
            }

        } catch(LobbyServiceException e){
            Debug.Log(e);
        }
    }

    private void Update(){
        HandleHeartBeat();
        HandlePerioudicListLobbies();
    }

    private void HandlePerioudicListLobbies(){
        if (joinedLobby == null && AuthenticationService.Instance.IsSignedIn && SceneManager.GetActiveScene().name == Loader.Scene.LobbyScene.ToString()){
            listLobbiesTimer -= Time.deltaTime;
            if (listLobbiesTimer <= 0){
                float listLobbiesTimerMax = 3f;
                listLobbiesTimer = listLobbiesTimerMax;
                ListLobbies();
            }
        }
    }

    private void HandleHeartBeat(){
        if (IsHost()){
            heartBeatTimer -= Time.deltaTime;
            if (heartBeatTimer <= 0){
                LobbyService.Instance.SendHeartbeatPingAsync(joinedLobby.Id);
                heartBeatTimer = 15f;
            }
        }

    }

    private async Task<Allocation> AllocateRelay(){
        try {
            Allocation allocation = await RelayService.Instance.CreateAllocationAsync(MAX_PLAYER_AMOUNT-1);

            return allocation;
        } catch(RelayServiceException e){
            Debug.Log(e);

            return default;
        }
    }

    private bool IsHost(){
        if (joinedLobby != null && joinedLobby.HostId == AuthenticationService.Instance.PlayerId){
            return true;
        }
        return false;
    }

    private async Task<string> GetRelayJoinCode(Allocation allocation){
        try {
            string relayJoinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);

            return relayJoinCode;
        } catch(RelayServiceException e){
            Debug.Log(e);
            return default;
        }
    }

    private async Task<JoinAllocation> JoinRelay(string relayJoinCode){
        try {
            JoinAllocation joinAllocation = await RelayService.Instance.JoinAllocationAsync(relayJoinCode);
            return joinAllocation;
        } catch(RelayServiceException e){
            Debug.Log(e);

            return default;
        }
    }

    public async void CreateLobby(string lobbyName, bool isPrivate){
        OnCreateLobbyStarted?.Invoke(this, EventArgs.Empty);
        try{
            joinedLobby = await LobbyService.Instance.CreateLobbyAsync(lobbyName, MAX_PLAYER_AMOUNT, new CreateLobbyOptions{
                IsPrivate = isPrivate
            });

            Allocation allocation = await AllocateRelay();

            string relayJoinCode = await GetRelayJoinCode(allocation);

            await LobbyService.Instance.UpdateLobbyAsync(joinedLobby.Id, new UpdateLobbyOptions{
                Data = new Dictionary<string, DataObject> {
                    {KEY_RELAY_JOIN_CODE, new DataObject(DataObject.VisibilityOptions.Member, relayJoinCode)}
                }
            });

            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(new RelayServerData(allocation, "dtls"));

            TankGameMultiplayer.Instance.StartHost();
            Loader.LoadNetwork(Loader.Scene.LobbyWaitingToStartScene);
        } catch(LobbyServiceException e){
            Debug.Log(e);
            OnCreateLobbyFailed?.Invoke(this, EventArgs.Empty);
        }
    }

    public async void JoinLobbyById(string lobbyId){
        try{
            OnJoinWithCodeStarted?.Invoke(this, EventArgs.Empty);
            joinedLobby = await LobbyService.Instance.JoinLobbyByIdAsync(lobbyId);

            string relayJoinCode = joinedLobby.Data[KEY_RELAY_JOIN_CODE].Value;
            JoinAllocation joinAllocation = await JoinRelay(relayJoinCode);

            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(new RelayServerData(joinAllocation, "dtls"));
            
            TankGameMultiplayer.Instance.StartClient();
        } catch(LobbyServiceException e){
            Debug.Log(e);
            OnJoinWithCodeFailed?.Invoke(this, EventArgs.Empty);
        }
    }

    public async void ListLobbies(){
        try{
            QueryLobbiesOptions queryLobbiesOptions = new QueryLobbiesOptions{
                Filters = new List<QueryFilter>{
                    new QueryFilter(QueryFilter.FieldOptions.AvailableSlots, "0", QueryFilter.OpOptions.GT)
                }
            };
            QueryResponse queryResponse = await LobbyService.Instance.QueryLobbiesAsync(queryLobbiesOptions);

            OnLobbyListChanged?.Invoke(this, new OnLobbyListChangedEventArgs{lobbyList = queryResponse.Results});
        } catch(LobbyServiceException e){
            Debug.Log(e);
        }
    }

    public async void JoinLobbyByCode(string lobbyCode){
        if (lobbyCode == null || lobbyCode == string.Empty){
            return;
        }
        try{
            OnJoinWithCodeStarted?.Invoke(this, EventArgs.Empty);
            joinedLobby = await LobbyService.Instance.JoinLobbyByCodeAsync(lobbyCode);

            string relayJoinCode = joinedLobby.Data[KEY_RELAY_JOIN_CODE].Value;
            JoinAllocation joinAllocation = await JoinRelay(relayJoinCode);

            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(new RelayServerData(joinAllocation, "dtls"));

            TankGameMultiplayer.Instance.StartClient();
        } catch(LobbyServiceException e){
            Debug.Log(e);
            OnJoinWithCodeFailed?.Invoke(this, EventArgs.Empty);
        }
    }

    public async void QuickJoin(){
        OnQuickJoinStarted?.Invoke(this, EventArgs.Empty);
        try{
            joinedLobby = await LobbyService.Instance.QuickJoinLobbyAsync();

            string relayJoinCode = joinedLobby.Data[KEY_RELAY_JOIN_CODE].Value;
            JoinAllocation joinAllocation = await JoinRelay(relayJoinCode);

            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(new RelayServerData(joinAllocation, "dtls"));

            TankGameMultiplayer.Instance.StartClient();
        } catch(LobbyServiceException e){
            Debug.Log(e);
            OnQuickJoinFailed?.Invoke(this, EventArgs.Empty);
        }
    }

    public async void DeleteLobby(){
        try{
            if (joinedLobby != null){
                await LobbyService.Instance.DeleteLobbyAsync(joinedLobby.Id);

                joinedLobby = null;
            }
        } catch(LobbyServiceException e){
            Debug.Log(e);
        }
    }

    public async void LeaveLobby(){
        if (joinedLobby != null){
            try {
                await LobbyService.Instance.RemovePlayerAsync(joinedLobby.Id, AuthenticationService.Instance.PlayerId);

                joinedLobby = null;
            } catch (LobbyServiceException e){
                Debug.Log(e);
            }
        }
    }

    public Lobby GetLobby(){
        return joinedLobby;
    }
}
