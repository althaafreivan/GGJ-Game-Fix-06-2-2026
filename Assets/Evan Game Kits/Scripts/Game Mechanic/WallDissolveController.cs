using UnityEngine;
using DG.Tweening;

namespace EvanGameKits.Mechanic
{
    public class WallDissolveController : MonoBehaviour
    {
        [Header("Transparency Settings")]
        [SerializeField] private Renderer wallRenderer;
        [SerializeField] private string colorPropertyName = "_BaseColor";
        [SerializeField] private float hideAlpha = 0.2f;
        [SerializeField] private float showAlpha = 1f;
        [SerializeField] private float transitionDuration = 0.3f;

        private Material wallMaterial;
        private Tween currentTween;
        private bool isHidden = false;
        private float lastHideTime;

        private void Start()
        {
            if (wallRenderer == null) wallRenderer = GetComponent<Renderer>();
            if (wallRenderer != null) wallMaterial = wallRenderer.material;
        }

        public void RequestHide()
        {
            lastHideTime = Time.time;
            if (!isHidden)
            {
                isHidden = true;
                AnimateAlpha(hideAlpha);
            }
        }

        private void Update()
        {
            // If we haven't been requested to hide for a frame, show ourselves again
            if (isHidden && Time.time > lastHideTime + 0.1f)
            {
                isHidden = false;
                AnimateAlpha(showAlpha);
            }
        }

        private void AnimateAlpha(float targetAlpha)
        {
            if (wallMaterial == null) return;
            currentTween?.Kill();
            
            Color targetColor = wallMaterial.GetColor(colorPropertyName);
            targetColor.a = targetAlpha;
            currentTween = wallMaterial.DOColor(targetColor, colorPropertyName, transitionDuration);
        }

        private void OnDestroy()
        {
            if (wallMaterial != null) Destroy(wallMaterial);
        }
    }
}