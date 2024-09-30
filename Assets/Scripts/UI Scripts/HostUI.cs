using System.Collections;
using System.Collections.Generic;
using Mono.CSharp;
using UnityEngine;
using UnityEngine.UI;

public class HostUI : MonoBehaviour
{
    [SerializeField] private Button StartGameButton;

    private void Awake() {
        if (TankGameMultiplayer.Instance.IsServer){
            Show();
        } else {
            Hide();
        }

        StartGameButton.onClick.AddListener(()=>{
            Loader.LoadNetwork(Loader.Scene.GameScene);
        });
    }
    
    private void Show(){
        gameObject.SetActive(true);
    }

    private void Hide(){
        gameObject.SetActive(false);
    }
}
