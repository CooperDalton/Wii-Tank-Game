using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PowerUpUI : MonoBehaviour{
    
    [SerializeField] private Animator anim;
    [SerializeField] private Text powerUpText;

    private void Start() {
        GameManager.Instance.OnGameLoaded += GameManager_OnGameLoaded;
    }

    private void GameManager_OnGameLoaded(object sender, EventArgs e)
    {
        Debug.Log("loaded");
        if (Player.LocalInstance != null){
            Debug.Log("player set");
            Player.LocalInstance.OnMissilePowerUp += OnMissilePowerUp;
            Player.LocalInstance.OnDoubleShotPowerUp += OnDoubleShotPowerUp;
            Player.LocalInstance.OnSpeedPowerUp += OnSpeedPowerUp;
            Player.LocalInstance.OnHealthPowerUp += OnHealthPowerUp;
        }
    }

    private void OnHealthPowerUp(object sender, EventArgs e)
    {
        powerUpText.text = "Health Pack";
        anim.SetTrigger("Health");
    }

    private void OnSpeedPowerUp(object sender, EventArgs e)
    {
        powerUpText.text = "Speed Boost";
        anim.SetTrigger("Speed");
    }

    private void OnDoubleShotPowerUp(object sender, EventArgs e)
    {
        powerUpText.text = "Double Shot";
        anim.SetTrigger("DoubleShot");
    }

    private void OnMissilePowerUp(object sender, EventArgs e)
    {
        powerUpText.text = "Missile";
        anim.SetTrigger("Missile");
    }
}
