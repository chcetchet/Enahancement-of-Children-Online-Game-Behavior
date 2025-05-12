using UnityEngine;
using TMPro; // Import TextMeshPro namespace

public class ScoreManager : MonoBehaviour
{
    public TMP_Text scoreText; // Assign in Inspector
    private float startTime;
    private int score;
    private int totalScore;

    void Start()
    {
        startTime = Time.time; // Record start time

        // Load previous total score (default 0 if not set)
        totalScore = PlayerPrefs.GetInt("TotalScore", 0);
        UpdateScoreText();
    }

    public void CalculateScore()
    {
        float timeTaken = Time.time - startTime; // Time elapsed
        score = Mathf.Max(200 - (int)(timeTaken * 2), 0); // Example scoring system
        totalScore += score; // Add to total score

        // Save new total score
        PlayerPrefs.SetInt("TotalScore", totalScore);
        PlayerPrefs.Save();

        UpdateScoreText();
    }

    void UpdateScoreText()
    {
        if (scoreText != null)
        {
            scoreText.text = "Score: " + totalScore;
        }
    }
}
