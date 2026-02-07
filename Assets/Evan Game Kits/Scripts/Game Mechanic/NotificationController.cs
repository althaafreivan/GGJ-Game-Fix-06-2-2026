using UnityEngine;
using TMPro;
using DG.Tweening;
using System.Collections;
using EvanGameKits.Mechanic;

namespace EvanGameKits.Mechanic
{
    public class NotificationController : MonoBehaviour
    {
        public static NotificationController instance;

        [Header("References")]
        [SerializeField] private TMP_Text notificationText;
        [SerializeField] private CanvasGroup canvasGroup;

        [Header("Settings")]
        [SerializeField] private float defaultDuration = 2f;
        [SerializeField] private float fadeDuration = 0.5f;

        private Coroutine hideCoroutine;

        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
            }
            else
            {
                Destroy(gameObject);
                return;
            }

            if (canvasGroup == null)
            {
                canvasGroup = GetComponent<CanvasGroup>();
                if (canvasGroup == null && notificationText != null)
                {
                    canvasGroup = notificationText.gameObject.AddComponent<CanvasGroup>();
                }
            }

            // Start hidden
            if (canvasGroup != null)
            {
                canvasGroup.alpha = 0;
                if (notificationText != null) notificationText.gameObject.SetActive(false);
            }
        }

        public void ShowNotification(string message)
        {
            ShowNotification(message, defaultDuration, Color.white);
        }

        public void ShowNotification(string message, float duration)
        {
            ShowNotification(message, duration, Color.white);
        }

        public void ShowNotification(string message, Color color)
        {
            ShowNotification(message, defaultDuration, color);
        }

        public void ShowNotification(string message, float duration, Color color)
        {
            if (notificationText == null || canvasGroup == null)
            {
                Debug.LogWarning("NotificationController: Missing references!");
                return;
            }

            notificationText.text = message;
            notificationText.color = color;
            notificationText.gameObject.SetActive(true);

            if (hideCoroutine != null)
            {
                StopCoroutine(hideCoroutine);
            }

            // DOTween Fade In
            canvasGroup.DOKill();
            canvasGroup.DOFade(1f, fadeDuration).SetUpdate(true);

            hideCoroutine = StartCoroutine(HideAfterDelay(duration));
        }

        private IEnumerator HideAfterDelay(float delay)
        {
            yield return new WaitForSecondsRealtime(delay);

            // DOTween Fade Out
            canvasGroup.DOFade(0f, fadeDuration).SetUpdate(true).OnComplete(() => {
                notificationText.gameObject.SetActive(false);
            });

            hideCoroutine = null;
        }
    }
}