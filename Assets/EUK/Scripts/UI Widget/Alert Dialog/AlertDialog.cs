using UnityEngine;
using System;
using TMPro;
using DG.Tweening;

namespace EvanUIKits.Confirmation
{
    public class Alert : MonoBehaviour
    {
        public static Alert Instance;

        public GameObject modal;
        public TextMeshProUGUI messageText;
        private Action onAction;

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

        public void Show(string message, Action ok)
        {
            messageText.text = message;
            onAction = ok;

            modal.SetActive(true);
            
            canvasGroup.DOKill();
            canvasGroup.alpha = 0f;
            canvasGroup.DOFade(1f, duration).SetUpdate(true);

            modal.transform.DOKill();
            modal.transform.localScale = Vector3.zero;
            modal.transform.DOScale(1f, duration).SetEase(showEase).SetUpdate(true);
        }

        public void onClick()
        {
            onAction?.Invoke();
            Hide();
        }

        private void Hide()
        {
            onAction = null;
            
            canvasGroup.DOKill();
            canvasGroup.DOFade(0f, duration).SetUpdate(true);

            modal.transform.DOKill();
            modal.transform.DOScale(0f, duration).SetEase(hideEase).SetUpdate(true).OnComplete(() => {
                modal.SetActive(false);
            });
        }
    }
}
