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

    
    public static TankGameMultiplayer Instance { get; private set; }

    public event EventHandler<OnPlayerWonEventArgs> OnPlayerWon;
    public class OnPlayerWonEventArgs : EventArgs{
        public Player player;
    }

    [SerializeField] private BulletList bulletList;
    [SerializeField] private GeneralObjectSOList generalObjectSOList;
    [SerializeField] private Vector3[] spawnLocations;

    private List<Player> players = new List<Player>();

    private void Awake() {
        Instance = this;
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


        while (!validPos){

            spawnPos = spawnLocations[UnityEngine.Random.Range(0, spawnLocations.Length)];

            validPos = true;
            foreach(Player player in GetAlivePlayers()){
                if (Vector3.Distance(spawnPos, player.transform.position) < 5f){
                    validPos = false;
                    break;
                }
            }
        }
        
        return spawnPos;

    }
}
