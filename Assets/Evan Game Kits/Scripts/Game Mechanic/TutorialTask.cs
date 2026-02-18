using UnityEngine;
using UnityEngine.InputSystem;
using System;
using System.Collections.Generic;
using EvanUIKits.Dialogue;
using DG.Tweening;

namespace EvanGameKits.Tutorial
{
    public enum RequirementType { Key, Action }

    [Serializable]
    public class TutorialRequirement
    {
        public RequirementType type;
        public Key key;
        public string actionName;
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

        [Header("UI References")]
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
            
            transform.DOKill();
            transform.localScale = Vector3.zero;
            transform.DOScale(1f, fadeDuration).SetEase(Ease.OutBack).SetUpdate(true);

            if (canvasGroup != null)
            {
                canvasGroup.DOKill();
                canvasGroup.alpha = 0f;
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
                if (req.keyUI != null) 
                {
                    req.keyUI.SetCompleted(false);
                    req.keyUI.SetPressed(false);
                }
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

        public void NotifyAction(string actionName)
        {
            if (!isStarted) return;

            foreach (var req in requirements)
            {
                if (req.type == RequirementType.Action && req.actionName == actionName && !req.isMet)
                {
                    req.isMet = true;
                    if (req.keyUI != null) 
                    {
                        req.keyUI.AnimateActionFeedback();
                        req.keyUI.SetCompleted(true);
                    }
                }
            }
        }

        private void Update()
        {
            if (!isStarted || requirements.Count == 0) return;

            bool allFinished = true;

            foreach (var req in requirements)
            {
                if (req.type == RequirementType.Key)
                {
                    // Check if key is currently being pressed for animation
                    bool isPressed = Keyboard.current[req.key].isPressed;
                    
                    if (isPressed && !req.isMet)
                    {
                        req.isMet = true;
                    }

                    if (req.keyUI != null) req.keyUI.SetPressed(isPressed);
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

            transform.DOKill();
            transform.DOScale(0f, fadeDuration).SetEase(Ease.InBack).SetUpdate(true);

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
