using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour{
    
    [SerializeField] private Player player;
    [SerializeField] private GameObject UI;
    [SerializeField] private float heightAbovePlayer;
    [SerializeField] private Image greenHealthBar;
    [SerializeField] private Image redDamageBar;
    [SerializeField] private float smoothTime;
    float healthVelocity = 0f;

    private void Start() {
        player.OnHealthChanged += Player_OnDamaged;
        greenHealthBar.fillAmount = 1f;
        redDamageBar.fillAmount = 1f;
    }

    private void Update() {
        ControlRotations();

        if (greenHealthBar.fillAmount != redDamageBar.fillAmount){
            SmoothRedBar();        
        }
    }

    private void SmoothRedBar(){
        redDamageBar.fillAmount = Mathf.SmoothDamp(redDamageBar.fillAmount, greenHealthBar.fillAmount, ref healthVelocity, smoothTime);
    }

    private void ControlRotations(){
        UI.transform.rotation = Quaternion.identity;
        transform.rotation = Quaternion.identity;
        transform.localPosition = new Vector3(0, heightAbovePlayer, 0);
    }

    private void Player_OnDamaged(object sender, Player.OnTookDamage e)
    {
        float health = e.health;
        greenHealthBar.fillAmount = health;
        //UpdateHealthServerRpc(health);
    }

    [ServerRpc(RequireOwnership = false)]
    private void UpdateHealthServerRpc(float health){
        UpdateHealthClientRpc(health);
    }

    [ClientRpc]
    private void UpdateHealthClientRpc(float health){
        greenHealthBar.fillAmount = health;
    }

    public void UpdateHealth(float health){
        greenHealthBar.fillAmount = health;
    }
}