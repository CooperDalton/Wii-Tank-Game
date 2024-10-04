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
                HandlePlayerBulletCollisionsServerRpc(this.player.NetworkObject, player.NetworkObject);
            }
            return;
        }
    }

    [ServerRpc]
    private void HandlePlayerBulletCollisionsServerRpc(NetworkObjectReference bulletPlayerNetworkObjectReference, NetworkObjectReference hitPlayerNetworkObjectReference){
        HandlePlayerBulletCollisionsClientRpc(bulletPlayerNetworkObjectReference, hitPlayerNetworkObjectReference);
    }

    [ClientRpc]
    private void HandlePlayerBulletCollisionsClientRpc(NetworkObjectReference bulletPlayerNetworkObjectReference, NetworkObjectReference hitPlayerNetworkObjectReference){
        if (!bulletPlayerNetworkObjectReference.TryGet(out NetworkObject bulletPlayerNetworkObject)){
            return;
        }
        hitPlayerNetworkObjectReference.TryGet(out NetworkObject hitPlayerNetworkObject);

        Player hitPlayer = hitPlayerNetworkObject.GetComponent<Player>();
        if (bulletPlayerNetworkObject.TryGetComponent(out Player bulletPlayer)){
            hitPlayer.SetLastHitPlayer(bulletPlayer);
        }
        
    }

    public virtual void DestroySelf(){
        TankGameMultiplayer.Instance.SpawnGeneralObject(bulletDestroyParticles, transform.position.x, transform.position.y);
        Destroy(gameObject);
    }

    public NetworkObject GetNetworkObject(){
        return NetworkObject;
    }

    public Player GetPlayer(){
        return player;
    }

}
