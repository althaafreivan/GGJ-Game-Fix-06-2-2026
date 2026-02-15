using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using EvanGameKits.Mechanic;
using EvanGameKits.GameMechanic;
using EvanGameKits.Entity.Module;
using System.Collections.Generic;
using System.Linq;

namespace EvanGameKits.Mechanic
{
    public class InteractionTrigger : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        public enum TriggerType { UIButton, Collider2D, Collider3D, Weight }
        public enum TriggerState { Switch, Hold }

        public TransformStateTweener TargetObject;
        public TriggerType triggerType;
        public TriggerState state;

        [Header("Weight Settings")]
        public LayerMask weightLayers = ~0;

        [Header("Visual Feedback Settings")]
        public Renderer buttonRenderer;
        [ColorUsage(true, true)] public Color onColor = Color.green;
        [ColorUsage(true, true)] public Color offColor = Color.black;

        private MaterialPropertyBlock propBlock;
        private bool isActive = false;
        private bool isFrozen = false;
        private bool wasOccupied = false;
        private float lastCheckTime = 0f;

        // Use HashSet to prevent double counting
        private HashSet<Collider> trackedColliders = new HashSet<Collider>();
        private HashSet<Collider2D> trackedColliders2D = new HashSet<Collider2D>();

        private void Start()
        {
            if ((triggerType.Equals(TriggerType.Collider3D)) && (Camera.main != null && Camera.main.GetComponent<PhysicsRaycaster>() == null)) Camera.main.AddComponent<PhysicsRaycaster>();
            if ((triggerType.Equals(TriggerType.Collider2D)) && (Camera.main != null && Camera.main.GetComponent<Physics2DRaycaster>() == null)) Camera.main.AddComponent<Physics2DRaycaster>();
            
            propBlock = new MaterialPropertyBlock();
            UpdateVisuals(false);
        }

        private void Update()
        {
            if (isFrozen) return;

            if (Time.time - lastCheckTime >= 1.0f)
            {
                lastCheckTime = Time.time;
                RefreshTriggerState();
            }
        }

        public void SetFrozen(bool state)
        {
            if (isFrozen == state) return;
            isFrozen = state;

            // When frozen, we do NOT change the color to offColor.
            // We want it to stay exactly as it was (Frozen in time).
            if (!isFrozen)
            {
                RefreshTriggerState();
            }
        }

        private void RefreshTriggerState()
        {
            if (isFrozen || TargetObject == null) return;

            // Cleanup null or destroyed objects.
            trackedColliders.RemoveWhere(c => c == null || !c.gameObject.activeInHierarchy);
            trackedColliders2D.RemoveWhere(c => c == null || !c.gameObject.activeInHierarchy);

            int totalCount = trackedColliders.Count + trackedColliders2D.Count;
            bool isOccupied = totalCount > 0;

            if (state == TriggerState.Hold)
            {
                TargetObject.SetState(isOccupied);
                UpdateVisuals(isOccupied);
            }
            else // Switch
            {
                if (isOccupied && !wasOccupied)
                {
                    TargetObject.ToggleState();
                }
                isActive = TargetObject.isTriggered;
                UpdateVisuals(isActive);
            }
            wasOccupied = isOccupied;
        }

        private void UpdateVisuals(bool isOn)
        {
            isActive = isOn;
            if (isFrozen) return;
            if (triggerType != TriggerType.Weight && triggerType != TriggerType.Collider3D) return;

            if (buttonRenderer != null)
            {
                buttonRenderer.GetPropertyBlock(propBlock);
                propBlock.SetColor("_EmissionColor", isOn ? onColor : offColor);
                buttonRenderer.SetPropertyBlock(propBlock);
            }
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (isFrozen) return;
            if (!triggerType.Equals(TriggerType.UIButton) || TargetObject == null) return;
            TargetObject.ToggleState();
            UpdateVisuals(!isActive);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (isFrozen) return;
            if (!triggerType.Equals(TriggerType.UIButton) || TargetObject == null) return;
            if (state.Equals(TriggerState.Hold))
            {
                TargetObject.ToggleState();
                UpdateVisuals(false);
            }
        }

        private bool IsWhiteCat(GameObject obj)
        {
            var identity = obj.GetComponent<Entity.Module.M_CatIdentity>();
            if (identity == null) identity = obj.GetComponentInParent<Entity.Module.M_CatIdentity>();
            return identity != null && identity.catType == Entity.Module.CatType.White;
        }

        private bool IsRelevant3D(Collider other)
        {
            if (triggerType == TriggerType.Weight)
                return ((1 << other.gameObject.layer) & weightLayers) != 0;
            return triggerType == TriggerType.Collider3D && other.CompareTag("Player");
        }

        private bool IsRelevant2D(Collider2D other)
        {
            if (triggerType == TriggerType.Weight)
                return ((1 << other.gameObject.layer) & weightLayers) != 0;
            return triggerType == TriggerType.Collider2D && other.CompareTag("Player");
        }

        private void OnTriggerEnter(Collider other)
        {
            if (TargetObject == null) return;

            if (IsRelevant3D(other))
            {
                if (trackedColliders.Add(other))
                {
                    if (!isFrozen)
                    {
                        RefreshTriggerState();
                    }
                }
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (TargetObject == null) return;

            if (trackedColliders.Remove(other))
            {
                if (!isFrozen)
                {
                    RefreshTriggerState();
                }
            }
        }

        private void OnTriggerStay(Collider other)
        {
            if (TargetObject == null) return;

            if (IsRelevant3D(other))
            {
                trackedColliders.Add(other);
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (TargetObject == null) return;

            if (IsRelevant2D(other))
            {
                if (trackedColliders2D.Add(other))
                {
                    if (!isFrozen)
                    {
                        RefreshTriggerState();
                    }
                }
            }
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (TargetObject == null) return;

            if (trackedColliders2D.Remove(other))
            {
                if (!isFrozen)
                {
                    RefreshTriggerState();
                }
            }
        }

        private void OnTriggerStay2D(Collider2D other)
        {
            if (TargetObject == null) return;

            if (IsRelevant2D(other))
            {
                trackedColliders2D.Add(other);
            }
        }
    }
}
