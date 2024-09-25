using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PowerUp : MonoBehaviour{
    
    [SerializeField] private float moveSpeedBuff;
    [SerializeField] private float bulletSpeedBuff;


    private void OnTriggerEnter2D(Collider2D other) {
        if (other.CompareTag("Player")) {
            Player player = other.GetComponent<Player>();

            player.IncreaseMovementSpead(moveSpeedBuff);
            player.IncreaseBulletSpeed(bulletSpeedBuff);
            player.IncreaseNumberOfBullets();


            Destroy(gameObject);
        }
    }

}
