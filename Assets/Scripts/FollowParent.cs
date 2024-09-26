using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class FollowParent : NetworkBehaviour
{
    
    [SerializeField] private Transform parent;

    private void Update() {
        if (parent == null) { return; }
        transform.position = parent.position;
        transform.rotation = parent.rotation;
    }

    public void SetParent(Transform newParent) {
        parent = newParent;
    }

    public Transform GetParent() {
        return parent;
    }

    public void ResetParent() {
        parent = null;
    }

    public NetworkObject GetNetworkObject(){
        return NetworkObject;
    }
    
}
