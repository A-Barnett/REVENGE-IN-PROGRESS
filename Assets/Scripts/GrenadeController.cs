using UnityEngine;
using Random = UnityEngine.Random;

public class GrenadeController : MonoBehaviour
{
    /// <summary>
    /// Created by: Alex Barnett
    /// Controls the frag grenade which is instantiated by the player in PlayerController
    /// </summary>

    [SerializeField] private float speed;
    [SerializeField] private float grenadeDelay;
    [SerializeField] private int numFrags;
    [SerializeField] private TrailRenderer trailRenderer;
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private ParticleSystem explosionParticles;
    [SerializeField] private GameObject fragment;

    private bool exploded;

    // runs when the grenade is instantiated, sets initial velocity to throw the grenade, and and a bit of random spin
    public void Start()
    {
        trailRenderer.emitting = true;
        Vector3 forwardDirection = transform.TransformDirection(Vector3.up);
        rb.velocity = forwardDirection * speed;
        rb.angularVelocity = Random.Range(-1000, 1000);
    }

    // counts down the grenade to explode, makes fragments looking in a circle away from the grenade, which are just bouncy bullets, and plays explosion effect then destroys self
    private void FixedUpdate()
    {
        if (grenadeDelay > 0)
        {
            grenadeDelay -= Time.deltaTime;
        }
        else if(!exploded)
        {
            int randomAdd = Random.Range(5, 35);
            for (int i = 0; i < numFrags; i++)
            {
                float angle = i * (360f / numFrags);
                Quaternion rotation = Quaternion.Euler(0, 0, angle+randomAdd);
                Instantiate(fragment, rb.transform.position, rotation);
            }
            explosionParticles.Play();
            exploded = true;
        }
        else
        {
            if (explosionParticles.isPlaying == false)
            {
                Destroy(explosionParticles);
                Destroy(gameObject);
            }
        }
    }
}
