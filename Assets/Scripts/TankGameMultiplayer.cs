using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.SocialPlatforms;
using Unity.VisualScripting.FullSerializer;
using System.Linq;
using System;
using Mono.CSharp;

public class TankGameMultiplayer : NetworkBehaviour{

    private const string PLAYER_NAME_PLAYER_PREFS = "PlayerName";

    public static TankGameMultiplayer Instance { get; private set; }

    public event EventHandler OnPlayerDied;
    public event EventHandler OnPlayerDataNetworkListChanged;
    public event EventHandler OnFailedToJoinGame;
    public event EventHandler<OnPlayerWonEventArgs> OnPlayerWon;
    public class OnPlayerWonEventArgs : EventArgs{
        public Player player;
    }

    [SerializeField] private BulletList bulletList;
    [SerializeField] private GeneralObjectSOList generalObjectSOList;
    [SerializeField] private Vector3[] spawnLocations;
    [SerializeField] private List<Color> playerColors;

    private List<Player> players = new List<Player>();
    private NetworkList<PlayerData> playerDataNetworkList;
    private string playerName;

    private void Awake() {
        Instance = this;

        DontDestroyOnLoad(gameObject);

        playerDataNetworkList = new NetworkList<PlayerData>();
        playerDataNetworkList.OnListChanged += PlayerDataNetworkList_OnListChanged;
    }

    private void Start() {
        playerName = PlayerPrefs.GetString(PLAYER_NAME_PLAYER_PREFS, "PlayerName" + UnityEngine.Random.Range(100, 10000));
    }

    private void PlayerDataNetworkList_OnListChanged(NetworkListEvent<PlayerData> changeEvent)
    {
        OnPlayerDataNetworkListChanged?.Invoke(this, EventArgs.Empty);
    }

    public void StartHost(){
        NetworkManager.Singleton.ConnectionApprovalCallback += NetworkManager_ConnectionApprovalCallback;
        NetworkManager.Singleton.OnClientConnectedCallback += NetworkManager_OnClientConnectedCallback;
        NetworkManager.Singleton.OnClientDisconnectCallback += NetworkManager_Server_OnClientDisconnectCallback;
        NetworkManager.Singleton.StartHost();
    }

    private void NetworkManager_Server_OnClientDisconnectCallback(ulong clientId)
    {
        for (int i = 0; i < playerDataNetworkList.Count; i++){
            PlayerData playerData = playerDataNetworkList[i];
            if (playerData.clientId == clientId){
                //Disconnected
                playerDataNetworkList.RemoveAt(i);
            }
        }
    }

    private void NetworkManager_OnClientConnectedCallback(ulong clientId)
    {
        playerDataNetworkList.Add(new PlayerData {
            clientId = clientId,
            colorId = GetFirstUnusedColorId()
        });
        SetPlayerNameServerRpc(playerName);
    }

    private void NetworkManager_ConnectionApprovalCallback(NetworkManager.ConnectionApprovalRequest request, NetworkManager.ConnectionApprovalResponse response)
    {
        //This is where we test if someones request to join should be approved or denied
        response.Approved = true;
    }

    [Rpc(SendTo.Server)]
    private void SetPlayerNameServerRpc(string playerName, RpcParams RpcParams = default){
        int playerDataIndex = GetPlayerDataIndexFromClientId(RpcParams.Receive.SenderClientId);

        PlayerData playerData = playerDataNetworkList[playerDataIndex];

        playerData.playerName = playerName;

        playerDataNetworkList[playerDataIndex] = playerData;
    }

    private int GetPlayerDataIndexFromClientId(ulong clientId){
        for (int i = 0; i < playerDataNetworkList.Count; i++){
            if (playerDataNetworkList[i].clientId == clientId){
                return i;
            }
        }

        return -1;
    }

    private int GetFirstUnusedColorId(){
        return playerDataNetworkList.Count;
    }

    public Color GetColorFromId(int id){
        return playerColors[id];
    }

    public void StartClient(){
        NetworkManager.Singleton.OnClientDisconnectCallback += NetworkManager_Client_OnClientDisconnectCallback;
        NetworkManager.Singleton.OnClientConnectedCallback += NetworkManager_Client_OnClientConnectedCallback;
        NetworkManager.Singleton.StartClient();
    }

