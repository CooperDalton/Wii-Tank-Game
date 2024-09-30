using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerNameTag : MonoBehaviour
{
    [SerializeField] private GameObject UI;
    [SerializeField] private float heightBelowPlayer;
    [SerializeField] private Text playerNametext;

    // Update is called once per frame
    void Update()
    {
        ControlRotations();
    }

    private void ControlRotations(){
        UI.transform.rotation = Quaternion.identity;
        transform.rotation = Quaternion.identity;
        transform.localPosition = new Vector3(0, heightBelowPlayer, 0);
    }

    public void SetText(string text, Color color){
        playerNametext.text = text;
        playerNametext.color = color;
    }
}
