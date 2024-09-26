using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;
using UnityEngine.Video;

public class Bullet : NetworkBehaviour
{
    private protected Player player;

    [SerializeField] private protected Rigidbody2D rb;
    [SerializeField] private protected float bulletSpeed;
    [SerializeField] private protected int bounces;
    [SerializeField] private protected Transform bulletDestroyParticles;
    [SerializeField] private Collider2D circleCollider;
    [SerializeField] private float damage;
    private protected int bouncesLeft;


    private void Awake() {
        circleCollider.enabled = true;
    }

    private void Start()
    {
        bouncesLeft = bounces;

        rb.velocity = -transform.up * bulletSpeed;
    }

    public void SetPlayer(Player player){
        this.player = player;
        bulletSpeed = player.GetBulletSpeed();
    }

    private void OnCollisionEnter2D(Collision2D other) {
        //Detecting Collisions With Walls
        if (other.gameObject.layer == LayerMask.NameToLayer("Wall") || other.gameObject.layer == LayerMask.NameToLayer("BreakableWall")) {
            //Reduces bounces after colliding with wall
            bouncesLeft--;

            if (bouncesLeft < 0) {
                if (IsServer){
                    TankGameMultiplayer.Instance.DestroyBullet(this);
                }
                return;
            }
        }

        //Detecting Collisions With Bullets
        if (other.gameObject.CompareTag("Bullet")){
            //Destroy other bullet and self
            if (IsServer){
                TankGameMultiplayer.Instance.DestroyBullet(this);
            }
            return;
        }

        //Detecting Collisions With Players
        if (other.gameObject.CompareTag("Player")){
            Player player = other.gameObject.GetComponent<Player>();
            if (player == this.player){
                player.DestroyOwnBullet(this, damage);
            }

            if (IsServer && player != this.player){
                TankGameMultiplayer.Instance.InflictDamage(player, damage);
                TankGameMultiplayer.Instance.DestroyBullet(this);
            }
            return;
        }
    }

    public void DestroySelf(){

        if (player != null){
            ClearPlayerBulletClientRpc();
        } else {
            Debug.Log("No player found");
        }

        TankGameMultiplayer.Instance.SpawnGeneralObject(bulletDestroyParticles, transform.position.x, transform.position.y);
        Destroy(gameObject);
    }

    [ClientRpc]
    private void ClearPlayerBulletClientRpc(){
        player.RemoveBullet(transform);
    }

    public NetworkObject GetNetworkObject(){
        return NetworkObject;
    }

    public Player GetPlayer(){
        return player;
    }

}
