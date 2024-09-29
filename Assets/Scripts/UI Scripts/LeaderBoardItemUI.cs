using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LeaderBoardItemUI : MonoBehaviour
{
    [SerializeField] private Text nameText;
    [SerializeField] private Text killsText;

    public void SetData(string name, int kills, Color color)
    {
        nameText.text = name;
        nameText.color = color;
        killsText.text = kills.ToString();
    }
}
