using UnityEngine;
using Random = UnityEngine.Random;

public class SmokeGrenade : MonoBehaviour
{
    
    /// <summary>
    /// Created by: Alex Barnett
    /// Controls the smoke grenade, is instantiated by player when playing level 2
    /// </summary>
    
    [SerializeField] private float speed = 1;
    [SerializeField] private float smokeDelay;
    [SerializeField] private TrailRenderer trailRenderer;
    [SerializeField] private Rigidbody2D rb;

    private GameObject smokeObj;
    private ParticleSystem smokeParticles;
    private bool exploded;

    // adds initial velocity and finds the smoke GameObject for future use, needs to use a smoke GameObject in the scene instead of using a prefab
    // this is because LevelCreate needs to add all the enemies smoke check collider into the trigger collider array for a smoke object in the scene, as this does not work for a prefab
    public void Start()
    {
        trailRenderer.emitting = true;
        Vector3 forwardDirection = transform.TransformDirection(Vector3.up);
        rb.velocity = forwardDirection * speed;
        rb.angularVelocity = Random.Range(-1000, 1000);
        smokeObj = GameObject.FindGameObjectWithTag("smoke");
    }

    // explodes smoke grenade after smokeDelay time, this plays the particle system
    // destroys smoke grenade and particle system when particle system has finished playing
    private void FixedUpdate()
    {
        if (smokeDelay > 0)
        {
            smokeDelay -= Time.deltaTime;
        }
        else if(!exploded)
        {
            Vector3 pos = rb.transform.position;
            pos.z = 0.1f;
            GameObject smoke = Instantiate(smokeObj, pos, Quaternion.identity);
            smokeParticles = smoke.GetComponent<ParticleSystem>();
            smokeParticles.Play();
            exploded = true;
        }
        else
        {
            if (smokeParticles.isPlaying == false)
            {
                Destroy(smokeParticles);
                Destroy(gameObject);
            }
        }
    }
}
