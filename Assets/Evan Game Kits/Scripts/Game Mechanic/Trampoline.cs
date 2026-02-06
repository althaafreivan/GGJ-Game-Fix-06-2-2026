using UnityEngine;
using EvanGameKits.Entity;
using DG.Tweening;

namespace EvanGameKits.Mechanic
{
    public class Trampoline : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private float bounceForce = 15f;
        [SerializeField] private bool useWorldUp = true;        
        [SerializeField] private float cooldown = 0.1f;
        [SerializeField] private LayerMask affectLayer = ~0;
        
        [Header("Visuals")]
        [SerializeField] private Transform meshTransform;
        [SerializeField] private float squishAmount = 0.3f;
        [SerializeField] private float squishDuration = 0.1f;

        [Header("Events")]
        public UnityEngine.Events.UnityEvent OnBounce;

        private float lastBounceTime;

        private void OnTriggerEnter(Collider other)
        {
            if (other.isTrigger) return; 
            if (((1 << other.gameObject.layer) & affectLayer) == 0) return;
            ProcessBounce(other.gameObject);
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (((1 << collision.gameObject.layer) & affectLayer) == 0) return;
            
            foreach (ContactPoint contact in collision.contacts)
            {
                Vector3 upDir = useWorldUp ? Vector3.up : transform.up;
                if (Vector3.Dot(contact.normal, upDir) < -0.5f) 
                {
                    ProcessBounce(collision.gameObject);
                    break;
                }
            }
        }

        private void ProcessBounce(GameObject go)
        {
            if (Time.time < lastBounceTime + cooldown) return;

            Rigidbody rb = go.GetComponentInParent<Rigidbody>();
            if (rb == null) rb = go.GetComponentInChildren<Rigidbody>();
            if (rb == null) rb = go.transform.root.GetComponentInChildren<Rigidbody>();
            
            if (rb != null)
            {
                ApplyBounce(rb);
            }
        }

        private void ApplyBounce(Rigidbody rb)
        {
            lastBounceTime = Time.time;

            Vector3 upDir = useWorldUp ? Vector3.up : transform.up;
            Vector3 velocity = rb.linearVelocity;
            
            Vector3 verticalComponent = Vector3.Project(velocity, upDir);
            velocity -= verticalComponent;
            
            velocity += upDir * bounceForce;
            
            rb.linearVelocity = velocity;

            catAnimator animator = rb.GetComponentInChildren<catAnimator>();
            if (animator != null)
            {
                animator.onJump(true);
            }

            OnBounce?.Invoke();

            if (meshTransform != null)
            {
                meshTransform.DOKill();
                Vector3 originalScale = meshTransform.localScale;
                meshTransform.DOScaleZ(originalScale.z * (1 - squishAmount), squishDuration)
                    .SetLoops(2, LoopType.Yoyo)
                    .SetEase(Ease.OutQuad);
            }
        }
    }
}
