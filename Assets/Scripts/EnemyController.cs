using System;
using System.Collections.Generic;
using UnityEngine;

public class EnemyController : MonoBehaviour
{

    /// <summary>
    /// Created by: Alex Barnett
    /// Main controller for all enemies in the game, includes Enemy class, movement, aiming and shooting
    /// </summary>
    
    [SerializeField]private float searchRadius;
    [SerializeField] private float minDistanceTogether;
    [SerializeField] private float groupUpBuffer;
    [SerializeField] private float playerMoveBuffer;
    [SerializeField] private float targetDistanceToPlayer;
    [SerializeField] private float maxDistanceToPlayer;
    [SerializeField] private GameObject player;
    [SerializeField] private GameObject particleCheck;
    [SerializeField] private LayerMask searchLayer;
    [SerializeField] private LayerMask everythingLayer;
    [SerializeField] private LayerMask playerLayer;
    [SerializeField]private Rigidbody2D rb;
    [SerializeField] private Color aimingColor;
    [SerializeField]private LineRenderer lineRenderer;
    [SerializeField] private TrailRenderer shotLineRender;
    [SerializeField] private ParticleSystem _particleSystem;
    
    private Vector2 objectPosition;
    private Vector2 moveDir;
    private Vector2 playerPos;
    private Vector2 directionToPlayer;
    private Vector2 directionToHit;
    private Vector2 playerPosOff;
    private Vector2 oldDirection;
    private List<Vector2> playerPosDelay;
    private RaycastHit2D hit;
    private RaycastHit2D hit2;
    private BoxCollider2D particleCollider;
    private Color startLineColour;
    private int playerSaveCounter;
    private int playerSaveLookup;
    private int frameAimedCount;
    private float distanceToPlayer;
    private float shotTimeCounter;
    private float aimedAtPlayerTimer;
    private bool aimedAtPlayer;
    private bool aimedCountdownActive;
    private bool gameStarted;
    private bool nearPlayer;
    private bool noLongerNearPlayer;
    [NonSerialized] public bool smokeBlocking;
    [NonSerialized] public Enemy enemy = new Enemy(0,0,0,0,0,0);
    
    // Enemy class to store variables such as speed and shot speed which changed between each level, and as the level progresses
    public class Enemy
    {
        public float groupSpeed;
        public float playerSpeed;
        public int aimFrameDelay;
        public int framesTillShoot;
        public float shotTime;
        public float shootNotAimTime;

        public Enemy(float groupSpeed, float playerSpeed, int aimFrameDelay, int framesTillShoot, float shotTime,
            float shootNotAimTime)
        {
            this.groupSpeed = groupSpeed;
            this.playerSpeed = playerSpeed;
            this.aimFrameDelay = aimFrameDelay;
            this.framesTillShoot = framesTillShoot;
            this.shotTime = shotTime;
            this.shootNotAimTime = shootNotAimTime;
        }
    }

    void Start()
    {
        player = GameObject.Find("Player");
        startLineColour = lineRenderer.startColor;
        particleCollider = particleCheck.GetComponent<BoxCollider2D>();
    }

    // used by LevelCreate to set enemy variables in order to control difficulty between each level
    public void CreateEnemy(LevelController.Level level)
    {
        enemy.groupSpeed = level.enemyGroupSpeed;
        enemy.playerSpeed = level.enemyPlayerSpeed;
        enemy.aimFrameDelay = level.aimFrameDelay;
        enemy.framesTillShoot = level.framesTillShoot;
        enemy.shotTime = level.shotTime;
        enemy.shootNotAimTime = level.shootNotAimTime;
        playerPosDelay = new List<Vector2>();
        for (int i = 0; i <= enemy.aimFrameDelay; i++)
        {
            playerPosDelay.Add(new Vector2(0,0));
        }
        playerSaveLookup = 1;
        gameStarted = true;
    }

