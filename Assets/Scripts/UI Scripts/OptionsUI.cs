using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class OptionsUI : MonoBehaviour{
    
    public static OptionsUI Instance { get; private set; }

    private const string MASTER_VOLUME = "MasterVolume";
    private const string MUSIC_VOLUME = "MusicVolume";

    public event EventHandler<MusicVolumeEventArgs> OnMusicVolumeChanged;
    public class MusicVolumeEventArgs : EventArgs{
        public float volume;
    }

    [SerializeField] private Transform container;

    [SerializeField] private Button closeButton;
    [SerializeField] private Button mainMenuButton;
    [SerializeField] private Slider masterSlider;
    [SerializeField] private Slider musicSlider;
    private float masterVolume;
    private float musicVolume;

    public bool isActive;

    private void Awake() {
        Instance = this;
        
        isActive = false;
        Hide();

        if (SceneManager.GetActiveScene().name == Loader.Scene.MainMenuScene.ToString()){
            mainMenuButton.gameObject.SetActive(false);
        } else {
            mainMenuButton.gameObject.SetActive(true);
        }

        masterVolume = PlayerPrefs.GetFloat(MASTER_VOLUME, 1f);
        musicVolume = PlayerPrefs.GetFloat(MUSIC_VOLUME, 0.1f);
        masterSlider.value = masterVolume;
        musicSlider.value = musicVolume;

        masterSlider.onValueChanged.AddListener((float value)=>{
            masterVolume = value;
            PlayerPrefs.SetFloat(MASTER_VOLUME, masterVolume);
            PlayerPrefs.Save();
        });

        musicSlider.onValueChanged.AddListener((float value)=>{
            musicVolume = value;
            OnMusicVolumeChanged?.Invoke(this, new MusicVolumeEventArgs{volume = musicVolume});
            PlayerPrefs.SetFloat(MUSIC_VOLUME, musicVolume);
            PlayerPrefs.Save();
        });

        mainMenuButton.onClick.AddListener(()=>{
            if (NetworkManager.Singleton != null){
                TankGameLobby.Instance.LeaveLobby();
                NetworkManager.Singleton.Shutdown();
                Loader.Load(Loader.Scene.MainMenuScene);
            } else{
                Loader.Load(Loader.Scene.MainMenuScene);
            }
        });

        closeButton.onClick.AddListener(()=>{
            Hide();
        });
    }

    private void Update() {
        if (Input.GetKeyDown(KeyCode.Escape)){
            if (isActive){
                Hide();
                isActive = false;
            } else {
                Show();
                isActive = true;
            }
        }
    }

    public float GetMasterVolume(){
        return masterVolume;
    }

    public float GetMusicVolume(){
        return musicVolume;
    }

    private void Hide(){
        isActive = false;
        container.gameObject.SetActive(false);
    }

    private void Show(){
        isActive = true;
        container.gameObject.SetActive(true);
    }

}
