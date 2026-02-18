using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;

namespace EvanGameKits.Tutorial
{
    public class TutorialKeyUI : MonoBehaviour
    {
        [Header("Animation Settings")]
        [SerializeField] private float pressedScale = 0.85f;
        [SerializeField] private float animationDuration = 0.1f;
        
        [Header("Completed State Visuals")]
        [SerializeField] private Color completedColor = Color.green;
        [SerializeField] private Graphic targetGraphic;
        [SerializeField] private GameObject checkmarkIcon;

        private bool isCurrentlyPressed = false;
        private bool isCompleted = false;
        private Vector3 originalScale;
        private Color originalColor;

        private void Awake()
        {
            originalScale = transform.localScale;
            if (targetGraphic != null) originalColor = targetGraphic.color;
            if (checkmarkIcon != null) checkmarkIcon.SetActive(false);
        }

        public void SetPressed(bool pressed)
        {
            if (pressed == isCurrentlyPressed) return;
            
            isCurrentlyPressed = pressed;
            transform.DOKill();
            
            if (pressed)
            {
                transform.DOScale(originalScale * pressedScale, animationDuration).SetUpdate(true);
            }
            else
            {
                transform.DOScale(originalScale, animationDuration).SetUpdate(true);
            }
        }

        public void AnimateActionFeedback()
        {
            transform.DOKill(true);
            transform.DOPunchScale(Vector3.one * 0.2f, 0.3f, 5, 1).SetUpdate(true);
        }

        public void SetCompleted(bool completed)
        {
            if (isCompleted == completed) return;
            isCompleted = completed;

            if (isCompleted)
            {
                if (targetGraphic != null) targetGraphic.DOColor(completedColor, 0.3f).SetUpdate(true);
                if (checkmarkIcon != null) checkmarkIcon.SetActive(true);
            }
            else
            {
                if (targetGraphic != null) targetGraphic.DOColor(originalColor, 0.3f).SetUpdate(true);
                if (checkmarkIcon != null) checkmarkIcon.SetActive(false);
            }
        }
    }
}
