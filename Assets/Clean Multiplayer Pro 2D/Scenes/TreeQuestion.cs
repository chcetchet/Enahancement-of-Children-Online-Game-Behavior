using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Fusion;
using System.Collections;

public class TreeQuestion : NetworkBehaviour
{
    public GameObject questionPanel;
    public TextMeshProUGUI questionText;
    public Button[] choiceButtons;

    public GameObject goldenAppleIcon;
    public UnityEngine.UI.Image crystalUIImage;
    public TextMeshProUGUI winMessageText;

    [Header("Tree Question Settings")]
    public string question;
    public string[] choices;
    public string correctAnswerValue;

    [Networked] private int totalPlayers { get; set; }
    [Networked] private int playersAnsweredCorrectly { get; set; }
    [Networked] public bool questionAnswered { get; set; }

    public override void Spawned()
    {
        questionPanel.SetActive(false);
        goldenAppleIcon.SetActive(false);

        foreach (Button btn in choiceButtons)
        {
            btn.onClick.RemoveAllListeners();
        }

        if (questionAnswered)
        {
            RPC_SyncQuestionAnswered();
        }

        if (Object.HasStateAuthority)
        {
            totalPlayers = Runner.SessionInfo.PlayerCount;
            RPC_GenerateAndSyncQuestion();
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && !questionAnswered)
        {
            if (Object.HasStateAuthority)
            {
                RPC_CheckAnimalRescueStatus();
            }
        }
    }

    void ShowQuestion()
    {
        questionPanel.SetActive(true);
        UpdateQuestionUI();

        foreach (Button btn in choiceButtons)
        {
            btn.interactable = true;
        }

        SetupChoiceButtons();
    }

    void UpdateQuestionUI()
    {
        if (questionText != null)
        {
            questionText.text = question;
            UnityEngine.Debug.Log($"[TreeQuestion] UI Updated: {question} | Correct Answer: {correctAnswerValue}");
        }
    }

    public void OnChoiceSelected(string answer)
    {
        UnityEngine.Debug.Log($"[TreeQuestion] Choice selected: {answer}");
        string selected = answer.Trim().ToLower();
        string correct = correctAnswerValue.Trim().ToLower();

        if (selected == correct)
        {
            UnityEngine.Debug.Log("[TreeQuestion] Correct answer!");
            questionPanel.SetActive(false); // ✅ Hides the UI for the answering player
            RPC_PlayerAnswered();
        }
        else
        {
            questionText.text = "Wrong! Try again. " + question;
        }
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    void RPC_PlayerAnswered()
    {
        playersAnsweredCorrectly++;
        UnityEngine.Debug.Log($"[TreeQuestion] Players Answered Correctly: {playersAnsweredCorrectly}/{totalPlayers}");

        if (playersAnsweredCorrectly >= totalPlayers)
        {
            RPC_MarkQuestionAnswered();
        }
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void RPC_MarkQuestionAnswered()
    {
        if (!questionAnswered)
        {
            questionAnswered = true;
            RPC_SyncQuestionAnswered();
        }
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    void RPC_SyncQuestionAnswered()
    {
        if (goldenAppleIcon != null)
            goldenAppleIcon.SetActive(true);
        else
            UnityEngine.Debug.LogError("[TreeQuestion] goldenAppleIcon is NULL!");

        if (crystalUIImage != null)
        {
            crystalUIImage.gameObject.SetActive(true);
            FindObjectOfType<ScoreManager>().CalculateScore();
        }
        else
            UnityEngine.Debug.LogError("[TreeQuestion] crystalUIImage is NULL!");

        if (winMessageText != null)
            winMessageText.text = "Congratulations! You collected all 3 crystals and won the game!";
        else
            UnityEngine.Debug.LogError("[TreeQuestion] winMessageText is NULL!");

        if (questionPanel != null)
            questionPanel.SetActive(false);
        else
            UnityEngine.Debug.LogError("[TreeQuestion] questionPanel is NULL!");

        
    }

    void SetupChoiceButtons()
    {
        if (choiceButtons.Length != choices.Length)
        {
            UnityEngine.Debug.LogError("Mismatch: Number of choice buttons and choices do not match!");
            return;
        }

        for (int i = 0; i < choices.Length; i++)
        {
            int index = i;
            TextMeshProUGUI btnText = choiceButtons[i].GetComponentInChildren<TextMeshProUGUI>();
            btnText.text = choices[i];

            choiceButtons[i].interactable = true;
            choiceButtons[i].onClick.RemoveAllListeners();
            choiceButtons[i].onClick.AddListener(() => OnChoiceSelected(choices[index]));
        }
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.StateAuthority)]
    public void RPC_CheckAnimalRescueStatus()
    {
        AnimalRescueQuestion animalRescue = FindObjectOfType<AnimalRescueQuestion>();

        if (animalRescue == null)
        {
            UnityEngine.Debug.LogError("AnimalRescueQuestion not found in the scene!");
            return;
        }

        if (animalRescue.trapRemoved)
        {
            UnityEngine.Debug.Log("Animal Rescue is completed! Enabling Tree question...");
            RPC_EnableTreeQuestion();
        }
        else
        {
            UnityEngine.Debug.Log("Player has NOT answered the Animal Rescue question.");
            RPC_ShowIncompleteAnimalRescueMessage();
        }
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    void RPC_EnableTreeQuestion()
    {
        UnityEngine.Debug.Log("Enabling Tree question for all players...");
        questionPanel.SetActive(true);
        UpdateQuestionUI();

        foreach (Button btn in choiceButtons)
        {
            btn.interactable = true;
        }

        SetupChoiceButtons();
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    void RPC_ShowIncompleteAnimalRescueMessage()
    {
        questionPanel.SetActive(true);
        questionText.text = "Rescue the animal first! 🐾";

        foreach (Button btn in choiceButtons)
        {
            btn.interactable = false;
        }
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    void RPC_GenerateAndSyncQuestion()
    {
        UpdateQuestionUI();
        SetupChoiceButtons();
    }

}

