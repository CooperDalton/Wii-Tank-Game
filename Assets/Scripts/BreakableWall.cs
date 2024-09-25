using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.Tilemaps;

public class BreakableWall : NetworkBehaviour, IGeneralObject
{
    [SerializeField] private Tilemap tilemap;

    public void DestroyTileFromVector3(Vector3 position){
        if (IsServer) {
            Vector3Int tilePos = tilemap.WorldToCell(position);
            DestroyTile(tilePos.x, tilePos.y);
        }
    }

    public void DestroySelf(){
        NetworkObject.Despawn(true);
    }

    private void DestroyTile(int x, int y){
        DestroyTileServerRpc(x, y);
    }

    [ServerRpc(RequireOwnership = false)]
    private void DestroyTileServerRpc(int x, int y){
        DestroyTileClientRpc(x, y);
    }

    [ClientRpc]
    private void DestroyTileClientRpc(int x, int y){
        Vector3Int tilePosition = new Vector3Int(x, y, 0);
        tilemap.SetTile(tilePosition, null);
    }

    public NetworkObject GetNetworkObject()
    {
        return NetworkObject;
    }
}
