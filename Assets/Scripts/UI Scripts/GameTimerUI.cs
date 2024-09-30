using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameTimerUI : MonoBehaviour
{
    [SerializeField] private Animator anim;
    [SerializeField] private Text gameTimerText;
    private float squishSpeed;
    private int oldNum;
    private int newNum;

    private void Start() {
        Hide();

        GameManager.Instance.OnStateChanged += GameManager_OnStateChanged;
    }

    private void GameManager_OnStateChanged(object sender, EventArgs e)
    {
        if (GameManager.Instance.IsGamePlaying()){
            Show();
        } else{
            Hide();
        }
    }

    private void Update() {
        squishSpeed = CalculateSquishSpeed();

        anim.SetFloat("GameTimer", GameManager.Instance.GetGameTimer());
        anim.SetFloat("SquishSpeed", squishSpeed);

        newNum = (int) GameManager.Instance.GetGameTimer();
        if (oldNum != newNum && newNum > 10){
            oldNum = newNum;
            gameTimerText.text = newNum.ToString();
        } else if (newNum < 10){
            gameTimerText.text = Math.Round(Convert.ToDecimal(GameManager.Instance.GetGameTimer().ToString()), 2).ToString();
        }
    }

    private float CalculateSquishSpeed(){
        float gameTimer = GameManager.Instance.GetGameTimer();
        float gameTimerMax = GameManager.Instance.GetGameTimerMax();

        return (float) (2.50 - (gameTimer / gameTimerMax)*1.5);
    }

    private void Show(){
        gameObject.SetActive(true);
    }

    private void Hide(){
        gameObject.SetActive(false);
    }
}
