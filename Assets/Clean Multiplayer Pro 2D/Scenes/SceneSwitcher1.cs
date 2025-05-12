using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class SceneSwitcher1 : MonoBehaviour
{
    public string nextSceneName; // Assign this in the Inspector

    public void OnButtonPress()
    {
        StartCoroutine(SwitchScene());
    }

    private IEnumerator SwitchScene()
    {
        Debug.Log("Preparing to switch scene...");

        // Step 1: Load the next scene additively
        Debug.Log("Loading next scene: " + nextSceneName);
        AsyncOperation loadOperation = SceneManager.LoadSceneAsync(nextSceneName, LoadSceneMode.Additive);
        while (!loadOperation.isDone)
            yield return null;

        // Step 2: Set the new scene as the active scene
        Scene newScene = SceneManager.GetSceneByName(nextSceneName);
        SceneManager.SetActiveScene(newScene);
        Debug.Log("Active scene set to: " + nextSceneName);

        // Step 3: Unload previous scenes (except "Game" and "DontDestroyOnLoad")
        foreach (Scene scene in SceneManager.GetAllScenes())
        {
            if (scene.name != "Game" && scene.name != "DontDestroyOnLoad" && scene.isLoaded)
            {
                Debug.Log("Unloading previous scene: " + scene.name);
                yield return SceneManager.UnloadSceneAsync(scene.name);
            }
        }

        // Step 4: Move the player to the spawn point
        GameObject spawnPoint = GameObject.FindGameObjectWithTag("PlayerSpawn");
        if (spawnPoint != null)
        {
            GameObject localPlayer = GameObject.FindGameObjectWithTag("Player");
            if (localPlayer != null)
            {
                localPlayer.transform.position = spawnPoint.transform.position;
                Debug.Log("Player repositioned to spawn point.");
            }
            else
            {
                Debug.LogWarning("Local player not found!");
            }
        }
        else
        {
            Debug.LogWarning("PlayerSpawn not found in the scene!");
        }

        Debug.Log("Scene switch complete. Now in: " + nextSceneName);
    }
}
