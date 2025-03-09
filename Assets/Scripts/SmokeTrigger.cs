using System;
using System.Collections.Generic;
using UnityEngine;

public class SmokeTrigger : MonoBehaviour
{
    
    /// <summary>
    /// Created by: Alex Barnett
    /// Controls the trigger system for the smoke grenade, so that enemies can detect when some is blocking vision
    /// </summary>
    
    private ParticleSystem ps;
    private List<Component> affectedComponents = new List<Component>();

    private void OnEnable()
    {
        ps = GetComponent<ParticleSystem>();
    }

    // affectedComponents list stops particles needing to find the colliders EnemyController if already set by another particle this frame, clears at end of each frame so smokeBlocking can be set again next frame
    private void LateUpdate()
    {
        affectedComponents.Clear();
    }

    // when a particle is inside an enemy smoke checking collider, sets smokeBlocking of that enemy to be true so they cannot see the player
    private void OnParticleTrigger()
    {
        int numInside = ps.GetTriggerParticles(ParticleSystemTriggerEventType.Inside, new List<ParticleSystem.Particle>(), out var insideData);
        for (int i = 0; i < numInside; i++)
        {
            for (int x = 0; x < insideData.GetColliderCount(i); x++)
            {
                Component col = insideData.GetCollider(i, x);
                if (!affectedComponents.Contains(col))
                {
                    affectedComponents.Add(col);
                    insideData.GetCollider(i, x).GetComponentInParent<EnemyController>().smokeBlocking = true;
                }
            }
        }
    }
}
