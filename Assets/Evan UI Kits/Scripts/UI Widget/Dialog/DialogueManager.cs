using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;
using UnityEngine.EventSystems;
using EvanUIKits.Tweening;

namespace EvanUIKits.Dialogue
{
    public class DialogueManager : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        public static DialogueManager instance;

        [Header("UI References")]
        [HideInInspector] public string dialogueKey;
        public TextMeshProUGUI nameText;
        public TextMeshProUGUI dialogueText;
        public Image portraitImage;
        public GameObject visualContainer;
        private CanvasGroup canvasGroup;

        [Header("Settings")]
        public float typingSpeed = 0.05f;
        public AnimateButton.AnimationType animationType = AnimateButton.AnimationType.Scale;

        private Queue<SentenceData> sentenceQueue = new Queue<SentenceData>();
        private SentenceData currentSentenceData;
        private DialogueEntry currentEntry;
        private bool isTyping = false;
        private Action onCompleteCallback;
        private RectTransform containerRect;

        [HideInInspector] public int selectedKeyIndex;

        private void Awake()
        {
            if (instance == null) instance = this;
            else Destroy(gameObject);

            if (visualContainer != null)
            {
                containerRect = visualContainer.GetComponent<RectTransform>();
                canvasGroup = visualContainer.GetComponent<CanvasGroup>();
                if (canvasGroup == null) canvasGroup = visualContainer.AddComponent<CanvasGroup>();
                
                visualContainer.SetActive(true);
                ToggleUI(false);
            }
        }

        private void ToggleUI(bool show)
        {
            if (canvasGroup != null)
            {
                canvasGroup.alpha = show ? 1 : 0;
                canvasGroup.interactable = show;
                canvasGroup.blocksRaycasts = show;
            }
        }

        private void OnEnable()
        {
            if (DialogueDatabase.Instance != null && !string.IsNullOrEmpty(dialogueKey))
            {
                var entry = DialogueDatabase.Instance.GetDialogue(dialogueKey);
                if (entry != null) StartDialogue(entry);
            }
        }

        public void StartDialogue(DialogueEntry entry, Action callback = null)
        {
            if (entry.isOneTime && entry.hasBeenPlayed) return;
            
            Time.timeScale = 0;
            entry.hasBeenPlayed = true;
            currentEntry = entry;
            onCompleteCallback = callback;
            ToggleUI(true);

            sentenceQueue.Clear();
            foreach (var data in entry.sentences)
            {
                sentenceQueue.Enqueue(data);
            }

            DisplayNextSentence();
        }

        public void PlayDialogue(string key)
        {
            if (DialogueDatabase.Instance != null && !string.IsNullOrEmpty(key))
            {
                var entry = DialogueDatabase.Instance.GetDialogue(key);
                if (entry != null)
                {
                    if (entry.isOneTime && entry.hasBeenPlayed) return;
                    StartDialogue(entry);
                }
            }
        }

        public void DisplayNextSentence()
        {
            if (isTyping)
            {
                CompleteSentenceInstantly();
                return;
            }

            currentSentenceData?.OnSentenceEnd?.Invoke();

            if (sentenceQueue.Count == 0)
            {
                EndDialogue();
                return;
            }

            currentSentenceData = sentenceQueue.Dequeue();
            
            UpdateCharacterUI();

            currentSentenceData.OnSentenceStart?.Invoke();
            StartCoroutine(TypeSentence(currentSentenceData.text));
        }

        private void UpdateCharacterUI()
        {
            if (currentEntry == null || currentSentenceData == null) return;

            if (currentSentenceData.useCharacter2)
            {
                if (nameText != null) nameText.text = currentEntry.characterName2;
                if (portraitImage != null)
                {
                    portraitImage.sprite = currentEntry.portrait2;
                    portraitImage.gameObject.SetActive(currentEntry.portrait2 != null);
                }
            }
            else
            {
                if (nameText != null) nameText.text = currentEntry.characterName;
                if (portraitImage != null)
                {
                    portraitImage.sprite = currentEntry.portrait;
                    portraitImage.gameObject.SetActive(currentEntry.portrait != null);
                }
            }
        }

        private IEnumerator TypeSentence(string sentence)
        {
            isTyping = true;
            if (dialogueText != null) dialogueText.text = "";

            foreach (char letter in sentence.ToCharArray())
            {
                if (dialogueText != null) dialogueText.text += letter;
                yield return new WaitForSecondsRealtime(typingSpeed);
            }

            isTyping = false;
        }

        private void CompleteSentenceInstantly()
        {
            StopAllCoroutines();
            if (dialogueText != null && currentSentenceData != null) dialogueText.text = currentSentenceData.text;
            isTyping = false;
        }

        private void EndDialogue()
        {
            Time.timeScale = 1;
            ToggleUI(false);
            onCompleteCallback?.Invoke();
            currentSentenceData = null;
            currentEntry = null;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (containerRect != null) AnimateButton.OnButtonDown(containerRect, type: animationType, ignoreTimeScale: true);    
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (containerRect != null)
            {
                AnimateButton.OnButtonUp(containerRect, type: animationType, ignoreTimeScale: true, onComplete: () => {
                    DisplayNextSentence();
                });
            }
            else
            {
                DisplayNextSentence();
            }
        }
    }
}
