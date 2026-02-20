using UnityEngine;
using EvanGameKits.Core;

namespace EvanGameKits.Core
{
    public class M_WindowPortalCalibrator : MonoBehaviour
    {
        [Header("Calibration Sliders (0-1)")]
        [Range(0f, 1f)] public float sensitivitySlider = 0.1f;
        [Range(0f, 1f)] public float maxDragSpeedSlider = 0.5f;
        [Range(0f, 1f)] public float minSmoothingSlider = 0.25f;
        [Range(0f, 1f)] public float maxSmoothingSlider = 0.05f;
        [Range(0f, 1f)] public float velocityThresholdSlider = 0.2f;
        [Range(0f, 1f)] public float threadUpdateRateSlider = 0.25f;

        [Header("Target Ranges")]
        public Vector2 sensitivityRange = new Vector2(0.0001f, 0.01f);
        public Vector2 maxDragSpeedRange = new Vector2(100f, 10000f);
        public Vector2 smoothingRange = new Vector2(1f, 100f);
        public Vector2 velocityThresholdRange = new Vector2(100f, 5000f);
        public Vector2 threadUpdateRateRange = new Vector2(60f, 4000f);

        private M_WindowPortalCamera portalCamera;

        private void Start()
        {
            portalCamera = M_WindowPortalCamera.instance;
            if (portalCamera == null)
            {
                portalCamera = FindFirstObjectByType<M_WindowPortalCamera>();
            }

            if (portalCamera != null)
            {
                // Initialize sliders based on current values
                sensitivitySlider = Mathf.InverseLerp(sensitivityRange.x, sensitivityRange.y, portalCamera.sensitivity);
                maxDragSpeedSlider = Mathf.InverseLerp(maxDragSpeedRange.x, maxDragSpeedRange.y, portalCamera.maxDragSpeed);
                minSmoothingSlider = Mathf.InverseLerp(smoothingRange.x, smoothingRange.y, portalCamera.minSmoothing);
                maxSmoothingSlider = Mathf.InverseLerp(smoothingRange.x, smoothingRange.y, portalCamera.maxSmoothing);
                velocityThresholdSlider = Mathf.InverseLerp(velocityThresholdRange.x, velocityThresholdRange.y, portalCamera.velocityThreshold);
                threadUpdateRateSlider = Mathf.InverseLerp(threadUpdateRateRange.x, threadUpdateRateRange.y, portalCamera.threadUpdateRate);
            }
        }

        private void Update()
        {
            if (portalCamera == null)
            {
                portalCamera = M_WindowPortalCamera.instance;
                if (portalCamera == null) return;
            }

            // Apply slider values to the portal camera
            portalCamera.sensitivity = Mathf.Lerp(sensitivityRange.x, sensitivityRange.y, sensitivitySlider);
            portalCamera.maxDragSpeed = Mathf.Lerp(maxDragSpeedRange.x, maxDragSpeedRange.y, maxDragSpeedSlider);
            portalCamera.minSmoothing = Mathf.Lerp(smoothingRange.x, smoothingRange.y, minSmoothingSlider);
            portalCamera.maxSmoothing = Mathf.Lerp(smoothingRange.x, smoothingRange.y, maxSmoothingSlider);
            portalCamera.velocityThreshold = Mathf.Lerp(velocityThresholdRange.x, velocityThresholdRange.y, velocityThresholdSlider);
            portalCamera.threadUpdateRate = (int)Mathf.Lerp(threadUpdateRateRange.x, threadUpdateRateRange.y, threadUpdateRateSlider);
        }

        [ContextMenu("Reset to Defaults")]
        public void ResetToDefaults()
        {
            sensitivitySlider = 0.1f; // 0.001
            maxDragSpeedSlider = 0.5f; // 5000
            minSmoothingSlider = 0.25f; // 25
            maxSmoothingSlider = 0.05f; // 5
            velocityThresholdSlider = 0.2f; // 1000
            threadUpdateRateSlider = 0.235f; // ~1000
        }
    }
}
