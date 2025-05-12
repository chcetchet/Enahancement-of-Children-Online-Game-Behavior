using UnityEngine;
using Firebase.Auth;
using Firebase.Extensions; // ✅ ADD THIS LINE
using TMPro;
using UnityEngine.SceneManagement;

public class ParentAuth : MonoBehaviour
{
    public GameObject parentPanel;
    public TMP_InputField passwordInput;
    public TextMeshProUGUI errorText;
    public string sceneToLoad; // Scene name from Inspector

    private FirebaseAuth auth;

    void Start()
    {
        auth = FirebaseAuth.DefaultInstance;
        parentPanel.SetActive(false);
    }

    public void ShowParentPanel()
    {
        parentPanel.SetActive(true);
    }

    public void CheckPassword()
    {
        string password = passwordInput.text;
        FirebaseUser user = auth.CurrentUser;

        var credential = EmailAuthProvider.GetCredential(user.Email, password);

        // ✅ FIX: use ContinueWithOnMainThread so we can call Unity functions like LoadScene
        user.ReauthenticateAsync(credential).ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted && !task.IsFaulted && !task.IsCanceled)
            {
                Debug.Log("✅ Parent verified.");
                SceneManager.LoadScene(sceneToLoad); // Use Inspector value
            }
            else
            {
                Debug.Log("❌ Wrong password.");
                ShowError("Incorrect password. Try again.");
            }
        });
    }

    void ShowError(string msg)
    {
        if (errorText != null)
        {
            errorText.text = msg;
        }
    }

    public void CloseParentPanel()
    {
        parentPanel.SetActive(false); // Hide the password input panel
    }

}


