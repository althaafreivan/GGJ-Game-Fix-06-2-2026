using EvanGameKits.Entity.Module;
using UnityEngine;
using EvanGameKits.Mechanic;

namespace EvanGameKits.Mechanic
{
    public class BlockTrigger : MonoBehaviour
    {
        public enum AffectedCat
        {
            Black,
            White,
            Both
        }

        [Header("Settings")]
        public AffectedCat affectedCat = AffectedCat.Both;
        public float pushForce = 20f;
        public float snapDistance = 0.5f;
        
        [Header("Notification")]
        [TextArea] public string notificationText = "This area is blocked!";
        public Color notificationColor = Color.yellow;
        public float notificationCooldown = 2.5f;

        private float lastNotificationTime;
        private Collider triggerCollider;

        private void Awake()
        {
            triggerCollider = GetComponent<Collider>();
            if (triggerCollider != null) triggerCollider.isTrigger = true;
        }

        private void OnTriggerStay(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                M_CatIdentity identity = GetCatIdentity(other);
                if (identity != null && IsAffected(identity))
                {
                    // Show notification with cooldown
                    if (Time.time > lastNotificationTime + notificationCooldown)
                    {
                        NotificationController.instance?.ShowNotification(notificationText, notificationColor);
                        lastNotificationTime = Time.time;
                    }

                    PushBack(other);
                }
            }
        }

        private M_CatIdentity GetCatIdentity(Collider other)
        {
            M_CatIdentity identity = other.GetComponent<M_CatIdentity>();
            if (identity == null) identity = other.GetComponentInParent<M_CatIdentity>();
            return identity;
        }

        private bool IsAffected(M_CatIdentity identity)
        {
            return (affectedCat == AffectedCat.Both) ||
                   (affectedCat == AffectedCat.Black && identity.catType == CatType.Black) ||
                   (affectedCat == AffectedCat.White && identity.catType == CatType.White);
        }

        private void PushBack(Collider other)
        {
            Rigidbody rb = other.GetComponent<Rigidbody>();
            if (rb == null) rb = other.GetComponentInParent<Rigidbody>();

            // Calculate direction away from the trigger center
            Vector3 playerPos = other.transform.position;
            Vector3 triggerCenter = triggerCollider.bounds.center;
            Vector3 pushDir = (playerPos - triggerCenter);
            pushDir.y = 0;
            
            if (pushDir.sqrMagnitude < 0.001f) pushDir = -other.transform.forward;
            pushDir = pushDir.normalized;

            if (rb != null)
            {
                // Aggressive snap: Move the player position out of the trigger immediately
                rb.position += pushDir * snapDistance;
                
                // Kill inward velocity and apply a strong outward blast
                Vector3 currentVel = rb.linearVelocity;
                if (Vector3.Dot(currentVel, pushDir) < 0) // If moving towards trigger
                {
                    currentVel = Vector3.ProjectOnPlane(currentVel, pushDir);
                }
                
                rb.linearVelocity = currentVel + (pushDir * pushForce);
            }
            else
            {
                other.transform.position += pushDir * snapDistance;
            }
        }
    }
}
