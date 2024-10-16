using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class DestroyParticleScript : NetworkBehaviour, IGeneralObject
{

    [SerializeField] private float lifeTime;

    // Start is called before the first frame update
    void Start(){
        if (IsServer){
            TankGameMultiplayer.Instance.DestroyGeneralObject(this);
        }
    }

    public void DestroySelf(){
        Destroy(gameObject, lifeTime);
    }

    public NetworkObject GetNetworkObject(){
        return NetworkObject;
    }
}
