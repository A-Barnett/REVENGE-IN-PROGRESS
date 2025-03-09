using System;
using System.Collections;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameController : MonoBehaviour
{
    /// <summary>
    /// Created by: Alex Barnett
    /// Controls many game systems, mainly menus such as the main menu and start/end of levels
    /// </summary>

    [SerializeField] private Canvas startCanvas;
    [SerializeField] private Canvas pauseCanvas;
    [SerializeField] private Canvas featuresCanvas;
    [SerializeField] private Canvas endCanvas;
    [SerializeField] private Canvas winCanvas;
    [SerializeField] private Canvas gameCanvas;
    [SerializeField] private Canvas levelSelectCanvas;
    [SerializeField] private Canvas level1StartCanvas;
    [SerializeField] private Canvas level2StartCanvas;
    [SerializeField] private Canvas level3StartCanvas;
    [SerializeField] private Canvas settingsCanvas;
    [SerializeField] private GameObject postProcess;
    [SerializeField] private GameObject menuPostProcess;
    [SerializeField] private GameObject level2Button;
    [SerializeField] private GameObject level3Button;
    [SerializeField] private GameObject cooldownImage;
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI scoreTextWin;
    [SerializeField] private TextMeshProUGUI totalScoreText;
    [SerializeField] private TextMeshProUGUI levelScoreText;
    [SerializeField] private TextMeshProUGUI level2text;
    [SerializeField] private TextMeshProUGUI level3text;
    [SerializeField] private TextMeshProUGUI fullscreenText;
    [SerializeField] private CamFollow camFollow;
    [SerializeField] private ScoreController scoreController;
    [SerializeField] private LevelController levelController;
    [SerializeField] private LevelCreate levelCreate;
    [SerializeField] private PlayerController player;
 
    private LevelController.Level chosenLevel;
    private bool inPauseMenu;
    private bool gameStarted;
    private bool gameEnded;
    private bool level2Unlocked;
    private bool level3Unlocked;
    private bool levelButtonDisabled;

    
    // sets up the menu start menu, unless the game has already started and LevelCreate is just doing a scene reset, then sets up the game instead
    void Start()
    {
        if (PlayerPrefs.GetInt("gameStarted") == 1)
        {
            switch (PlayerPrefs.GetInt("levelSelect"))
            {
                case 1:
                    chosenLevel = levelController.level1;
                    break;
                case 2:
                    chosenLevel = levelController.level2;
                    break;
                case 3:
                    chosenLevel = levelController.level3;
                    break;
            }
            gameStarted = true;
            StartGame();
        }
        else
        {
            SetupStartMenu();
        }
    }

    // sets startCanvas to be enables, all other canvases to be disabled, timeScale to 0 and displays player's score, also enables the menu post-processing 
    private void SetupStartMenu()
    {
       
        Time.timeScale = 0;
        if (PlayerPrefs.GetInt("miniGame") == 1)
        {
            PlayerPrefs.SetInt("Score",PlayerPrefs.GetInt("Score") + PlayerPrefs.GetInt("CurrentScore"));
            PlayerPrefs.SetInt("miniGame", 0);
            PlayerPrefs.SetInt("CurrentScore",0);
        }
        SetWindowResolution();
        totalScoreText.text = "SCORE: \n" + PlayerPrefs.GetInt("Score");
        totalScoreText.GetComponent<TextFlash>().ellipsisText = "SCORE: \n" + PlayerPrefs.GetInt("Score");
        SetLevelButtons();
        startCanvas.enabled = true;
        level1StartCanvas.enabled = false;
        level2StartCanvas.enabled = false;
        level3StartCanvas.enabled = false;
        pauseCanvas.enabled = false;
        featuresCanvas.enabled = false;
        endCanvas.enabled = false;
        winCanvas.enabled = false;
        gameCanvas.enabled = false;
        levelSelectCanvas.enabled = false;
        menuPostProcess.SetActive(true);
        postProcess.SetActive(false);
    }

    // for setting windowed/fullscreeen at start of game according to player's preference, and when switching in SwitchWindowedMode()
    private void SetWindowResolution()
    {
        if (PlayerPrefs.GetInt("FullScreen") == 1)
        {
            Screen.SetResolution(Screen.currentResolution.width, Screen.currentResolution.height, true);
        }
        else
        {
            Screen.SetResolution(1920,  1080, false);
        }

    }
    
    // sets buttons do display if level can be played, or needs to be unlocked
    private void SetLevelButtons()
    {
        levelScoreText.text = "SCORE: \n" + PlayerPrefs.GetInt("Score");
        levelScoreText.GetComponent<TextFlash>().ellipsisText = "SCORE: \n" + PlayerPrefs.GetInt("Score");
        level2Unlocked = PlayerPrefs.GetInt("Level2") == 1;
        level3Unlocked = PlayerPrefs.GetInt("Level3") == 1;
        level2text.text = level2Unlocked ? "LEVEL 2" : "LEVEL 2\nCOST 10000";
        level3text.text = level3Unlocked ? "LEVEL 3" : "LEVEL 3\nCOST 25000";
        level2text.fontSize = level2Unlocked ? 24 : 13;
        level3text.fontSize = level3Unlocked ? 24 : 13;
    }


    // in-game pause menu on Esc
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) && gameStarted && !gameEnded)
        {
            if (!inPauseMenu)
            {
                PauseGame();
            }
            else
            {
                ResumeButton();
            }
        }
    }

    // player has been killed, stops time, enables loss canvas and player does not get points 
    public void PlayerDied()
    {
        Time.timeScale = 0;
        endCanvas.enabled = true;
        startCanvas.enabled = false;
        pauseCanvas.enabled = false;
        gameCanvas.enabled = false;
        menuPostProcess.SetActive(true);
        postProcess.SetActive(false);
        int score = (int)Math.Round(scoreController.score);
        scoreText.text = "SCORE: " + score;
        StartCoroutine(LoseAllPoints(score));
        scoreText.GetComponent<TextFlash>().ellipsisText = "SCORE: " + Math.Round(scoreController.score);
    }
    
    // counts down points to 0, to show what the player has lost
    private IEnumerator LoseAllPoints(int score)
    {
        TextFlash flash = scoreText.GetComponent<TextFlash>();
        flash.enabled = false;
        int startingScore = score;
        score = 0;
        float timer = 0;
        yield return new WaitForSecondsRealtime(1);
        while (timer < 4)
        {
            float t = timer / 4;
            int newPoints = Mathf.RoundToInt(Mathf.Lerp(startingScore, score, t));
            scoreText.text = "SCORE: " + newPoints;
            timer += Time.unscaledDeltaTime;
            yield return null;
        }
        scoreText.text = "SCORE: 0";
        flash.ellipsisText = scoreText.text;
        flash.enabled = true;
    }
    
    // player has won, stops time, enables win canvas and points are saved as a PlayerPref for use in end mini-game scene
    public void PlayerWins()
    {
        Time.timeScale = 0;
        winCanvas.enabled = true;
        startCanvas.enabled = false;
        pauseCanvas.enabled = false;
        gameCanvas.enabled = false;
        menuPostProcess.SetActive(true);
        postProcess.SetActive(false);
        int score = (int)Math.Round(scoreController.score);
        scoreTextWin.text = "SCORE: " + score;
        PlayerPrefs.SetInt("CurrentScore",score);
        scoreTextWin.GetComponent<TextFlash>().ellipsisText = "SCORE: " + score;
    }

    // starts the game state, enabling the game canvas and adding the cooldown for grenades when needed, changes post-processing to in-game postProcess
    private void StartGame()
    {
        PlayerPrefs.SetInt("gameStarted",1);
        levelCreate.StartLevels(chosenLevel);
        switch (PlayerPrefs.GetInt("levelSelect"))
        {
            case 1:
                level1StartCanvas.enabled = true;
                cooldownImage.SetActive(false);
                break;
            case 2:
                level2StartCanvas.enabled = true;
                cooldownImage.SetActive(true);
                break;
            case 3:
                level3StartCanvas.enabled = true;
                cooldownImage.SetActive(true);
                break;
        }
        camFollow.gameActive = true;
        startCanvas.enabled = false;
        pauseCanvas.enabled = false;
        levelSelectCanvas.enabled = false;
        inPauseMenu = false;
        menuPostProcess.SetActive(false);
        postProcess.SetActive(true);
        endCanvas.enabled = false;
    }
    
    // pauses game when called in Update(), stops time, enables pause canvas and menu post-processing
    private void PauseGame()
    {
        pauseCanvas.enabled = true;
        Time.timeScale = 0;
        inPauseMenu = true;
        gameCanvas.enabled = false;
        menuPostProcess.SetActive(true);
        postProcess.SetActive(false);
        endCanvas.enabled = false;
    }
    

    // resumes game when called in Update(), starts time, enables game canvas and game post-processing
    public void ResumeButton()
    {
        inPauseMenu = false;
        camFollow.gameActive = true;
        startCanvas.enabled = false;
        pauseCanvas.enabled = false;
        level1StartCanvas.enabled = false;
        level2StartCanvas.enabled = false;
        level3StartCanvas.enabled = false;
        gameCanvas.enabled = true;
        Time.timeScale = 1;
        player.gameStarted = true;
    }
    
    // goes to level select menu canvas
    public void LevelSelectButton()
    {
        levelSelectCanvas.enabled = true;
        startCanvas.enabled = false;
    }
    
    // goes to the features list canvas
    public void FeaturesButton()
    {
        menuPostProcess.SetActive(false);
        postProcess.SetActive(true);
        featuresCanvas.enabled = true;
        startCanvas.enabled = false;
    }

    // goes to settings menu canvas
    public void SettingsButton()
    {
        fullscreenText.text = PlayerPrefs.GetInt("FullScreen") == 1 ? "WINDOWED" : "FULLSCREEN";
        settingsCanvas.enabled = true;
        startCanvas.enabled = false;
    }

    // toggles between fullscreen and windowed, saves choice as playerPref
    public void SwitchWindowedMode()
    {
        bool isFullscreen = PlayerPrefs.GetInt("FullScreen") == 1;
        PlayerPrefs.SetInt("FullScreen", isFullscreen? 0 : 1);
        fullscreenText.text = isFullscreen ? "FULLSCREEN" : "WINDOWED";
        SetWindowResolution();

    }
    
    // returns to main menu from in sub-menu
    public void SubMenuReturnToMenu()
    {
        menuPostProcess.SetActive(true);
        postProcess.SetActive(false);
        settingsCanvas.enabled = false;
        levelSelectCanvas.enabled = false;
        featuresCanvas.enabled = false;
        startCanvas.enabled = true;
    }
    
    // return back to main menu from in-game
    public void MenuButton()
    {
        PlayerPrefs.SetInt("gameStarted",0);
        PlayerPrefs.SetInt("levelSelect",0);
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

   
    // Restarts the level, gameStarted PlayerPref is still 1, so Start() just restarts scene not main menu
    public void RestartButton()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
    
    // Exits the game
    public void ExitButton()
    {
        //enable `UnityEditor.EditorApplication.isPlaying = false` for testing, enable `Application.Quit()` for build, only have one enabled at a time;
        UnityEditor.EditorApplication.isPlaying = false;
        //Application.Quit();
    }

    // replays the opening cutscene by opening the scene
    public void IntroButton()
    {
        Time.timeScale = 1;
        SceneManager.LoadScene("CutScene");
    }
    
    // starts the mini-game when the level is won by loading the scene
    public void StartMiniGameButton()
    {
        SceneManager.LoadScene("MiniGame");
    }

    // starts level 1, LevelCreate makes the level and StartGame() sets up the scene such as timeScale and canvases
    public void ChooseLevel1()
    {
        PlayerPrefs.SetInt("levelSelect",1);
        chosenLevel = levelController.level1;
        gameStarted = true;
        StartGame();
    }
    
    // starts level 2 if it is unlocked, LevelCreate makes the level and StartGame() sets up the scene such as timeScale and canvases
    // if not unlocked, attempts to purchase the level using PlayerPref "Score", if not affordable flashes button red to indicate so
    public void ChooseLevel2()
    {
        if (level2Unlocked)
        {
            PlayerPrefs.SetInt("levelSelect",2);
            chosenLevel = levelController.level2;
            gameStarted = true;
            StartGame();
        }
        else
        {
            if (PlayerPrefs.GetInt("Score") >= 10000)
            {
                PlayerPrefs.SetInt("Score",PlayerPrefs.GetInt("Score")-10000);
                PlayerPrefs.SetInt("Level2",1);
                SetLevelButtons();
            }
            else if(!levelButtonDisabled)
            {
                StartCoroutine(FlashImageRed(level2Button));
            }
        }
    }
    
    // starts level 3 if it is unlocked, LevelCreate makes the level and StartGame() sets up the scene such as timeScale and canvases
    // if not unlocked, attempts to purchase the level using PlayerPref "Score", if not affordable flashes button red to indicate so
    public void ChooseLevel3()
    {
        if (level3Unlocked)
        {
            PlayerPrefs.SetInt("levelSelect",3);
            chosenLevel = levelController.level3;
            gameStarted = true;
            StartGame();
        }
        else
        {
            if (PlayerPrefs.GetInt("Score") >= 25000)
            {
                PlayerPrefs.SetInt("Score",PlayerPrefs.GetInt("Score")-25000);
                PlayerPrefs.SetInt("Level3",1);
                SetLevelButtons();
            }
            else if(!levelButtonDisabled)
            {
                StartCoroutine(FlashImageRed(level3Button));
            }
        }
     
    }
    
    // flashes the image on the GameObject obj to red over `duration` seconds, then back to white.
    private IEnumerator FlashImageRed(GameObject obj)
    {
        levelButtonDisabled = true;
        UnityEngine.UI.Image image = obj.GetComponent<UnityEngine.UI.Image>();
        float duration = 0.5f;
        float elapsedTime = 0f;
        while (elapsedTime < duration)
        {
            image.color = Color.Lerp(Color.white, Color.red, elapsedTime / duration);
            elapsedTime += Time.unscaledDeltaTime;
            yield return null;
        }
        image.color = Color.red;
        yield return new WaitForSecondsRealtime(0.1f); 
        elapsedTime = 0f;
        while (elapsedTime < duration)
        {
            image.color = Color.Lerp(Color.red, Color.white, elapsedTime / duration);
            elapsedTime += Time.unscaledDeltaTime;
            yield return null;
        }
        image.color = Color.white;
        levelButtonDisabled = false;
    }
    
    // resets the PlayerPrefs which are used to transfer variables across scenes, does not reset progress based PlayerPrefs such as "Score"
    private void OnApplicationQuit()
    {
        PlayerPrefs.SetInt("gameStarted",0);
        PlayerPrefs.SetInt("levelSelect",0);
        PlayerPrefs.SetInt("miniGame", 0);
        PlayerPrefs.SetInt("CurrentScore",0);
    }
}
