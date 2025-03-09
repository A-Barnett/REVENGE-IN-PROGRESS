using UnityEngine;

public class LevelController : MonoBehaviour
{
    
    /// <summary>
    /// Created by: Alex Barnett
    /// Helper script for LevelCreate, controls difficulty of levels using Level class, LevelCreate sets the variables of enemies in each level according to the Level level
    /// </summary>
    
    public class Level
    {
        public float enemyGroupSpeed;
        public float enemyPlayerSpeed;
        public int aimFrameDelay;
        public int framesTillShoot;
        public float shotTime;
        public float shootNotAimTime;
        public int numLevelPieces;
        public float bulletSpeed;
        public Level(float enemyGroupSpeed, float enemyPlayerSpeed, int aimFrameDelay, int framesTillShoot, float shotTime,
            float shootNotAimTime, int numLevelPieces, float bulletSpeed)
        {
            this.enemyGroupSpeed = enemyGroupSpeed;
            this.enemyPlayerSpeed = enemyPlayerSpeed;
            this.aimFrameDelay = aimFrameDelay;
            this.framesTillShoot = framesTillShoot;
            this.shotTime = shotTime;
            this.shootNotAimTime = shootNotAimTime;
            this.numLevelPieces = numLevelPieces;
            this.bulletSpeed = bulletSpeed;
        }
    }

    // this is where the difficulty for each level is set, to change difficulty of every enemy in a level just change the variable for the corresponding level
    public Level level1 = new Level(3f, 4f, 30, 120, 2f, 1f, 6,10f);
    public Level level2 = new Level(4f, 5f, 20, 80, 1f, 0.75f, 10,20f);
    public Level level3 = new Level(5f, 7f, 10, 40, 0.5f, 0.5f, 15,40f);
    
}
