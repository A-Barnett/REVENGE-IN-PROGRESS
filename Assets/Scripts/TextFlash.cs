using System.Collections;
using TMPro;
using UnityEngine;
using Random = UnityEngine.Random;

public class TextFlash : MonoBehaviour
{
    
    /// <summary>
    /// Created by: Alex Barnett
    /// Flashes text and can add moving ellipsis onto the end of text
    /// </summary>
    
    [SerializeField] private bool isTitleCard;
    [SerializeField] private float minDisplayTime;
    [SerializeField] private float maxDisplayTime;
    [SerializeField] private float minHideTime;
    [SerializeField] private float maxHideTime;
    [SerializeField] private float maxFlickerTimes;
    [SerializeField] private float ellipsisDelay;
    [SerializeField] private TextMeshProUGUI flashText;
   
    private string text;
    public string ellipsisText;
    private int ellipsisCount;
    
    // sets starting variables and starts flashing coroutine FlashText(), if title card then ellipsis is also added through EllipsisAdd()
    void Start()
    {
        text = flashText.text;
        ellipsisText = text;
        StartCoroutine(FlashText());
        if (isTitleCard)
        {
            StartCoroutine(EllipsisAdd());
        }
    }
    
    // flashes text on and off randomly timed and a random amount of times according to variables, does this by clearing text then setting it back
    private IEnumerator FlashText()
    {
        while (true)
        {
            yield return new WaitForSecondsRealtime(Random.Range(minDisplayTime, maxDisplayTime));
            for (int i = 0; i < Random.Range(1, maxFlickerTimes); i++)
            {
                flashText.text = string.Empty;
                yield return new WaitForSecondsRealtime(Random.Range(minHideTime, maxHideTime));
                flashText.text = ellipsisText;
                yield return new WaitForSecondsRealtime(Random.Range(minHideTime, maxHideTime));
            }
        }
    }
    
    // adds ellipsis onto the end of text, then cycles through ellipsis state
    private IEnumerator EllipsisAdd()
    {
        ellipsisText = text;
        flashText.text = ellipsisText;
        yield return new WaitForSecondsRealtime(ellipsisDelay*2);
        while (true)
        {
            ellipsisText = text + ".";
            flashText.text = ellipsisText;
            yield return new WaitForSecondsRealtime(ellipsisDelay);
            ellipsisText = text + "..";
            flashText.text = ellipsisText;
            yield return new WaitForSecondsRealtime(ellipsisDelay);
            ellipsisText = text + "...";
            flashText.text = ellipsisText;
            yield return new WaitForSecondsRealtime(ellipsisDelay);
        }
    }

}
