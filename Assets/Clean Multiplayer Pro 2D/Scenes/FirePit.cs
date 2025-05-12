using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Fusion;
using System.Diagnostics;
using System;

// Added: using System.Collections is already present if needed.
public class FirePit : NetworkBehaviour
{
    public GameObject mathCanvas; // UI for fire question
    public TextMeshProUGUI mathQuestionText;
    // REMOVED: public TMP_InputField mathAnswerInput;
    // REMOVED: public Button submitButton;
    public Button[] choiceButtons; // ADDED: Array of buttons for multiple-choice answers

    public GameObject fireEffect; // Fire object (inactive at start)
    public GameObject FirewoodIcon1;
    public GameObject FirewoodIcon2;
    public GameObject FirewoodIcon3;
    public TextMeshProUGUI gameObjectiveText;

    [Networked] public int playersAddedWood { get; set; }
    [Networked] public int totalPlayers { get; set; } // Auto-detected number of players

    private bool hasAnswered = false;
    private bool fireStarted = false;

    private string correctAnswer; // will store the correct answer as text

    public override void Spawned()
    {
        fireEffect.SetActive(false); // Fire is off at start
        mathCanvas.SetActive(false); // Hide math UI at start

        // Remove any listeners from choice buttons (new multiple-choice system)
        foreach (Button btn in choiceButtons)
        {
            btn.onClick.RemoveAllListeners();
            // Add a listener that passes the button's current text when clicked.
            btn.onClick.AddListener(() => OnChoiceSelected(btn.GetComponentInChildren<TextMeshProUGUI>().text));
        }

        if (Object.HasStateAuthority)
        {
            totalPlayers = Runner.SessionInfo.PlayerCount;
            UnityEngine.Debug.Log($" Total Players Detected: {totalPlayers}");
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        UnityEngine.Debug.Log("Player touched fire pit!");

        if (other.CompareTag("Player"))
        {
            PlayerRef player = other.GetComponent<NetworkObject>().InputAuthority;

            // ✅ NEW: Check if player has answered all Firewood questions
            if (!HasPlayerAnsweredAllFirewoodQuestions(player))
            {
                UnityEngine.Debug.Log("Player has NOT answered all Firewood questions.");
                ShowIncompleteFirewoodMessage();
                return; // ✅ Stop execution, prevent question from appearing
            }

            // ✅ Check if the player has firewood
            if (playersWithFirewood.ContainsKey(player) && playersWithFirewood[player])
            {
                UnityEngine.Debug.Log("Player has firewood! Showing Firepit question...");
                ShowMathQuestion();
            }
            else
            {
                UnityEngine.Debug.Log("Player has NOT collected firewood.");
            }
        }
    }

    // ✅ NEW: Function to check if the player has answered all 3 Firewood questions
    bool HasPlayerAnsweredAllFirewoodQuestions(PlayerRef player)
    {
        return Firewood.collectedFirewood >= Firewood.totalFirewood;
    }

    // ✅ NEW: Show message if the player hasn’t answered all Firewood questions
    void ShowIncompleteFirewoodMessage()
    {
        mathCanvas.SetActive(true);
        mathQuestionText.text = "Collect firewood first! 🔥";

        // Disable answer buttons so players can't interact
        foreach (Button btn in choiceButtons)
        {
            btn.interactable = false;
        }
    }

    void ShowMathQuestion()
    {
        mathCanvas.SetActive(true);
        mathQuestionText.text = "What is the freezing point of water?";
        correctAnswer = "0";

        // Enable buttons when real question appears
        foreach (Button btn in choiceButtons)
        {
            btn.interactable = true;
        }

        SetupChoiceButtons(new string[] { "0", "32", "100", "212" });
    }

    // NEW: Setup the text on each multiple-choice button.
    void SetupChoiceButtons(string[] choices)
    {
        if (choiceButtons.Length != choices.Length)
        {
            UnityEngine.Debug.LogError("Mismatch: Number of choice buttons and choices do not match!");
            return;
        }
        for (int i = 0; i < choices.Length; i++)
        {
            TextMeshProUGUI btnText = choiceButtons[i].GetComponentInChildren<TextMeshProUGUI>();
            btnText.text = choices[i];
        }
    }

    // NEW: Called when a player clicks one of the multiple-choice buttons.
    public void OnChoiceSelected(string answer)
    {
        UnityEngine.Debug.Log("[FirePit] Choice selected: " + answer);

        // Check the answer (case-insensitive comparison)
        if (string.Equals(answer.Trim(), correctAnswer, StringComparison.OrdinalIgnoreCase))
        {
            UnityEngine.Debug.Log(" Correct answer by a player!");
            hasAnswered = true;
            playersAddedWood++;
            mathCanvas.SetActive(false);

            FirewoodIcon1.SetActive(false);
            FirewoodIcon2.SetActive(false);
            FirewoodIcon3.SetActive(false);

            UnityEngine.Debug.Log($" Firewood Added: {playersAddedWood}/{totalPlayers}");

            if (playersAddedWood >= totalPlayers)
            {
                RPC_StartFire();
            }
        }
        else
        {
            // Provide feedback if the answer is wrong.
            mathQuestionText.text = "Wrong! Try again. What is the freezing point of water?";
        }
    }

    [Rpc(RpcSources.All, RpcTargets.All)]
    void RPC_StartFire()
    {
        fireStarted = true;
        fireEffect.SetActive(true); // Show fire effect
        UnityEngine.Debug.Log(" Fire started!");

        if (gameObjectiveText != null)
        {
            gameObjectiveText.text = "Cross the bridge"; // Change the text
        }
    }

    [Networked] private NetworkDictionary<PlayerRef, bool> playersWithFirewood { get; } = new NetworkDictionary<PlayerRef, bool>();

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void RPC_PlayerCollectedFirewood(PlayerRef player)
    {
        if (!playersWithFirewood.ContainsKey(player))
        {
            playersWithFirewood.Add(player, true);
            UnityEngine.Debug.Log($" Player {player} has collected firewood!");
        }
    }
}