using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Fusion;
using System.Collections;

public class TreeRevivalQuestion : NetworkBehaviour
{
    public GameObject questionPanel;         // UI Panel for the question
    public TextMeshProUGUI questionText;       // The question prompt
    public Button[] answerButtons;             // Array of 4 answer buttons

    public SpriteRenderer treeSprite;          // The withered tree's SpriteRenderer
    public Sprite revivedTreeSprite;           // The revived tree sprite
    public TextMeshProUGUI actionMessageText;

    [Networked] private int totalPlayers { get; set; }
    [Networked] private int playersAnsweredCorrectly { get; set; }
    [Networked] public bool treeRevived { get; set; }

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
        if (other.CompareTag("Player") && !treeRevived)
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
        //question for Tree Revival
        string question = "If you mix blue and yellow, what color do you get?";
        string[] choices = { "Purple", "Orange", "Green", "Red" };
        correctAnswer = "Green";

        questionText.text = question;

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
        UnityEngine.Debug.Log($"[TreeRevival] UI Updated: {questionText.text} (Correct: {correctAnswer})");
    }

    void OnAnswerSelected(string answer)
    {
        UnityEngine.Debug.Log($"[TreeRevival] Button clicked with answer: {answer}");
        if (answer == correctAnswer)
        {
            UnityEngine.Debug.Log("[TreeRevival] Correct answer!");
            RPC_PlayerAnswered();
        }
        else
        {
            UnityEngine.Debug.Log("[TreeRevival] Incorrect answer, try again.");
            questionText.text = " Wrong! Try again. If you mix blue and yellow, what color do you get?";
        }
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    void RPC_PlayerAnswered()
    {
        playersAnsweredCorrectly++;
        UnityEngine.Debug.Log($"[TreeRevival] Players answered correctly: {playersAnsweredCorrectly}/{totalPlayers}");

        if (playersAnsweredCorrectly >= totalPlayers)
        {
            RPC_ReviveTree();
        }
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    void RPC_ReviveTree()
    {
        if (treeRevived) return;
        treeRevived = true; // Ensure this is set when the tree is revived
        treeSprite.sprite = revivedTreeSprite; // Change to revived tree sprite
        questionPanel.SetActive(false);

        if (actionMessageText != null)
        {
            actionMessageText.text = "Rescue the animal";
        }

        UnityEngine.Debug.Log("[TreeRevival] Tree revived!");
    }
}
