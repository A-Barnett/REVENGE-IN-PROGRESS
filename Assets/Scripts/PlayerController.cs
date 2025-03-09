using System;
using System.Collections.Generic;
using UnityEngine;
using Image = UnityEngine.UI.Image;

public class PlayerController : MonoBehaviour
{
    
    /// <summary>
    /// Created by: Alex Barnett
    /// Player controller for movement, shooting and throwing grenades
    /// </summary>
    
    [SerializeField] private bool invincible;
    [SerializeField] private float speed;
    [SerializeField] private float diagonalDampener;
    [SerializeField] private float shotSpeed;
    [SerializeField] private float searchRadius;
    [SerializeField] private float smokeCooldown;
    [SerializeField] private float grenadeCooldown;
    [SerializeField] private LineRenderer lineRenderer;
    [SerializeField] private TrailRenderer shotLineRender;
    [SerializeField] private LayerMask aimLayerMask;
    [SerializeField] private LayerMask searchLayer;
    [SerializeField] private GameController gameController;
    [SerializeField] private ParticleSystem _particleSystem;
    [SerializeField] private GameObject smokeGrenade;
    [SerializeField] private GameObject grenade;
    [SerializeField] private Image cooldownImage;
    [SerializeField] private Camera mainCam;
    [NonSerialized]public bool gameStarted;

    private List<Collider2D> foundColliders = new List<Collider2D>();
    private RaycastHit2D hit;
    private Rigidbody2D rb;
    private Vector3 direction;
    private float smokeTimer;
    private float shotTimer;
    private float grenadeTimer;
    
    void Start()
    {
        rb = gameObject.GetComponentInChildren<Rigidbody2D>();
    }
    
    // aiming done in Update() for smoother look, finds mouse position in world point, sets lineRender points to player and hit.point where possible
    void Update()
    {
        if (!gameStarted)
        {
            return;
        }
        Vector3 mousePosition = Input.mousePosition;
        mousePosition.z = -mainCam.transform.position.z;
        Vector3 mouseWorldPosition = mainCam.ScreenToWorldPoint(mousePosition);
        Vector3 objectPosition = rb.transform.position;
        direction = (mouseWorldPosition - objectPosition).normalized;
        hit = Physics2D.Raycast(objectPosition, direction, Mathf.Infinity, aimLayerMask);
        lineRenderer.positionCount = 2;
        lineRenderer.SetPosition(0, objectPosition);
        lineRenderer.SetPosition(1,hit.collider != null? hit.point : mouseWorldPosition);
    }

    // enables nearby enemies, moves using input axes and checks if the player wants to shoot of throw grenade
    void FixedUpdate()
    {
        if (!gameStarted)
        {
            return;
        }
        EnableNearbyEnemies();
        Movement();
        if (Input.GetAxis("Fire1") > 0.1f && shotTimer <= 0)
        {
            Fire();
            shotTimer = shotSpeed;
        }
        if (Input.GetAxis("Fire2") > 0.1f && smokeTimer <= 0 && PlayerPrefs.GetInt("levelSelect")==2)
        {
            ThrowSmoke();
            smokeTimer = smokeCooldown;
        }else if (Input.GetAxis("Fire2") > 0.1f && grenadeTimer <= 0 && PlayerPrefs.GetInt("levelSelect") == 3)
        {
            ThrowGrenade();
            grenadeTimer = grenadeCooldown;
        }
        CountdownTimers();
    }

    // player enables enemies nearby which it has not already found
    // this is more scalable than each enemy checking it's distance from player to enable it's ai, as it is a fixed processing cost whereas if each enemy checked it would lag with more enemies added
    private void EnableNearbyEnemies()
    {
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, searchRadius, searchLayer);
       
        foreach (Collider2D collider in colliders)
        {
            if (!foundColliders.Contains(collider))
            {
                EnemyController enemyController = collider.gameObject.GetComponent<EnemyController>();
                enemyController.NearPlayer();
                foundColliders.Add(collider);
            }
        }
    }

    // counts down timers for shooting and grenade
    private void CountdownTimers()
    {
        if (shotTimer > 0)
        {
            shotTimer -= Time.deltaTime;
        }
        if (smokeTimer > 0 && PlayerPrefs.GetInt("levelSelect")==2)
        {
            smokeTimer -= Time.deltaTime;
            cooldownImage.fillAmount = (smokeCooldown-smokeTimer) / smokeCooldown;
        }else if (grenadeTimer > 0 && PlayerPrefs.GetInt("levelSelect") == 3)
        {
            grenadeTimer -= Time.deltaTime;
            cooldownImage.fillAmount = (grenadeCooldown-grenadeTimer) / grenadeCooldown;
        }
    }

    // simple movement using input axes and diagonal dampening
    private void Movement()
    {
        float inputX = Input.GetAxis("Horizontal");
        float inputY = Input.GetAxis("Vertical");
        if ((inputX > 0.01 || inputX < -0.01) && (inputY >0.01 || inputY < -0.01))
        {
            rb.velocity += new Vector2(inputX*speed*diagonalDampener, inputY*speed*diagonalDampener);
        }
        else
        {
            rb.velocity += new Vector2(inputX*speed, inputY*speed);
        }
    }
    
    // fire weapon used in FixedUpdate(), instantiates bullet which has it's own controller
    private void Fire()
    {
        _particleSystem.transform.LookAt(hit.point);
        _particleSystem.Play();
        float angle = Mathf.Atan2(direction.y, direction.x);
        float angleInDegrees = angle * Mathf.Rad2Deg;
        Instantiate(shotLineRender, rb.transform.position, Quaternion.Euler(0f, 0f, angleInDegrees - 90));
    }
    

    // instantiate smoke grenade prefab with it's own controller
    private void ThrowSmoke()
    {
        float angle = Mathf.Atan2(direction.y, direction.x);
        float angleInDegrees = angle * Mathf.Rad2Deg;
        Vector3 pos = rb.transform.position;
        pos.z = -0.1f;
        Instantiate(smokeGrenade, pos, Quaternion.Euler(0f, 0f, angleInDegrees - 90));
    }

    // instantiate frag grenade prefab with it's own controller
    private void ThrowGrenade()
    {
        float angle = Mathf.Atan2(direction.y, direction.x);
        float angleInDegrees = angle * Mathf.Rad2Deg;
        Vector3 pos = rb.transform.position;
        pos.z = -0.1f;
        Instantiate(grenade, pos, Quaternion.Euler(0f, 0f, angleInDegrees - 90));
    }
    
    // collision with enemy bullet or frag bullet kills player, if hit's end trigger game is won so calls gameController.PlayerWins()
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("BulletEnemy") || other.CompareTag("Frag"))
        {
            Death();
        }else if (other.CompareTag("endTrigger"))
        {
            gameController.PlayerWins();
        }
    }
    
    // invincible option for testing purposes, else runs gameController.PlayerDied() to handle player death
    private void Death()
    {
        if (!invincible)
        {
            gameController.PlayerDied();
        }
    }
}
