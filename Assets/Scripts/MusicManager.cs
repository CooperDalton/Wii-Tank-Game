using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MusicManager : MonoBehaviour
{
    [SerializeField] private AudioSource audioSource;

    private void Start() 
    {
        OptionsUI.Instance.OnMusicVolumeChanged += OptionsUI_OnMusicVolumeChanged;

        audioSource.volume = OptionsUI.Instance.GetMusicVolume();
    }

    private void OptionsUI_OnMusicVolumeChanged(object sender, OptionsUI.MusicVolumeEventArgs e)
    {
        audioSource.volume = e.volume;
    }

}
