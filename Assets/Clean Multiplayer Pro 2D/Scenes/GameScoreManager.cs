using UnityEngine;
using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using System;
using System.Diagnostics;

public class GameScoreManager : NetworkBehaviour
{
    public static GameScoreManager Instance { get; private set; }

    private Dictionary<PlayerRef, int> playerScores = new Dictionary<PlayerRef, int>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            // Move the object to the Persistent Scene to prevent destruction
            SceneManager.MoveGameObjectToScene(gameObject, SceneManager.GetSceneByName("PersistentScene"));

            UnityEngine.Debug.Log("[GameScoreManager] Instance created and set to persist.");
        }
        else
        {
            Destroy(gameObject); // Prevent duplicates
        }
    }

    private void Start()
    {
        StartCoroutine(InitializeAfterNetworkRunner());
    }

    private IEnumerator InitializeAfterNetworkRunner()
    {
        // ✅ Wait until the NetworkRunner is fully running
        while (Runner == null || !Runner.IsRunning)
        {
            yield return null;
        }

        if (Runner.IsServer && (!Object || !Object.IsValid))
        {
            Runner.Spawn(gameObject);
            UnityEngine.Debug.Log("[GameScoreManager] Spawned in the network.");
        }
        else
        {
            UnityEngine.Debug.Log("[GameScoreManager] Successfully initialized in the network.");
        }
    }

    public void AddPoints(PlayerRef player, int points)
    {
        // ✅ Ensure the object is valid before proceeding
        if (Runner == null || !Runner.IsRunning || Object == null || !Object.IsValid)
        {
            UnityEngine.Debug.LogError("[GameScoreManager] ERROR: Trying to add points before GameScoreManager is fully initialized.");
            StartCoroutine(WaitForInitializationAndAddPoints(player, points));
            return;
        }

        if (!playerScores.ContainsKey(player))
        {
            playerScores[player] = 0;
        }

        playerScores[player] += points;
        UnityEngine.Debug.Log($"[GameScoreManager] Player {player.PlayerId} Score: {playerScores[player]}");

        RPC_UpdateScore(player, playerScores[player]);
    }

    private IEnumerator WaitForInitializationAndAddPoints(PlayerRef player, int points)
    {
        // ✅ Wait until Fusion is fully initialized
        while (Runner == null || !Runner.IsRunning || Object == null || !Object.IsValid)
        {
            yield return null;
        }

        AddPoints(player, points); // ✅ Retry adding points after initialization
    }

    public int GetPlayerScore(PlayerRef player)
    {
        return playerScores.ContainsKey(player) ? playerScores[player] : 0;
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_UpdateScore(PlayerRef player, int newScore)
    {
        playerScores[player] = newScore;
    }
}



