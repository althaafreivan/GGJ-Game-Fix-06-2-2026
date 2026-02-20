using UnityEngine;
using DG.Tweening;

namespace EvanGameKits.VFX
{
    [RequireComponent(typeof(CanvasGroup))]
    public class UIEnableAnimation : MonoBehaviour
    {
        [Header("Animation Settings")]
        [SerializeField] private float duration = 0.4f;
        [SerializeField] private float delay = 0f;
        [SerializeField] private Ease easeType = Ease.OutBack;
        
        [Header("Effect Toggle")]
        [SerializeField] private bool scaleEffect = true;
        [SerializeField] private bool fadeEffect = true;
        [SerializeField] private Vector3 startScale = Vector3.zero;

        private CanvasGroup canvasGroup;

        private void Awake()
        {
            canvasGroup = GetComponent<CanvasGroup>();
        }

        private void OnEnable()
        {
            // Reset state
            if (fadeEffect) canvasGroup.alpha = 0;
            if (scaleEffect) transform.localScale = startScale;

            // Kill any existing animations to prevent conflicts
            canvasGroup.DOKill();
            transform.DOKill();

            // Run Animations
            if (fadeEffect)
            {
                canvasGroup.DOFade(1f, duration)
                    .SetDelay(delay)
                    .SetUpdate(true); // Works even when paused
            }

            if (scaleEffect)
            {
                transform.DOScale(1f, duration)
                    .SetDelay(delay)
                    .SetEase(easeType)
                    .SetUpdate(true);
            }
        }

        private void OnDisable()
        {
            // Optional: Cleanup on disable
            canvasGroup.DOKill();
            transform.DOKill();
        }
    }
}