using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Fusion;
using System;
using System.Diagnostics;

public class TreeGrowth : NetworkBehaviour
{
    public GameObject treeCanvas;  // UI Panel for the tree math question
    public TextMeshProUGUI treeQuestionText;
    public TMP_InputField treeAnswerInput;
    public Button treeSubmitButton;

    public SpriteRenderer treeSprite; // Tree's SpriteRenderer
    public Sprite grownTreeSprite; // Grown tree sprite
    public TextMeshProUGUI pollinateText;

    private bool hasAnswered = false;

    [Networked] private int number1 { get; set; }
    [Networked] private int number2 { get; set; }
    [Networked] private int correctAnswer { get; set; }
    [Networked] private int totalPlayers { get; set; } // Number of players in the room
    [Networked] private int playersWithWater { get; set; } // Players who collected water
    [Networked] private int playersAnsweredCorrectly { get; set; } // Players who answered correctly
    [Networked] private bool treeGrown { get; set; } // Ensures tree grows only once

    // ✅ Networked variable to track if each player collected water
    [Networked] private NetworkDictionary<PlayerRef, bool> playerHasWater { get; }

    public override void Spawned()
    {
        treeCanvas.SetActive(true); // Ensure UI is visible for all players
        treeSubmitButton.onClick.AddListener(OnSubmitAnswer);

        if (Object.HasStateAuthority)
        {
            totalPlayers = Runner.SessionInfo.PlayerCount; // Get the number of players in the room
            GenerateAndSyncTreeQuestion();
        }
    }

    public void UnlockTreeWatering()
    {
        PlayerRef localPlayer = Runner.LocalPlayer;

        if (playerHasWater.ContainsKey(localPlayer) && playerHasWater[localPlayer])
        {
            UnityEngine.Debug.Log($"[TreeGrowth] Player {localPlayer} already has water! ✅");
            return;
        }

        UnityEngine.Debug.Log($"[TreeGrowth] Unlocking water for Player {localPlayer} ✅");
        RPC_PlayerCollectedWater(localPlayer);
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void RPC_PlayerCollectedWater(PlayerRef player)
    {
        if (playerHasWater.ContainsKey(player) && playerHasWater[player])
        {
            UnityEngine.Debug.Log($"[TreeGrowth] Player {player} already collected water. ✅");
            return;
        }

        // ✅ Properly set the player's water collection status
        playerHasWater.Set(player, true);

        playersWithWater++;
        UnityEngine.Debug.Log($"[TreeGrowth] Player {player} collected water. Total: {playersWithWater}/{totalPlayers} ✅");

        if (playersWithWater >= totalPlayers)
        {
            UnityEngine.Debug.Log("[TreeGrowth] All players collected water! Tree challenge unlocked. ✅");
        }
    }

    void GenerateAndSyncTreeQuestion()
    {
        int generatedNum1 = UnityEngine.Random.Range(5, 10);
        int generatedNum2 = UnityEngine.Random.Range(1, generatedNum1);
        int generatedCorrectAnswer = generatedNum1 - generatedNum2;

        // Sync the generated question to all players
        RPC_SyncTreeQuestion(generatedNum1, generatedNum2, generatedCorrectAnswer);
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    void RPC_SyncTreeQuestion(int num1, int num2, int answer)
    {
        number1 = num1;
        number2 = num2;
        correctAnswer = answer;

        UnityEngine.Debug.Log($"[TreeGrowth] Math Question Synced: {number1} - {number2} ✅");

        // 🔹 Ensure that the UI updates properly
        UpdateTreeQuestionUI();
    }

    void UpdateTreeQuestionUI()
    {
        if (treeQuestionText != null) // Prevent null reference errors
        {
            treeQuestionText.text = $"Solve: {number1} - {number2}";
            UnityEngine.Debug.Log($"[TreeGrowth] UI Updated: {number1} - {number2} ✅");
        }
        else
        {
            UnityEngine.Debug.LogError("[TreeGrowth] treeQuestionText is NULL! ❌");
        }
    }

    void OnSubmitAnswer()
    {
        if (treeGrown) return;

        PlayerRef localPlayer = Runner.LocalPlayer;

        if (!playerHasWater.ContainsKey(localPlayer) || !playerHasWater[localPlayer])
        {
            treeQuestionText.text = "You must collect water first! Solve:" + number1 + " - " + number2;
            return;
        }

        int playerAnswer;
        if (int.TryParse(treeAnswerInput.text, out playerAnswer))
        {
            if (playerAnswer == correctAnswer)
            {
                RPC_CorrectAnswer();
            }
            else
            {
                treeQuestionText.text = "Wrong! Try again. Solve: " + number1 + " - " + number2;
            }
        }
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    void RPC_CorrectAnswer()
    {
        playersAnsweredCorrectly++;

        if (playersAnsweredCorrectly >= totalPlayers) // ✅ All players answered
        {
            RPC_GrowTree();
        }

        // ✅ Notify PollinationManager that this player can answer bee questions
        FindObjectOfType<PollinationManager>()?.RPC_PlayerUnlockedPollination(Object.InputAuthority);
    }

    [Rpc(RpcSources.All, RpcTargets.All)]
    void RPC_GrowTree()
    {
        if (treeGrown) return; // Prevent duplicate execution

        treeGrown = true;
        treeSprite.sprite = grownTreeSprite; // Change to the grown tree sprite
        treeCanvas.SetActive(false); // Hide the UI after success

        pollinateText.text = "Pollinate the flowers";
    }
}
