using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class KilledPlayerUI : MonoBehaviour{
    
    [SerializeField] private Animator anim;
    [SerializeField] private Text nameText;
    [SerializeField] private RectTransform imageRectTransform;
    [SerializeField] private GameObject killContainer;

    private void Awake() {
        Hide();
    }

    private void Start() {
        GameManager.Instance.OnGameLoaded += GameManager_OnGameLoaded;
    }

    private void GameManager_OnGameLoaded(object sender, EventArgs e)
    {
        if (Player.LocalInstance != null){
            Player.LocalInstance.OnKilledPlayerEvent += OnKilledPlayerEvent;
        }
    }

    private void OnKilledPlayerEvent(object sender, Player.OnKilledPlayer e)
    {
        SetName(e.playerName, e.color);
        Show();
        anim.SetTrigger("Kill2");
    }

    public void SetName(string name, Color color){
        int nameLength = name.Length;

        nameText.text = name;
        nameText.color = color;

        //imageRectTransform.position = new Vector2(nameLength * 24.62f, imageRectTransform.position.y);
        imageRectTransform.anchoredPosition = new Vector2(35f + nameLength * 24.62f, imageRectTransform.anchoredPosition.y);
    }

    private void Show(){
        killContainer.SetActive(true);
    }

    private void Hide(){
        killContainer.SetActive(false);
    }
}