    private void NetworkManager_Client_OnClientConnectedCallback(ulong obj)
    {
        SetPlayerNameServerRpc(GetPlayerName());
    }

    private void NetworkManager_Client_OnClientDisconnectCallback(ulong obj)
    {
        OnFailedToJoinGame?.Invoke(this, EventArgs.Empty);
    }

    public void SpawnBullet(Transform bullet, float x, float y, float rotationZ, Player player, bool isNormalBullet){
        int bulletIndex = GetBulletIndex(bullet);
        SpawnBulletServerRpc(bulletIndex, x,  y, rotationZ, player.GetNetworkObject(), isNormalBullet);
    }

    [ServerRpc(RequireOwnership = false)]
    private void SpawnBulletServerRpc(int bulletIndex, float x, float y, float rotationZ, NetworkObjectReference playerNetworkObjectReference, bool isNormalBullet){
        playerNetworkObjectReference.TryGet(out NetworkObject playerNetworkObject);
        Player player = playerNetworkObject.GetComponent<Player>();

        Transform bulletPrefab = bulletList.bullets[bulletIndex];

        Transform bullet = Instantiate(bulletPrefab, new Vector2(x, y), Quaternion.Euler(0,0,rotationZ));

        NetworkObject bulletNetworkObject = bullet.GetComponent<NetworkObject>();
        bulletNetworkObject.Spawn(true);

        SyncBulletToPlayerClientRpc(bulletNetworkObject, playerNetworkObjectReference, isNormalBullet);
    }

    [ClientRpc]
    private void SyncBulletToPlayerClientRpc(NetworkObjectReference bulletNetworkObjectReference, NetworkObjectReference playerNetworkObjectReference, bool isNormalBullet){
        bulletNetworkObjectReference.TryGet(out NetworkObject bulletNetworkObject);
        playerNetworkObjectReference.TryGet(out NetworkObject playerNetworkObject);

        Player player = playerNetworkObject.GetComponent<Player>();
        Bullet bullet = bulletNetworkObject.GetComponent<Bullet>();
        bullet.SetPlayer(player);
        if (isNormalBullet){
            player.AddBullet(bullet.transform);
        }
    }

    public void SpawnExplosionCollider(Transform explosionCollider, float x, float y, Player player){
        int generalObjectIndex = GetGeneralObjectIndex(explosionCollider);
        SpawnExplosionColliderServerRpc(generalObjectIndex, x,  y, player.GetNetworkObject());
    }

    [ServerRpc(RequireOwnership = false)]
    private void SpawnExplosionColliderServerRpc(int generalObjectIndex, float x, float y, NetworkObjectReference playerNetworkObjectReference){
        playerNetworkObjectReference.TryGet(out NetworkObject playerNetworkObject);
        Player player = playerNetworkObject.GetComponent<Player>();

        Transform explosionColliderPrefab = generalObjectSOList.GeneralObjects[generalObjectIndex];

        Transform explosionCollider = Instantiate(explosionColliderPrefab, new Vector2(x, y), Quaternion.identity);

        NetworkObject explosionColliderNetworkObject = explosionCollider.GetComponent<NetworkObject>();
        explosionColliderNetworkObject.Spawn(true);

        SpawnExplosionColliderClientRpc(explosionColliderNetworkObject, playerNetworkObjectReference);
    }

    [ClientRpc]
    private void SpawnExplosionColliderClientRpc(NetworkObjectReference explosionColliderNetworkObjectReference, NetworkObjectReference playerNetworkObjectReference){
        explosionColliderNetworkObjectReference.TryGet(out NetworkObject bulletNetworkObject);
        playerNetworkObjectReference.TryGet(out NetworkObject playerNetworkObject);

        Player player = playerNetworkObject.GetComponent<Player>();
        ExplosionCollider explosionCollider = bulletNetworkObject.GetComponent<ExplosionCollider>();
        explosionCollider.SetPlayer(player);
    }

    public void InflictDamage(Player player, float damage){
        InflictDamageServerRpc(player.GetNetworkObject(), damage);
    }

