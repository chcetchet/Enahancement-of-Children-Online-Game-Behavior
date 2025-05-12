using UnityEngine;
using UnityEngine.SceneManagement;

public class PersistentSceneManager : MonoBehaviour
{
    private static bool isInitialized = false;

    private void Awake()
    {
        if (!isInitialized)
        {
            isInitialized = true;
            DontDestroyOnLoad(gameObject);

            // ✅ Move all child objects to DontDestroyOnLoad
            foreach (Transform child in transform)
            {
                DontDestroyOnLoad(child.gameObject);
            }

            Debug.Log("[PersistentSceneManager] Persistent Scene initialized and moved to DontDestroyOnLoad.");
        }
        else
        {
            Destroy(gameObject); // Prevent duplicates
        }
    }
}
