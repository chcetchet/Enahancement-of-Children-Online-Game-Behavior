using UnityEngine;
using Firebase;
using Firebase.Auth;
using Firebase.Firestore;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

public class ParentalControlsManager : MonoBehaviour
{
    private FirebaseAuth auth;
    private FirebaseFirestore firestore;
    private FirebaseUser user;

    [Header("UI Elements")]
    public TMP_InputField timeLimitInput;
    public TMP_InputField allowedFriendsInput;
    public TMP_Text messageText;

    [Header("Next Scene")]
    public string nextSceneName = "Menu";

    void Start()
    {
        auth = FirebaseAuth.DefaultInstance;
        firestore = FirebaseFirestore.DefaultInstance;
        user = auth.CurrentUser;

        if (user == null)
        {
            messageText.text = "User not logged in!";
        }
    }

    public void SaveParentalControls()
    {
        UnityEngine.Debug.Log("SaveParentalControls method triggered!");

        if (user == null)
        {
            messageText.text = "Error: No user logged in.";
            UnityEngine.Debug.LogError("No user logged in.");
            return;
        }

        string timeLimit = timeLimitInput.text;
        string allowedFriends = allowedFriendsInput.text;

        if (string.IsNullOrEmpty(timeLimit) || string.IsNullOrEmpty(allowedFriends))
        {
            messageText.text = "Please fill out both fields.";
            UnityEngine.Debug.LogWarning("One or more fields are empty.");
            return;
        }

        if (int.TryParse(timeLimit, out int timeInMinutes))
        {
            if (timeInMinutes > 60)
            {
                messageText.text = "Time limit cannot be more than 60 minutes.";
                UnityEngine.Debug.LogWarning("Time limit exceeds 60 minutes.");
                return;
            }
        }
        else
        {
            messageText.text = "Time limit must be a number.";
            UnityEngine.Debug.LogWarning("Time limit is not a valid number.");
            return;
        }


        UnityEngine.Debug.Log("Saving data... Time Limit: " + timeLimit + ", Allowed Friends: " + allowedFriends);

        StartCoroutine(SaveParentalControlsCoroutine(timeLimit, allowedFriends));
    }

    private IEnumerator SaveParentalControlsCoroutine(string timeLimit, string allowedFriends)
    {
        Dictionary<string, object> parentalSettings = new Dictionary<string, object>
        {
            { "TimeLimit", timeLimit },
            { "AllowedFriends", allowedFriends },
            { "UserID", user.UserId }
        };

        var saveTask = firestore.Collection("ParentalControls").Document(user.UserId).SetAsync(parentalSettings);
        yield return new WaitUntil(() => saveTask.IsCompleted);

        if (saveTask.Exception != null)
        {
            messageText.text = "Error saving parental controls.";
            UnityEngine.Debug.LogError("Error saving parental controls: " + saveTask.Exception);
            yield break;
        }

        messageText.text = "Parental controls saved!";
        UnityEngine.Debug.Log("Parental controls saved successfully.");

        yield return new WaitForSeconds(1); // Small delay before switching scene
        SceneManager.LoadScene(nextSceneName);
    }
}