    [ServerRpc(RequireOwnership = false)]
    private void InflictDamageServerRpc(NetworkObjectReference playerNetworkObjectReference, float damage){
        InflictDamageClientRpc(playerNetworkObjectReference, damage);
    }

    [ClientRpc]
    private void InflictDamageClientRpc(NetworkObjectReference playerNetworkObjectReference, float damage){
        playerNetworkObjectReference.TryGet(out NetworkObject playerNetworkObject);
        Player player = playerNetworkObject.GetComponent<Player>();
        player.TakeDamage(damage);
    }

    public void SpawnGeneralObject(Transform generalObject, float x, float y){
        int generalObjectIndex = GetGeneralObjectIndex(generalObject);
        SpawnGeneralObjectServerRpc(generalObjectIndex, x, y);
    }

    [ServerRpc(RequireOwnership = false)]
    private void SpawnGeneralObjectServerRpc(int generalObjectIndex, float x, float y){
        Transform generalObjectPrefab = GetGeneralObjectFromIndex(generalObjectIndex);

        Transform generalObject = Instantiate (generalObjectPrefab, new Vector2(x, y), Quaternion.identity);

        NetworkObject generalObjectNetworkObject = generalObject.GetComponent<NetworkObject>();
        generalObjectNetworkObject.Spawn(true);

    }

    public void SpawnGeneralObjectWithParent(Transform generalObject, float x, float y, Player player, float rotation){
        int generalObjectIndex = GetGeneralObjectIndex(generalObject);
        SpawnGeneralObjectWithParentServerRpc(generalObjectIndex, x, y, player.GetNetworkObject(), rotation);
    }

    [ServerRpc(RequireOwnership = false)]
    private void SpawnGeneralObjectWithParentServerRpc(int generalObjectIndex, float x, float y, NetworkObjectReference playerNetworkObjectReference, float rotation){
        Transform generalObjectPrefab = GetGeneralObjectFromIndex(generalObjectIndex);
        playerNetworkObjectReference.TryGet(out NetworkObject playerNetworkObject);
        Player player = playerNetworkObject.GetComponent<Player>();

        Transform generalObject = Instantiate (generalObjectPrefab, new Vector2(x, y), Quaternion.Euler(0,0,rotation));

        NetworkObject generalObjectNetworkObject = generalObject.GetComponent<NetworkObject>();
        generalObjectNetworkObject.Spawn(true);

        if (generalObject.TryGetComponent(out FollowParent followParent)){
            GeneralObjectSetParentClientRpc(playerNetworkObject, followParent.GetNetworkObject());
        }

    }

    [ClientRpc]
    private void GeneralObjectSetParentClientRpc(NetworkObjectReference playerNetworkBehaviourReference, NetworkObjectReference followParentNetworkBehaviourReference){
        playerNetworkBehaviourReference.TryGet(out NetworkObject playerNetworkObject);
        Player player = playerNetworkObject.GetComponent<Player>();
        
        followParentNetworkBehaviourReference.TryGet(out NetworkObject followParentNetworkObject);
        FollowParent followParent = followParentNetworkObject.GetComponent<FollowParent>();

        followParent.SetParent(player.getGunShotTransform());
    }

    public void DestroyGeneralObject(IGeneralObject generalObject){
        DestroyGeneralObjectServerRpc(generalObject.GetNetworkObject());
    }

    [ServerRpc(RequireOwnership = false)]
    private void DestroyGeneralObjectServerRpc(NetworkObjectReference generalObjectNetworkObjectReference){
        generalObjectNetworkObjectReference.TryGet(out NetworkObject generalObjectNetworkObject);
        generalObjectNetworkObject.GetComponent<IGeneralObject>().DestroySelf();
    }
    
    public void DestroyBullet(Bullet bullet){
        DestroyBulletServerRpc(bullet.GetNetworkObject());
    }

    [ServerRpc(RequireOwnership = false)]
    public void DestroyBulletServerRpc(NetworkObjectReference bulletNetworkObjectReference){
        bulletNetworkObjectReference.TryGet(out NetworkObject bulletNetworkObject);
        Bullet bullet = bulletNetworkObject.GetComponent<Bullet>();

        bullet.DestroySelf();
    }

