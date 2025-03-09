using UnityEngine;

public class EnemyControllerCutscene : MonoBehaviour
{
    
    /// <summary>
    /// Created by: Alex Barnett
    /// Basically a striped down version of EnemyController for controlling the enemies in the intro cutscene
    /// </summary>
    
    [SerializeField] private float playerMoveBuffer;
    [SerializeField] private float playerSpeed;
    [SerializeField] private float targetDistanceToPlayer;
    [SerializeField] private float timeTillStart;
    [SerializeField] private float timeTillAim;
    [SerializeField] public int framesTillShot;
    [SerializeField] private GameObject player;
    [SerializeField]private Rigidbody2D rb;
    [SerializeField]private LineRenderer lineRenderer;
    [SerializeField] private TrailRenderer shotLineRender;
    [SerializeField] private LayerMask playerLayer;
    [SerializeField] private Color aimingColor;
    [SerializeField] private ParticleSystem _particleSystem;

    private Vector2 objectPosition;
    private Vector2 moveDir;
    private Vector2 playerPos;
    private Vector2 directionToPlayer;
    private RaycastHit2D hit;
    private Color startLineColour;
    private float distanceToPlayer;
    private int frameAimedCount;
    private bool hasShot;
    
    void Start()
    {
        startLineColour = lineRenderer.startColor;
    }
    

    // starts after timeTillStart has counted down, aims at and shoots "robo-wife" during the opening cutscene
    void FixedUpdate()
    {
        if (hasShot)
        {
            return;
        }
        if (timeTillStart > 0)
        {
            timeTillStart -= Time.deltaTime;
            return;
        }
        CalculateDistances();
        MoveToPlayer();
        rb.velocity = moveDir;
        if (timeTillAim > 0)
        {
            timeTillAim -= Time.deltaTime;
            return;
        }
        AimAtPlayer();
    }
    
    // calculates distance to player and direction, for future use
    private void CalculateDistances()
    {
        objectPosition = rb.transform.position;
        playerPos = player.transform.position;
        distanceToPlayer = Vector2.Distance(objectPosition,playerPos);
        directionToPlayer = (playerPos - objectPosition).normalized;
        moveDir = new Vector2(0, 0);
    }

    // simple movement to stay a desired distance to/from the player
    private void MoveToPlayer()
    {
        if (distanceToPlayer > targetDistanceToPlayer + playerMoveBuffer)
        {
            moveDir += directionToPlayer*playerSpeed;
        }
        else if (distanceToPlayer < targetDistanceToPlayer-playerMoveBuffer)
        {
            moveDir -= directionToPlayer*playerSpeed;
        }
    }
    
    
    // handles aiming and shooting at player same as EnemyController.AimAtPlayer() but without the delay, and only shoots once as needed in cutscene
    private void AimAtPlayer()
    {
        hit = Physics2D.Raycast(objectPosition, directionToPlayer, 10,playerLayer);
        if (hit == false)
        {
            lineRenderer.positionCount = 0;
        }else if (!hasShot){
            lineRenderer.positionCount = 2;
            lineRenderer.SetPosition(0, objectPosition);
            lineRenderer.SetPosition(1, hit.point);
            if(hit.collider && hit.collider.CompareTag("Wife"))
            {
                frameAimedCount++;
                lineRenderer.startColor = Color.Lerp(startLineColour,aimingColor,(float)frameAimedCount/framesTillShot);
                lineRenderer.endColor = Color.Lerp(startLineColour,aimingColor,(float)frameAimedCount/framesTillShot);
                if (frameAimedCount >= framesTillShot && !hasShot)
                {
                    Shoot(hit,directionToPlayer);
                    lineRenderer.positionCount = 0;
                    hasShot = true;
                }
            }
            else
            {
                frameAimedCount--;
                lineRenderer.startColor = Color.Lerp(startLineColour,aimingColor,(float)frameAimedCount/framesTillShot);
                lineRenderer.endColor = Color.Lerp(startLineColour,aimingColor,(float)frameAimedCount/framesTillShot);
                if (frameAimedCount < 0)
                {
                    frameAimedCount = 0;
                }
            }
        }
     
    }

    // shoots towards direction, by instantiating shotLineRender with it's own script to control the bullet, resets moveDir as can cause errors moving and shooting on the same frame
    private void Shoot(RaycastHit2D hit, Vector2 direction)
    {
        _particleSystem.transform.LookAt(hit.point);
        _particleSystem.Play();
        float angle = Mathf.Atan2(direction.y, direction.x);
        float angleInDegrees = angle * Mathf.Rad2Deg;
        Instantiate(shotLineRender, rb.transform.position, Quaternion.Euler(0f, 0f, angleInDegrees - 90)).GetComponent<ShotLine>().inCutscene = true; ;
    }
}