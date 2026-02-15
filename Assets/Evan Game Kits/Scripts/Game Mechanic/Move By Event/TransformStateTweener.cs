using DG.Tweening;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace EvanGameKits.Mechanic
{
    public class TransformStateTweener : MonoBehaviour
    {
        [Header("Target Transform")]
        public Transform m_Target;

        public Vector3 startPosition, endPosition;
        public Vector3 startRotation, endRotation;
        public Vector3 startScale, endScale;

        public float invokeDuration = 1f, revokeDuration = 1f;
        public bool useCurrentPosition = true;
        public DG.Tweening.Ease ease = Ease.InOutCubic;
        public UnityEvent invokeTrigger, revokeTrigger;

        public bool isTriggered = false;
        private bool isFrozen = false;
        private Sequence currentSequence;
        private Rigidbody targetRigidbody;

        private void Awake()
        {
            m_Target = transform;
            targetRigidbody = m_Target.GetComponent<Rigidbody>();
            if (targetRigidbody != null)
            {
                targetRigidbody.isKinematic = true;
                targetRigidbody.interpolation = RigidbodyInterpolation.Interpolate;
            }
        }

        private void Start()
        {
            if (m_Target == null) m_Target = transform;
            
            if (useCurrentPosition)
            {
                startPosition = m_Target.position;
                startRotation = m_Target.eulerAngles;
                startScale = m_Target.localScale;
            }
        }

        public void SetFrozen(bool state)
        {
            isFrozen = state;
            if (isFrozen)
            {
                currentSequence?.Pause();
            }
            else
            {
                currentSequence?.Play();
            }
        }

        public void ToggleState()
        {
            SetState(!isTriggered);
        }

        public void SetState(bool state)
        {
            if (isTriggered == state) return;
            
            isTriggered = state;
            currentSequence?.Kill();
            currentSequence = DOTween.Sequence();

            if (isTriggered)
            {
                if (startPosition != endPosition)
                {
                    if (targetRigidbody != null)
                    {
                        currentSequence.Join(targetRigidbody.DOMove(endPosition, invokeDuration).SetEase(ease).SetUpdate(UpdateType.Fixed)
                            .OnUpdate(() => {
                                // DOTween doesn't automatically set Rigidbody.linearVelocity when using DOMove on a kinematic Rigidbody.
                                // We manually calculate and set it so characters using velocity-based movement can detect it.
                                if (invokeDuration > 0)
                                    targetRigidbody.linearVelocity = (endPosition - startPosition) / invokeDuration; 
                            })
                            .OnComplete(() => targetRigidbody.linearVelocity = Vector3.zero));
                    }
                    else
                        currentSequence.Join(m_Target.DOMove(endPosition, invokeDuration).SetEase(ease));
                }

                if (startRotation != endRotation)
                {
                    if (targetRigidbody != null)
                        currentSequence.Join(targetRigidbody.DORotate(endRotation, invokeDuration).SetEase(ease).SetUpdate(UpdateType.Fixed));
                    else
                        currentSequence.Join(m_Target.DORotate(endRotation, invokeDuration).SetEase(ease));
                }

                if (startScale != endScale) currentSequence.Join(m_Target.DOScale(endScale, invokeDuration).SetEase(ease));
                
                invokeTrigger?.Invoke();
            }
            else
            {
                if (startPosition != endPosition)
                {
                    if (targetRigidbody != null)
                    {
                        currentSequence.Join(targetRigidbody.DOMove(startPosition, revokeDuration).SetEase(ease).SetUpdate(UpdateType.Fixed)
                            .OnUpdate(() => {
                                if (revokeDuration > 0)
                                    targetRigidbody.linearVelocity = (startPosition - endPosition) / revokeDuration;
                            })
                            .OnComplete(() => targetRigidbody.linearVelocity = Vector3.zero));
                    }
                    else
                        currentSequence.Join(m_Target.DOMove(startPosition, revokeDuration).SetEase(ease));
                }

                if (startRotation != endRotation)
                {
                    if (targetRigidbody != null)
                        currentSequence.Join(targetRigidbody.DORotate(startRotation, revokeDuration).SetEase(ease).SetUpdate(UpdateType.Fixed));
                    else
                        currentSequence.Join(m_Target.DORotate(startRotation, revokeDuration).SetEase(ease));
                }

                if (startScale != endScale) currentSequence.Join(m_Target.DOScale(startScale, revokeDuration).SetEase(ease));

                revokeTrigger?.Invoke();
            }

            if (isFrozen) currentSequence.Pause();
        }
    }
}
