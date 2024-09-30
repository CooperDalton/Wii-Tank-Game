using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroySelf : MonoBehaviour
{
    [SerializeField] private float timeAlive;

    private void Start() {
        Destroy(gameObject, timeAlive);
    }
}
