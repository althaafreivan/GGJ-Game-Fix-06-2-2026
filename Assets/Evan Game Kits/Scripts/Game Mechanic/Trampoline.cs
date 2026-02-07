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

        [Header("Sauce (Not Working As Expected)")]
        [Range(0f, 1f)] [SerializeField] private float glitchChance = 0.3f;
        [Range(0f, 1f)] [SerializeField] private float permanentStretchChance = 0.02f;
        [SerializeField] private bool randomTorque = true;

        [Header("Events")]
        public UnityEngine.Events.UnityEvent OnBounce;
        public UnityEngine.Events.UnityEvent OnGlitch;

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
                if (Random.value < glitchChance)
                {
                    HandleGlitch(rb);
                }
                else
                {
                    ApplyBounce(rb, 1f, Vector3.zero);
                }
            }
        }

        private void HandleGlitch(Rigidbody rb)
        {
            lastBounceTime = Time.time;
            OnGlitch?.Invoke();

            int glitchType = Random.Range(0, 3);
            switch (glitchType)
            {
                case 0: // Delayed "Struggle" Launch
                    if (meshTransform != null) meshTransform.DOShakePosition(0.5f, 0.1f);
                    DOVirtual.DelayedCall(0.5f, () => ApplyBounce(rb, 1.5f, Vector3.zero));
                    break;
                case 1: // Weak "Fart" Bounce
                    ApplyBounce(rb, 0.4f, Vector3.zero);
                    break;
                case 2: // "Drunk" Launch (Wrong Angle)
                    Vector3 randomAngle = new Vector3(Random.Range(-0.3f, 0.3f), 0, Random.Range(-0.3f, 0.3f));
                    ApplyBounce(rb, 1.2f, randomAngle);
                    break;
            }
        }

        private void ApplyBounce(Rigidbody rb, float forceMultiplier, Vector3 angleOffset)
        {
            lastBounceTime = Time.time;

            Vector3 upDir = (useWorldUp ? Vector3.up : transform.up) + angleOffset;
            upDir.Normalize();

            Vector3 velocity = rb.linearVelocity;
            Vector3 verticalComponent = Vector3.Project(velocity, upDir);
            velocity -= verticalComponent;
            
            velocity += upDir * (bounceForce * forceMultiplier);
            rb.linearVelocity = velocity;

            if (randomTorque)
            {
                rb.AddTorque(Random.insideUnitSphere * 10f, ForceMode.Impulse);
            }

            // Always squash/stretch the player (100% chance)
            rb.transform.DOKill();
            if (Random.value < permanentStretchChance)
            {
                // Permanent glitch until death
                rb.transform.DOScaleX(1.8f, 0.15f).SetEase(Ease.OutElastic);
                rb.transform.DOScaleY(0.4f, 0.15f).SetEase(Ease.OutElastic);
            }
            else
            {
                // Normal temporary squash
                rb.transform.DOScaleX(1.5f, 0.1f).SetLoops(2, LoopType.Yoyo);
                rb.transform.DOScaleY(0.5f, 0.1f).SetLoops(2, LoopType.Yoyo);
            }

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
