using UnityEngine;
using static System.Net.Mime.MediaTypeNames;

public class ExitGame : MonoBehaviour
{
    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false; // Stop play mode in Unity Editor
#else
        Application.Quit(); // Quit the game in a built version
#endif
    }
}
