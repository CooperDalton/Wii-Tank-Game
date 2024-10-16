using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerNameItem : MonoBehaviour{

    [SerializeField] private Text playerNameText;

    public void SetText(string text,Color color){
        playerNameText.text = text;
        playerNameText.color = color;
    }
}
