using UnityEngine;

public class WifeController : MonoBehaviour
{

    /// <summary>
    /// Created by: Alex Barnett
    /// Controls the "robo-wife" object in the intro cutscene
    /// </summary>
    
    [SerializeField] private GameObject playerBody;
    [SerializeField] private GameObject light2D;
    private bool dead;
    
    // when shot by enemy bullet, runs Death()
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("BulletEnemy"))
        {
            Death();
            dead = true;
        }
    }

    // destroys the SpriteRenderer playerBody and Light light2D, does not destroy main object with collider as to make sure all enemy bullets are stopped
    private void Death()
    {
        if (dead)
        {
            return;
        }
        Destroy(playerBody);
        Destroy(light2D);
    }
}
