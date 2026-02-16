using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using EvanGameKits.Entity;
using EvanGameKits.Entity.Module;

namespace EvanGameKits.Mechanic
{
    public class Portal : MonoBehaviour
    {
        [Header("Portal Link")]
        public Portal targetPortal;
        public Transform spawnPoint;     

        [Header("Filters")]
        public LayerMask allowedLayer;
        public string playerTag = "Player";

        [Header("Visual Settings")]
        public float dissolveDuration = 1.2f;    
        public string dissolvePropertyName = "_DissolveAmount";
        public float dissolveStartValue = 0f;
        public float dissolveEndValue = 1f;

        [Header("State")]
        public bool isFrozen = false;       

        [Header("Cooldown")]
        public float teleportCooldown = 0f;
        private static float lastTeleportTime;
        private static bool isAnyPortalTeleporting = false;

        private List<Material> portalMaterials = new List<Material>();
        private HashSet<GameObject> objectsInTrigger = new HashSet<GameObject>();
        private HashSet<GameObject> activeTeleporters = new HashSet<GameObject>();
        private Tween currentTween;

        public Tween CurrentTween => currentTween;

        private void Awake()
        {
            isAnyPortalTeleporting = false;
        }

        private void Start()
        {
            Renderer[] renderers = GetComponentsInChildren<Renderer>(true);
            foreach (var r in renderers)
            {
                foreach (var m in r.materials)
                {
                    if (m != null && m.HasProperty(dissolvePropertyName))
                    {
                        portalMaterials.Add(m);
                        m.SetFloat(dissolvePropertyName, dissolveStartValue);
                    }
                }
            }
        }

        public void SetFrozen(bool state) 
        {
            isFrozen = state;
            if (isFrozen)
            {
                currentTween?.Pause();
            }
            else
            {
                currentTween?.Play();
            }
        }

        private string GetCatName(GameObject obj)
        {
            var identity = obj.GetComponent<Entity.Module.M_CatIdentity>();
            if (identity == null) identity = obj.GetComponentInParent<Entity.Module.M_CatIdentity>();
            if (identity == null) return "Unknown";
            return identity.catType == Entity.Module.CatType.White ? "Expected" : "Nothing";
        }

        private void OnTriggerEnter(Collider other)
        {
            HandleTrigger(other);
        }

        private void OnTriggerStay(Collider other)
        {
            HandleTrigger(other);
        }

        private void HandleTrigger(Collider other)
        {
            GameObject playerRoot = FindPlayerRoot(other.gameObject);
            if (playerRoot == null) return;

            if (((1 << playerRoot.layer) & allowedLayer) == 0) return;
            if (!playerRoot.CompareTag(playerTag)) return;

            objectsInTrigger.Add(playerRoot);

            if (!activeTeleporters.Contains(playerRoot))
            {
                StartCoroutine(TeleportCountdown(playerRoot));
            }
        }

        private void OnTriggerExit(Collider other)
        {
            GameObject playerRoot = FindPlayerRoot(other.gameObject);
            if (playerRoot != null)
            {
                objectsInTrigger.Remove(playerRoot);
            }
        }

        private GameObject FindPlayerRoot(GameObject hit)
        {
            Player p = hit.GetComponentInParent<Player>();
            if (p != null) return p.gameObject;
            Rigidbody rb = hit.GetComponentInParent<Rigidbody>();
            if (rb != null) return rb.gameObject;
            return null;
        }

        private void SetPlayerFrozen(GameObject playerObj, bool freeze)
        {
            Player p = playerObj.GetComponent<Player>();
            if (p == null) p = playerObj.GetComponentInParent<Player>();
            
            if (p != null)
            {
                p.MuteInput(freeze);
            }

            Rigidbody rb = playerObj.GetComponent<Rigidbody>();
            if (rb == null) rb = playerObj.GetComponentInParent<Rigidbody>();

            if (rb != null)
            {
                if (freeze)
                {
                    rb.linearVelocity = Vector3.zero;
                    rb.angularVelocity = Vector3.zero;
                    rb.isKinematic = true;
                }
                else
                {
                    rb.isKinematic = false;
                }
            }
        }

