using System;
using TMPro;
using UnityEngine;
using DG.Tweening;

namespace EvanUIKits.Confirmation
{
    public class Confirmation : MonoBehaviour
    {
        public static Confirmation Instance;

        public GameObject modal;
        public TextMeshProUGUI messageText;
        private Action onYesAction;
        private Action onNoAction;

        [Header("Animation Settings")]
        [SerializeField] private float duration = 0.3f;
        [SerializeField] private Ease showEase = Ease.OutBack;
        [SerializeField] private Ease hideEase = Ease.InBack;

        private CanvasGroup canvasGroup;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);

            if (modal != null)
            {
                canvasGroup = modal.GetComponent<CanvasGroup>();
                if (canvasGroup == null) canvasGroup = modal.AddComponent<CanvasGroup>();
                modal.SetActive(false);
            }
        }

        public void ShowWithoutCallback(string message)
        {
            messageText.text = message;
            AnimateShow();
        }

        public void Show(string message, Action yesCallback, Action noCallback = null)   
        {
            messageText.text = message;
            onYesAction = yesCallback;
            onNoAction = noCallback;

            AnimateShow();
        }

        private void AnimateShow()
        {
            modal.SetActive(true);
            
            if (canvasGroup == null) canvasGroup = modal.GetComponent<CanvasGroup>();

            canvasGroup.DOKill();
            canvasGroup.alpha = 0f;
            canvasGroup.DOFade(1f, duration).SetUpdate(true);

            modal.transform.DOKill();
            modal.transform.localScale = Vector3.zero;
            modal.transform.DOScale(1f, duration).SetEase(showEase).SetUpdate(true);
        }

        public void yes()
        {
            onYesAction?.Invoke();
            Hide();
        }

        public void no()
        {
            onNoAction?.Invoke();
            Hide();
        }

        private void Hide()
        {
            onYesAction = null;
            onNoAction = null;
            
            canvasGroup.DOKill();
            canvasGroup.DOFade(0f, duration).SetUpdate(true);

            modal.transform.DOKill();
            modal.transform.DOScale(0f, duration).SetEase(hideEase).SetUpdate(true).OnComplete(() => {
                modal.SetActive(false);
            });
        }
    }
}
