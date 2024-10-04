using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class SpeedPowerUp : NetworkBehaviour, IPowerUp
{
    private void OnTriggerEnter2D(Collider2D other) {
        if (!IsServer) {
            return;
        }
        
        if (other.gameObject.tag == "Player") {
            Player player = other.GetComponent<Player>();
            PowerUpEffectRpc(player.NetworkObject);
        }
    }

    [Rpc(SendTo.ClientsAndHost)]
    public void PowerUpEffectRpc(NetworkObjectReference playerNetworkObjectReference){
        playerNetworkObjectReference.TryGet(out NetworkObject playerNetworkObject);
        Player player = playerNetworkObject.GetComponent<Player>();

        player.SpeedPowerUp();
        
        DestroySelf();
    }

    public void DestroySelf(){
        Destroy(gameObject);
    }
}
