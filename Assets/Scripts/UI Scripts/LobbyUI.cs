using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class LobbyUI : MonoBehaviour{
    
    [SerializeField] private TMP_InputField joinCodeInputField;

    private void Start() {
        joinCodeInputField.text = TankGameMultiplayer.Instance.GetPlayerName();
        joinCodeInputField.onValueChanged.AddListener((string value) =>{
            TankGameMultiplayer.Instance.SetPlayerName(value);
        });
    }
}
