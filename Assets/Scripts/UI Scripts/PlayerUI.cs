using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerUI : MonoBehaviour{
    
    [SerializeField] private Text numMissilesText;
    [SerializeField] private Image missileImage;
    private int numMissiles = 0;

    private void Start() {
        GameManager.Instance.OnGameLoaded += GameManager_OnGameLoaded;

        UpdateNumMissilesUI(numMissiles);
    }

    private void GameManager_OnGameLoaded(object sender, EventArgs e)
    {
        if (Player.LocalInstance != null){
            Player.LocalInstance.OnNumMissilesChanged += OnNumMissilesChanged;
        }
    }

    private void OnNumMissilesChanged(object sender, Player.OnNumMissiles e){
        UpdateNumMissilesUI(e.numMissiles);
    }

    private void UpdateNumMissilesUI(int numMissiles){
        if (numMissiles == 0){
            missileImage.color = new Color(missileImage.color.r, missileImage.color.g, missileImage.color.b, 0.2f);
            numMissilesText.text = "";
        } else{
            missileImage.color = new Color(missileImage.color.r, missileImage.color.g, missileImage.color.b, 1f);
        }
        numMissilesText.text = numMissiles.ToString();
    }
}
