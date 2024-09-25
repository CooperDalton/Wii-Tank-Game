using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct SoundEffect
{
    public string soundEffectName;
    public AudioClip clip;
    public float volume;
}

public class PlayerAudio : MonoBehaviour
{
    [SerializeField] private Player player;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private SoundEffect[] soundEffects;

    private void Start() {
        player.OnAltShoot += Player_OnAltShoot;
    }

    private void Player_OnAltShoot(object sender, EventArgs e)
    {
        PlaySoundEffect("AltFire");
    }

    private void PlaySoundEffect(string name)
    {
        //audioSource.clip = GetAudioClipFromName(name);
        //audioSource.Play();
        audioSource.PlayOneShot(GetAudioClipFromName(name), GetVolumeFromName(name));
    }

    private AudioClip GetAudioClipFromName(string name)
    {
        foreach (SoundEffect soundEffect in soundEffects)
        {
            if (soundEffect.soundEffectName == name)
            {
                return soundEffect.clip;
            }
        }
        return null;
    }

    private float GetVolumeFromName(string name){
        foreach (SoundEffect soundEffect in soundEffects)
        {
            if (soundEffect.soundEffectName == name)
            {
                return soundEffect.volume;
            }
        }
        return 1.0f;
    }
} 
