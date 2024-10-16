using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VolumeContoller : MonoBehaviour
{
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private float volume;

    private void Awake() {
        audioSource.volume = volume * OptionsUI.Instance.GetMasterVolume();
    }
}
