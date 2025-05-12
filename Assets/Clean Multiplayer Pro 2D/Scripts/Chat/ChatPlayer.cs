#if CMPSETUP_COMPLETE
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using AvocadoShark;
using StarterAssets;

namespace AvocadoShark
{
    public class ChatPlayer : NetworkBehaviour
    {
        private ChangeDetector _publicChatChangeDetector;
        [Networked] public Chat LastPublicChat { get; set; }



        [Networked] public int WarningCount { get; set; } // Track warnings
        [Networked] public TickTimer BanTimer { get; set; } // Ban timer (Fusion-specific)




        public string playerName;

        [SerializeField] private StarterAssetsInputs _input;
        private PlayerWorldUIManager _playerWorldUIManager;


        private void Awake()
        {
            GetComponent<PlayerStats>().OnPlayerStatsReady += PlayerStatsReady;
            _playerWorldUIManager = GetComponent<PlayerWorldUIManager>();
            _input = GetComponent<StarterAssetsInputs>(); // 🔹 Assign if missing
        }

        public override void Spawned()
        {
            _publicChatChangeDetector = GetChangeDetector(ChangeDetector.Source.SimulationState);
        }

        //🔥 Add warning system (call this when toxicity is detected)
        public void AddWarning()
        {
            WarningCount++;
            if (WarningCount >= 3)
            {
                BanTimer = TickTimer.CreateFromSeconds(Runner, 300); // Ban for 5 mins
                Debug.Log($"{playerName} banned for toxicity!");
            }
        }


        //🔥 Fusion's FixedUpdateNetwork to handle ban expiration
        public override void FixedUpdateNetwork()
        {
            if (BanTimer.Expired(Runner))
            {
                BanTimer = TickTimer.None;
                WarningCount = 0; // Reset after ban
            }
        }


        public override void Render()
        {
            foreach (var change in _publicChatChangeDetector.DetectChanges(this, out var previousBuffer,
                         out var currentBuffer))
            {
                switch (change)
                {
                    case nameof(LastPublicChat):
                        var reader = GetPropertyReader<Chat>(nameof(LastPublicChat));
                        var (previous, current) = reader.Read(previousBuffer, currentBuffer);
                        OnLastPublicChat(previous, current);
                        break;
                }
            }
        }

        public void PlayerStatsReady(string playerName)
        {
            if (Object.HasStateAuthority)
            {
                this.playerName = playerName;
                Debug.Log("PlayerChat" + this.playerName);
                ChatSystem.Instance.SetChatPlayer(this);
            }
        }

        public void SendChat(Chat chat)
        {
            LastPublicChat = chat;
        }

        public void SetPlayerIsWriting(bool value)
        {
            if (value)
            {
                _input.DisablePlayerInput();
            }
            else
            {
                _input.EnablePlayerInput();
            }
        }

        public void OnLastPublicChat(Chat previous, Chat current)
        {
            ChatSystem.Instance.AddChatEntry(!HasStateAuthority, current);
            if (!Object.HasStateAuthority)
                _playerWorldUIManager.QueueChat(current);
        }
    }

    public struct Chat : INetworkStruct
    {
        public NetworkString<_128> Sender, Message;

        public Chat(NetworkString<_128> sender, NetworkString<_128> message)
        {
            Sender = sender;
            Message = message;
        }
    }
}
#endif