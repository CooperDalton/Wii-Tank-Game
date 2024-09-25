using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using System;
using System.Linq;
using UnityEngine.UIElements;

public class ExplosionCollider : NetworkBehaviour{
    
    [SerializeField] private float explosionRadius;
    [SerializeField] private LayerMask collidedLayers;
    [SerializeField] private LayerMask blockLOSLayers;
    [SerializeField] private float magnitudeShake;
    [SerializeField] private float timeShake;
    [SerializeField] private Vector3[] directionsToBreakWallsList;
    [SerializeField] private float maxDamage;
    //[SerializeField] private Transform marker;

    private void Awake() {
        Destroy(this, 1f);
    }

    private void Start() {
        TankGameMultiplayer.Instance.ShakePlayersScreens(transform.position, magnitudeShake, timeShake);

        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, explosionRadius, collidedLayers);
        //Players (Need to check players first so LOS breaking is detected first)
        foreach (Collider2D collider in colliders) {
            GameObject hitObject = collider.gameObject;

            if (hitObject.CompareTag("Player")) {
                
                Debug.DrawRay(transform.position, hitObject.transform.position - transform.position, Color.red, 10f);
                if (Physics2D.Raycast(transform.position, hitObject.transform.position - transform.position, Vector3.Distance(transform.position, hitObject.transform.position), blockLOSLayers)){
                    Debug.Log("LOS blocked");
                    break;
                }
                //If nothing blocking Line of sight kill player
                Player player = hitObject.GetComponent<Player>();
                player.TakeDamage(maxDamage*(Vector3.Distance(player.transform.position, transform.position)/explosionRadius));
            }
        }

        //Walls
        foreach (Collider2D collider in colliders) {
            GameObject hitObject = collider.gameObject;
            
            //If its a breakable wall
            if (hitObject.layer == LayerMask.NameToLayer("BreakableWall")) {
                BreakableWall breakableWall = hitObject.GetComponent<BreakableWall>();

                foreach(Vector3 direction in directionsToBreakWallsList){
                    Vector3 hitPosition = transform.position + direction.normalized*(explosionRadius/1.5f);
                    breakableWall.DestroyTileFromVector3(hitPosition);
                    //Instantiate(marker, hitPosition, Quaternion.identity);
                }
            }

        }
    }
}
