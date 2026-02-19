using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using DG.Tweening;
using TMPro;

namespace EvanGameKits.Mechanic
{
    public class ModalGuideController : MonoBehaviour, IPointerDownHandler
    {
        [Header("References")]
        [SerializeField] private GameObject modalRoot;
        [SerializeField] private Image guideImageDisplay;
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private TMP_Text continueText;

        [Header("Settings")]
        [SerializeField] private float fadeDuration = 0.3f;
        [SerializeField] private float blinkDuration = 0.8f;

        [Header("Events")]
        public UnityEvent onGuideClosed;

        private bool isActive = false;
        private Tween blinkTween;

        private void Awake()
        {
            if (modalRoot != null) modalRoot.SetActive(false);
            if (canvasGroup != null)
            {
                canvasGroup.alpha = 0;
                canvasGroup.blocksRaycasts = false;
                canvasGroup.interactable = false;
            }
        }

        public void ShowGuide(Sprite guideSprite)
        {
            if (guideImageDisplay != null)
            {
                if (guideSprite != null)
                {
                    guideImageDisplay.sprite = guideSprite;
                    guideImageDisplay.gameObject.SetActive(true);
                }
                else
                {
                    // If null is passed, hide the image component so it doesn't block the video
                    guideImageDisplay.gameObject.SetActive(false);
                }
            }
            
            Open();
        }

        public void ShowGuide()
        {
            Open();
        }

        private void Open()
        {
            isActive = true;
            modalRoot.SetActive(true);

            if (canvasGroup != null)
            {
                canvasGroup.blocksRaycasts = true;
                canvasGroup.interactable = true;
                canvasGroup.DOKill();
                canvasGroup.DOFade(1f, fadeDuration).SetUpdate(true);
            }

            // Start blinking animation
            if (continueText != null)
            {
                continueText.DOKill();
                // Ensure it starts visible
                Color c = continueText.color;
                c.a = 1f;
                continueText.color = c;
                
                blinkTween = continueText.DOFade(0.2f, blinkDuration)
                    .SetLoops(-1, LoopType.Yoyo)
                    .SetEase(Ease.InOutSine)
                    .SetUpdate(true);
            }
        }

        public void HideGuide()
        {
            if (!isActive) return;

            isActive = false;
            
            // Stop blinking
            if (blinkTween != null) blinkTween.Kill();
            if (continueText != null) continueText.DOKill();

            if (canvasGroup != null)
            {
                canvasGroup.blocksRaycasts = false;
                canvasGroup.interactable = false;
                canvasGroup.DOKill();
                canvasGroup.DOFade(0f, fadeDuration).SetUpdate(true).OnComplete(() =>
                {
                    modalRoot.SetActive(false);
                    onGuideClosed?.Invoke();
                });
            }
            else
            {
                modalRoot.SetActive(false);
                onGuideClosed?.Invoke();
            }
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            HideGuide();
        }
    }
}