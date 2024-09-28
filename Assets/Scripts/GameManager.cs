using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class GameManager : NetworkBehaviour
{
    private NetworkVariable<float> gameTimer = new NetworkVariable<float>(120f);

    
    
}
