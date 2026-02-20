using DG.Tweening;
using UnityEngine;

namespace EvanUIKits.Tweening
{
    public static class AnimateButton
    {
        public enum AnimationType
        {
            Scale,
            Punch,
            Tilt,
            MaterialOnly,
            Combined
        }

        public static void OnButtonDown(
            RectTransform button,
            RectTransform shadow = null,
            AnimationType type = AnimationType.Scale,
            Material mat = null,
            string matProperty = "_ShineIntensity",
            float effectScale = 0.9f,
            float duration = 0.1f,
            bool ignoreTimeScale = false)
        {
            button.DOKill();
            if (shadow != null) shadow.DOKill();

            switch (type)
            {
                case AnimationType.Scale:
                    button.DOScale(effectScale, duration).SetEase(Ease.OutQuad).SetUpdate(ignoreTimeScale);
                    if (shadow != null) shadow.DOScale(effectScale, duration * 2f).SetEase(Ease.OutQuad).SetUpdate(ignoreTimeScale);
                    break;

                case AnimationType.Punch:
                    button.DOPunchScale(new Vector3(-0.1f, -0.1f, 0f), duration, 1, 0.5f).SetUpdate(ignoreTimeScale);
                    break;

                case AnimationType.Tilt:
                    button.DORotate(new Vector3(0, 0, -3f), duration).SetEase(Ease.OutQuad).SetUpdate(ignoreTimeScale);
                    button.DOScale(effectScale, duration).SetEase(Ease.OutQuad).SetUpdate(ignoreTimeScale);
                    break;

                case AnimationType.MaterialOnly:
                    HandleMaterial(mat, matProperty, 1f, duration, ignoreTimeScale);
                    break;

                case AnimationType.Combined:
                    button.DOScale(effectScale, duration).SetEase(Ease.OutQuad).SetUpdate(ignoreTimeScale);
                    HandleMaterial(mat, matProperty, 1f, duration, ignoreTimeScale);
                    break;
            }
        }

        public static void OnButtonUp(
            RectTransform button,
            RectTransform shadow = null,
            AnimationType type = AnimationType.Scale,
            Material mat = null,
            string matProperty = "_ShineIntensity",
            float targetScale = 1f,
            float duration = 0.1f,
            System.Action onComplete = null,
            bool ignoreTimeScale = false)
        {
            button.DOKill();
            if (shadow != null) shadow.DOKill();

            Tween mainTween = null;

            switch (type)
            {
                case AnimationType.Scale:
                    if (shadow != null) shadow.DOScale(targetScale, duration * 2f).SetEase(Ease.OutElastic).SetUpdate(ignoreTimeScale);
                    mainTween = button.DOScale(targetScale, duration).SetEase(Ease.OutElastic).SetUpdate(ignoreTimeScale);
                    break;

                case AnimationType.Punch:
                    mainTween = button.DOScale(targetScale, duration).SetEase(Ease.OutQuad).SetUpdate(ignoreTimeScale);
                    break;

                case AnimationType.Tilt:
                    button.DORotate(Vector3.zero, duration).SetEase(Ease.OutElastic).SetUpdate(ignoreTimeScale);
                    mainTween = button.DOScale(targetScale, duration).SetEase(Ease.OutElastic).SetUpdate(ignoreTimeScale);
                    break;

                case AnimationType.MaterialOnly:
                    mainTween = HandleMaterial(mat, matProperty, 0f, duration, ignoreTimeScale);
                    break;

                case AnimationType.Combined:
                    HandleMaterial(mat, matProperty, 0f, duration, ignoreTimeScale);
                    mainTween = button.DOScale(targetScale, duration).SetEase(Ease.OutElastic).SetUpdate(ignoreTimeScale);
                    break;
            }

            if (mainTween != null)
            {
                mainTween.OnComplete(() => onComplete?.Invoke());
            }
            else
            {
                onComplete?.Invoke();
            }
        }

        private static Tween HandleMaterial(Material mat, string property, float target, float duration, bool ignoreTimeScale = false)
        {
            if (mat == null || string.IsNullOrEmpty(property)) return null;

            int propertyId = Shader.PropertyToID(property);
            float value = Mathf.Clamp01(target);
            return mat.DOFloat(value, propertyId, duration).SetUpdate(ignoreTimeScale);
        }
    }
}