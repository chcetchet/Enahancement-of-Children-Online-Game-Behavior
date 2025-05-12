using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Fusion;
using System.Collections; // Needed for IEnumerator
using System.Diagnostics;
using System;
using static System.Net.Mime.MediaTypeNames;

public class IceBridge : NetworkBehaviour
{
    // UI for the math question that appears when players enter the Ice Bridge area
    public GameObject mathCanvas;         // The UI Canvas (set to Screen Space - Overlay)
    public TextMeshProUGUI mathQuestionText; // The text component showing the question
    // REMOVED: public TMP_InputField mathAnswerInput;  // Input field for the answer
    // REMOVED: public Button submitButton;             // Button to submit the answer
    public Button[] choiceButtons;            // ADDED: Array of buttons for multiple choice answers

    // The Ice Bridge GameObject that will be activated (unfrozen) once the question is answered
    public GameObject iceBridgeObject;
    public TextMeshProUGUI winMessageText;

    public UnityEngine.UI.Image crystalUIImage;

    [Networked] private int totalPlayers { get; set; }
    [Networked] private int playersAnsweredCorrectly { get; set; }
    [Networked] private bool bridgeActivated { get; set; }

    // Local flag to prevent multiple answers per player
    private bool hasAnswered = false;

    // The correct answer for our question
    private string correctAnswer;

    public override void Spawned()
    {
        mathCanvas.SetActive(false);
        iceBridgeObject.SetActive(false);

        // Set up multiple-choice buttons: remove previous listeners and add new ones.
        foreach (Button btn in choiceButtons)
        {
            btn.onClick.RemoveAllListeners();
            // Add listener that calls OnChoiceSelected with the button's TextMeshProUGUI text
            btn.onClick.AddListener(() => OnChoiceSelected(btn.GetComponentInChildren<TextMeshProUGUI>().text));
        }

        if (Object.HasStateAuthority)
        {
            totalPlayers = Runner.SessionInfo.PlayerCount;
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        UnityEngine.Debug.Log("IceBridge trigger entered by: " + other.gameObject.name);

        if (other.CompareTag("Player") && !bridgeActivated)
        {
            PlayerRef player = other.GetComponent<NetworkObject>().InputAuthority;

            // Always check if the Firepit question was answered
            if (!HasPlayerAnsweredFirepit(player))
            {
                UnityEngine.Debug.Log("Player has NOT answered the Firepit question.");
                ShowIncompleteFirepitMessage();
            }
            else
            {
                UnityEngine.Debug.Log("Player has answered Firepit! Showing IceBridge question...");
                UpdateMathUI();
            }
        }
    }


    bool HasPlayerAnsweredFirepit(PlayerRef player)
    {
        FirePit firePit = FindObjectOfType<FirePit>(); // ✅ Find FirePit instance in scene

        if (firePit == null)
        {
            UnityEngine.Debug.LogError("FirePit not found in the scene!");
            return false;
        }

        return firePit.playersAddedWood >= firePit.totalPlayers; // ✅ Ensure Firepit was solved
    }

    void ShowIncompleteFirepitMessage()
    {
        mathCanvas.SetActive(true);
        mathQuestionText.text = "Start fire first! 🔥";

        // ❌ Keep buttons disabled since Firepit is not answered
        foreach (Button btn in choiceButtons)
        {
            btn.interactable = false;
        }
    }



    void UpdateMathUI()
    {
        mathCanvas.SetActive(true);
        if (mathQuestionText != null)
        {
            // Set the question text and the correct answer.
            mathQuestionText.text = "Which organ pumps blood through the body?";
            correctAnswer = "heart";  // Accepts "heart" or "the heart" (case-insensitive)
            hasAnswered = false;

            UnityEngine.Debug.Log("[IceBridge] UI Updated: " + mathQuestionText.text);

            // ✅ NEW: Enable answer buttons when showing the real question
            foreach (Button btn in choiceButtons)
            {
                btn.interactable = true;
            }

            // Update the multiple-choice buttons with answer choices
            SetupChoiceButtons();
        }
        else
        {
            UnityEngine.Debug.LogError("mathQuestionText is not assigned!");
        }
    }

    // NEW: This function sets up the text for each multiple-choice button.
    void SetupChoiceButtons()
    {
        // Define the choices for this question.
        string[] choices = new string[] { "Heart", "Lungs", "Kidney", "Liver" };
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

    // NEW: This method is called when a player clicks one of the multiple-choice buttons.
    public void OnChoiceSelected(string answer)
    {
        UnityEngine.Debug.Log("[IceBridge] Choice selected: " + answer);

        // Compare answer in lower-case
        string selectedAnswer = answer.Trim().ToLower();
        if (selectedAnswer == "heart" || selectedAnswer == "the heart")
        {
            hasAnswered = true;
            UnityEngine.Debug.Log("[IceBridge] Correct answer by a player!");
            mathCanvas.SetActive(false);
            RPC_PlayerAnswered();
        }
        else
        {
            mathQuestionText.text = "Wrong! Try again. Which organ pumps blood through the body?";
        }
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    void RPC_PlayerAnswered()
    {
        playersAnsweredCorrectly++;
        UnityEngine.Debug.Log($"[IceBridge] Players Answered Correctly: {playersAnsweredCorrectly}/{totalPlayers}");

        if (playersAnsweredCorrectly >= totalPlayers)
        {
            RPC_ActivateBridge();
        }
    }

    [Rpc(RpcSources.All, RpcTargets.All)]
    void RPC_ActivateBridge()
    {
        if (bridgeActivated) return;
        bridgeActivated = true;
        mathCanvas.SetActive(false); // Hide the math UI
        iceBridgeObject.SetActive(true); // Activate the ice bridge
        UnityEngine.Debug.Log("Ice bridge activated!");

        if (winMessageText != null)
        {
            winMessageText.text = "Congratulations, you won Level 2 and got your second crystal!";
        }

        crystalUIImage.gameObject.SetActive(true);
        FindObjectOfType<ScoreManager>().CalculateScore();
    }
}