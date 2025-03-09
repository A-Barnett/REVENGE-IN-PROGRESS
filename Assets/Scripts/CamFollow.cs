using System;
using UnityEngine;

public class CamFollow : MonoBehaviour
{
    /// <summary>
    /// Created by: Alex Barnett
    /// Simple script for the camera to follow the player
    /// </summary>
    
    [SerializeField] private GameObject player;
    [NonSerialized] public bool gameActive;
    private float origPosZ;
    void Start()
    {
        origPosZ = gameObject.transform.position.z;
    }
    
    // sets cam position to player position, except z pos
    void Update()
    {
        if (gameActive)
        {
            Vector3 targetPos = player.transform.position;
            targetPos.z = origPosZ;
            gameObject.transform.position = targetPos;
        }
    }
}
