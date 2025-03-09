using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Random = UnityEngine.Random;


public class MinigameController : MonoBehaviour
{
    
    /// <summary>
    /// Created by: Alex Barnett
    /// Main controller for the ending mini-game, some dialog then higher/lower game
    /// </summary>
    
    [SerializeField] private bool skipText;
    [SerializeField] private string dialog1;
    [SerializeField] private string dialog2;
    [SerializeField] private string dialog3;
    [SerializeField] private string dialog4;
    [SerializeField] private string dialog5;
    [SerializeField] private string dialog6;
    [SerializeField] private float textDelay;
    [SerializeField] private float startDelay;
    [SerializeField] private float numberRandomizeDuration;
    [SerializeField] private float waitBetweenDialogs;
    [SerializeField] private float changePointsTime;
    [SerializeField] private float changePointsStartDelay;
    [SerializeField] private float minDisplayTime;
    [SerializeField] private float maxDisplayTime;
    [SerializeField] private float minHideTime;
    [SerializeField] private float maxHideTime;
    [SerializeField] private float maxFlickerTimes;
    [SerializeField] private Canvas mainCanvas;
    [SerializeField] private Canvas winCanvas;
    [SerializeField] private Canvas dieCanvas;
    [SerializeField] private TextMeshProUGUI dialogTextBox;
    [SerializeField] private TextMeshProUGUI scoreBox;
    [SerializeField] private TextMeshProUGUI scoreBoxWin;
    [SerializeField] private TextMeshProUGUI scoreBoxLose;
    [SerializeField] private TextMeshProUGUI titleWin;
    [SerializeField] private TextMeshProUGUI titleLose;
    [SerializeField] private TextMeshProUGUI number1Text;
    [SerializeField] private TextMeshProUGUI number2Text;
    [SerializeField] private TextMeshProUGUI number3Text;
    [SerializeField] private TextMeshProUGUI number4Text;
    [SerializeField] private Button[] higherButtons;
    [SerializeField] private Button[] lowerButtons;
    
    private bool[] higherChoices;
    private string text;
    private int[] numbersSelected = new [] {0,0,0,0};
    private int numWins;
    private int score;
    private bool inDialog;
    private float lowestY = -172.2f;
    private float highestY = 80.5f;
  
    
    // setup mini-game, disables higher/lower buttons then starts the dialog going
    void Start()
    {
        PlayerPrefs.SetInt("miniGame", 1);
        mainCanvas.enabled = true;
        winCanvas.enabled = false;
        dieCanvas.enabled = false;
        Time.timeScale = 1;
        score = PlayerPrefs.GetInt("CurrentScore");
        scoreBox.text = "SCORE: " + score;
        DisableButtons();
        if (skipText)
        {
            StartCoroutine(RandomNumberDisplay(0));
            return;
        }

        inDialog = true;
        StartCoroutine(DialogAdd(1));
        StartCoroutine(FlashText(dialogTextBox,true));
    }

    // disables interactivity of higher/lower buttons and darkens them to display this
    // in-built change of a button's colour when disabled does not always work, so darkens them manually here instead
    private void DisableButtons()
    {
        foreach (Button button in higherButtons)
        {
            button.interactable = false;
            Color color = button.GetComponent<Image>().color;
            color.a = 15/225f;
            button.GetComponent<Image>().color = color;
        }
        foreach (Button button in lowerButtons)
        {
            button.interactable = false;
            Color color = button.GetComponent<Image>().color;
            color.a = 15/225f;
            button.GetComponent<Image>().color = color;
        }
    }

