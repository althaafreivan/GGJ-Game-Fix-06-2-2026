using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using DG.Tweening;
using EvanGameKits.Core;
using Unity.Cinemachine;

namespace EvanGameKits.Mechanic
{
    public class SceneLensAnimation : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Volume globalVolume;
        [SerializeField] private GameObject explosionVFX;
        
        [Header("Scene Settings")]
        [SerializeField] private string sceneToLoad;
        [SerializeField] private float startDelay = 1.0f;
        [SerializeField] private float explosionDuration = 6.0f;
        [SerializeField] private float loadDelayAfterExplosion = 1.5f;

        [Header("Noise Settings")]
        [SerializeField] private float targetAmplitude = 2f;
        [SerializeField] private float targetFrequency = 2f;
        [SerializeField] private float noiseFadeDuration = 1f;

        [Header("Animation Settings")]
        [SerializeField] private float animationDuration = 2f;
        [SerializeField] private Ease animationEase = Ease.OutCubic;

        private LensDistortion lensDistortion;

        private void Awake()
        {
            if (globalVolume != null)
            {
                globalVolume.profile.TryGet(out lensDistortion);
            }

            if (lensDistortion != null)
            {
                // Force initial state immediately
                Vector2 center = lensDistortion.center.value;
                center.x = -5f;
                lensDistortion.center.value = center;
            }

            if (explosionVFX != null)
            {
                explosionVFX.SetActive(false);
            }
        }

        private void Start()
        {
            if (GameCore.instance != null)
            {
                GameCore.instance.onWin.AddListener(PlayWinAnimation);
            }

            PlayStartAnimation();
        }

        private void OnDestroy()
        {
            if (GameCore.instance != null)
            {
                GameCore.instance.onWin.RemoveListener(PlayWinAnimation);
            }
        }

        [ContextMenu("Play Start Animation")]
        public void PlayStartAnimation()
        {
            if (lensDistortion == null) return;

            // Start: x center to 0.5 after delay
            DOTween.To(() => lensDistortion.center.value.x, x => 
            {
                Vector2 center = lensDistortion.center.value;
                center.x = x;
                lensDistortion.center.value = center;
            }, 0.5f, animationDuration)
            .SetDelay(startDelay)
            .SetEase(animationEase)
            .SetUpdate(true);
        }

        [ContextMenu("Play Win Animation")]
        public void PlayWinAnimation()
        {
            Sequence winSequence = DOTween.Sequence();

            // 1. Trigger Explosion Immediately
            winSequence.AppendCallback(() => {
                if (explosionVFX != null) explosionVFX.SetActive(true);
            });

            // 2. Wait 1s, then interpolate noise
            winSequence.AppendInterval(1.0f);
            winSequence.AppendCallback(() => {
                InterpolateNoise(targetAmplitude, targetFrequency, noiseFadeDuration);
            });

            // 3. Wait for the explosion to play out (remaining duration)
            winSequence.AppendInterval(Mathf.Max(0, explosionDuration - 1.0f));

            // 4. Play Win Lens Animation (x center to 1.5) - Called AFTER explosion duration
            if (lensDistortion != null)
            {
                winSequence.Append(DOTween.To(() => lensDistortion.center.value.x, x => 
                {
                    Vector2 center = lensDistortion.center.value;
                    center.x = x;
                    lensDistortion.center.value = center;
                }, 5f, animationDuration)
                .SetEase(animationEase));
            }

            // 5. Fade out noise and wait extra delay
            winSequence.AppendCallback(() => {
                InterpolateNoise(0, 0, noiseFadeDuration);
            });
            winSequence.AppendInterval(loadDelayAfterExplosion);

            // 6. Load Scene
            winSequence.OnComplete(() => {
                if (!string.IsNullOrEmpty(sceneToLoad))
                {
                    SceneManager.LoadScene(sceneToLoad);
                }
            });

            winSequence.SetUpdate(true);
        }

        private void InterpolateNoise(float amplitude, float frequency, float duration)
        {
            var noise = GetNoiseComponent();
            if (noise == null) return;

            noise.enabled = true;
            DOTween.To(() => noise.AmplitudeGain, x => noise.AmplitudeGain = x, amplitude, duration).SetUpdate(true);
            DOTween.To(() => noise.FrequencyGain, x => noise.FrequencyGain = x, frequency, duration).SetUpdate(true);
        }

        private CinemachineBasicMultiChannelPerlin GetNoiseComponent()
        {
            if (Camera.main == null) return null;
            var brain = Camera.main.GetComponent<CinemachineBrain>();
            if (brain == null) return null;

            var vcam = brain.ActiveVirtualCamera as CinemachineVirtualCameraBase;
            if (vcam == null) return null;

            var noise = vcam.GetComponent<CinemachineBasicMultiChannelPerlin>();
            if (noise == null) noise = vcam.GetComponentInChildren<CinemachineBasicMultiChannelPerlin>();
            
            return noise;
        }
    }
}
