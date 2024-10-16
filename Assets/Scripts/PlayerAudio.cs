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
        player.OnShoot += Player_OnShoot;
        player.OnHealthPowerUp += Player_OnHealthPowerUp;
        player.OnSpeedPowerUp += Player_OnSpeedPowerUp;
        player.OnDoubleShotPowerUp += Player_OnDoubleShotPowerUp;
        player.OnMissilePowerUp += Player_OnMissilePowerUp;
    }

    private void Player_OnMissilePowerUp(object sender, EventArgs e)
    {
        PlaySoundEffect("MissilePowerUp");
    }

    private void Player_OnDoubleShotPowerUp(object sender, EventArgs e)
    {
        PlaySoundEffect("DoubleShotPowerUp");
    }

    private void Player_OnSpeedPowerUp(object sender, EventArgs e)
    {
        PlaySoundEffect("SpeedPowerUp");
    }

    private void Player_OnHealthPowerUp(object sender, EventArgs e)
    {
        PlaySoundEffect("HealthPowerUp");
    }

    private void Player_OnShoot(object sender, EventArgs e)
    {
        int randomNumber = UnityEngine.Random.Range(0, 6);
        switch (randomNumber){
            case 0:
                PlaySoundEffect("PrimaryFire1");
                break;
            case 1:
                PlaySoundEffect("PrimaryFire2");
                break;
            case 2:
                PlaySoundEffect("PrimaryFire3");
                break;
            case 3:
                PlaySoundEffect("PrimaryFire4");
                break;
            case 4:
                PlaySoundEffect("PrimaryFire5");
                break;
            case 5:
                PlaySoundEffect("PrimaryFire6");
                break;
        }
    }

    private void Player_OnAltShoot(object sender, EventArgs e)
    {
        PlaySoundEffect("AltFire");
    }

    private void PlaySoundEffect(string name)
    {
        //audioSource.clip = GetAudioClipFromName(name);
        //audioSource.Play();
        audioSource.PlayOneShot(GetAudioClipFromName(name), GetVolumeFromName(name) * OptionsUI.Instance.GetMasterVolume());
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
