using UnityEngine;
using Firebase;
using Firebase.Auth;
using Firebase.Firestore;
using System.Collections;
using TMPro;
using UnityEngine.SceneManagement;
using System;

public class FirebaseManager : MonoBehaviour
{
    public static FirebaseManager Instance;
    private FirebaseAuth auth;
    private FirebaseUser user;
    private FirebaseFirestore firestore;

    [Header("UI Elements")]
    public TMP_InputField emailInput;
    public TMP_InputField passwordInput;
    public TMP_Text messageText;

    [Header("Scene Settings")]
    public string nextSceneName = "Menu"; // Change to your actual scene name

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task =>
        {
            FirebaseApp app = FirebaseApp.DefaultInstance;
            auth = FirebaseAuth.DefaultInstance;
            firestore = FirebaseFirestore.DefaultInstance;
        });
    }

    public void SignUp()
    {
        string email = emailInput.text;
        string password = passwordInput.text;

        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
        {
            messageText.text = "Please enter an email and password.";
            return;
        }

        StartCoroutine(CheckPlaytimeBeforeSignUp(email, password));
    }

    private IEnumerator CheckPlaytimeBeforeSignUp(string email, string password)
    {
        var signUpTask = auth.CreateUserWithEmailAndPasswordAsync(email, password);
        yield return new WaitUntil(() => signUpTask.IsCompleted);

        if (signUpTask.Exception != null)
        {
            messageText.text = "Sign-Up Failed: " + signUpTask.Exception.Message;
            yield break;
        }

        user = signUpTask.Result.User;
        StartCoroutine(CheckPlaytimeBeforeProceeding());
    }

    public void SignIn()
    {
        string email = emailInput.text;
        string password = passwordInput.text;

        StartCoroutine(SignInCoroutine(email, password));
    }

    private IEnumerator SignInCoroutine(string email, string password)
    {
        var signInTask = auth.SignInWithEmailAndPasswordAsync(email, password);
        yield return new WaitUntil(() => signInTask.IsCompleted);

        if (signInTask.Exception != null)
        {
            messageText.text = "Sign-In Failed: " + signInTask.Exception.Message;
            yield break;
        }

        user = signInTask.Result.User;
        StartCoroutine(CheckPlaytimeBeforeProceeding());
    }

    private IEnumerator CheckPlaytimeBeforeProceeding()
    {
        DocumentReference docRef = firestore.Collection("ParentalControls").Document(user.UserId);
        var getTask = docRef.GetSnapshotAsync();

        yield return new WaitUntil(() => getTask.IsCompleted);

        if (getTask.Result.Exists)
        {
            var data = getTask.Result;

            if (data.ContainsField("LastPlayTime"))
            {
                long lastPlayTimestamp = data.GetValue<long>("LastPlayTime");
                DateTime lastPlayTime = DateTimeOffset.FromUnixTimeSeconds(lastPlayTimestamp).UtcDateTime;

                if ((DateTime.UtcNow - lastPlayTime).TotalHours < 24)
                {
                    DateTime nextAllowedTime = lastPlayTime.AddHours(24);
                    messageText.text = $"Playtime exceeded! Try again after {nextAllowedTime.ToLocalTime():hh:mm tt}.";
                    auth.SignOut();
                    yield break;
                }
            }
        }

        messageText.text = "Login Successful!";
        yield return new WaitForSeconds(1);
        SceneManager.LoadScene(nextSceneName); // Load next scene
    }

    public void SignOut()
    {
        auth.SignOut();
        messageText.text = "Signed out.";
    }
}