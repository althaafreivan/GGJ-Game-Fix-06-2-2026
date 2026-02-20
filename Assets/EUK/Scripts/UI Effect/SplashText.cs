using TMPro;
using UnityEngine;
using DG.Tweening;
using System.Collections.Generic;

namespace EvanUIKits.Tweening
{
    public class SplashText : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private TextMeshPro textMesh;

        [Header("Settings")]
        [SerializeField] private List<string> splashTexts = new List<string> { "Splash!" };
        [SerializeField] private float pulseScale = 1.2f;
        [SerializeField] private float pulseDuration = 0.5f;
        [SerializeField] private Ease pulseEase = Ease.InOutQuad;
        [SerializeField] private bool ignoreTimeScale = true;

        private void OnEnable()
        {
            if (textMesh == null) textMesh = GetComponent<TextMeshPro>();
            
            SetRandomSplash();
            StartPulseAnimation();
        }

        private void OnDisable()
        {
            textMesh.transform.DOKill();
        }

        public void SetRandomSplash()
        {
            if (textMesh == null || splashTexts == null || splashTexts.Count == 0) return;

            string randomText = splashTexts[Random.Range(0, splashTexts.Count)];
            textMesh.text = randomText;
        }

        private void StartPulseAnimation()
        {
            if (textMesh == null) return;

            textMesh.transform.localScale = Vector3.one;
            textMesh.transform.DOScale(pulseScale, pulseDuration)
                .SetEase(pulseEase)
                .SetLoops(-1, LoopType.Yoyo)
                .SetUpdate(ignoreTimeScale);
        }
    }
}
