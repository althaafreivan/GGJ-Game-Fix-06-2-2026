using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;
using UnityEngine.EventSystems;
using EvanUIKits.Tweening;
using EvanUIKits.Audio;
using EvanGameKits.Mechanic;

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
        public string tutorialLockedMessage = "Please complete the tutorial before proceeding!";

        private Queue<SentenceData> sentenceQueue = new Queue<SentenceData>();
        private SentenceData currentSentenceData;
        private DialogueEntry currentEntry;
        private bool isTyping = false;
        private bool isDialogueActive = false;
        private Action onCompleteCallback;
        private RectTransform containerRect;

        public ITutorialTask activeTutorial { get; private set; }

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

        private void Update()
        {
            if (isDialogueActive && Input.GetMouseButtonUp(0))
            {
                DisplayNextSentence();
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

            // Deselect any currently selected UI object to prevent accidental triggers (like space/enter)
            if (EventSystem.current != null) EventSystem.current.SetSelectedGameObject(null);

            entry.hasBeenPlayed = true;
            currentEntry = entry;
            onCompleteCallback = callback;
            isDialogueActive = true;
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

        public void SetActiveTutorial(ITutorialTask tutorial)
        {
            activeTutorial = tutorial;
        }

        public void ClearActiveTutorial()
        {
            activeTutorial?.StopTutorial();
            activeTutorial = null;
        }

        public void DisplayNextSentence()
        {
            if (isTyping)
            {
                CompleteSentenceInstantly();
                return;
            }

            if (activeTutorial != null && !activeTutorial.IsCompleted())
            {
                if (NotificationController.instance != null)
                {
                    NotificationController.instance.ShowNotification(tutorialLockedMessage);
                }
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
                
                if (AudioManager.instance != null && !char.IsWhiteSpace(letter))
                {
                    AudioManager.instance.PlaySFX("Typing");
                }
                
                yield return new WaitForSeconds(typingSpeed);
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
            isDialogueActive = false;
            ToggleUI(false);
            onCompleteCallback?.Invoke();
            currentSentenceData = null;
            currentEntry = null;
            ClearActiveTutorial();
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (isDialogueActive && containerRect != null) AnimateButton.OnButtonDown(containerRect, type: animationType, ignoreTimeScale: true);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (!isDialogueActive) return;

            if (AudioManager.instance != null)
            {
                AudioManager.instance.PlaySFX("Button");
            }

            if (containerRect != null)
            {
                AnimateButton.OnButtonUp(containerRect, type: animationType, ignoreTimeScale: true, onComplete: () => {
                    // Click advanced the dialogue via Update, so we don't necessarily need to call it again here 
                    // unless we want clicking the box specifically to advance as well (which Update already handles).
                });
            }
        }
    }
}
