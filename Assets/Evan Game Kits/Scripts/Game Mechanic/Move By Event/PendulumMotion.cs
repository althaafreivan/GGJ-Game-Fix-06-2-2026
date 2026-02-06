using DG.Tweening;
using UnityEngine;

namespace EvanGameKits.Mechanic
{
    public class PendulumMotion : MonoBehaviour
    {
        public enum MotionType { Rotation, Position }

        [Header("Target")]
        public Transform target;

        [Header("Config")]
        public MotionType motionType = MotionType.Rotation;
        public Vector3 range = new Vector3(0, 0, 45f); // Max swing/offset in each direction
        public float duration = 2f;
        public Ease ease = Ease.InOutSine;
        public bool autoStart = true;

        private Tween motionTween;
        private Vector3 initialPosition;
        private Quaternion initialRotation;

        private void Start()
        {
            if (target == null) target = transform;

            initialPosition = target.localPosition;
            initialRotation = target.localRotation;

            if (autoStart) StartMotion();
        }

        public void StartMotion()
        {
            StopMotion();
            if (target == null) target = transform;

            if (motionType == MotionType.Rotation)
            {
                // Start from negative range
                target.localRotation = initialRotation * Quaternion.Euler(-range);
                
                // Tween to positive range and loop
                motionTween = target.DOLocalRotate((initialRotation * Quaternion.Euler(range)).eulerAngles, duration)
                    .SetEase(ease)
                    .SetLoops(-1, LoopType.Yoyo);
            }
            else
            {
                // Start from negative offset
                target.localPosition = initialPosition - range;

                // Tween to positive offset and loop
                motionTween = target.DOLocalMove(initialPosition + range, duration)
                    .SetEase(ease)
                    .SetLoops(-1, LoopType.Yoyo);
            }
        }

        public void StopMotion()
        {
            if (motionTween != null)
            {
                motionTween.Kill();
                motionTween = null;
            }
        }

        private void OnDestroy()
        {
            StopMotion();
        }
    }
}
