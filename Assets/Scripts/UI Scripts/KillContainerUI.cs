using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class KillContainerUI : MonoBehaviour{
    
    [SerializeField] private Animator anim;
    [SerializeField] private Text nameText;
    [SerializeField] private GameObject imageGameObject;

    private void Start() {
        if (Player.LocalInstance != null){
            
        }
    }

    public void SetName(string name, Color color){
        nameText.text = name;
        nameText.color = color;
    }

    private void Show(){
        gameObject.SetActive(true);
    }

    private void Hide(){
        gameObject.SetActive(false);
    }
}
