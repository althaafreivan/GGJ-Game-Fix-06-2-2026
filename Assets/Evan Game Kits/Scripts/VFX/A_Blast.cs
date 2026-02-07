using DG.Tweening;
using Unity.Cinemachine;
using UnityEngine;
using System.Collections.Generic;

public class A_Blast : MonoBehaviour
{
    [Header("Timing")]
    [SerializeField] private float mainDuration = 2.5f;
    [SerializeField] private float secondaryDuration = 0.5f;
    [SerializeField] private float overScaleMultiplier = 1.1f;

    [Header("Star Settings")]
    [SerializeField] private float starMaximalSize = 40f;
    [SerializeField] private float starDuration = .5f;
    [SerializeField] private bool automaticStarAlignment = true;

    [Header("Finish Settings")]
    [SerializeField] private float finishDuration = 0.5f;

    [Header("Easing")]
    [SerializeField] private Ease primaryEase = Ease.InOutExpo;
    [SerializeField] private Ease secondaryEase = Ease.OutQuad;
    [SerializeField] private Ease finishEase = Ease.InExpo;

    [Header("Effect Parts")]
    [SerializeField] private Transform Star;
    [SerializeField] private Transform[] fullScaleParts;
    [SerializeField] private Transform[] xyScaleParts;

    private Dictionary<Transform, Vector3> originalScales = new Dictionary<Transform, Vector3>();
    private Tween shakeTimer;      

    private void Awake()
    {
        
        foreach (var part in fullScaleParts) originalScales[part] = part.localScale;
        foreach (var part in xyScaleParts) originalScales[part] = part.localScale;
    }

    private void OnEnable() => Explode();

    private void OnDisable()
    {
        if (shakeTimer != null) shakeTimer.Kill();

        DOTween.Kill(Star);
        foreach (var part in fullScaleParts) DOTween.Kill(part);
        foreach (var part in xyScaleParts) DOTween.Kill(part);

        StopShakeInternal();
    }

    public void Explode()
    {
        if (shakeTimer != null) shakeTimer.Kill();

        DOTween.Kill(Star);
        foreach (var part in fullScaleParts) { part.gameObject.SetActive(false); DOTween.Kill(part); }
        foreach (var part in xyScaleParts) { part.gameObject.SetActive(false); DOTween.Kill(part); }

        Star.gameObject.SetActive(true);
        Star.localScale = Vector3.zero;
        if(automaticStarAlignment) Star.localRotation = Camera.main.transform.localRotation;
        Star.DOScale(starMaximalSize, starDuration).SetEase(primaryEase).OnComplete(() =>
        {
            Star.DOScale(Vector3.zero, starDuration).SetEase(primaryEase).OnComplete(() =>
            {
                Star.gameObject.SetActive(false);

                float totalEffectDuration = mainDuration + secondaryDuration + finishDuration;
                StartShake(totalEffectDuration);

                foreach (var part in fullScaleParts)
                {
                    part.gameObject.SetActive(true);
                    CreateScaleSequence(part, Vector3.zero, originalScales[part]);
                }

                foreach (var part in xyScaleParts)
                {
                    part.gameObject.SetActive(true);
                    Vector3 startScale = new Vector3(0, 0, originalScales[part].z);
                    CreateScaleSequence(part, startScale, originalScales[part]);
                }
            });
        });
    }

    private void CreateScaleSequence(Transform part, Vector3 startScale, Vector3 targetScale)
    {
        Vector3 overScale = targetScale * overScaleMultiplier;
        Sequence s = DOTween.Sequence().SetTarget(part);      

        s.Append(part.DOScale(targetScale, mainDuration).From(startScale).SetEase(primaryEase));
        s.Append(part.DOScale(overScale, secondaryDuration).SetEase(secondaryEase));
        s.Append(part.DOScale(Vector3.zero, finishDuration).SetEase(finishEase));
        s.OnComplete(() => part.gameObject.SetActive(false));
    }

    private void StartShake(float duration)
    {
        var noise = GetNoise();
        if (noise != null)
        {
            noise.enabled = true;
            shakeTimer = DOVirtual.DelayedCall(duration, () => noise.enabled = false);
        }
    }

    private void StopShakeInternal()
    {
        var noise = GetNoise();
        if (noise != null) noise.enabled = false;
    }

    private CinemachineBasicMultiChannelPerlin GetNoise()
    {
        if (Camera.main == null) return null;
        var brain = Camera.main.GetComponent<CinemachineBrain>();
        if (brain == null) return null;

        ICinemachineCamera cam = brain.ActiveVirtualCamera;
        if (cam is CinemachineVirtualCameraBase vcam)
        {
            return vcam.gameObject.GetComponent<CinemachineBasicMultiChannelPerlin>();
        }
        return null;
    }
}