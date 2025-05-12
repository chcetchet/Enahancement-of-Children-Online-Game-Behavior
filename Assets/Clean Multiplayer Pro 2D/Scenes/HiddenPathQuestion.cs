using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Fusion;
using System.Collections;
using System;

public class HiddenPathQuestion : NetworkBehaviour
{
    // UI Elements (assign in the Inspector)
    public GameObject questionPanel;         // The UI Panel (e.g., AutumnQuestionPanel)
    public TextMeshProUGUI questionText;       // The question prompt text
    public Button[] answerButtons;   // Array of 4 answer buttons
    public GameObject obstacle;
    public TextMeshProUGUI actionMessageText;

    // Networked cooperative variables
    [Networked] private int totalPlayers { get; set; }
    [Networked] private int playersAnsweredCorrectly { get; set; }
    [Networked] private bool actionTriggered { get; set; }

    // The correct answer text (must exactly match one of the choices)
    private string correctAnswer;

    // Action callback (e.g., remove obstacle)
    public System.Action OnActionCompleted;

    public override void Spawned()
    {
        // Hide the UI initially
        questionPanel.SetActive(false);

        

        // Remove any previous listeners from the buttons
        foreach (Button btn in answerButtons)
        {
            btn.onClick.RemoveAllListeners();
        }

        if (obstacle != null)
        {
            OnActionCompleted += () =>
            {
                obstacle.SetActive(false); // Deactivate the obstacle
                UnityEngine.Debug.Log("Obstacle deactivated via callback.");
            };
        }
        else
        {
            UnityEngine.Debug.LogWarning("No obstacle assigned!");
        }


        if (Object.HasStateAuthority)
        {
            totalPlayers = Runner.SessionInfo.PlayerCount;
            GenerateAndSyncQuestion();
        }
    }

    // Trigger the question when a player enters the obstacle's area
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && !actionTriggered)
        {
            ShowQuestion();
        }
    }

    void ShowQuestion()
    {
        questionPanel.SetActive(true);
        UpdateQuestionUI();
    }

    void GenerateAndSyncQuestion()
    {
        // Updated question and choices
        string question = "The more you take from me, the bigger I get. What am I?";
        string[] choices = { "A snowman", "A hole", "A mountain", "A puddle" };
        correctAnswer = "A hole"; // The correct answer

        // Set the question prompt
        questionText.text = question;

        // Assign choices to buttons
        for (int i = 0; i < answerButtons.Length; i++)
        {
            int index = i;
            TextMeshProUGUI btnText = answerButtons[i].GetComponentInChildren<TextMeshProUGUI>();
            btnText.text = choices[i];
            answerButtons[i].onClick.RemoveAllListeners();
            answerButtons[i].onClick.AddListener(() => OnAnswerSelected(choices[index]));
        }
    }

    void UpdateQuestionUI()
    {
        // Optionally update UI if needed; in this case, our question and buttons are already set.
        UnityEngine.Debug.Log($"[HiddenPathQuestion] UI Updated: {questionText.text} (Correct: {correctAnswer})");
    }

    void OnAnswerSelected(string answer)
    {
        UnityEngine.Debug.Log($"[HiddenPathQuestion] Button clicked with answer: {answer}");
        if (answer == correctAnswer)
        {
            UnityEngine.Debug.Log("[HiddenPathQuestion] Correct answer!");
            RPC_PlayerAnswered();
        }
        else
        {
            UnityEngine.Debug.Log("[HiddenPathQuestion] Incorrect answer. Try again.");
            questionText.text = " Wrong! Try again. The more you take from me, the bigger I get. What am I?";
        }
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    void RPC_PlayerAnswered()
    {
        playersAnsweredCorrectly++;
        UnityEngine.Debug.Log($"[HiddenPathQuestion] Players answered correctly: {playersAnsweredCorrectly}/{totalPlayers}");

        if (playersAnsweredCorrectly >= totalPlayers)
        {
            RPC_TriggerAction();
        }
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    void RPC_TriggerAction()
    {
        if (actionTriggered) return;
        actionTriggered = true;
        questionPanel.SetActive(false); // Hide the UI
        if (actionMessageText != null)
        {
            actionMessageText.text = "Revive the tree";
        }
        UnityEngine.Debug.Log("[HiddenPathQuestion] All players answered correctly! Action triggered.");
        // Invoke the action callback if assigned
        OnActionCompleted?.Invoke();
    }
}
