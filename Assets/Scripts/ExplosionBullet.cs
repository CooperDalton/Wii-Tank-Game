using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class ExplosionBullet : Bullet
{

    [SerializeField] private float newBulletSpeed;

    private void Start() {
        rb.velocity = -transform.up * newBulletSpeed;
    }

    public new void SetPlayer(Player player){
        this.player = player;
    }

    private void OnTriggerEnter2D(Collider2D other) {
        if (other.gameObject.layer == LayerMask.NameToLayer("Wall") || other.gameObject.layer == LayerMask.NameToLayer("BreakableWall")){
            //Reduces bounces after colliding with wall
            if (IsServer){
                TankGameMultiplayer.Instance.DestroyBullet(this);
            }
            return;  
        }

        //Detecting Collisions With Bullets
        if (other.gameObject.CompareTag("Bullet")){
            //Don't explode if bullet is from same player
            if (other.gameObject.GetComponent<Bullet>().GetPlayer() == player){
                return;
            }
            //Destroy other bullet and self
            if (IsServer){
                TankGameMultiplayer.Instance.DestroyBullet(this);
                TankGameMultiplayer.Instance.DestroyBullet(other.gameObject.GetComponent<Bullet>());
            }
            return;
        }

        //Detecting Collisions With Players
        if (other.gameObject.CompareTag("Player")){
            Player hitPlayer = other.gameObject.GetComponent<Player>();

            if (IsServer && hitPlayer != player){
                TankGameMultiplayer.Instance.DestroyBullet(this);
            }
            return;
        }
    }

    public new void DestroySelf(){

        TankGameMultiplayer.Instance.SpawnGeneralObject(bulletDestroyParticles, transform.position.x, transform.position.y);
        Destroy(gameObject);
    }
}
