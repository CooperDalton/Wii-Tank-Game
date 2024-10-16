using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class PlayerVisual : NetworkBehaviour
{

    [SerializeField] private Player player;
    [SerializeField] private Animator gunAnim;
    [SerializeField] private Animator bodyAnim;

    private float health;

    private void Start() {
        player.OnShoot += Player_OnShoot;
        player.OnAltShoot += Player_OnAltShoot;
        player.OnHealthChanged += Player_OnHealthChanged;

        health = 100f;
    }

    private void Player_OnHealthChanged(object sender, Player.OnTookDamage e)
    {
        if (e.health < health){
            gunAnim.SetTrigger("TookDamage");
            bodyAnim.SetTrigger("TookDamage");
        }

        health = e.health;
    }

    private void Player_OnAltShoot(object sender, EventArgs e)
    {
        if (!IsOwner) return;
        gunAnim.SetTrigger("Shoot");
    }

    private void Player_OnShoot(object sender, EventArgs e){
        if (!IsOwner) return;
        gunAnim.SetTrigger("ShootPrimary");
    }

    public void SetTurning(bool isTurning){
        bodyAnim.SetBool("Turning", isTurning);
    }

    public void SetMovingForward(bool movingForward){
        bodyAnim.SetBool("GoingForward", movingForward);
    }

    public void SetDeltaAngle(float deltaAngle){
        bodyAnim.SetFloat("DeltaAngle", deltaAngle);
    }

    public void SetMoving(bool moving){
        bodyAnim.SetBool("Moving", moving);
    }

    public void SetTurningRight(bool movingRight){
        bodyAnim.SetBool("TurningRight", movingRight);
    }
}
