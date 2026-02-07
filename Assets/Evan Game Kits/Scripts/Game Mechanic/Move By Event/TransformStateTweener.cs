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

        private void Start()
        {
            m_Target = transform;

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
                if (startPosition != endPosition) currentSequence.Join(m_Target.DOMove(endPosition, invokeDuration).SetEase(ease));
                if (startRotation != endRotation) currentSequence.Join(m_Target.DORotate(endRotation, invokeDuration).SetEase(ease));
                if (startScale != endScale) currentSequence.Join(m_Target.DOScale(endScale, invokeDuration).SetEase(ease));
                
                invokeTrigger?.Invoke();
            }
            else
            {
                if (startPosition != endPosition) currentSequence.Join(m_Target.DOMove(startPosition, revokeDuration).SetEase(ease));
                if (startRotation != endRotation) currentSequence.Join(m_Target.DORotate(startRotation, revokeDuration).SetEase(ease));
                if (startScale != endScale) currentSequence.Join(m_Target.DOScale(startScale, revokeDuration).SetEase(ease));

                revokeTrigger?.Invoke();
            }

            if (isFrozen) currentSequence.Pause();
        }
    }
}
