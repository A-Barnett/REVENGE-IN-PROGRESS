using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

public class LevelCreate : MonoBehaviour
{
    
    /// <summary>
    /// Created by: Alex Barnett
    /// Creates procedural levels using prefab level pieces, ran by GameController when a player picks a level
    /// </summary>

    [SerializeField] private GameObject[] levelPrefs;
    [SerializeField] private GameObject lastLevelPref;
    [SerializeField] private int levelCount;
    [SerializeField] private GameObject startPiece;
    [SerializeField] private ParticleSystem smokePref;
    [SerializeField] private ShotLine enemyShot;
    private GameObject lastLevelCentre;
    private int startRotation;
    private int xCount;
    private int yCount;
    private LevelPiece lastPiece;
    private List<Vector2> piecePos = new List<Vector2>();
    private Vector2 currPos = new Vector2(0,0);
    private List<LevelPiece> pieceOptions = new List<LevelPiece>();
    private  LevelPiece chosenPiece = new LevelPiece(0,0,null);
    private int levelsPlaced;
    private List<GameObject> allEnemiesParticleCheck;
    private LevelController.Level selectedLevel;
    private bool lastLevelPlaced;
    private bool gameLoaded;
    private GameObject levelPlaced;
    private LevelPiece finalLevelPiece;
    

    // LevelPiece class to store info for each piece, including the side which is the entrance(inSide), exit(outSide) and the level prefab(level)
    // for inSide/outSide an int is used for each side, 0 is left, 1 is up, 2 is right and 3 is down
    private class LevelPiece
    {
        public int inSide;
        public int outSide;
        public GameObject level;

        public LevelPiece(int inSide, int outSide, GameObject level)
        {
            this.inSide = inSide;
            this.outSide = outSide;
            this.level = level;
        }
    }

    // ArrayList of all pieces available which is populated in CreatePieces()
    private ArrayList pieces = new ArrayList();
        
    // ran by GameController when level is selected, passes relevant level from LevelController
    // sets variables according to level, rotates the start piece, populates pieces ArrayList then begins ChoosePiece() chain according the rotation of the start piece
    public void StartLevels(LevelController.Level level)
    {
        selectedLevel = level;
        enemyShot.speed = level.bulletSpeed;
        levelCount = selectedLevel.numLevelPieces;
        allEnemiesParticleCheck = new List<GameObject>();
        startRotation = Random.Range(0, 4);
        RotateStart();
        CreatePieces();
        piecePos.Add(currPos);
        ChoosePiece(InToOutSide(startRotation));
    }
    
    // rotation of start piece according to the random startRotation, then adjusts enemy variables according to selectedLevel difficulty using enemy.CreateEnemy(selectedLevel) and rotates enemies so they still face the correct orientation
    private void RotateStart()
    {
        EnemyController[] enemies = startPiece.GetComponentsInChildren<EnemyController>();
        float rotationAmount = RotateAmount(startRotation);
        startPiece.transform.rotation = Quaternion.Euler(0,0,rotationAmount);
        foreach (EnemyController enemy in enemies)
        {
            enemy.CreateEnemy(selectedLevel);
            enemy.gameObject.transform.localRotation = Quaternion.Euler(0,0,-rotationAmount);
            allEnemiesParticleCheck.Add(enemy.GetComponentInChildren<BoxCollider2D>().gameObject);
        }
    }
    
    // ran at start to populate pieces Arraylist with Serialized levelPrefs array
    private void CreatePieces()
    {
        LevelPiece level1 = new LevelPiece(0, 1, levelPrefs[0]);
        pieces.Add(level1);
        LevelPiece level2 = new LevelPiece(0, 2, levelPrefs[1]);
        pieces.Add(level2);
        LevelPiece level3 = new LevelPiece(0, 3, levelPrefs[2]);
        pieces.Add(level3);
        LevelPiece level4 = new LevelPiece(1, 0, levelPrefs[3]);
        pieces.Add(level4);
        LevelPiece level5 = new LevelPiece(1, 2, levelPrefs[4]);
        pieces.Add(level5);
        LevelPiece level6 = new LevelPiece(1, 3, levelPrefs[5]);
        pieces.Add(level6);
        LevelPiece level7 = new LevelPiece(2, 0, levelPrefs[6]);
        pieces.Add(level7);
        LevelPiece level8 = new LevelPiece(2, 1, levelPrefs[7]);
        pieces.Add(level8);
        LevelPiece level9 = new LevelPiece(2, 3, levelPrefs[8]);
        pieces.Add(level9);
        LevelPiece level10 = new LevelPiece(3, 0, levelPrefs[9]);
        pieces.Add(level10);
        LevelPiece level11 = new LevelPiece(3, 1, levelPrefs[10]);
        pieces.Add(level11);
        LevelPiece level12 = new LevelPiece(3, 2, levelPrefs[11]);
        pieces.Add(level12);
        finalLevelPiece = new LevelPiece(0, -1, lastLevelPref);
    }
    
