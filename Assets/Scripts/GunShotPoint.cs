using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class GunShotPoint : MonoBehaviour{

    [SerializeField] private float gunPointRadius;
    [SerializeField] private LayerMask wallLayers;
    private bool isColliding;
    public event EventHandler OnCollidedWithWall;
    public event EventHandler StoppedCollidingWithWall;
    
    private void Awake() {
        isColliding = false;
    }

    private void Update(){
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, gunPointRadius, wallLayers);

        if (colliders.Length > 0 && !isColliding){
            isColliding = true;
            OnCollidedWithWall?.Invoke(this, EventArgs.Empty);
        }
        if (colliders.Length == 0 && isColliding){
            isColliding = false;
            StoppedCollidingWithWall?.Invoke(this, EventArgs.Empty);
        }
    }
}
