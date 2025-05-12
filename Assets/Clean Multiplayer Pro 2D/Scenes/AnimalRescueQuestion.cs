using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Fusion;
using System.Collections;

public class AnimalRescueQuestion : NetworkBehaviour
{
    public GameObject questionPanel;         // UI Panel for the question
    public TextMeshProUGUI questionText;       // The question prompt
    public Button[] answerButtons;             // Array of 4 answer buttons

    public GameObject trapObject;              // The net/cage that traps the animal
    public GameObject animal;                  // The trapped animal
    public TextMeshProUGUI actionMessageText;

    [Networked] private int totalPlayers { get; set; }
    [Networked] private int playersAnsweredCorrectly { get; set; }
    [Networked] public bool trapRemoved { get; set; }

    private string correctAnswer;

    public override void Spawned()
    {
        questionPanel.SetActive(false);

        

        foreach (Button btn in answerButtons)
        {
            btn.onClick.RemoveAllListeners();
        }

        if (Object.HasStateAuthority)
        {
            totalPlayers = Runner.SessionInfo.PlayerCount;
            GenerateAndSyncQuestion();
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && !trapRemoved)
        {
            PlayerRef player = other.GetComponent<NetworkObject>().InputAuthority;

            // ✅ NEW: Check if the player has answered the Tree Revival question before showing Animal Rescue question
            if (!HasPlayerAnsweredTreeRevival(player))
            {
                UnityEngine.Debug.Log("Player has NOT answered the Tree Revival question.");
                ShowIncompleteTreeRevivalMessage();
                return; // ✅ Stop execution, prevent Animal Rescue question from appearing
            }

            UnityEngine.Debug.Log("Player has answered Tree Revival! Showing Animal Rescue question...");
            ShowQuestion(); // ✅ Show real question and re-enable buttons
        }
    }


    bool HasPlayerAnsweredTreeRevival(PlayerRef player)
    {
        TreeRevivalQuestion treeRevival = FindObjectOfType<TreeRevivalQuestion>(); // ✅ Find Tree Revival instance

        if (treeRevival == null)
        {
            UnityEngine.Debug.LogError("TreeRevival not found in the scene!");
            return false;
        }

        return treeRevival.treeRevived; // ✅ Ensure Tree Revival was completed
    }

    void ShowIncompleteTreeRevivalMessage()
    {
        questionPanel.SetActive(true);
        questionText.text = "Revive the tree first! 🌳";

        // ❌ Keep buttons disabled since Tree Revival is not answered
        foreach (Button btn in answerButtons)
        {
            btn.interactable = false;
        }
    }

    void ShowQuestion()
    {
        questionPanel.SetActive(true);
        UpdateQuestionUI();

        // ✅ Ensure buttons are interactable when the question is shown
        foreach (Button btn in answerButtons)
        {
            btn.interactable = true;
        }

        GenerateAndSyncQuestion(); // ✅ Regenerate question and update buttons
    }

    void GenerateAndSyncQuestion()
    {
        // ✅ Question for Animal Rescue
        string question = "Which is lighter: a kilogram of feathers or a kilogram of bricks?";
        string[] choices = { "Feathers", "Bricks", "They are the same", "It depends on the weather" };
        correctAnswer = "They are the same";

        questionText.text = question;

        for (int i = 0; i < answerButtons.Length; i++)
        {
            int index = i;
            TextMeshProUGUI btnText = answerButtons[i].GetComponentInChildren<TextMeshProUGUI>();
            btnText.text = choices[i];

            // ✅ Reset button state and add listeners correctly
            answerButtons[i].interactable = true; // Ensure buttons are active
            answerButtons[i].onClick.RemoveAllListeners();
            answerButtons[i].onClick.AddListener(() => OnAnswerSelected(choices[index]));
        }
    }

    void UpdateQuestionUI()
    {
        UnityEngine.Debug.Log($"[AnimalRescue] UI Updated: {questionText.text} (Correct: {correctAnswer})");
    }

    void OnAnswerSelected(string answer)
    {
        UnityEngine.Debug.Log($"[AnimalRescue] Button clicked with answer: {answer}");
        if (answer == correctAnswer)
        {
            UnityEngine.Debug.Log("[AnimalRescue] Correct answer!");
            RPC_PlayerAnswered();
        }
        else
        {
            UnityEngine.Debug.Log("[AnimalRescue] Incorrect answer, try again.");
            questionText.text = " Wrong! Try again. Which is lighter: a kilogram of feathers or a kilogram of bricks?";
        }
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    void RPC_PlayerAnswered()
    {
        playersAnsweredCorrectly++;
        UnityEngine.Debug.Log($"[AnimalRescue] Players answered correctly: {playersAnsweredCorrectly}/{totalPlayers}");

        if (playersAnsweredCorrectly >= totalPlayers)
        {
            RPC_FreeAnimal();
        }
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    void RPC_FreeAnimal()
    {
        if (trapRemoved) return;
        trapRemoved = true;
        questionPanel.SetActive(false);

        // ✅ Hide the cage but keep it in the scene
        if (trapObject != null)
        {
            // ✅ Disable its visuals (make it invisible)
            SpriteRenderer sprite = trapObject.GetComponent<SpriteRenderer>();
            if (sprite != null) sprite.enabled = false;

            // ✅ Disable its colliders (so players can pass through)
            Collider2D collider = trapObject.GetComponent<Collider2D>();
            if (collider != null) collider.enabled = false;
        }

        if (actionMessageText != null)
        {
            actionMessageText.text = "Collect golden apples from the trees";
        }

        UnityEngine.Debug.Log("[AnimalRescue] Animal rescued!");
    }

}

