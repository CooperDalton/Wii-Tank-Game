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
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private float explosionVolume;
    //[SerializeField] private Transform marker;
    Player player;

    private void Awake() {
        audioSource.volume = explosionVolume * OptionsUI.Instance.GetMasterVolume();

        Destroy(this, 1f);
    }

    private void Start() {
        TankGameMultiplayer.Instance.ShakePlayersScreens(transform.position, magnitudeShake, timeShake);


        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, explosionRadius, collidedLayers);

        //Players (Need to check players first so LOS breaking is detected first)
        foreach (Collider2D collider in colliders) {
            GameObject hitObject = collider.gameObject;

            if (hitObject.CompareTag("Player")) {
                if (IsServer){
                    if (Physics2D.Raycast(transform.position, hitObject.transform.position - transform.position, Vector3.Distance(transform.position, hitObject.transform.position), blockLOSLayers)){
                        Debug.Log("LOS blocked");
                        break;
                    }
                    //If nothing blocking Line of sight kill player
                    Player player = hitObject.GetComponent<Player>();
                    if (this.player != null){
                        HandlePlayerBulletCollisionsServerRpc(this.player.NetworkObject, player.NetworkObject);
                    }

                    float damage = maxDamage*Vector3.Distance(player.transform.position, transform.position)/explosionRadius;
                    float damageClamped = Mathf.Clamp(damage, 0, maxDamage);
                    TakeDamageRpc(player.GetNetworkObject(), damageClamped);
                }
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

    public void SetPlayer(Player player){
        this.player = player;
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void TakeDamageRpc(NetworkObjectReference playerNetworkObjectReference, float damage){
        playerNetworkObjectReference.TryGet(out NetworkObject playerNetworkObject);
        Player player = playerNetworkObject.GetComponent<Player>();
        player.TakeDamage(damage);
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void HandlePlayerBulletCollisionsServerRpc(NetworkObjectReference bulletPlayerNetworkObjectReference, NetworkObjectReference hitPlayerNetworkObjectReference){
        bulletPlayerNetworkObjectReference.TryGet(out NetworkObject bulletPlayerNetworkObject);
        hitPlayerNetworkObjectReference.TryGet(out NetworkObject hitPlayerNetworkObject);

        Player bulletPlayer = bulletPlayerNetworkObject.GetComponent<Player>();
        Player hitPlayer = hitPlayerNetworkObject.GetComponent<Player>();
        
        hitPlayer.SetLastHitPlayer(bulletPlayer);
    }

    /*
    [ServerRpc(RequireOwnership = false)]
    private void TakeDamageServerRpc(NetworkObjectReference playerNetworkObjectReference, float damage){
        TakeDamageClientRpc(playerNetworkObjectReference, damage);
    }

    [ClientRpc]
    private void TakeDamageClientRpc(NetworkObjectReference playerNetworkObjectReference, float damage){
        playerNetworkObjectReference.TryGet(out NetworkObject playerNetworkObject);
        Player player = playerNetworkObject.GetComponent<Player>();
        player.TakeDamage(damage);
    }*/
}