    public void ShakePlayersScreens(Vector3 explosionPosition, float maxMagnitude, float maxTime){
        float MaxDistance = 40f;

        foreach(Player player in players){
            float distance = Vector3.Distance(player.transform.position, explosionPosition);

            if (distance < MaxDistance) {
                float shakeMultiplier = 1f - (distance / MaxDistance);

                player.ShakeCamera(maxMagnitude * shakeMultiplier, maxTime);
            }
        }
    }

    public int GetBulletIndex(Transform bullet){
        return bulletList.bullets.IndexOf(bullet);
    }

    public Transform GetBullet(int bulletIndex){
        return bulletList.bullets[bulletIndex];
    }

    public int GetGeneralObjectIndex(Transform generalObject){
        return generalObjectSOList.GeneralObjects.IndexOf(generalObject);
    }

    public Transform GetGeneralObjectFromIndex(int generalObjectIndex){
        return generalObjectSOList.GeneralObjects[generalObjectIndex];
    }

    public void AddPlayer(Player player){
        players.Add(player);
    }

    public List<Player> GetPlayers(){
        return players;
    }

    public void RemovePlayer(Player player){
        players.Remove(player);
    }

    private List<Player> GetAlivePlayers(){
        List<Player> alivePlayers = new List<Player>(players);

        for(int i = 0; i < alivePlayers.Count; i++){
            if (alivePlayers.ElementAt(i).IsAlive() == false){
                alivePlayers.RemoveAt(i);
                i--;
            }
        }

        return alivePlayers;

    }

    public Player GetNextSpectatePlayer(Player player, bool right){
        int index = players.IndexOf(player);
        List<Player> alivePlayers = GetAlivePlayers();
        if (alivePlayers.Count <= 0){
            return null;
        }

        if (right){
            index++;
        } else {
            index--;
        }

        if (index > alivePlayers.Count - 1){
            return player;
        }
        
        if (index < 0){
            return player;
        }

        return alivePlayers.ElementAt(index);
    }

    [Rpc(SendTo.ClientsAndHost)]
    public void TriggerPlayerDiedEventRpc(){
        OnPlayerDied?.Invoke(this, EventArgs.Empty);
    }

    public Player SpectatePlayerFromIndex(int index){
        //Always runs when player dies
        List<Player> alivePlayers = GetAlivePlayers();
        if (alivePlayers.Count <= 0){
            return null;
        }
        return alivePlayers.ElementAt(index);
    }

    public Vector3 GetSpawnPosition(){

        bool validPos = false;
        Vector3 spawnPos = Vector3.zero;
        float spawnRange = 10f;

        while (!validPos){

            spawnPos = spawnLocations[UnityEngine.Random.Range(0, spawnLocations.Length)];

            validPos = true;
            foreach(Player player in GetAlivePlayers()){
                if (Vector3.Distance(spawnPos, player.transform.position) < spawnRange){
                    validPos = false;
                    break;
                }
            }
        }
        
        return spawnPos;

    }

    public List<ulong> GetClientIdsList(){
        List<ulong> playerIds = new List<ulong>();
        for (int i = 0; i < playerDataNetworkList.Count; i++){
            playerIds.Add(playerDataNetworkList[i].clientId);
        }

        return playerIds;
    }

    public string GetPlayerNameFromClientId(ulong clientId){
        for (int i = 0; i < playerDataNetworkList.Count; i++){
            if (playerDataNetworkList[i].clientId == clientId){
                return "" + playerDataNetworkList[i].playerName;
            }
        }

        return "";
    }

    public Color GetColorFromName(string name){
        for (int i = 0; i < playerDataNetworkList.Count; i++){
            if (playerDataNetworkList[i].playerName == name){
                return playerColors[playerDataNetworkList[i].colorId];
            }
        }

        return Color.white;
    }

    public string GetPlayerName(){
        return playerName;
    }

    public void SetPlayerName(string name){
        PlayerPrefs.SetString(PLAYER_NAME_PLAYER_PREFS, name);
        playerName = name;
    }
}
