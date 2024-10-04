using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using System;
using Unity.VisualScripting;

public class PowerUpSpawner : NetworkBehaviour{
    
    [Header("Power Ups")]
    [SerializeField] private Transform missilePowerUp;
    [SerializeField] private Transform healthPowerUp;
    [SerializeField] private Transform speedPowerUp;
    [SerializeField] private Transform doubleShotPowerUp;

    [Header("Settings")]
    [SerializeField] private LayerMask powerUpLayer;
    [SerializeField] private float missilePowerUpTimerMax;
    private float missilePowerUpTimer;
    [SerializeField] private float healthPowerUpTimerMax;
    private float healthPowerUpTimer;
    [SerializeField] private float speedPowerUpTimerMax;
    private float speedPowerUpTimer;
    [SerializeField] private float doubleShotPowerUpTimerMax;
    private float doubleShotPowerUpTimer;

    [Header("Spawn Positions")]
    [SerializeField] private List<Vector2> missilePowerUpSpawns;
    [SerializeField] private List<Vector2> healthPowerUpSpawns;
    [SerializeField] private List<Vector2> speedPowerUpSpawns;
    [SerializeField] private List<Vector2> doubleShotPowerUpSpawns;

    private void Start(){
        GameManager.Instance.OnGameStarted += GameManager_OnGameStarted;
        GameManager.Instance.OnGameRestarted += GameManager_OnGameStarted;

        missilePowerUpTimer = missilePowerUpTimerMax;
        healthPowerUpTimer = healthPowerUpTimerMax;
        speedPowerUpTimer = speedPowerUpTimerMax;
        doubleShotPowerUpTimer = doubleShotPowerUpTimerMax;
    }

    private void Update(){
        if (!IsServer){
            return;
        }
        missilePowerUpTimer -= Time.deltaTime;
        healthPowerUpTimer -= Time.deltaTime;
        speedPowerUpTimer -= Time.deltaTime;
        doubleShotPowerUpTimer -= Time.deltaTime;

        if (missilePowerUpTimer < 0f){
            SpawnPowerUp(missilePowerUpSpawns, missilePowerUp);
            missilePowerUpTimer = missilePowerUpTimerMax;
        }

        if (healthPowerUpTimer < 0f){
            SpawnPowerUp(healthPowerUpSpawns, healthPowerUp);
            healthPowerUpTimer = healthPowerUpTimerMax;
        }

        if (speedPowerUpTimer < 0f){
            SpawnPowerUp(speedPowerUpSpawns, speedPowerUp);
            speedPowerUpTimer = speedPowerUpTimerMax;
        }

        if (doubleShotPowerUpTimer < 0f){
            SpawnPowerUp(doubleShotPowerUpSpawns, doubleShotPowerUp);
            doubleShotPowerUpTimer = doubleShotPowerUpTimerMax;
        }

    }

    private void GameManager_OnGameStarted(object sender, EventArgs e){
        if (!IsServer){
            return;
        }

        foreach(Vector2 spawn in missilePowerUpSpawns){
            if (Physics2D.OverlapCircle(spawn, 1f, powerUpLayer)){
                continue;
            } else{
                TankGameMultiplayer.Instance.SpawnGeneralObject(missilePowerUp, spawn.x, spawn.y);
            }
        }
    }

    private void SpawnPowerUp(List<Vector2> spawnPositions, Transform powerUpTransform){
        List<Vector2> spawns = new List<Vector2>(spawnPositions);
        
        do {
            int randomIndex = UnityEngine.Random.Range(0, spawns.Count);
            Vector2 spawn = spawns[randomIndex];
            
            if (Physics2D.OverlapCircle(spawn, 1f, powerUpLayer)){
                spawns.Remove(spawn);
            } else{
                TankGameMultiplayer.Instance.SpawnGeneralObject(powerUpTransform, spawn.x, spawn.y);
                return;
            }
        } while(spawns.Count > 0);
    }
}