    // the main class running each physics update, mostly runs other functions
    void FixedUpdate()
    {
        if (!nearPlayer || !gameStarted)
        {
            return;
        }
        CalculateDistances();
        CheckTooFarFromPlayer();
        if (noLongerNearPlayer)
        {
            return;
        }
        StickToFriends();
        MoveToPlayer();
        SavePlayerPos();
        AimAtPlayer();
        rb.velocity = moveDir;
        CountdownTimers();
    }
    
    // sets smokeBlocking back to false, needs to be in LateUpdate so that if true it is not set false before other functions use it
    private void LateUpdate()
    {
        smokeBlocking = false;
    }

    // calculates distance to player and direction, for future use
    private void CalculateDistances()
    {
        objectPosition = rb.transform.position;
        playerPos = player.transform.position;
        distanceToPlayer = Vector2.Distance(objectPosition,playerPos);
        particleCollider.size = new Vector2(0.3f,distanceToPlayer*3+0.5f);
        particleCollider.offset = new Vector2(0, particleCollider.size.y / 2);
        directionToPlayer = (playerPos - objectPosition).normalized;
        moveDir = new Vector2(0, 0);
    }

    // if too far from player, turns off for optimisation, if close again continues FixedUpdate() 
    private void CheckTooFarFromPlayer()
    {
        if (distanceToPlayer > maxDistanceToPlayer)
        {
            frameAimedCount = 0;
            lineRenderer.startColor = startLineColour;
            lineRenderer.endColor = startLineColour;
            lineRenderer.positionCount = 0;
            noLongerNearPlayer = true;
        }
        else
        {
            noLongerNearPlayer = false;
        }
    }

    // counts down timers, takes a missing shot if the enemy saw the player previously
    private void CountdownTimers()
    {
        if (shotTimeCounter > 0)
        {
            shotTimeCounter -= Time.deltaTime;
        }
        if (aimedAtPlayerTimer > 0)
        {
            aimedAtPlayerTimer -= Time.deltaTime;
            if (aimedAtPlayerTimer <= 0 && aimedCountdownActive)
            {
                aimedCountdownActive = false;
                if (!smokeBlocking && hit2.collider && !hit2.collider.CompareTag("Player"))
                {
                    Shoot(hit2, oldDirection);
                }
            }
        }
    }

    // simple movement to stay a desired distance to/from the player, changes moveDir used in FixedUpdate once all movement calculations are done
    private void MoveToPlayer()
    {
        directionToHit = (hit.point - objectPosition).normalized;
        if (hit.collider && (hit.collider.CompareTag("Player") || hit.collider.CompareTag("Door")))
        {
            if (distanceToPlayer > targetDistanceToPlayer + playerMoveBuffer)
            {
                moveDir += directionToHit*enemy.playerSpeed;
            }
            else if (distanceToPlayer < targetDistanceToPlayer-playerMoveBuffer)
            {
                moveDir -= directionToHit*enemy.playerSpeed;
            }
        }
    }
    
    // simple boid like behavior to stay set distance to other enemies it can see. Along with MoveToPlayer, this creates flocking behavior
    private void StickToFriends()
    {
        Collider2D[] colliders = Physics2D.OverlapCircleAll(objectPosition, searchRadius, searchLayer);
        foreach (Collider2D collider in colliders)
        {
            Vector2 enemyPos = collider.gameObject.transform.position;
            Vector2 direction = (enemyPos - objectPosition).normalized;
            RaycastHit2D[] hits = Physics2D.RaycastAll(objectPosition, direction, searchRadius+10,everythingLayer);
            float distanceToEnemy = Vector2.Distance(objectPosition, enemyPos);
            if (hits.Length > 1)
            {
                if (hits[1].rigidbody && hits[1].rigidbody.CompareTag("Enemy"))
                {
                    if (distanceToEnemy > minDistanceTogether + groupUpBuffer)
                    {
                        moveDir += direction/(distanceToEnemy+0.01f)*enemy.groupSpeed;
                    }
                    else if (distanceToEnemy < minDistanceTogether-groupUpBuffer)
                    {
                        moveDir -= direction/(distanceToEnemy+0.01f)*enemy.groupSpeed;
                    }
                }
            }
        }
    }

