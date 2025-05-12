using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Fusion;
using System;

public class Firewood : NetworkBehaviour
{
    public GameObject mathCanvas; // UI for math question
    public TextMeshProUGUI mathQuestionText;
    // Removed TMP_InputField and submitButton, and replaced with multiple choice buttons:
    // public TMP_InputField mathAnswerInput;
    // public Button submitButton;
    public Button[] choiceButtons; // NEW: Array of 4 buttons for answer choices

    public GameObject firewoodIcon; // UI icon showing firewood is collected
    public static int totalFirewood = 3; // Total number of firewood required
    public static int collectedFirewood = 0; // Tracks collected firewood
    public TextMeshProUGUI instructionText; // UI Text for instructions

    // New: assign which question to show (0, 1, or 2)
    public int questionIndex;

    private bool hasAnswered = false;
    private bool isCarryingWood = false;
    private string correctAnswer;

    public override void Spawned()
    {
        mathCanvas.SetActive(false); // Hide math UI at start

        // Remove any listeners from the multiple-choice buttons
        foreach (Button btn in choiceButtons)
        {
            btn.onClick.RemoveAllListeners();
        }
        // (submitButton is no longer used)
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && !hasAnswered)
        {
            ShowMathQuestion();
        }
    }

    void ShowMathQuestion()
    {
        mathCanvas.SetActive(true);
        // No input field to clear since we use buttons now

        // Set the question and the correct answer based on questionIndex.
        switch (questionIndex)
        {
            case 0:
                mathQuestionText.text = "What planet do we live on?";
                correctAnswer = "Earth";
                SetupChoiceButtons(new string[] { "Mercury", "Venus", "Earth", "Mars" });
                break;
            case 1:
                mathQuestionText.text = "What is the closest star to Earth?";
                correctAnswer = "Sun";
                SetupChoiceButtons(new string[] { "Moon", "Mercury", "Sun", "Venus" });
                break;
            case 2:
                mathQuestionText.text = "Which force pulls things down toward Earth?";
                correctAnswer = "Gravity";
                SetupChoiceButtons(new string[] { "Magnetism", "Gravity", "Friction", "Wind" });
                break;
            default:
                mathQuestionText.text = "Unknown question";
                correctAnswer = "";
                break;
        }
    }

    // NEW: Set up the choice buttons with given answer options
    void SetupChoiceButtons(string[] choices)
    {
        // Assuming there are exactly 4 buttons in the array.
        for (int i = 0; i < choiceButtons.Length; i++)
        {
            int index = i; // local copy for the lambda
            TextMeshProUGUI btnText = choiceButtons[i].GetComponentInChildren<TextMeshProUGUI>();
            btnText.text = choices[i];
            // Remove previous listeners
            choiceButtons[i].onClick.RemoveAllListeners();
            // Add a listener that calls OnChoiceSelected with the selected answer
            choiceButtons[i].onClick.AddListener(() => OnChoiceSelected(choices[index]));
        }
    }

    // NEW: Called when one of the multiple-choice buttons is pressed
    public void OnChoiceSelected(string answer)
    {
        UnityEngine.Debug.Log("Choice selected: " + answer);
        // Check answer (case-insensitive)
        if (string.Equals(answer, correctAnswer, StringComparison.OrdinalIgnoreCase))
        {
            hasAnswered = true;
            collectedFirewood++;

            mathCanvas.SetActive(false);
            firewoodIcon.SetActive(true); // Show UI icon
            gameObject.SetActive(false); // Hide firewood object

            // Update instruction text when all firewood is collected
            if (collectedFirewood >= totalFirewood && instructionText != null)
            {
                instructionText.text = "Place firewood in the firepit and start a fire";
            }

            // Notify FirePit that this player has firewood
            FindObjectOfType<FirePit>()?.RPC_PlayerCollectedFirewood(Object.InputAuthority);
        }
        else
        {
            // If wrong, show feedback and let the player try again
            mathQuestionText.text = "Wrong! Try again. " + mathQuestionText.text;
        }
    }

    public bool IsCarryingWood()
    {
        return isCarryingWood;
    }
}
