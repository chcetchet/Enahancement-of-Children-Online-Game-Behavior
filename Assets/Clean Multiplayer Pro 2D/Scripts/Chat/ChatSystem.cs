#if CMPSETUP_COMPLETE
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static Unity.Collections.Unicode;
using Fusion;

namespace AvocadoShark
{
    public class ChatSystem : MonoBehaviour
    {
        [SerializeField] private ChatItem chatPrefab;
        [SerializeField] private GameObject chatContainer;
        [SerializeField] private TMP_InputField chatInput;
        [SerializeField] private TextMeshProUGUI chatLimitDisplay;
        [SerializeField] private ScrollRect scrollRect;
        public static ChatSystem Instance = null;
        private ChatPlayer _chatPlayer;
        private NetworkRunner _runner;


        private bool _hasUserScroll;

        // 🔹 Add a reference to the ToxicityChecker
        private ToxicityChecker toxicityChecker;

        [Obsolete]
        private void Awake()
        {
            Instance = this;
            chatInput.onFocusSelectAll = false;
            scrollRect.onValueChanged.AddListener((x) =>
            {
                _hasUserScroll = true;
            });

            // 🔹 Find the ToxicityChecker in the scene
            toxicityChecker = FindObjectOfType<ToxicityChecker>();
            _runner = FindObjectOfType<NetworkRunner>();
        }

        private void OnEnable()
        {
            chatInput.onSelect.AddListener(InputInFocus);
            chatInput.onDeselect.AddListener(InputLostFocus);
            chatInput.onSubmit.AddListener(InputSubmit);
            chatInput.onValueChanged.AddListener(CharacterCountUpdate);
        }

        private void OnDisable()
        {
            chatInput.onSelect.RemoveListener(InputInFocus);
            chatInput.onDeselect.RemoveListener(InputLostFocus);
            chatInput.onSubmit.RemoveListener(InputSubmit);
            chatInput.onValueChanged.RemoveListener(CharacterCountUpdate);
        }

        public void SetChatPlayer(ChatPlayer chatPlayer)
        {
            _chatPlayer = chatPlayer;
            // 🔥 Add this line to initialize ToxicityChecker with the player
            toxicityChecker?.Initialize(_chatPlayer); 
        }

        public void AddChatEntry(bool isLeft, Chat chat)
        {
            var go = Instantiate(chatPrefab, chatContainer.transform);
            go.Init(isLeft, chat);
            if (_hasUserScroll && !(scrollRect.verticalNormalizedPosition <= 0.1f))
                return;
            Canvas.ForceUpdateCanvases();
            scrollRect.verticalNormalizedPosition = 0f;
        }

        public void InputInFocus(string text)
        {
            chatInput.MoveTextEnd(false);
            _chatPlayer.SetPlayerIsWriting(true);
        }

        public void InputLostFocus(string text)
        {
            _chatPlayer.SetPlayerIsWriting(false);
        }


        // 
        public void InputSubmit(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return;

            chatInput.ActivateInputField();
            var trimmedText = text.Trim();
            chatInput.text = "";

            // Check for toxicity asynchronously
            StartCoroutine(CheckToxicityAndSendMessage(trimmedText));
        }

        private IEnumerator CheckToxicityAndSendMessage(string trimmedText)
        {
            // 🔒 Prevent chat if player is banned
            if (_chatPlayer.BanTimer.IsRunning && !_chatPlayer.BanTimer.Expired(_runner))
            {
                Debug.LogWarning("Player is banned from chatting.");
                ChatSystem.Instance.AddChatEntry(true, new Chat("System", $"🚫 Chat disabled due to repeated toxic behavior. Try again later."));
                yield break;
            }

            // Use the IsToxic method asynchronously
            yield return StartCoroutine(toxicityChecker.IsToxic(trimmedText, (isToxic) =>
            {
                if (isToxic)
                {
                    Debug.LogWarning("Toxic message blocked.");
                    AddChatEntry(true, new Chat("System", "⚠️ Message blocked due to inappropriate language."));
                }
                else
                {
                    var newChat = new Chat(_chatPlayer.playerName, trimmedText);
                    _chatPlayer.SendChat(newChat);
                }
            }));
        }




        public void CharacterCountUpdate(string text)
        {
            chatLimitDisplay.text = text.Count() == chatInput.characterLimit
                ? $"<color=#D96222>{text.Count()}/{chatInput.characterLimit}</color>"
                : $"{text.Count()}/{chatInput.characterLimit}";
        }
    }
}
#endif