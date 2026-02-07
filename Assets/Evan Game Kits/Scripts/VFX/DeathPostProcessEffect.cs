using DG.Tweening;
using EvanGameKits.Core;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.Rendering;
// Using common namespaces for PostProcessing effects that work across most Volume systems
using UnityEngine.Rendering.Universal; 

namespace EvanGameKits.VFX
{
    public class DeathPostProcessEffect : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private Volume volume;
        [SerializeField] private float effectDuration = 0.5f;
        [SerializeField] private Color flashColor = Color.red;
        [SerializeField] private float maxVignetteIntensity = 0.45f;
        [SerializeField] private float maxChromaticAberration = 1f;

        [Header("Screen Shake")]
        [SerializeField] private CinemachineBasicMultiChannelPerlin noiseComponent;
        [SerializeField] private float shakeAmplitude = 1.5f;
        [SerializeField] private float shakeFrequency = 2.0f;

        private Vignette vignette;
        private ChromaticAberration chromaticAberration;
        private int lastHeartCount;

        private void Start()
        {
            if (volume == null) volume = GetComponent<Volume>();
            
            if (volume != null)
            {
                volume.profile.TryGet(out vignette);
                volume.profile.TryGet(out chromaticAberration);
            }

            if (GameCore.instance != null)
            {
                lastHeartCount = GameCore.instance.maxHearts;
                GameCore.instance.onHeartsChanged.AddListener(OnHeartsChanged);
            }
        }

        private void OnHeartsChanged(int currentHearts)
        {
            if (currentHearts < lastHeartCount && currentHearts >= 0)
            {
                PlayDeathEffect();
            }
            lastHeartCount = currentHearts;
        }

        public void PlayDeathEffect()
        {
            // Kill any existing tweens to prevent overlap
            DOTween.Kill(this);

            if (volume != null)
            {
                if (vignette != null)
                {
                    Color originalColor = vignette.color.value;
                    float originalIntensity = vignette.intensity.value;

                    vignette.color.Override(flashColor);
                    
                    DOTween.To(() => vignette.intensity.value, x => vignette.intensity.value = x, maxVignetteIntensity, effectDuration * 0.2f)
                        .SetTarget(this)
                        .SetEase(Ease.OutQuad)
                        .OnComplete(() =>
                        {
                            DOTween.To(() => vignette.intensity.value, x => vignette.intensity.value = x, originalIntensity, effectDuration * 0.8f)
                                .SetTarget(this)
                                .SetEase(Ease.InQuad)
                                .OnComplete(() => vignette.color.Override(originalColor));
                        });
                }

                if (chromaticAberration != null)
                {
                    float originalIntensity = chromaticAberration.intensity.value;

                    DOTween.To(() => chromaticAberration.intensity.value, x => chromaticAberration.intensity.value = x, maxChromaticAberration, effectDuration * 0.2f)
                        .SetTarget(this)
                        .SetEase(Ease.OutQuad)
                        .OnComplete(() =>
                        {
                            DOTween.To(() => chromaticAberration.intensity.value, x => chromaticAberration.intensity.value = x, originalIntensity, effectDuration * 0.8f)
                                .SetTarget(this)
                                .SetEase(Ease.InQuad);
                        });
                }
            }

            // Screen Shake
            StartCoroutine(ShakeSequence());
        }

        private System.Collections.IEnumerator ShakeSequence()
        {
            if (noiseComponent != null)
            {
                // Ensure the component is active
                noiseComponent.enabled = true;
                
                // Force override existing values
                noiseComponent.AmplitudeGain = shakeAmplitude;
                noiseComponent.FrequencyGain = shakeFrequency;
                
                yield return new WaitForSeconds(effectDuration * 0.3f);
                
                // Smoothly fade out to zero
                DOTween.To(() => noiseComponent.AmplitudeGain, x => noiseComponent.AmplitudeGain = x, 0f, effectDuration * 0.7f).SetTarget(this);
                DOTween.To(() => noiseComponent.FrequencyGain, x => noiseComponent.FrequencyGain = x, 0f, effectDuration * 0.7f).SetTarget(this);
                
                // Wait for fade to finish then disable to be clean
                yield return new WaitForSeconds(effectDuration * 0.7f);
                noiseComponent.enabled = false;
            }
        }

        private void OnDestroy()
        {
            if (GameCore.instance != null)
            {
                GameCore.instance.onHeartsChanged.RemoveListener(OnHeartsChanged);
            }
            DOTween.Kill(this);
        }
    }
}
