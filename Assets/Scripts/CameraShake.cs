using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;

public class CameraShake : MonoBehaviour{
    
    public static CameraShake Instance {get; private set;}

    private CinemachineVirtualCamera cinemachineVirtualCamera;
    private float timer = 0f;

    private CinemachineBasicMultiChannelPerlin perlinChannel;

    private void Awake() {
        cinemachineVirtualCamera = GetComponent<CinemachineVirtualCamera>();
        Instance = this;
    }

    private void Start() {
        StopShake();
    }
  
    public void ShakeCamera(float intensity, float time){
        timer = time;
        perlinChannel = cinemachineVirtualCamera.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
        perlinChannel.m_AmplitudeGain = intensity;
    }

    private void StopShake(){
        perlinChannel = cinemachineVirtualCamera.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
        perlinChannel.m_AmplitudeGain = 0;
        timer = 0;
    }

    private void Update() {
        if (timer > 0){
            timer -= Time.deltaTime;
            if (timer <= 0){
                StopShake();
            }
        }
    }

}
