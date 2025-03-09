using System;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

public class ScoreController : MonoBehaviour
{
    
    /// <summary>
    /// Created by: Alex Barnett
    /// Main calculator of player score, using multiplier and displaying to game canvas
    /// </summary>
  
    [SerializeField] private float multiplier;
    [SerializeField] private float points;
    [SerializeField] private float multiDecreaseMultiplier;
    [SerializeField] private float pointsMultiDivide;
    [SerializeField] private TextMeshProUGUI multiDisplay;
    [SerializeField] private Volume postProcess;
    [SerializeField] private TextMeshProUGUI scoreText;
    
    [NonSerialized] public float score;
    private ChromaticAberration chromaticAberration;
    private PaniniProjection paniniProjection;
    private MotionBlur motionBlur;
    
    // gets the post-processor for use in FixedUpdate()
    private void Start()
    {
        postProcess.profile.TryGet(out chromaticAberration);
        postProcess.profile.TryGet(out paniniProjection);
        postProcess.profile.TryGet(out motionBlur);
    }

    // runs in ShotLine when an enemy is killed, adds to score and increases multiplier
    public void KillScore()
    {
        score += points * multiplier;
        multiplier += points / pointsMultiDivide;
    }
    
    // increases effect intensities as multiplier increases to display how well the player is doing
    private void FixedUpdate()
    {
        float intensity = (multiplier-1f)/30f;
        chromaticAberration.intensity.Override(intensity);
        paniniProjection.distance.Override(intensity);
        motionBlur.intensity.Override(intensity);
        if (multiplier > 1)
        {
            multiplier -= Time.deltaTime * multiDecreaseMultiplier;
        }
    }

    // displays score and multiplier to game canvas, in LateUpdate() so it does not change while being displayed
    private void LateUpdate()
    {
        multiDisplay.text = Math.Round(multiplier, 1)+ "x";
        scoreText.text = Math.Round(score, 0).ToString();
    }

    // reset button on main menu, resets game progress, mostly for testing purposes
    public void ResetScore()
    {
        PlayerPrefs.SetInt("Level2", 0);
        PlayerPrefs.SetInt("Level3", 0);
        PlayerPrefs.SetInt("Score",0);
        SceneManager.LoadScene("MainScene");
    }
}
