using EvanGameKits.Entity.Module;
using UnityEngine;
using UnityEngine.Events;
using EvanGameKits.Mechanic;

public class EnterTrigger : MonoBehaviour
{

    public enum allowedCat
    {
        white,
        black,
        both
    }

    [Header("Settings")]
    public allowedCat ct = allowedCat.both;
    public bool blockUnallowed = true;
    public float pushForce = 20f;
    public float snapDistance = 0.5f;
    
    [Header("Events")]
    public UnityEvent onPlayerEnter;

    private float lastNotificationTime;
    private const float notificationCooldown = 2.5f;
    private Collider triggerCollider;

    private void Awake()
    {
        triggerCollider = GetComponent<Collider>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            M_CatIdentity identity = GetCatIdentity(other);
            if (identity != null && IsAllowed(identity))
            {
                onPlayerEnter?.Invoke();
            }
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (blockUnallowed && other.CompareTag("Player"))
        {
            M_CatIdentity identity = GetCatIdentity(other);
            if (identity != null && !IsAllowed(identity))
            {
                if (Time.time > lastNotificationTime + notificationCooldown)
                {
                    string neededCat = (ct == allowedCat.black) ? "Nothing (Black)" : "Expected (White)";
                    NotificationController.instance?.ShowNotification($"Switch to {neededCat} cat to continue", Color.yellow);
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

    private bool IsAllowed(M_CatIdentity identity)
    {
        return (ct == allowedCat.both) ||
               (ct == allowedCat.black && identity.catType == CatType.Black) ||
               (ct == allowedCat.white && identity.catType == CatType.White);
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
            // This fights against any velocity-based movement trying to push in
            rb.position += pushDir * snapDistance;
            
            // Kill inward velocity and apply a strong outward blast
            Vector3 currentVel = rb.linearVelocity;
            if (Vector3.Dot(currentVel, pushDir) < 0) // If moving towards trigger
            {
                currentVel = Vector3.ProjectOnPlane(currentVel, pushDir);
            }
            
            rb.linearVelocity = currentVel + (pushDir * pushForce) + (Vector3.up * 2f);
        }
        else
        {
            other.transform.position += pushDir * snapDistance;
        }
    }
}
