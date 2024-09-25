using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public interface IGeneralObject{
    
    public void DestroySelf();

    public NetworkObject GetNetworkObject();

}
