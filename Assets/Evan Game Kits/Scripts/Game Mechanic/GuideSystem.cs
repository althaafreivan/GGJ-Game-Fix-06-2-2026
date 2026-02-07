using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using System.Collections.Generic;

namespace EvanGameKits.Mechanic
{
    public class GuideSystem : MonoBehaviour
    {
        [System.Serializable]
        public class GuideStep
        {
            public string description;
            public Sprite image;
        }

        public static GuideSystem instance;

        [Header("References")]
        [SerializeField] private CanvasGroup mainCanvasGroup;
        [SerializeField] private Image guideImage;
        [SerializeField] private TMP_Text guideText;
        [SerializeField] private Button nextButton;
        [SerializeField] private Button prevButton;
        [SerializeField] private Button closeButton;
        [SerializeField] private TMP_Text pageIndicator;

        [Header("Content")]
        public List<GuideStep> steps = new List<GuideStep>();

        [Header("Settings")]
        [SerializeField] private float fadeDuration = 0.3f;
        [SerializeField] private float moveDuration = 0.3f;
        [SerializeField] private Ease easeType = Ease.OutBack;

        private int currentIndex = 0;

        private void Awake()
        {
            if (instance == null) instance = this;
            else Destroy(gameObject);

            if (mainCanvasGroup != null)
            {
                mainCanvasGroup.alpha = 0;
                mainCanvasGroup.interactable = false;
                mainCanvasGroup.blocksRaycasts = false;
                mainCanvasGroup.transform.localScale = Vector3.zero;
            }

            nextButton.onClick.AddListener(NextStep);
            prevButton.onClick.AddListener(PreviousStep);
            if (closeButton != null) closeButton.onClick.AddListener(CloseGuide);
        }

        public void ShowGuide()
        {
            if (steps.Count == 0) return;

            currentIndex = 0;
            UpdateUI();

            mainCanvasGroup.DOKill();
            mainCanvasGroup.DOFade(1f, fadeDuration).SetUpdate(true);
            mainCanvasGroup.transform.DOKill();
            mainCanvasGroup.transform.DOScale(1f, moveDuration).SetEase(easeType).SetUpdate(true);
            mainCanvasGroup.interactable = true;
            mainCanvasGroup.blocksRaycasts = true;
        }

        public void CloseGuide()
        {
            mainCanvasGroup.DOKill();
            mainCanvasGroup.DOFade(0f, fadeDuration).SetUpdate(true);
            mainCanvasGroup.transform.DOKill();
            mainCanvasGroup.transform.DOScale(0f, moveDuration).SetEase(Ease.InBack).SetUpdate(true).OnComplete(() => {
                mainCanvasGroup.interactable = false;
                mainCanvasGroup.blocksRaycasts = false;
            });
        }

        public void NextStep()
        {
            if (currentIndex < steps.Count - 1)
            {
                currentIndex++;
                AnimateTransition(1);
            }
        }

        public void PreviousStep()
        {
            if (currentIndex > 0)
            {
                currentIndex--;
                AnimateTransition(-1);
            }
        }

        private void AnimateTransition(int direction)
        {
            // Simple slide-out / slide-in effect for content
            float moveDist = 50f;
            Transform contentTransform = guideImage.transform.parent; // Assuming image and text are in a container
            CanvasGroup cg = contentTransform.GetComponent<CanvasGroup>();

            Sequence seq = DOTween.Sequence().SetUpdate(true);
            seq.Append(contentTransform.DOLocalMoveX(-moveDist * direction, fadeDuration / 2f).SetRelative().SetEase(Ease.InQuad));
            
            if (cg != null) seq.Join(cg.DOFade(0f, fadeDuration / 2f));
            else seq.Join(contentTransform.DOScale(0.95f, fadeDuration / 2f));
            
            seq.AppendCallback(() => {
                UpdateUI();
                contentTransform.localPosition = new Vector3(moveDist * direction, contentTransform.localPosition.y, contentTransform.localPosition.z);
            });

            seq.Append(contentTransform.DOLocalMoveX(0, fadeDuration / 2f).SetEase(Ease.OutQuad));
            
            if (cg != null) seq.Join(cg.DOFade(1f, fadeDuration / 2f));
            else seq.Join(contentTransform.DOScale(1f, fadeDuration / 2f));
        }

        private void UpdateUI()
        {
            if (currentIndex >= 0 && currentIndex < steps.Count)
            {
                guideText.text = steps[currentIndex].description;
                guideImage.sprite = steps[currentIndex].image;
                guideImage.gameObject.SetActive(steps[currentIndex].image != null);

                if (pageIndicator != null)
                {
                    pageIndicator.text = $"{currentIndex + 1} / {steps.Count}";
                }

                nextButton.interactable = currentIndex < steps.Count - 1;
                prevButton.interactable = currentIndex > 0;
            }
        }
    }
}