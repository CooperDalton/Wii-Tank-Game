using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class MainMenuCleanUp : MonoBehaviour{
    
    private void Awake() {
        if (NetworkManager.Singleton != null){
            Destroy(NetworkManager.Singleton.gameObject);
        }

        if (TankGameMultiplayer.Instance != null){
            Destroy(TankGameMultiplayer.Instance.gameObject);
        }

        if (TankGameLobby.Instance != null){
            Destroy(TankGameLobby.Instance.gameObject);
        }
    }

}
