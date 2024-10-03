using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class CountDownUI : MonoBehaviour
{
    [SerializeField] private Text countDownText;
    [SerializeField] private Animator anim;
    int oldNum = 0;
    int newNum;
    
    private void Start() {
        GameManager.Instance.OnStateChanged += GameManager_OnStateChanged;
        countDownText.text = ((int) GameManager.Instance.GetCountDownTimer()).ToString();
        Show();
    }

    private void Update() {
        if (!GameManager.Instance.IsCountDownToStart()){
            return;
        }
        newNum = (int) GameManager.Instance.GetCountDownTimer() + 1;
        if (oldNum != newNum){
            oldNum = newNum;
            anim.SetTrigger("NewNumber");
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
