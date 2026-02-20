using DG.Tweening;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

public class EnteringAnimation : MonoBehaviour
{
    public Transform Pad;
    public GameObject ExplosionVFX;
    public Transform Cam;
    public Transform tv;
    public Transform background;
    public string sceneToLoad;

    [Header("Post Processing")]
    public Volume globalVolume;
    private LensDistortion lensDistortion;

    private void Start()
    {
        if (globalVolume != null && globalVolume.profile.TryGet(out lensDistortion))
        {
            // Initial state if needed
        }
    }

    public void startAnimate()
    {
        Vector3 targetRot = new Vector3(90, 0, 0);
        Sequence s = DOTween.Sequence();
        Vector3 startPos = Cam.localPosition;

        // 1. Initial Sequence: Pad rotates and Camera starts creeping forward
        s.Append(Pad.DORotate(targetRot, 2f).SetEase(Ease.InOutQuad));
        s.Join(background.DOScale(0f, 2f).SetEase(Ease.InOutQuad));
        s.Join(Cam.DOLocalMove(startPos + new Vector3(0, 5, -5), 2f).SetEase(Ease.InOutExpo));

        // 2. VFX Enabled (Duration: 5 seconds before warp)
        s.AppendCallback(() => ExplosionVFX.SetActive(true));
        
        // After 1 second of VFX, enable intense shake
        s.AppendInterval(2f); 
        s.AppendCallback(() => SetNoiseEnabled(true));
        
        // Continue movement and shake for 4 more seconds (Total VFX time: 5s)
        s.Join(Cam.DOLocalMove(startPos + new Vector3(0, 15, -30), 4f).SetEase(Ease.InOutQuad).OnStart(() => { tv.gameObject.SetActive(false); }));
        s.AppendInterval(1.5f);
        
        // 3. Final Sequence: Stop noise and Warp (Lens Distortion)
        s.AppendCallback(() => SetNoiseEnabled(false));
        if (lensDistortion != null)
        {
            s.Append(DOTween.To(() => lensDistortion.center.value.x, x => 
            {
                Vector2 center = lensDistortion.center.value;
                center.x = x;
                lensDistortion.center.value = center;
            }, 1.5f, 2f).From(0.5f).SetEase(Ease.InExpo));
        }

        // Finalize
        s.OnComplete(() => SceneManager.LoadScene(sceneToLoad));
    }

    private void SetNoiseEnabled(bool enabled)
    {
        if (Camera.main == null) return;
        var brain = Camera.main.GetComponent<CinemachineBrain>();
        if (brain == null) return;

        var vcam = brain.ActiveVirtualCamera as CinemachineVirtualCameraBase;
        if (vcam != null)
        {
            var noise = vcam.GetComponent<CinemachineBasicMultiChannelPerlin>();
            if (noise == null) noise = vcam.GetComponentInChildren<CinemachineBasicMultiChannelPerlin>();
            
            if (noise != null) noise.enabled = enabled;
        }
    }
}
