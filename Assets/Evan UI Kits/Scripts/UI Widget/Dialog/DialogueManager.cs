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

        [Header("Settings")]
        public float typingSpeed = 0.05f;
        public AnimateButton.AnimationType animationType = AnimateButton.AnimationType.Scale;

        private Queue<SentenceData> sentenceQueue = new Queue<SentenceData>();
        private SentenceData currentSentenceData;
        private bool isTyping = false;
        private Action onCompleteCallback;
        private RectTransform containerRect;

        [HideInInspector] public int selectedKeyIndex;

        private void Awake()
        {
            if (instance == null) instance = this;
            else Destroy(gameObject);

            if (visualContainer != null) containerRect = visualContainer.GetComponent<RectTransform>();
            visualContainer.SetActive(false);
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
            onCompleteCallback = callback;
            visualContainer.SetActive(true);

            nameText.text = entry.characterName;

            if (portraitImage != null)
            {
                portraitImage.sprite = entry.portrait;
                portraitImage.gameObject.SetActive(entry.portrait != null);
            }

            sentenceQueue.Clear();
            foreach (var data in entry.sentences)
            {
                sentenceQueue.Enqueue(data);
            }

            DisplayNextSentence();
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
            currentSentenceData.OnSentenceStart?.Invoke();

            StartCoroutine(TypeSentence(currentSentenceData.text));
        }
        private IEnumerator TypeSentence(string sentence)
        {
            isTyping = true;
            dialogueText.text = "";

            foreach (char letter in sentence.ToCharArray())
            {
                dialogueText.text += letter;
                yield return new WaitForSeconds(typingSpeed);
            }

            isTyping = false;
        }

        private void CompleteSentenceInstantly()
        {
            StopAllCoroutines();
            dialogueText.text = currentSentenceData.text;
            isTyping = false;
        }

        private void EndDialogue()
        {
            visualContainer.SetActive(false);
            onCompleteCallback?.Invoke();
            currentSentenceData = null;
        }

        public void OnPointerDown(PointerEventData eventData) 
        {
            if (containerRect != null) AnimateButton.OnButtonDown(containerRect, type: animationType);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (containerRect != null) 
            {
                AnimateButton.OnButtonUp(containerRect, type: animationType, onComplete: () => {
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