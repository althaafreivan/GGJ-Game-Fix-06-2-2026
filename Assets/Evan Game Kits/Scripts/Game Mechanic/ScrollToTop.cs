using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace EvanGameKits.Mechanic
{
    /// <summary>
    /// Automatically resets a ScrollRect to the top position when the object is enabled.
    /// </summary>
    public class ScrollToTop : MonoBehaviour
    {
        [SerializeField] private ScrollRect scrollRect;

        private void Awake()
        {
            if (scrollRect == null)
                scrollRect = GetComponent<ScrollRect>();
        }

        private void OnEnable()
        {
            if (scrollRect != null)
            {
                // 1f is the top for vertical scrolling
                DOVirtual.DelayedCall(.5f, () => { scrollRect.DOVerticalNormalizedPos(1f, .5f).SetEase(Ease.InOutCubic); });
            }
        }
    }
}
