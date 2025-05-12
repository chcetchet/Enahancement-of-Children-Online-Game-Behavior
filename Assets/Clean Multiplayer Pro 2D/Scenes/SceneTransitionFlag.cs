using UnityEngine;
using UnityEngine.SceneManagement;
using Fusion;
using System.Collections;

public class SceneTransitionFlag : NetworkBehaviour
{
    public string nextSceneName; // Set this in the Inspector

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            UnityEngine.Debug.Log("Player touched the flag!");

            if (Runner.IsServer || Runner.IsClient)
            {
                StartCoroutine(SwitchScene());
            }
        }
    }

    private IEnumerator SwitchScene()
    {
        UnityEngine.Debug.Log("Preparing to switch scene...");

        // Step 1: Load Next Scene First
        UnityEngine.Debug.Log("Loading next scene: " + nextSceneName);
        Runner.LoadScene(nextSceneName, LoadSceneMode.Additive);

        // Step 2: Wait until the next scene is fully loaded
        yield return new WaitUntil(() => SceneManager.GetSceneByName(nextSceneName).isLoaded);

        // ***** ADDED CODE STARTS HERE *****
        // Step 4: Set the new scene as the active scene and reposition the local player
        Scene newScene = SceneManager.GetSceneByName(nextSceneName);
        SceneManager.SetActiveScene(newScene);
        UnityEngine.Debug.Log("Active scene set to: " + nextSceneName);

        // Find the spawn point (make sure you have an object tagged "PlayerSpawn" in your new scene)
        GameObject spawnPoint = GameObject.FindGameObjectWithTag("PlayerSpawn");
        if (spawnPoint != null)
        {
            // Reposition the local player to the spawn point
            GameObject localPlayer = GameObject.FindGameObjectWithTag("Player");
            if (localPlayer != null)
            {
                localPlayer.transform.position = spawnPoint.transform.position;
                UnityEngine.Debug.Log("Player repositioned to spawn point.");
            }
            else
            {
                UnityEngine.Debug.LogWarning("Local player not found!");
            }
        }
        else
        {
            UnityEngine.Debug.LogWarning("PlayerSpawn not found in the scene!");
        }
        // ***** ADDED CODE ENDS HERE *****

        // Step 3: Unload Previous Scenes (Except "Game" and "DontDestroyOnLoad")
        foreach (Scene scene in SceneManager.GetAllScenes())
        {
            if (scene.name != "Game" && scene.name != "DontDestroyOnLoad" && scene.isLoaded)
            {
                UnityEngine.Debug.Log("Unloading previous scene: " + scene.name);
                yield return SceneManager.UnloadSceneAsync(scene.name);
            }
        }

        UnityEngine.Debug.Log("Scene switch complete. Now in: " + nextSceneName);
    }
}
