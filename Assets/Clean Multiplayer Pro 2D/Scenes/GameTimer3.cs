using UnityEngine;
using UnityEngine.SceneManagement;
using Fusion;
using System.Collections;
using TMPro; // Import TextMeshPro

public class GameTimer3 : NetworkBehaviour
{
    [Networked] private float Timer { get; set; } = 150f; // Set your desired time limit

    private bool timeUp = false;
    public TextMeshProUGUI gameOverText; // Reference to TextMeshPro UI element
    public TextMeshProUGUI timerText; // Reference to Timer UI

    void Start()
    {
        // Hide game over text at the start
        if (gameOverText != null)
        {
            gameOverText.gameObject.SetActive(false);
        }

        // Initialize timer display
        UpdateTimerUI();
    }

    void Update()
    {
        if (!HasStateAuthority) return; // Ensure only the host updates the timer

        if (Timer > 0)
        {
            Timer -= Time.deltaTime;
            UpdateTimerUI(); // Update the UI timer text
        }
        else if (!timeUp)
        {
            timeUp = true;
            ShowGameOverMessage();
        }
    }

    void ShowGameOverMessage()
    {
        Debug.Log("⏳ Time is up! You lost the game.");

        // Show TextMeshPro Game Over Message
        if (gameOverText != null)
        {
            gameOverText.text = "Time is up! You lost the game.";
            gameOverText.gameObject.SetActive(true);
        }
        else
        {
            Debug.LogWarning("⚠ GameOverText UI (TextMeshPro) is not assigned in the Inspector!");
        }

        // Hide Timer UI when time is up
        if (timerText != null)
        {
            timerText.gameObject.SetActive(false);
        }

        // Wait a few seconds before switching scenes
        Invoke(nameof(ResetGame), 3f);
    }

    void UpdateTimerUI()
    {
        // Update the TextMeshPro timer display
        if (timerText != null)
        {
            timerText.text = "Time Left: " + Mathf.CeilToInt(Timer) + "s";
        }
        else
        {
            Debug.LogWarning("⚠ TimerText UI (TextMeshPro) is not assigned in the Inspector!");
        }
    }

    void ResetGame()
    {
        Debug.Log("🔄 ResetGame() called! Switching back to Environment 1...");

        // Hide game over text before switching scenes
        if (gameOverText != null)
        {
            gameOverText.gameObject.SetActive(false);
        }

        if (Runner.IsServer || Runner.IsClient)
        {
            StartCoroutine(SwitchScene());
        }
    }

    private IEnumerator SwitchScene()
    {
        Debug.Log("⏳ Preparing to switch back to Environment 1...");

        // Step 1: Load Environment 1 Additively
        Debug.Log("🔄 Loading Environment 1...");
        Runner.LoadScene("Environment 1", LoadSceneMode.Additive);

        // Step 2: Wait until the next scene is fully loaded
        yield return new WaitUntil(() => SceneManager.GetSceneByName("Environment 1").isLoaded);
        Debug.Log("✅ Environment 1 is loaded!");

        // Step 3: Set Environment 1 as the Active Scene
        Scene newScene = SceneManager.GetSceneByName("Environment 1");
        SceneManager.SetActiveScene(newScene);
        Debug.Log("✅ Active scene set to: Environment 1");

        // Step 4: Define a Specific Spawn Position
        Vector3 specificSpawnPosition = new Vector3(5f, 1f, 0f); // Change this to your preferred spawn point

        // Step 5: Ensure Player Exists & Move to the Specific Spawn Position
        yield return new WaitForSeconds(1f); // Small delay to ensure objects are loaded
        GameObject localPlayer = GameObject.FindGameObjectWithTag("Player");

        if (localPlayer != null)
        {
            localPlayer.transform.position = specificSpawnPosition;
            Debug.Log($"✅ Player repositioned to specific spawn point: {specificSpawnPosition}");
        }
        else
        {
            Debug.LogWarning("⚠ Local player not found! Ensure player spawns correctly.");
        }

        // Step 6: Unload Previous Scenes (Except "Game" and "DontDestroyOnLoad")
        foreach (Scene scene in SceneManager.GetAllScenes())
        {
            if (scene.name != "Game" && scene.name != "DontDestroyOnLoad" && scene.isLoaded)
            {
                Debug.Log($"🔄 Unloading previous scene: {scene.name}");
                yield return SceneManager.UnloadSceneAsync(scene.name);
            }
        }

        Debug.Log("✅ Scene reset complete. Now in Environment 1.");
    }
}