    // for skipping dialog in cutscene, stops text coroutine, sets text correct for finish and starts random number game
    private void Update()
    {
        if ((Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.Return)) && inDialog)
        {
            StopAllCoroutines();
            text = "          DEAL?\n     LETS START";
            dialogTextBox.text = text;
            StartCoroutine(RandomNumberDisplay(0));
            inDialog = false;
        }
    }

    // adds dialog one character at a time, takes input for which dialog to display, works recursively to go through all dialogs, then starts displaying the random numbers for mini-game
    private IEnumerator DialogAdd(int dialog)
    {
        string chosenDialog;
        switch (dialog)
        {
            case 1:
                yield return new WaitForSecondsRealtime(startDelay);
                chosenDialog = dialog1;
                break;
            case 2:
                chosenDialog = dialog2;
                break;
            case 3:
                chosenDialog = dialog3;
                break;
            case 4:
                chosenDialog = dialog4;
                break;
            case 5:
                chosenDialog = dialog5;
                break;
            case 6:
                chosenDialog = dialog6;
                break;
            default:
                inDialog = false;
                StartCoroutine(RandomNumberDisplay(0));
                yield break;
        }
        dialogTextBox.text = "";
        text = "";
        char[] dialogChars = chosenDialog.ToCharArray();
        for (int i = 0; i < dialogChars.Length; i++)
        {
            if (dialogChars[i].Equals('a'))
            {
                text += "\n";
            }
            else
            {
                text += dialogChars[i];
            }
            dialogTextBox.text = text;
            yield return new WaitForSecondsRealtime(textDelay);
        }
        dialogTextBox.text = text;
        yield return new WaitForSecondsRealtime(waitBetweenDialogs);
        StartCoroutine(DialogAdd(dialog + 1));
    }

    // for flashing the text on and off, similar to TextFlash but works while changing the text rapidly in DialogAdd()
    private IEnumerator FlashText(TextMeshProUGUI textBox, bool mainDialog)
    {
        string startText = textBox.text;
        while (true)
        {
            yield return new WaitForSecondsRealtime(Random.Range(minDisplayTime, maxDisplayTime));
            for (int i = 0; i < Random.Range(1, maxFlickerTimes); i++)
            {
                textBox.text = string.Empty;
                yield return new WaitForSecondsRealtime(Random.Range(minHideTime, maxHideTime));
                textBox.text = mainDialog ? text : startText;
                yield return new WaitForSecondsRealtime(Random.Range(minHideTime, maxHideTime));
            }
        }
    }
    

    // chooses a random number and shows the number chosen as it rises to display, then enables buttons to allow the next higher/lower choice
    private IEnumerator RandomNumberDisplay(int numberChosen)
    {
        TextMeshProUGUI numberText;
        switch (numberChosen)
        {
            case 0:
                yield return new WaitForSecondsRealtime(startDelay);
                numberText = number1Text;
                break;
            case 1:
                numberText = number2Text;
                break;
            case 2:
                numberText = number3Text;
                break;
            case 3:
                numberText = number4Text;
                break;
            default:
                Debug.Log("WINNNNNER");
                yield break;
        }

        int randomChosen = -1;
        if (numberChosen == 0)
        {
            randomChosen = Random.Range(0, 10);
            numbersSelected[numberChosen] = randomChosen;
        }
        else
        {
            while(numbersSelected[numberChosen] == numbersSelected[numberChosen-1] || randomChosen ==-1)
            {
                randomChosen = Random.Range(0, 10);
                numbersSelected[numberChosen] = randomChosen;
            }
        }
        float elapsedTime = 0f;
        while (elapsedTime < numberRandomizeDuration)
        {
            float t = elapsedTime / numberRandomizeDuration;
            int currentNumber = Mathf.RoundToInt(Mathf.Lerp(0, 9, t));
            if (currentNumber == randomChosen)
            {
                break;
            }
            numberText.text = currentNumber.ToString();
            float newY = Mathf.Lerp(lowestY, highestY, t);
            numberText.rectTransform.localPosition = new Vector3(numberText.rectTransform.localPosition.x, newY, numberText.rectTransform.localPosition.z);
            elapsedTime += Time.unscaledDeltaTime;
            yield return null;
        }
        numberText.text = randomChosen.ToString();
        if (numberChosen < 3)
        {
            EnableButtons(numberChosen);
        }
    }

    // player guesses higher, num is taken to show which number this is a guess for
    // sets corresponding lower button do disabled then runs ChooseButton() to check if player was correct
    public void ChooseHigher(int num)
    {
        higherButtons[num].interactable = false;
        lowerButtons[num].interactable = false;
        Color color =  lowerButtons[num].GetComponent<Image>().color;
        color.a = 15/225f;
        lowerButtons[num].GetComponent<Image>().color = color;
        ChooseButton(true, num+1);
    }
    
    // player guesses lower, num is taken to show which number this is a guess for
    // sets corresponding higer button do disabled then runs ChooseButton() to check if player was correct
    public void ChooseLower(int num)
    {
        lowerButtons[num].interactable = false;
        higherButtons[num].interactable = false;
        Color color =  higherButtons[num].GetComponent<Image>().color;
        color.a = 15/225f;
        higherButtons[num].GetComponent<Image>().color = color;
        ChooseButton(false, num+1);
    }

    // enables the next higher/lower buttons, used at the end of RandomNumberDisplay()
    private void EnableButtons(int num)
    {
        higherButtons[num].interactable = true;
        Color color =  higherButtons[num].GetComponent<Image>().color;
        color.a = 1;
        higherButtons[num].GetComponent<Image>().color = color;
        lowerButtons[num].interactable = true;
        lowerButtons[num].GetComponent<Image>().color = color;
    }

    // checks if the player was correct by calculating the next number using RandomNumberDisplay() and checking if higher or lower, if wrong runs Death(), if correct 3 times runs Win()
    private void ChooseButton(bool higher, int num)
    {
        StartCoroutine(RandomNumberDisplay(num));
        if (numbersSelected[num] > numbersSelected[num-1])
        {
            if (higher)
            {
                numWins++;
            }
            else
            {
                StartCoroutine(Death());
            }
        }
        else
        {
            if(!higher)
            {
                numWins++;
            }
            else
            {
                StartCoroutine(Death());
            }
        }

        if (numWins >= 3)
        {
            StartCoroutine(Win());
        }
        
    }
    
    // disables buttons and waits till the last number is shown
    // enables lose canvas with score then runs LoseAllPoints() to display the player losing all their points
    private IEnumerator Death()
    {
        scoreBoxLose.text = "SCORE: " + score;
        StartCoroutine(LoseAllPoints());
        foreach (Button button in higherButtons)
        {
            button.interactable = false;
        }
        foreach (Button button in lowerButtons)
        {
            button.interactable = false;
        }
        yield return new WaitForSecondsRealtime(2);
        StartCoroutine(FlashText(titleLose, false));
        mainCanvas.enabled = false;
        dieCanvas.enabled = true;
        
       
    }

    // disables buttons and waits till the last number is shown
    // enables win canvas with score then runs GainDoublePoints() to display the player doubling their points
    private IEnumerator Win()
    {
        foreach (Button button in higherButtons)
        {
            button.interactable = false;
        }
        foreach (Button button in lowerButtons)
        {
            button.interactable = false;
        }
        yield return new WaitForSecondsRealtime(2);
        mainCanvas.enabled = false;
        winCanvas.enabled = true;
        StartCoroutine(FlashText(titleWin, false));
        scoreBoxWin.text = "SCORE: " + score;
        StartCoroutine(GainDoublePoints());
      
       
    }

    // decrements points and displays the loss one point at a time over changePointsTime seconds, then enables FlashText() for the score
    private IEnumerator LoseAllPoints()
    {
        int startingScore = score;
        score = 0;
        yield return new WaitForSecondsRealtime(2);
        float timer = 0;
        yield return new WaitForSecondsRealtime(changePointsStartDelay);
        while (timer < changePointsTime)
        {
            float t = timer / numberRandomizeDuration;
            int newPoints = Mathf.RoundToInt(Mathf.Lerp(startingScore, score, t));
            scoreBoxLose.text = "SCORE: " + newPoints;
            timer += Time.unscaledDeltaTime;
            yield return null;
        } 
        StartCoroutine(FlashText(scoreBoxLose, false));
    }
    
    // increments points and displays the gains one point at a time over changePointsTime seconds, then enables FlashText() for the score
    private IEnumerator GainDoublePoints()
    {
        int startingScore = score;
        score *= 2;
        float timer = 0;
        yield return new WaitForSecondsRealtime(changePointsStartDelay);
        while (timer < changePointsTime)
        {
            float t = timer / numberRandomizeDuration;
            int newPoints = Mathf.RoundToInt(Mathf.Lerp(startingScore, score, t));
            scoreBoxWin.text = "SCORE: " + newPoints;
            timer += Time.unscaledDeltaTime;
            yield return null;
        } 
        StartCoroutine(FlashText(scoreBoxWin, false));
    }

    // for the player to leave back to MainScene anytime, if during or before mini-game they get the normal points, if after game then affected score is already saved once mini-game over
    public void LeaveButton()
    {
        PlayerPrefs.SetInt("gameStarted",0);
        PlayerPrefs.SetInt("levelSelect",0);
        PlayerPrefs.SetInt("CurrentScore",score);
        SceneManager.LoadScene("MainScene");
    }
    
}
