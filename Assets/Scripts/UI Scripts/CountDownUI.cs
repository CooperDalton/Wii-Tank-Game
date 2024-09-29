using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CountDownUI : MonoBehaviour
{
    [SerializeField] private Text countDownText;
    [SerializeField] private Animator anim;
    int oldNum = 0;
    int newNum;

    private void Awake() {
        GameManager.Instance.OnStateChanged += GameManager_OnStateChanged;
    }

    private void Start() {
        Show();
    }

    private void Update() {
        newNum = (int) GameManager.Instance.GetCountDownTimer();
        if (oldNum != newNum){
            oldNum = newNum;
            anim.SetTrigger("NewNum");
            countDownText.text = newNum.ToString();
        }
    }

    private void GameManager_OnStateChanged(object sender, EventArgs e)
    {
        if (GameManager.Instance.IsCountDownToStart()){
            Show();
        } else{
            Hide();
        }

    }

    private void Show(){
        gameObject.SetActive(true);
    }

    private void Hide(){
        gameObject.SetActive(false);
    }
}
