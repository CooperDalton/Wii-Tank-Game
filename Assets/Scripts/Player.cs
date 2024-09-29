using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Netcode;
using System.Runtime.CompilerServices;
using Unity.VisualScripting;

public class Player : NetworkBehaviour{

    public EventHandler OnShoot;
    public event EventHandler OnAltShoot;
    public event EventHandler<OnTookDamage> OnDamaged;
    public class OnTookDamage : EventArgs {
        public float health;
    }


    [Header("References")]
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private PlayerVisual playerVisual;
    [SerializeField] private GunShotPoint gunShotPoint;
    [SerializeField] private Transform headOrientation;
    [SerializeField] private Transform bodyOrientation;
    [SerializeField] private Transform gunShotPointTransform;
    [SerializeField] private Transform bulletPrefab;
    [SerializeField] private Transform explosionBulletPrefab;
    [SerializeField] private Transform altFireSmoke;
    [SerializeField] private Transform primaryFireSmoke;
    [SerializeField] private Transform aliveContainer;
    [SerializeField] private Transform deathExplosionEffect;
    [SerializeField] private Transform deathLocationPrefab;

    [Header("Settings")]
    [SerializeField] private float speed;
    [SerializeField] private float maxHealth;
    private float health;
    [SerializeField] private float shootingSpeed;
    [SerializeField] private int numberOfBullets;
    [SerializeField] private float bulletSpeed;
    private float shootingTimer;
    [SerializeField] private float turnSpeed;

    private bool isShooting;
    private int numExplosionBullets = 9999;
    private bool canShoot;
    private bool isAlive;
    //private float prevXDir = 0f;
    private CircleCollider2D circleCollider;
    private Transform deathLocation;
    private int numKills;
    private Player lastHitPlayer;

    //private Player spectatePlayer;
    private List<Transform> bulletList = new List<Transform>();

    private void Awake() {
        GameInput.Instance.OnShootAction += ShootStarted;
        GameInput.Instance.OnShootCanceledAction += ShootCanceled;
        GameInput.Instance.OnAltFireAction += AltFire;
        gunShotPoint.OnCollidedWithWall += GunShotPoint_OnCollidedWithWall;
        gunShotPoint.StoppedCollidingWithWall += GunShotPoint_StoppedCollidingWithWall;
        NetworkManager.Singleton.OnClientDisconnectCallback += NetworkManager_OnClientDisconnected;
        
        TankGameMultiplayer.Instance.AddPlayer(this);
        isAlive = true;
    }

    private void Start() {
        numKills = 0;

        circleCollider = GetComponent<CircleCollider2D>();
        health = maxHealth;
        if (!IsOwner) return;
        CinemaMachine.Instance.SetPlayerToCamera(this);
        canShoot = true;

        transform.position = TankGameMultiplayer.Instance.GetSpawnPosition();

    }

    private void Update() {
        if (!IsOwner) return;
        if (isAlive){
            //Player is Alive
            shootingTimer -= Time.deltaTime;

            Vector3 moveVector = GameInput.Instance.GetMovementVectorNormalized();
            
            RotateOrientationToMouse();
            HandleMovement(moveVector);

            if (isShooting){
                Shooting();
            }
        } else{
            //Player is Dead
            HandleSpectate();
        }
    }

    private void HandleSpectate()
    {

        //SPECTATE SYSTEM
        /*
        if (spectatePlayer != null){
            CinemaMachine.Instance.SetPlayerToCamera(spectatePlayer);
        }

        Vector3 moveVector = GameInput.Instance.GetMovementVectorNormalized();

        float xDir = moveVector.x;
        if (prevXDir != xDir){
            prevXDir = xDir;
            if (xDir > 0){
                Player nextSpectatePlayer = TankGameMultiplayer.Instance.GetNextSpectatePlayer(spectatePlayer, true);
                if (nextSpectatePlayer != null){
                    spectatePlayer = nextSpectatePlayer;
                }
            }
            if (xDir < 0){
                Player nextSpectatePlayer = TankGameMultiplayer.Instance.GetNextSpectatePlayer(spectatePlayer, false);
                if (nextSpectatePlayer != null){
                    spectatePlayer = nextSpectatePlayer;
                }
            }
        } */
    }

    private void AltFire(object sender, EventArgs e){
        if (!IsOwner) return;

        if (numExplosionBullets > 0){
            numExplosionBullets--;
            TankGameMultiplayer.Instance.SpawnBullet(explosionBulletPrefab, gunShotPointTransform.position.x, gunShotPointTransform.position.y, headOrientation.rotation.eulerAngles.z, this, false);
            TankGameMultiplayer.Instance.SpawnGeneralObjectWithParent(altFireSmoke, gunShotPointTransform.position.x, gunShotPointTransform.position.y, this, gunShotPointTransform.rotation.z);
            
            OnAltShoot?.Invoke(this, EventArgs.Empty);
        }
    }

