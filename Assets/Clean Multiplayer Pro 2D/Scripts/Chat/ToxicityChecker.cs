using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using System.Text;
using Newtonsoft.Json.Linq;
using System;
using System.Threading.Tasks;
using AvocadoShark;

public class ToxicityChecker : MonoBehaviour
{
    [SerializeField] private string apiKey = "AIzaSyB-b_tbsu7d9AacqEUSvc2GvmO9qTH4F7w";
    private ChatPlayer _chatPlayer; // 🔹 Reference to the player for warnings

    // 🔹 Initialize with the player reference (call this from ChatSystem when setting up)
    public void Initialize(ChatPlayer chatPlayer)
    {
        _chatPlayer = chatPlayer;
    }


    public void CheckMessage(string message, string playerId, Action<bool> callback)
    {
        StartCoroutine(SendToPerspectiveAPI(message, callback));
    }

    // internal IEnumerator IsToxic(string trimmedText, Action<bool> callback)
    // {
    //     bool isToxic = false;
    //     // Use the existing CheckMessage to get the result asynchronously
    //     CheckMessage(trimmedText, "playerId", (result) =>
    //     {
    //         isToxic = result;
    //         callback(isToxic); // Call the callback with the result
    //     });

    //     // Wait until the result is received
    //     yield return null;
    // }
    internal IEnumerator IsToxic(string trimmedText, Action<bool> callback)
    {
        bool isToxic = false;
        CheckMessage(trimmedText, "playerId", (result) =>
        {
            isToxic = result;
            if (isToxic)
            {
                // Step 1: Add warning
                _chatPlayer?.AddWarning();
                
                // Step 2: Replace toxic words
                trimmedText = ToxicWordFilter.FilterMessage(trimmedText);
                
                // Step 3: Send filtered message
                var newChat = new Chat(_chatPlayer.playerName, trimmedText);
                _chatPlayer?.SendChat(newChat);
                
                // Step 4: Notify player
                ChatSystem.Instance.AddChatEntry(true, new Chat("System", $"⚠️ Be kind! Warnings: {_chatPlayer.WarningCount}/3"));
            }
            callback(isToxic);
        });
        yield return null;
    }



    IEnumerator SendToPerspectiveAPI(string message, Action<bool> callback)
    {
        string url = "https://commentanalyzer.googleapis.com/v1alpha1/comments:analyze?key=" + apiKey;

        JObject requestBody = new JObject
        {
            { "comment", new JObject { { "text", message } } },
            { "languages", new JArray { "en" } },
            { "requestedAttributes", new JObject { { "TOXICITY", new JObject() } } }
        };

        byte[] bodyRaw = Encoding.UTF8.GetBytes(requestBody.ToString());

        UnityWebRequest request = new UnityWebRequest(url, "POST");
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("API Error: " + request.error);
            callback(false); // fallback to allow
        }
        else
        {
            var response = JObject.Parse(request.downloadHandler.text);
            float score = (float)response["attributeScores"]["TOXICITY"]["summaryScore"]["value"];
            Debug.Log($"Toxicity score: {score}");

            // Consider toxic if score >= 0.6
            callback(score >= 0.2f);
        }
    }
}