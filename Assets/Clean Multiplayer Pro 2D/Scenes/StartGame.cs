using UnityEngine;
using UnityEngine.SceneManagement;

public class StartGame : MonoBehaviour
{
    public void OnPlayClicked()
    {
        SceneManager.LoadScene("Menu");
    }
}