    private void Shooting(){
        float timeTillShot = 1 / shootingSpeed;
        if (shootingTimer <= 0 && bulletList.Count < numberOfBullets && canShoot) {
            shootingTimer = timeTillShot;
            TankGameMultiplayer.Instance.SpawnBullet(bulletPrefab, gunShotPointTransform.position.x, gunShotPointTransform.position.y, headOrientation.rotation.eulerAngles.z, this, true);
            TankGameMultiplayer.Instance.SpawnGeneralObjectWithParent(primaryFireSmoke, gunShotPointTransform.position.x, gunShotPointTransform.position.y, this, gunShotPointTransform.rotation.z);

            OnShoot?.Invoke(this, EventArgs.Empty);
        }
    }

    private void NetworkManager_OnClientDisconnected(ulong obj)
    {
        TankGameMultiplayer.Instance.RemovePlayer(this);
    }

    public void AddBullet(Transform bullet){
        bulletList.Insert(0, bullet);
    }

    public void RemoveBullet(Transform bullet){
        bulletList.Remove(bullet);
    }

    private void HandleMovement(Vector3 moveVector){
        if (moveVector == Vector3.zero) {
            playerVisual.SetMoving(false);
        } else{
            playerVisual.SetMoving(true);
        }

        float moveAngle = Mathf.Atan2(moveVector.y, moveVector.x) * Mathf.Rad2Deg + 90f;
        moveAngle = (moveAngle + 360) % 360;
        float bodyZ = transform.eulerAngles.z;

        if (moveVector != Vector3.zero) {
            float deltaAngle = Mathf.DeltaAngle(bodyZ, moveAngle);
            playerVisual.SetDeltaAngle(deltaAngle);
            //Debug.Log(deltaAngle);
            if (Mathf.Abs(deltaAngle) <= 90) {
                playerVisual.SetMovingForward(true);

                rb.velocity = -transform.up * speed;
                if (Mathf.Abs(deltaAngle) < 0.5f){
                    playerVisual.SetTurning(false);
                    return;
                }

                playerVisual.SetTurning(true);
                if (deltaAngle > 0){
                    if (Mathf.DeltaAngle(bodyZ + turnSpeed * Time.deltaTime,  moveAngle) < 0){
                        transform.rotation = Quaternion.Euler(0, 0, moveAngle);
                    } else{
                        transform.rotation = Quaternion.Euler(0, 0, bodyZ + turnSpeed * Time.deltaTime);
                        playerVisual.SetTurningRight(true);
                    }
                } else{
                    if (Mathf.DeltaAngle(bodyZ + turnSpeed * Time.deltaTime,  moveAngle) > 0){
                        transform.rotation = Quaternion.Euler(0, 0, moveAngle);
                    } else{
                        transform.rotation = Quaternion.Euler(0, 0, bodyZ - turnSpeed * Time.deltaTime);
                        playerVisual.SetTurningRight(false);
                    }
                }
                
            } else{
                playerVisual.SetMovingForward(false);
                rb.velocity = transform.up * speed;
                float angleBodyZ = bodyZ + 180f;

                deltaAngle = Mathf.DeltaAngle(angleBodyZ, moveAngle);

                if (Mathf.Abs(deltaAngle) < 0.5f){
                    playerVisual.SetTurning(false);
                    return;
                }

                playerVisual.SetTurning(true);

                if (deltaAngle > 0){
                    if (Mathf.DeltaAngle(angleBodyZ + turnSpeed * Time.deltaTime,  moveAngle) < 0){
                        transform.rotation = Quaternion.Euler(0, 0, moveAngle + 180f);
                    } else{
                        transform.rotation = Quaternion.Euler(0, 0, bodyZ + turnSpeed * Time.deltaTime);
                        playerVisual.SetTurningRight(true);
                    }
                } else{
                    if (Mathf.DeltaAngle(angleBodyZ + turnSpeed * Time.deltaTime,  moveAngle) > 0){
                        transform.rotation = Quaternion.Euler(0, 0, moveAngle + 180f);
                    } else{
                        transform.rotation = Quaternion.Euler(0, 0, bodyZ - turnSpeed * Time.deltaTime);
                        playerVisual.SetTurningRight(false);
                    }
                }
            }

        }else{
            rb.velocity = Vector2.zero;
        }

        
    }

    private void RotateOrientationToMouse(){
        Vector2 cursorPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector3 cursorPosV3 = new Vector3(cursorPos.x, cursorPos.y, 0);
        Vector3 cursorToPlayer = cursorPosV3 - base.transform.position;
        float angle = Mathf.Atan2(cursorToPlayer.y, cursorToPlayer.x) * Mathf.Rad2Deg + 90f;

        headOrientation.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
    }

    public void DestroyOwnBullet(Bullet bullet, float damage){
        if (!IsOwner) return;
        TankGameMultiplayer.Instance.InflictDamage(this, damage);
        TankGameMultiplayer.Instance.DestroyBullet(bullet);
    }

