using UnityEngine;
using UnityEngine.InputSystem;
using System;
using System.Collections.Generic;
using EvanUIKits.Dialogue;
using DG.Tweening;

namespace EvanGameKits.Tutorial
{
    [Serializable]
    public class TutorialRequirement
    {
        public Key key;
        public TutorialKeyUI keyUI;
        [HideInInspector] public bool isMet = false;
    }

    public class TutorialTask : MonoBehaviour, ITutorialTask
    {
        [Header("Requirements")]
        public List<TutorialRequirement> requirements = new List<TutorialRequirement>();
        
        [Header("Settings")]
        public bool autoStart = false;
        public bool deactivateOnComplete = false;
        public float fadeDuration = 0.5f;

        [Header("References")]
        [SerializeField] private CanvasGroup canvasGroup;

        private bool isStarted = false;

        private void Awake()
        {
            if (canvasGroup == null) canvasGroup = GetComponent<CanvasGroup>();
        }

        private void Start()
        {
            if (autoStart) StartTutorial();
            else 
            {
                if (canvasGroup != null) canvasGroup.alpha = 0;
                gameObject.SetActive(false);
            }
        }

        public void StartTutorial()
        {
            isStarted = true;
            gameObject.SetActive(true);
            
            if (canvasGroup != null)
            {
                canvasGroup.DOKill();
                canvasGroup.DOFade(1f, fadeDuration).SetUpdate(true);
            }

            if (DialogueManager.instance != null)
            {
                DialogueManager.instance.SetActiveTutorial(this);
            }
            
            // Reset state
            foreach (var req in requirements)
            {
                req.isMet = false;
                if (req.keyUI != null) req.keyUI.SetCompleted(false);
            }
        }

        public bool IsCompleted()
        {
            foreach (var req in requirements)
            {
                if (!req.isMet) return false;
            }
            return true;
        }

        private void Update()
        {
            if (!isStarted) return;

            bool allFinished = true;
            foreach (var req in requirements)
            {
                // Check if key is currently being pressed for animation
                if (Keyboard.current[req.key].isPressed)
                {
                    req.isMet = true; // Mark as met if pressed at least once
                    if (req.keyUI != null) req.keyUI.SetPressed(true);
                }
                else
                {
                    if (req.keyUI != null) req.keyUI.SetPressed(false);
                }
                
                // Update the completed visual state
                if (req.keyUI != null) req.keyUI.SetCompleted(req.isMet);
                
                if (!req.isMet) allFinished = false;
            }

            if (allFinished)
            {
                StopTutorial();
            }
        }

        public void StopTutorial()
        {
            if (!isStarted) return;
            isStarted = false;

            if (DialogueManager.instance != null)
            {
                DialogueManager.instance.ClearActiveTutorial();
            }

            if (canvasGroup != null)
            {
                canvasGroup.DOKill();
                canvasGroup.DOFade(0f, fadeDuration).SetUpdate(true).OnComplete(() =>
                {
                    if (deactivateOnComplete) gameObject.SetActive(false);
                });
            }
            else if (deactivateOnComplete)
            {
                gameObject.SetActive(false);
            }
        }
    }
}
