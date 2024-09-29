using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class LeaderBoardUI : MonoBehaviour{
    
    [SerializeField] private Transform containerGroupLayout;

    Dictionary<string, int> dict = new Dictionary<string, int>();

    private void Start() {
        TankGameMultiplayer.Instance.OnPlayerDied += TankGameMultiplayer_OnPlayerDied;
        GameManager.Instance.OnStateChanged += GameManager_OnStateChanged;
        Hide();
    }

    private void GameManager_OnStateChanged(object sender, EventArgs e)
    {
        if (GameManager.Instance.IsGamePlaying()){
            Show();
        } else{
            Hide();
        }
    }

    private void TankGameMultiplayer_OnPlayerDied(object sender, EventArgs e)
    {
        RefreshLeaderboard();
    }

    private void RefreshLeaderboard()
    {
        foreach (Transform child in containerGroupLayout)
        {
            Destroy(child.gameObject);
        }

        dict.Clear();
        //TO DO loop through player data and find corresponding player script and remake dict with proper value (On WHiteboard)

        //I don't know how this works but its supposed to sort the dict by values
        var sortedDict = dict.OrderBy(pair => pair.Value).ToDictionary(pair => pair.Key, pair => pair.Value);

        //TO DO: loop through dict creating each element
    }

    private void Show(){
        gameObject.SetActive(true);
    }

    private void Hide(){
        gameObject.SetActive(false);
    }
}
