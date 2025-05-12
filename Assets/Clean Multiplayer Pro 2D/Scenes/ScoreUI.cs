using UnityEngine;
using TMPro;
using Fusion;
using System.Diagnostics;

public class ScoreUI : MonoBehaviour
{
    public TextMeshProUGUI playerScoreText;
    private NetworkRunner runner; // ✅ Store the NetworkRunner instance

    private void Start()
    {
        FindRunner(); // ✅ Ensure the correct Runner instance is set
        UpdateScoreUI(); // ✅ Update UI when level starts
    }

    private void Update()
    {
        if (GameScoreManager.Instance != null && runner != null)
        {
            PlayerRef localPlayer = runner.LocalPlayer;
            int score = GameScoreManager.Instance.GetPlayerScore(localPlayer);
            playerScoreText.text = "Score: " + score;
        }
    }

    private void FindRunner()
    {
        runner = FindObjectOfType<NetworkRunner>(); // ✅ Find Runner dynamically
        if (runner == null)
        {
            UnityEngine.Debug.LogError("NetworkRunner not found in the scene!");
        }
    }

    private void OnEnable()
    {
        UpdateScoreUI(); // ✅ Force UI update when enabled
    }

    private void UpdateScoreUI()
    {
        if (GameScoreManager.Instance != null && runner != null)
        {
            PlayerRef localPlayer = runner.LocalPlayer;
            int score = GameScoreManager.Instance.GetPlayerScore(localPlayer);
            playerScoreText.text = "Score: " + score;
        }
    }
}