    // if places all pieces needed, places final piece rotated accordingly
    // otherwise check all levels in pieces arraylist and adds all possible pieces to pieceOptions List, checks if any are a possibility with CheckAnyPossible() then starts piece placement with PlacePiece()
    private void ChoosePiece(int inSide)
    {
        if (levelCount <= 0)
        {
            if (!gameLoaded)
            {
                if (!lastLevelPlaced)
                {
                    PlacePiece(lastPiece.outSide, finalLevelPiece,true);
                }
                GameObject centre = levelPlaced.transform.GetChild(0).gameObject;
                float rotation = RotateAmount(InToOutSide(lastPiece.outSide));
                centre.transform.rotation = Quaternion.Euler(0,0,rotation);
                int index = 0;
                foreach (GameObject box in allEnemiesParticleCheck)
                {
                    smokePref.trigger.SetCollider(index,box.transform);
                    index++;
                }
                gameLoaded = true;
            }
            return;
        }
        pieceOptions = new List<LevelPiece>();
        int neededSide = InToOutSide(inSide);
        foreach (LevelPiece level in pieces)
        {
            if (level.inSide == neededSide)
            {
                pieceOptions.Add(level);
            }
        }
        if (pieceOptions.Count == 0)
        {
            Debug.LogError("Piece Options Empty");
            return;
        }
        CheckAnyPossible();
        levelCount--;
        PlacePiece(inSide,chosenPiece,false);
    }
    
    
    // checks if any selected pieces in pieceOptions will be possible without causing pieces to be placed atop another, if the level-generator is stuck it reloads scene and tries again
    // if the levels needed to be significantly longer then a lookahead would be necessary, but the max pieces placed is 20, so this is not usually very problematic and generation is quick enough that it is not particularly noticeable
    private void CheckAnyPossible()
    {
        bool isPossible = false;
        foreach (LevelPiece level in pieceOptions)
        {
            if (CheckPiecePossible(level.outSide))
            {
                isPossible = true;
            }
        }

        if (!isPossible)
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
        chosenPiece = pieceOptions[Random.Range(0, pieceOptions.Count)];
        int count = 0;
        while (lastPiece == chosenPiece || !CheckPiecePossible(chosenPiece.outSide))
        {
            chosenPiece = pieceOptions[Random.Range(0, pieceOptions.Count)];
            count++;
            if (count >= 100)
            {
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
                break;
            }
        }
    }
    
    // checks with piecePos List that next piece will not overlap using outSide, as there is not already a piece in that position
    private bool CheckPiecePossible(int outSide)
    {
        switch (outSide)
        {
            case 0:
                if (piecePos.Contains(new Vector2(currPos.x-1, currPos.y)))
                {
                    return false;
                }
                return true;
            case 1:
                if (piecePos.Contains(new Vector2(currPos.x, currPos.y+1)))
                {
                    return false;
                }
                return true;
            case 2:
                if (piecePos.Contains(new Vector2(currPos.x+1, currPos.y)))
                {
                    return false;
                }
                return true;
            case 3:
                if (piecePos.Contains(new Vector2(currPos.x, currPos.y-1)))
                {
                    return false;
                }
                return true;
        }
        return false;
    }
    
    // adjust x/y counters for level position in grid, instantiates level prefab piece at grid position
    // goes through each enemy in the level and adjusts their variables according the the selected level difficulty using enemy.CreateEnemy(), and then changes some variables by how far through the level is to increase difficulty as the player progresses
    // then adds position into piecePos Vector2 List using AddPosToList() and sets lastPiece to the piece just placed
    private void PlacePiece(int inSide, LevelPiece level, bool isFinalPiece)
    {
        switch (inSide)
        {
            case 0:
                xCount--;
                break;
            case 1:
                yCount++;
                break;
            case 2:
                xCount++;
                break;
            case 3:
                yCount--;
                break;
        }
        
        if (xCount == 0 && yCount == 0)
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
            return;
        }
        levelPlaced = Instantiate(level.level, new Vector3(xCount*9.77f, yCount*9.77f), Quaternion.identity);
        EnemyController[] enemies = levelPlaced.transform.GetChild(0).GetComponentsInChildren<EnemyController>();
        foreach (EnemyController enemy in enemies)
        {
            enemy.CreateEnemy(selectedLevel);
            enemy.enemy.framesTillShoot -= levelsPlaced / 3;
            enemy.enemy.aimFrameDelay -= levelsPlaced / 2;
            enemy.enemy.shootNotAimTime -= (float)levelsPlaced/65;
            allEnemiesParticleCheck.Add(enemy.gameObject.GetComponentInChildren<BoxCollider2D>().gameObject);
        }
        levelsPlaced++;
        AddPosToList(chosenPiece.outSide);
        lastPiece = chosenPiece;

        if (!isFinalPiece)
        {
            ChoosePiece(level.outSide);
        }
    }

    // adds the position of the next level to the piecePos Vector2 List, this is needed to make sure pieces are not placed overlapping atop each-other in the same grid position
    private void AddPosToList(int outSide)
    {
        switch (outSide)
        {
            case 0:
                currPos = new Vector2(currPos.x - 1, currPos.y);
                piecePos.Add(currPos);
                break;
            case 1:
                currPos = new Vector2(currPos.x, currPos.y + 1);
                piecePos.Add(currPos);
                break;
            case 2:
                currPos = new Vector2(currPos.x + 1, currPos.y);
                piecePos.Add(currPos);
                break;
            case 3:
                currPos = new Vector2(currPos.x, currPos.y - 1);
                piecePos.Add(currPos);
                break;
        }
    }


    // helper function to return the needed outSide for a certain inSide in ChoosePiece(), is also used for start and end piece rotation
    private int InToOutSide(int inSide)
    {
        switch (inSide)
        {
            case 0:
                return 2;
            case 1:
                return 3;
            case 2:
                return 0;
            case 3:
                return 1;
        }
        return -1;
    }
    
    
    // helper function to return the rotate amount needed according to rotateCase, used for start and end piece rotation
    private float RotateAmount(int rotateCase)
    {
        switch (rotateCase)
        {
            case 0:
                return 0;
            case 1:
                return -90f;
            case 2:
                return 180f;
            case 3:
                return 90f;
        }
        return -1;
    }
    
}
