using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Fusion;
using System;
using System.Collections;

public class MathChallenge : NetworkBehaviour
{
    public GameObject mathCanvas;
    public TextMeshProUGUI questionText;
    public TMP_InputField answerInput;
    public Button submitButton;
    public TextMeshProUGUI growTreeText;
    private bool hasAnswered = false;

    [Networked] private int number1 { get; set; }
    [Networked] private int number2 { get; set; }
    [Networked] private int correctAnswer { get; set; }
    [Networked] public NetworkBool waterCollected { get; set; }

    private TreeGrowth treeGrowth;

    public override void Spawned()
    {
        UnityEngine.Debug.Log("[MathChallenge] UI should be visible. ✅");

        if (submitButton != null)
        {
            submitButton.onClick.AddListener(CheckAnswer);
            UnityEngine.Debug.Log("[MathChallenge] Submit Button Listener Added ✅");
        }
        else
        {
            UnityEngine.Debug.LogError("[MathChallenge] Submit Button is NULL! ❌");
        }

        treeGrowth = FindObjectOfType<TreeGrowth>();

        if (Object.HasStateAuthority)
        {
            GenerateAndSyncMathQuestion();
        }

        Invoke(nameof(UpdateQuestionText), 0.1f);
    }

    private void GenerateAndSyncMathQuestion()
    {
        int generatedNumber1 = UnityEngine.Random.Range(1, 10);
        int generatedNumber2 = UnityEngine.Random.Range(1, 10);
        int generatedCorrectAnswer = generatedNumber1 + generatedNumber2;

        RPC_SyncMathQuestion(generatedNumber1, generatedNumber2, generatedCorrectAnswer);
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_SyncMathQuestion(int num1, int num2, int answer)
    {
        number1 = num1;
        number2 = num2;
        correctAnswer = answer;

        UpdateQuestionText();
        UnityEngine.Debug.Log($"[MathChallenge] New Question Synced: {number1} + {number2} ✅");
    }

    private void UpdateQuestionText()
    {
        if (questionText != null)
        {
            questionText.text = $"Solve: {number1} + {number2}";
            questionText.ForceMeshUpdate();
            UnityEngine.Debug.Log($"[MathChallenge] Question Updated: {questionText.text} ✅");
        }
        else
        {
            UnityEngine.Debug.LogError("[MathChallenge] questionText is NULL! ❌");
        }
    }

    public void CheckAnswer()
    {
        if (!mathCanvas.activeSelf) return;

        UnityEngine.Debug.Log("[MathChallenge] Submit Button Clicked ✅");

        int playerAnswer;
        if (int.TryParse(answerInput.text, out playerAnswer))
        {
            if (playerAnswer == correctAnswer)
            {
                UnityEngine.Debug.Log("[MathChallenge] Correct! Water collected. ✅");
                questionText.text = "Water Collected!";
                growTreeText.text = "Grow The Tree";
                questionText.ForceMeshUpdate();

                waterCollected = true;
                RPC_NotifyTreeGrowth(Runner.LocalPlayer);

                answerInput.interactable = false;
                submitButton.interactable = false;
            }
            else
            {
                UnityEngine.Debug.Log("[MathChallenge] Incorrect, try again. ❌");
                questionText.text = "Wrong, try again! Solve:" + number1 + " + " + number2;
                questionText.ForceMeshUpdate();
            }
        }
        else
        {
            UnityEngine.Debug.Log("[MathChallenge] Invalid Input! ❌");
        }
    }


    private void ShowMathUI()
    {
        if (mathCanvas != null)
        {
            UnityEngine.Debug.Log("[MathChallenge] Showing UI... ✅");
            mathCanvas.SetActive(true);
        }
        else
        {
            UnityEngine.Debug.LogError("[MathChallenge] mathCanvas is NULL! ❌");
        }

        if (answerInput != null)
        {
            answerInput.interactable = true;
            answerInput.text = "";
        }

        if (submitButton != null)
        {
            submitButton.interactable = true;
        }

        UpdateQuestionText();
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    void RPC_NotifyTreeGrowth(PlayerRef player)
    {
        if (treeGrowth != null)
        {
            UnityEngine.Debug.Log($"[MathChallenge] Sending water collection update for Player {player} ✅");
            treeGrowth.RPC_PlayerCollectedWater(player);
        }
        else
        {
            UnityEngine.Debug.LogError("[MathChallenge] TreeGrowth reference is NULL! ❌");
        }
    }
}