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
        public float teleportCooldown = 2.0f;
        private static float lastTeleportTime;
        private static bool isAnyPortalTeleporting = false;

        private List<Material> portalMaterials = new List<Material>();
        private HashSet<GameObject> objectsInTrigger = new HashSet<GameObject>();
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
            GameObject playerRoot = FindPlayerRoot(other.gameObject);
            if (playerRoot == null) return;

            if (((1 << playerRoot.layer) & allowedLayer) == 0) return;
            if (!playerRoot.CompareTag(playerTag)) return;

            if (isAnyPortalTeleporting)
            {
                NotificationController.instance?.ShowNotification("Portal is currently busy...", 1f);
                return;
            }

            if (Time.time < lastTeleportTime + teleportCooldown)
            {
                NotificationController.instance?.ShowNotification("Portal is recharging...", 1f);
                return;
            }

            if (objectsInTrigger.Add(playerRoot))
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
            isAnyPortalTeleporting = true;
            string catName = GetCatName(playerObj);
            AnimateDissolve(1f, 0f);
            if (isFrozen) currentTween.Pause();
            
            yield return currentTween.WaitForCompletion();

            // Notify why it's waiting
            if (isFrozen || (targetPortal != null && targetPortal.isFrozen))
            {
                if (catName == "Expected")
                {
                    NotificationController.instance?.ShowNotification("Expected can't teleport while looking at the portal! Move your window away and check from map.", 3f);
                }
                else
                {
                    NotificationController.instance?.ShowNotification("Nothing can only teleport while looking at the portal!", 2f);
                }
            }

            while (isFrozen || (targetPortal != null && targetPortal.isFrozen))
            {
                yield return null;
            }

            if (objectsInTrigger.Contains(playerObj))
            {
                lastTeleportTime = Time.time;

                targetPortal.AnimateDissolve(1f, 0f);
                if (targetPortal.isFrozen) targetPortal.CurrentTween.Pause();
                yield return targetPortal.CurrentPortalTweenWait();

                Transform destination = targetPortal.spawnPoint != null ? targetPortal.spawnPoint : targetPortal.transform;
                playerObj.transform.position = destination.position;
                playerObj.transform.rotation = destination.rotation;

                AnimateDissolve(0f, 1f);
                if (isFrozen) currentTween.Pause();
                
                targetPortal.AnimateDissolve(0f, 1f);
                if (targetPortal.isFrozen) targetPortal.CurrentTween.Pause();

                yield return currentTween.WaitForCompletion();
                
                objectsInTrigger.Remove(playerObj);
            }
            else
            {
                NotificationController.instance?.ShowNotification($"{catName} left the portal area.", 1.5f);
                AnimateDissolve(0f, 1f);
                if (isFrozen) currentTween.Pause();
                yield return currentTween.WaitForCompletion();
                objectsInTrigger.Remove(playerObj);
            }
            isAnyPortalTeleporting = false;
        }

        private IEnumerator CurrentPortalTweenWait()
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
