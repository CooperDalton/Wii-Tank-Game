using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class ExplosionBulletMainMenu : MonoBehaviour
{

    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private GameObject bulletDestroyParticles;
    [SerializeField] private float newBulletSpeed;

    private void Start() {
        rb.velocity = -transform.up * newBulletSpeed;
    }


    private void OnTriggerEnter2D(Collider2D other) {
        if (other.gameObject.layer == LayerMask.NameToLayer("Wall")){
            //Reduces bounces after colliding with wall
            DestroySelf();

            if (other.gameObject.name == "PlayButton"){
                Loader.Load(Loader.Scene.LobbyScene);
            }
            if (other.gameObject.name == "QuitButton"){
                Application.Quit();
            }

            CameraShake.Instance.ShakeCamera(30f, 0.4f);
            return;  
        }

    }

    public void DestroySelf(){

        Instantiate(bulletDestroyParticles, transform.position, Quaternion.identity);
        Destroy(gameObject);
    }
}
