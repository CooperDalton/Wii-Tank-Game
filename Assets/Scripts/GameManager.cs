using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using System;
using Unity.VisualScripting;

public class GameManager : NetworkBehaviour
{
    public static GameManager Instance { get; private set; }

    public event EventHandler OnStateChanged;
    public event EventHandler OnGameStarted;
    public event EventHandler OnGameOver;

    private enum State{
        CountDownToStart,
        GamePlaying,
        GameOver
    }

    private NetworkVariable<State> state = new NetworkVariable<State>(State.CountDownToStart);

    private float countDownTimerMax = 5.99f;
    private NetworkVariable<float> countDownTimer = new NetworkVariable<float>(5f);
    private float gameTimerMax = 120f;
    private NetworkVariable<float> gameTimer = new NetworkVariable<float>(120f);
    
    public override void OnNetworkSpawn(){
        state.OnValueChanged += State_OnValueChanged;
    }

    private void State_OnValueChanged(State previousValue, State newValue)
    {
        OnStateChanged?.Invoke(this, EventArgs.Empty);
    }

    private void Awake() {
        Instance = this;

        countDownTimer.Value = countDownTimerMax;
        gameTimer.Value = gameTimerMax;
    }

    private void Update() {
        if (!IsServer) return;

        switch(state.Value){
            case State.CountDownToStart:
                countDownTimer.Value -= Time.deltaTime;
                if (countDownTimer.Value < 0){
                    state.Value = State.GamePlaying;
                    OnGameStarted?.Invoke(this, EventArgs.Empty);

                    countDownTimer.Value = countDownTimerMax;
                }
                break;
            case State.GamePlaying:
                gameTimer.Value -= Time.deltaTime;
                if (gameTimer.Value < 0){
                    state.Value = State.GameOver;
                    OnGameOver?.Invoke(this, EventArgs.Empty);

                    gameTimer.Value = gameTimerMax;
                }
                break;
            case State.GameOver:

                break;
        }
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
}
