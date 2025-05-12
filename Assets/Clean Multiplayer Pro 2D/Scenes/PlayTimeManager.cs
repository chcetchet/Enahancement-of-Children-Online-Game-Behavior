using UnityEngine;
using Firebase.Auth;
using Firebase.Firestore;
using UnityEngine.SceneManagement;
using System;
using System.Collections;
using System.Diagnostics;

public class PlayTimeManager : MonoBehaviour
{
    private FirebaseAuth auth;
    private FirebaseFirestore firestore;
    private FirebaseUser user;

    private int playTimeLimit = 30; // Default playtime in minutes
    private float timeRemaining;
    private DateTime lastPlayTime;
    private bool isBanned;

    void Start()
    {
        auth = FirebaseAuth.DefaultInstance;
        firestore = FirebaseFirestore.DefaultInstance;
        user = auth.CurrentUser;

        if (user == null)
        {
            UnityEngine.Debug.Log("No user logged in! Redirecting to login screen...");
            SceneManager.LoadScene("Authentication");
            return;
        }

        StartCoroutine(CheckPlayTimeRestrictions());
    }

    IEnumerator CheckPlayTimeRestrictions()
    {
        DocumentReference docRef = firestore.Collection("ParentalControls").Document(user.UserId);
        var getTask = docRef.GetSnapshotAsync();

        yield return new WaitUntil(() => getTask.IsCompleted);

        if (getTask.Result.Exists)
        {
            var data = getTask.Result;
            playTimeLimit = int.Parse(data.GetValue<string>("TimeLimit")); // Get parental control time limit

            // Check if LastPlayTime exists
            if (data.ContainsField("LastPlayTime"))
            {
                long lastPlayTimestamp = data.GetValue<long>("LastPlayTime");
                lastPlayTime = DateTimeOffset.FromUnixTimeSeconds(lastPlayTimestamp).UtcDateTime;

                // 🚨 *Check if 24 hours have passed*
                if ((DateTime.UtcNow - lastPlayTime).TotalHours < 24)
                {
                    isBanned = true;
                    UnityEngine.Debug.Log("🚨 Player is banned! They must wait until: " + lastPlayTime.AddHours(24));
                    SceneManager.LoadScene("Menu"); // Prevent player from playing
                    yield break;
                }
            }
        }
        else
        {
            UnityEngine.Debug.LogWarning("No parental control settings found. Using default time limit.");
        }

        timeRemaining = playTimeLimit * 60; // Convert minutes to seconds
        StartCoroutine(StartTimer());
    }

    IEnumerator StartTimer()
    {
        while (timeRemaining > 0)
        {
            yield return new WaitForSeconds(1);
            timeRemaining--;

            if (timeRemaining % 60 == 0) // Log every minute
            {
                UnityEngine.Debug.Log("Time left: " + (timeRemaining / 60) + " minutes");
            }
        }

        UnityEngine.Debug.Log("⏳ Time limit reached! Saving restriction...");
        SaveLastPlayTime();
        SceneManager.LoadScene("TimeLimit"); // Send player back to menu
    }

    void SaveLastPlayTime()
    {
        long currentTime = ((DateTimeOffset)DateTime.UtcNow).ToUnixTimeSeconds();

        DocumentReference docRef = firestore.Collection("ParentalControls").Document(user.UserId);
        docRef.UpdateAsync("LastPlayTime", currentTime).ContinueWith(task =>
        {
            if (task.IsCompleted)
            {
                UnityEngine.Debug.Log("✅ Last play time saved successfully.");
            }
            else
            {
                UnityEngine.Debug.LogError("❌ Failed to save last play time: " + task.Exception);
            }
        });
    }
}