    private void GunShotPoint_StoppedCollidingWithWall(object sender, EventArgs e)
    {
        canShoot = true;
    }

    private void GunShotPoint_OnCollidedWithWall(object sender, EventArgs e)
    {
        canShoot = false;
    }

    private void ShootCanceled(object sender, EventArgs e)
    {
        isShooting = false;
    }

    private void ShootStarted(object sender, EventArgs e)
    {
        isShooting = true;
    }

    public void ShakeCamera(float magnitude, float time){
        if (!IsOwner) return;
        CameraShake.Instance.ShakeCamera(magnitude, time);
    }

    public NetworkObject GetNetworkObject(){
        return NetworkObject;
    }

    public void IncreaseNumberOfBullets(){
        numberOfBullets++;
    }

    public void IncreaseMovementSpead(float speed){
        this.speed += speed;
    }

    public void IncreaseBulletSpeed(float speed){
        bulletSpeed += speed;
    }

    public float GetBulletSpeed(){
        return bulletSpeed;
    }

    public Transform getGunShotTransform(){
        return gunShotPointTransform;
    }

    public void TakeDamage(float damage){
        health -= damage;

        float damageShakeTime = Mathf.Clamp(damage/100, 0.1f, 0.5f);
        ShakeCamera(Mathf.Clamp(damage, 0, 40), damageShakeTime);

        OnDamaged?.Invoke(this, new OnTookDamage{
            health = health/maxHealth
        });

        if (health <= 0){
            Die();
        }
    }

    private void DeactiveBody(){
        DeactiveBodyServerRpc();
    }

    public void SetLastHitPlayer(Player player){
        if (player != this){
            lastHitPlayer = player;
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void DeactiveBodyServerRpc(){
        DeactiveBodyClientRpc();
    }

    [ClientRpc]
    private void DeactiveBodyClientRpc(){
        aliveContainer.gameObject.SetActive(false);
        circleCollider.enabled = false;
    }

    public void Die(){
        isAlive = false;
        //SPECTATE SYSTEM. HAD BUGS AND NOT REALLY IMPORTANT TO GAMEPLAY
        /*
        Player nextSpectatePlayer = TankGameMultiplayer.Instance.SpectatePlayerFromIndex(0);
        if (nextSpectatePlayer != null){
            spectatePlayer = nextSpectatePlayer;
        } else {
            spectatePlayer = this;
        }
        */
        //spectatePlayer = this;


        ClearInputData();
        DeactiveBody();
        

        if (IsOwner){
            TankGameMultiplayer.Instance.TriggerPlayerDiedEventRpc();
            TankGameMultiplayer.Instance.SpawnGeneralObject(deathExplosionEffect, transform.position.x, transform.position.y);
            //Make temporary object so the camera looks at it while we teleport the player
            deathLocation = Instantiate(deathLocationPrefab, transform.position, Quaternion.identity);
            CinemaMachine.Instance.SetCameraToTransform(deathLocation.transform);
            transform.position = TankGameMultiplayer.Instance.GetSpawnPosition();

            lastHitPlayer.GiveKillCredit(this);

            RespawningUI.Instance.Show();
            StartCoroutine(RespawnTimer());
            rb.velocity = Vector3.zero;
        }
    }

    public void GiveKillCredit(Player player){
        KillCreditSendToAllRpc();
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void KillCreditSendToAllRpc(){
        numKills++;
        if (IsOwner){
            //Play visuals for the person who got the kill
        }
    }

    public bool IsAlive(){
        return isAlive;
    }

    private void ClearInputData(){
        GameInput.Instance.OnShootAction -= ShootStarted;
        GameInput.Instance.OnShootCanceledAction -= ShootCanceled;
        GameInput.Instance.OnAltFireAction -= AltFire;
    }

    private IEnumerator RespawnTimer(){
        //waits 3 seconds then respawn the player
        float respawnTime = 3f;
        yield return new WaitForSeconds(respawnTime);
        Respawn();
    }


    private void Respawn()
    {
        isAlive = true;

        CinemaMachine.Instance.SetPlayerToCamera(this);
        deathLocation.GetComponent<DeathLocation>().DestroySelf();

        GameInput.Instance.OnShootAction += ShootStarted;
        GameInput.Instance.OnShootCanceledAction += ShootCanceled;
        GameInput.Instance.OnAltFireAction += AltFire;
        
        RespawnServerRpc();

    }

    [ServerRpc(RequireOwnership = false)]
    private void RespawnServerRpc(){
        RespawnClientRpc();
    }

    [ClientRpc]
    private void RespawnClientRpc(){
        isAlive = true;
        health = maxHealth;

        OnDamaged?.Invoke(this, new OnTookDamage{
            health = health/maxHealth
        });
        
        
        aliveContainer.gameObject.SetActive(true);
        circleCollider.enabled = true;

    }
}
