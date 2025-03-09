using System;
using UnityEngine;

public class ShotLine : MonoBehaviour
{
    
    /// <summary>
    /// Created by: Alex Barnett
    /// Controls the bullets, adding initial velocity and destroying enemies, is instantiated by player, enemy or grenade
    /// </summary>
    
    [SerializeField] private int bounces;
    [SerializeField] private float bulletAliveTime;
    [SerializeField] public float speed;
    [SerializeField] private TrailRenderer trailRenderer;
    [SerializeField] private Rigidbody2D rb;
    
    [NonSerialized] public bool inCutscene;
    private ScoreController scoreController;

    // initial velocity added when instantiated and finds scoreController for future use
    private void Start()
    {
        trailRenderer.emitting = true;
        Vector3 forwardDirection = transform.TransformDirection(Vector3.up);
        rb.velocity = forwardDirection * speed;
        if (!inCutscene)
        {
            scoreController = GameObject.Find("Score Controller").GetComponent<ScoreController>();
        }
    }

    // countdown timer for bullet to destroy self after bulletAliveTime
    private void FixedUpdate()
    {
        bulletAliveTime -= Time.deltaTime;
        if (bulletAliveTime <= 0)
        {
            Destroy(gameObject);
        }
    }

    // used when instantiated by player or grenade, kills enemy and adds score using scoreController.KillScore()
    private void OnTriggerEnter2D(Collider2D col)
    {
        if (col.gameObject.CompareTag("Enemy"))
        {
            scoreController.KillScore();
            Destroy(col.gameObject);
        }
    }

    // decreases bounce counter and destroys self is bounces below 0, when colliding with any collider
    private void OnCollisionEnter2D(Collision2D col)
    {
        if (bounces <= 0)
        {
            Destroy(gameObject);
        }
        bounces--;
    }
}
