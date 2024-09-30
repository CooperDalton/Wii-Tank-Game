using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class NetworkUI : MonoBehaviour
{

    [SerializeField] private Button startHostButton;
    [SerializeField] private Button startClientButton;
    
    private void Awake() {
        startHostButton.onClick.AddListener(() =>{
            Debug.Log("Host");
            TankGameMultiplayer.Instance.StartHost();
            Loader.LoadNetwork(Loader.Scene.LobbyWaitingToStartScene);
            Hide();
        });

        startClientButton.onClick.AddListener(() =>{
            Debug.Log("Client");
            TankGameMultiplayer.Instance.StartClient();
            //Loader.LoadNetwork(Loader.Scene.LobbyWaitingToStartScene);
            Hide();
        });
    }

    private void Start(){
        Show();
    }

    private void Show(){
        gameObject.SetActive(true);
    }

    private void Hide(){
        gameObject.SetActive(false);
    }
}