    // handles aiming and shooting at player
    private void AimAtPlayer()
    {
        // first checks if enemy can see current player position
        hit = Physics2D.Raycast(objectPosition, directionToPlayer, 10,playerLayer);
        if (hit == false)
        {
            lineRenderer.positionCount = 0;
            return;
        }
        if (hit.collider && hit.collider.CompareTag("Player"))
        {
            // looks for player at old player position, this is created in SavePlayerPos() and stores the delayed position of the player, so the aim of enemies is off by a set amount of frames
            oldDirection = (playerPosOff - objectPosition).normalized;
            hit2 = Physics2D.Raycast(objectPosition, oldDirection, 10,playerLayer);
            float angle = Mathf.Atan2(oldDirection.y, oldDirection.x) * Mathf.Rad2Deg;
            particleCheck.transform.rotation = Quaternion.Euler(0, 0, angle-90);
            if (hit2)
            {
                // sets the aiming at player line
                lineRenderer.positionCount = 2;
                lineRenderer.SetPosition(0, objectPosition);
                lineRenderer.SetPosition(1, hit2.point);
                if(hit2.collider && hit2.collider.CompareTag("Player") && !smokeBlocking)
                {
                    // if aiming at player without smoke blocking, increases frameAimedCount counter, sets off aimedAtPlayerTimer for delayed shot in CountdownTimers() if not already running
                    if (!aimedCountdownActive)
                    {
                        aimedAtPlayerTimer = enemy.shootNotAimTime;
                        aimedCountdownActive = true;
                    }
                    frameAimedCount++;
                    lineRenderer.startColor = Color.Lerp(startLineColour,aimingColor,(float)frameAimedCount/enemy.framesTillShoot);
                    lineRenderer.endColor = Color.Lerp(startLineColour,aimingColor,(float)frameAimedCount/enemy.framesTillShoot);
                    // shoots at player if aiming long enough directly at player in delayed position
                    if (frameAimedCount >= enemy.framesTillShoot && shotTimeCounter <= 0)
                    {
                        Shoot(hit2,directionToPlayer);
                    }
                }
                else
                {
                    // decreases counter if not currently aiming at player
                    frameAimedCount--;
                    lineRenderer.startColor = Color.Lerp(startLineColour,aimingColor,(float)frameAimedCount/enemy.framesTillShoot);
                    lineRenderer.endColor = Color.Lerp(startLineColour,aimingColor,(float)frameAimedCount/enemy.framesTillShoot);
                    if (frameAimedCount < 0)
                    {
                        frameAimedCount = 0;
                    }
                }
            }
        }
        else
        {
            lineRenderer.positionCount = 0;
        }
    }

    
    // shoots towards direction, by instantiating shotLineRender with it's own script to control the bullet, resets moveDir as can cause errors moving and shooting on the same frame
    private void Shoot(RaycastHit2D hitPoint, Vector2 direction)
    {
        _particleSystem.transform.LookAt(hitPoint.point);
        _particleSystem.Play();
        float angle = Mathf.Atan2(direction.y, direction.x);
        float angleInDegrees = angle * Mathf.Rad2Deg;
        Instantiate(shotLineRender, rb.transform.position, Quaternion.Euler(0f, 0f, angleInDegrees - 90));
        shotTimeCounter = enemy.shotTime;
        moveDir = new Vector2(0, 0);
    }

    // setters for nearPlayer, used by PlayerController
    public void NearPlayer()
    {
        nearPlayer = true;
    }
    
    // sets the position of the player into an array, saves the old position to playerPosOff. 
    // the size of the array sets how delayed the position is, looks up player position one spot in front of where it is saving the position, both lookup and save position loop through array
    private void SavePlayerPos()
    {
        playerPosDelay[playerSaveCounter] = playerPos;
        playerPosOff = playerPosDelay[playerSaveLookup];
        if (playerSaveCounter >= enemy.aimFrameDelay)
        {
            playerSaveCounter = 0;
        }
        else
        {
            playerSaveCounter++;
        }
        if (playerSaveLookup >= enemy.aimFrameDelay)
        {
            playerSaveLookup = 0;
        }
        else
        {
            playerSaveLookup++;
        }
    }
}