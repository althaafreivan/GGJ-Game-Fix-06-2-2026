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
                        m.SetFloat(dissolvePropertyName, 1f);
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
                if (isAnyPortalTeleporting || (teleportCooldown > 0 && Time.time < lastTeleportTime + teleportCooldown))
                {
                    yield return null;
                    continue;
                }

                // Teleport
                isAnyPortalTeleporting = true;
                lastTeleportTime = Time.time;

                // Visuals - start them but don't wait for "instant" feel
                AnimateDissolve(1f, 0f);
                if (isFrozen) currentTween?.Pause();
                
                if (targetPortal != null)
                {
                    targetPortal.AnimateDissolve(1f, 0f);
                    if (targetPortal.isFrozen) targetPortal.CurrentTween?.Pause();

                    Transform destination = targetPortal.spawnPoint != null ? targetPortal.spawnPoint : targetPortal.transform;
                    playerObj.transform.position = destination.position;
                    playerObj.transform.rotation = destination.rotation;
                }

                // Brief lock to prevent physics double-teleport issues, then release busy state
                yield return new WaitForFixedUpdate();
                isAnyPortalTeleporting = false;

                // Visuals - return to normal
                AnimateDissolve(0f, 1f);
                if (isFrozen) currentTween?.Pause();
                
                if (targetPortal != null)
                {
                    targetPortal.AnimateDissolve(0f, 1f);
                    if (targetPortal.isFrozen) targetPortal.CurrentTween?.Pause();
                }

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
    }
}