        private IEnumerator TeleportCountdown(GameObject playerObj)
        {
            activeTeleporters.Add(playerObj);
            string catName = GetCatName(playerObj);
            bool notifiedWaiting = false;

            while (objectsInTrigger.Contains(playerObj))
            {
                // Wait for unfreeze if either this portal or target is frozen
                if (isFrozen || (targetPortal != null && targetPortal.isFrozen))
                {
                    if (!notifiedWaiting)
                    {
                        if (catName == "Expected")
                            NotificationController.instance?.ShowNotification("Expected can't teleport while looking at the portal! Move your window away and check from map.", 3f);
                        else if (catName == "Nothing")
                            NotificationController.instance?.ShowNotification("Nothing can only teleport while looking at the portal!", 2f);
                        notifiedWaiting = true;
                    }
                    yield return null;
                    continue;
                }

                // Wait for global busy state or cooldown
                if (isAnyPortalTeleporting || (Time.time < lastTeleportTime + teleportCooldown))
                {
                    yield return null;
                    continue;
                }

                // --- STAGE 1: Charging (Dissolve In) ---
                // Start visual charging immediately
                AnimateDissolve(dissolveStartValue, dissolveEndValue);
                if (targetPortal != null) targetPortal.AnimateDissolve(dissolveStartValue, dissolveEndValue);

                float elapsed = 0f;
                bool cancelled = false;
                while (elapsed < dissolveDuration)
                {
                    if (!objectsInTrigger.Contains(playerObj))
                    {
                        cancelled = true;
                        break;
                    }
                    if (isFrozen || (targetPortal != null && targetPortal.isFrozen))
                    {
                        cancelled = true;
                        break;
                    }
                    elapsed += Time.deltaTime;
                    yield return null;
                }

                if (cancelled)
                {
                    AnimateDissolve(dissolveEndValue, dissolveStartValue);
                    if (targetPortal != null) targetPortal.AnimateDissolve(dissolveEndValue, dissolveStartValue);
                    // Give a small grace period before allowing re-trigger
                    yield return new WaitForSeconds(0.2f);
                    continue; 
                }

                // --- STAGE 2: Instant Teleport ---
                isAnyPortalTeleporting = true;
                lastTeleportTime = Time.time;

                // Move instantly
                if (targetPortal != null)
                {
                    Transform destination = targetPortal.spawnPoint != null ? targetPortal.spawnPoint : targetPortal.transform;
                    playerObj.transform.position = destination.position;

                    // Immediately start reverse dissolve (appearing at destination)
                    AnimateDissolve(dissolveEndValue, dissolveStartValue); // Source fades out
                    targetPortal.AnimateDissolve(dissolveEndValue, dissolveStartValue); // Target fades out
                }
                else
                {
                    AnimateDissolve(dissolveEndValue, dissolveStartValue);
                }

                // Player remains moveable immediately
                // Wait a frame to let physics catch up before allowing another teleport
                yield return new WaitForFixedUpdate();
                isAnyPortalTeleporting = false;
                break;
            }

            activeTeleporters.Remove(playerObj);
            objectsInTrigger.Remove(playerObj);
        }

        public IEnumerator CurrentPortalTweenWait()
        {
            if (currentTween != null) yield return currentTween.WaitForCompletion();
        }

        public Tween AnimateDissolve(float start, float end)
        {
            if (portalMaterials.Count == 0) return null;

            if (currentTween != null && currentTween.IsActive()) currentTween.Kill();

            Sequence seq = DOTween.Sequence();
            foreach (var m in portalMaterials)
            {
                m.SetFloat(dissolvePropertyName, start);
                seq.Join(m.DOFloat(end, dissolvePropertyName, dissolveDuration).SetEase(Ease.OutSine));
            }
            currentTween = seq;
            return seq;
        }

        private void OnDrawGizmos()
        {
            if (spawnPoint != null)
            {
                Gizmos.color = Color.cyan;
                Gizmos.DrawWireSphere(spawnPoint.position, 0.5f);
                Gizmos.DrawLine(spawnPoint.position, spawnPoint.position + spawnPoint.forward * 1.0f);
                
                // Draw a simple arrow head
                Vector3 right = spawnPoint.right;
                Vector3 forward = spawnPoint.forward;
                Gizmos.DrawLine(spawnPoint.position + forward * 1.0f, spawnPoint.position + forward * 0.7f + right * 0.2f);
                Gizmos.DrawLine(spawnPoint.position + forward * 1.0f, spawnPoint.position + forward * 0.7f - right * 0.2f);
            }
        }
    }
}
