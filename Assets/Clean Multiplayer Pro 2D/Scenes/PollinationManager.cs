using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Fusion;
using System;
using System.Collections; // Needed for IEnumerator (coroutines)
using System.Collections.Generic; // Needed for HashSet<>



public class PollinationManager : NetworkBehaviour
{
    public GameObject bee; // Assign Bee GameObject
    public GameObject[] flowers; // Assign all flowers in Inspector
    public GameObject springCrystal; // Assign the crystal (Hidden at start)
    public Image crystalUIImage;

    public GameObject mathCanvas; // UI for math question
    public TextMeshProUGUI mathQuestionText;
    public TMP_InputField mathAnswerInput;
    public Button submitButton;
    private HashSet<PlayerRef> playersWhoUnlockedPollination = new HashSet<PlayerRef>(); // Track players who answered tree question
    public TextMeshProUGUI congratulationsText;


    [Networked] private int number1 { get; set; }
    [Networked] private int number2 { get; set; }
    [Networked] private int correctAnswer { get; set; }
    [Networked] private int totalPlayers { get; set; }
    [Networked] private int playersAnsweredCorrectly { get; set; }
    [Networked] private int currentFlowerIndex { get; set; }
    [Networked] private bool allFlowersPollinated { get; set; }

    private bool hasAnswered = false; // Player-specific flag

    public override void Spawned()
    {
        mathCanvas.SetActive(true);
        springCrystal.SetActive(false); // Hide the crystal initially
        submitButton.onClick.RemoveAllListeners(); // Ensure only one listener is assigned
        submitButton.onClick.AddListener(OnSubmitAnswer);

        if (Object.HasStateAuthority)
        {
            totalPlayers = Runner.SessionInfo.PlayerCount;
            GenerateAndSyncMathQuestion();
        }
    }

    void GenerateAndSyncMathQuestion()
    {
        int generatedNum1 = UnityEngine.Random.Range(1, 6);
        int generatedNum2 = UnityEngine.Random.Range(1, 6);
        int generatedCorrectAnswer = generatedNum1 * generatedNum2;

        RPC_SyncMathQuestion(generatedNum1, generatedNum2, generatedCorrectAnswer);
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    void RPC_SyncMathQuestion(int num1, int num2, int answer)
    {
        number1 = num1;
        number2 = num2;
        correctAnswer = answer;

        UnityEngine.Debug.Log($" Math Question Synced: {number1} * {number2}, Correct Answer: {correctAnswer}");

        //  FIX: Ensure values are updated before updating UI
        Invoke(nameof(UpdateMathUI), 0.1f);
    }


    void UpdateMathUI()
    {
        if (mathQuestionText != null)
        {
            mathQuestionText.text = $"Solve: {number1} * {number2}";
            mathAnswerInput.text = ""; // Clear previous input
            hasAnswered = false; // Reset for each question
            UnityEngine.Debug.Log($" UI Updated: {mathQuestionText.text}");
        }
        else
        {
            UnityEngine.Debug.LogError(" mathQuestionText is not assigned in the Inspector!");
        }
    }


    public void OnSubmitAnswer()
    {
        UnityEngine.Debug.Log(" Submit Button Clicked! Checking Answer...");

        // Prevent answering bee questions unless the player solved the tree question
        if (!playersWhoUnlockedPollination.Contains(Object.InputAuthority))
        {
            UnityEngine.Debug.Log(" You must answer the tree question first!");
            mathQuestionText.text = " Solve the tree question first! Solve:" + number1 + " * " + number2;
            return;
        }

        if (allFlowersPollinated || hasAnswered) return;

        if (string.IsNullOrWhiteSpace(mathAnswerInput.text))
        {
            UnityEngine.Debug.Log(" No input detected.");
            mathQuestionText.text = " Enter a number!";
            return;
        }

        int playerAnswer;
        bool isNumber = int.TryParse(mathAnswerInput.text, out playerAnswer);

        if (!isNumber)
        {
            UnityEngine.Debug.Log(" Not a valid number input.");
            mathQuestionText.text = " Enter a valid number!";
            return;
        }

        if (playerAnswer == correctAnswer)
        {
            UnityEngine.Debug.Log(" Correct! One player answered!");
            hasAnswered = true;
            RPC_PlayerAnswered();
        }
        else
        {
            UnityEngine.Debug.Log(" Incorrect answer.");
            mathQuestionText.text = " Wrong! Try again. Solve:" + number1 + " * " + number2;
        }
    }


    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    void RPC_PlayerAnswered()
    {
        playersAnsweredCorrectly++;

        UnityEngine.Debug.Log($" Players Answered: {playersAnsweredCorrectly}/{totalPlayers}");

        if (playersAnsweredCorrectly >= totalPlayers) // All players answered
        {
            UnityEngine.Debug.Log(" All players answered! Moving the bee.");
            RPC_MoveBeeToFlower();
        }
    }


    [Rpc(RpcSources.All, RpcTargets.All)]
    void RPC_MoveBeeToFlower()
    {
        if (currentFlowerIndex >= flowers.Length) return;

        UnityEngine.Debug.Log($" Moving bee to flower {currentFlowerIndex + 1}");

        // Get the target flower position
        Vector3 targetPosition = flowers[currentFlowerIndex].transform.position;

        // Move the bee smoothly instead of teleporting
        StartCoroutine(MoveBeeSmoothly(targetPosition));

        // Change flower color to show it's pollinated
        flowers[currentFlowerIndex].GetComponent<SpriteRenderer>().color = Color.yellow;
        currentFlowerIndex++;
        playersAnsweredCorrectly = 0; // Reset answer count

        if (currentFlowerIndex >= flowers.Length)
        {
            RPC_ShowCrystal();
        }
        else
        {
            GenerateAndSyncMathQuestion(); // New question for next flower
        }
    }


    [Rpc(RpcSources.All, RpcTargets.All)]
    void RPC_ShowCrystal()
    {
        allFlowersPollinated = true;
        springCrystal.SetActive(true);
        crystalUIImage.gameObject.SetActive(true);

        //  Hide Math UI when the last question is answered
        mathCanvas.SetActive(false);
        mathQuestionText.gameObject.SetActive(false);
        mathAnswerInput.gameObject.SetActive(false);
        submitButton.gameObject.SetActive(false);

        UnityEngine.Debug.Log(" All flowers pollinated! Math UI hidden.");
        congratulationsText.text = "Congratulations! You won Level 1 and got your first crystal!";

        FindObjectOfType<ScoreManager>().CalculateScore();
    }


    IEnumerator MoveBeeSmoothly(Vector3 targetPosition)
    {
        float speed = 3f; // Adjust speed as needed

        while (Vector3.Distance(bee.transform.position, targetPosition) > 0.1f)
        {
            bee.transform.position = Vector3.MoveTowards(bee.transform.position, targetPosition, speed * Time.deltaTime);
            yield return null; // Wait for the next frame
        }

        UnityEngine.Debug.Log($" Bee reached flower at {targetPosition}");
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void RPC_PlayerUnlockedPollination(PlayerRef playerRef) //  Change "void" to "public void"
    {
        if (!playersWhoUnlockedPollination.Contains(playerRef))
        {
            playersWhoUnlockedPollination.Add(playerRef);
            UnityEngine.Debug.Log($" Player {playerRef} is now allowed to answer pollination questions!");
        }
    }



}

