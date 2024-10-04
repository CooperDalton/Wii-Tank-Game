using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.UI;
using UnityEngine.UIElements;
using Unity.Netcode;

public class GameEndUI : MonoBehaviour
{
    [SerializeField] private UnityEngine.UI.Button playAgainButton;
    [SerializeField] private UnityEngine.UI.Button menuButton;

    [Header("Kill Leaderboard")]
    [SerializeField] private Transform containerGroupLayout;
    [SerializeField] private Transform leaderboardItem;
    [SerializeField] private Text playerText;
    [SerializeField] private RectTransform imageTransform;
    [SerializeField] private RectTransform winsTextTransform;

    private string winningPlayer;
    private Color winningPlayerColor;

    Dictionary<string, int> dict = new Dictionary<string, int>();

    private void Awake() {
        playAgainButton.onClick.AddListener(()=>{
            GameManager.Instance.RestartGame();
            Hide();
        });

        menuButton.onClick.AddListener(() => {
            NetworkManager.Singleton.Shutdown();
            Loader.Load(Loader.Scene.MainMenuScene);
        });

        if (!NetworkManager.Singleton.IsServer) {
            playAgainButton.gameObject.SetActive(false);
        }
    }

    private void Start() {
        GameManager.Instance.OnStateChanged += GameManager_OnStateChanged;
        Hide();
    }

    private void GameManager_OnStateChanged(object sender, EventArgs e)
    {
        if(GameManager.Instance.IsGameOver()){
            RefreshLeaderboard();
            PositionImages();
            Show();
        } else{
            Hide();
        }
        
    }

    private void RefreshLeaderboard()
    {
        foreach (Transform child in containerGroupLayout)
        {
            Destroy(child.gameObject);
        }

        dict.Clear();
        //TO DO loop through player data and find corresponding player script and remake dict with proper value (On WHiteboard)
        foreach (ulong clientId in TankGameMultiplayer.Instance.GetClientIdsList()){
            foreach(Player player in TankGameMultiplayer.Instance.GetPlayers()){
                if (player.OwnerClientId == clientId){
                    dict.Add(TankGameMultiplayer.Instance.GetPlayerNameFromClientId(clientId), player.GetNumberOfKills());
                    Debug.Log(dict.Count);
                }
            }
        }
        Debug.Log("Final count" + dict.Count);
        //I don't know how this works but its supposed to sort the dict by values
        var sortedDict = dict.OrderBy(pair => pair.Value).ToDictionary(pair => pair.Key, pair => pair.Value);

        winningPlayer = sortedDict.ElementAt(sortedDict.Count-1).Key;
        winningPlayerColor = TankGameMultiplayer.Instance.GetColorFromName(winningPlayer);
        //TO DO: loop through dict creating each element
        for (int i = sortedDict.Count-1; i >= 0; i--){
            KeyValuePair<string, int> kvp = new KeyValuePair<string, int>(sortedDict.ElementAt(i).Key, sortedDict.ElementAt(i).Value);
            LeaderBoardItemUI item = Instantiate(leaderboardItem, containerGroupLayout).GetComponent<LeaderBoardItemUI>();
            item.SetData(kvp.Key, kvp.Value, TankGameMultiplayer.Instance.GetColorFromName(kvp.Key));
        }
    }

    private void PositionImages(){
        int nameLength = winningPlayer.Length;

        playerText.text = winningPlayer;
        playerText.color = winningPlayerColor;

        imageTransform.anchoredPosition = new Vector2(-35f + nameLength * -24.62f, imageTransform.anchoredPosition.y);
        winsTextTransform.anchoredPosition = new Vector2(428.92f + 15f + nameLength * 24.62f, winsTextTransform.anchoredPosition.y);
    }

    private void Show(){
        gameObject.SetActive(true);
    }

    private void Hide(){
        gameObject.SetActive(false);
    }
}
