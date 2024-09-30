using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using System;
using Unity.VisualScripting;
using UnityEngine.SceneManagement;

public class GameManager : NetworkBehaviour
{
    public static GameManager Instance { get; private set; }

    public event EventHandler OnStateChanged;
    public event EventHandler OnGameStarted;
    public event EventHandler OnGameOver;
    public event EventHandler OnGameLoaded;

    [SerializeField] private Transform playerPrefab;

    private enum State{
        CountDownToStart,
        GamePlaying,
        GameOver
    }

    private NetworkVariable<State> state = new NetworkVariable<State>(State.CountDownToStart);

    private float countDownTimerMax = 1.99f;
    private NetworkVariable<float> countDownTimer = new NetworkVariable<float>(1.99f);
    private float gameTimerMax = 120f;
    private NetworkVariable<float> gameTimer = new NetworkVariable<float>(120f);
    

    private void State_OnValueChanged(State previousValue, State newValue)
    {
        OnStateChanged?.Invoke(this, EventArgs.Empty);
    }

    private void Awake() {
        Instance = this;
        
    }

    public override void OnNetworkSpawn(){
        state.OnValueChanged += State_OnValueChanged;

        if (IsServer){
            NetworkManager.Singleton.SceneManager.OnLoadEventCompleted += SceneManager_OnLoadEventCompleted;
        }
    }


    private void Update() {
        if (!IsServer) return;

        switch(state.Value){
            case State.CountDownToStart:
                countDownTimer.Value -= Time.deltaTime;
                if (countDownTimer.Value < 0){
                    state.Value = State.GamePlaying;
                    TriggerOnGameStartedRpc();

                    countDownTimer.Value = countDownTimerMax;
                    gameTimer.Value = gameTimerMax;
                }
                break;
            case State.GamePlaying:
                gameTimer.Value -= Time.deltaTime;
                if (gameTimer.Value < 0){
                    state.Value = State.GameOver;
                    TriggerOnGameOverRpc();

                    gameTimer.Value = gameTimerMax;
                }
                break;
            case State.GameOver:

                break;
        }
    }

    private void SceneManager_OnLoadEventCompleted(string sceneName, LoadSceneMode loadSceneMode, List<ulong> clientsCompleted, List<ulong> clientsTimedOut)
    {
        foreach (ulong clientId in NetworkManager.Singleton.ConnectedClientsIds){
            Transform playerTransform = Instantiate(playerPrefab);
            playerTransform.GetComponent<NetworkObject>().SpawnAsPlayerObject(clientId, true);
        }

        OnGameLoaded?.Invoke(this, EventArgs.Empty);
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void TriggerOnGameOverRpc(){
        OnGameOver?.Invoke(this, EventArgs.Empty);
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void TriggerOnGameStartedRpc(){
        OnGameStarted?.Invoke(this, EventArgs.Empty);
    }

    public float GetGameTimer(){
        return gameTimer.Value;
    }

    public float GetCountDownTimer(){
        return countDownTimer.Value;
    }

    public bool IsCountDownToStart(){
        return state.Value == State.CountDownToStart;
    }

    public bool IsGamePlaying(){
        return state.Value == State.GamePlaying;
    }

    public bool IsGameOver(){
        return state.Value == State.GameOver;
    }

    public float GetGameTimerMax(){
        return gameTimerMax;
    }
}
