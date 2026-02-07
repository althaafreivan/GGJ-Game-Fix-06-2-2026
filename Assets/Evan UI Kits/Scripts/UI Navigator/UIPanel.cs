using UnityEngine;
using DG.Tweening;

namespace EvanUIKits.PanelController
{
    public class UIPanel : MonoBehaviour
    {
        [Header("Animation Settings")]
        [SerializeField] private float duration = 0.3f;
        [SerializeField] private Ease showEase = Ease.OutBack;
        [SerializeField] private Ease hideEase = Ease.InBack;

        [HideInInspector] public PanelMap panel;

        private void Start()
        {
            if (panel.canvasGroup == null)
            {
                panel.canvasGroup = GetComponent<CanvasGroup>();
                if (panel.canvasGroup == null) panel.canvasGroup = gameObject.AddComponent<CanvasGroup>();
            }
        }

        public void Show()
        {
            gameObject.SetActive(true);
            
            if (panel.canvasGroup == null) panel.canvasGroup = GetComponent<CanvasGroup>();
            
            panel.canvasGroup.DOKill();
            panel.canvasGroup.alpha = 0f;
            panel.canvasGroup.DOFade(1f, duration).SetUpdate(true);

            transform.DOKill();
            transform.localScale = Vector3.zero;
            transform.DOScale(1f, duration).SetEase(showEase).SetUpdate(true);
        }

        public void Hide()
        {
            if (panel.canvasGroup == null) panel.canvasGroup = GetComponent<CanvasGroup>();

            panel.canvasGroup.DOKill();
            panel.canvasGroup.DOFade(0f, duration).SetUpdate(true);

            transform.DOKill();
            transform.DOScale(0f, duration).SetEase(hideEase).SetUpdate(true).OnComplete(() => {
                gameObject.SetActive(false);
            });
        }

        [System.Serializable]
        public class PanelMap
        {
            [HideInInspector] public CanvasGroup canvasGroup;
        }
    }
}
