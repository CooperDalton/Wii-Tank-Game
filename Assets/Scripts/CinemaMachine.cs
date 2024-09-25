using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using Unity.VisualScripting;
using UnityEngine;

public class CinemaMachine : MonoBehaviour{
    public static CinemaMachine Instance { get; private set; }

    [SerializeField] CinemachineVirtualCamera virtualCamera;

    private void Awake() {
        Instance = this;
    }

    public void SetPlayerToCamera(Player player){
        virtualCamera.Follow = player.transform;
    }
}
