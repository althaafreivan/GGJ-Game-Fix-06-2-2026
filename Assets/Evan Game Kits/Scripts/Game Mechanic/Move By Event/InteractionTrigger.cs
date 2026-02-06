using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;

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

        private int objectsOnTrigger = 0;
        private MaterialPropertyBlock propBlock;
        private bool isActive = false;

        private void Start()
        {
            if ((triggerType.Equals(TriggerType.Collider3D)) && (Camera.main.GetComponent<PhysicsRaycaster>() == null)) Camera.main.AddComponent<PhysicsRaycaster>();
            if ((triggerType.Equals(TriggerType.Collider2D)) && (Camera.main.GetComponent<Physics2DRaycaster>() == null)) Camera.main.AddComponent<Physics2DRaycaster>();
            
            propBlock = new MaterialPropertyBlock();
            UpdateVisuals(false);
        }

        private void UpdateVisuals(bool isOn)
        {
            if (triggerType != TriggerType.Weight && triggerType != TriggerType.Collider3D) return;

            isActive = isOn;
            if (buttonRenderer != null)
            {
                buttonRenderer.GetPropertyBlock(propBlock);
                propBlock.SetColor("_EmissionColor", isOn ? onColor : offColor);
                buttonRenderer.SetPropertyBlock(propBlock);
            }
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (!triggerType.Equals(TriggerType.UIButton) || TargetObject == null) return;
            TargetObject.ToggleState();
            UpdateVisuals(!isActive);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (!triggerType.Equals(TriggerType.UIButton) || TargetObject == null) return;
            if (state.Equals(TriggerState.Hold))
            {
                TargetObject.ToggleState();
                UpdateVisuals(false);
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (TargetObject == null) return;

            if (triggerType == TriggerType.Weight)
            {
                if (((1 << other.gameObject.layer) & weightLayers) != 0)
                {
                    objectsOnTrigger++;
                    if (objectsOnTrigger == 1)
                    {
                        if (state == TriggerState.Hold)
                        {
                            TargetObject.SetState(true);
                            UpdateVisuals(true);
                        }
                        else
                        {
                            TargetObject.ToggleState();
                            UpdateVisuals(!isActive);
                        }
                    }
                }
                return;
            }

            if (!triggerType.Equals(TriggerType.Collider3D)) return;
            if (other.CompareTag("Player"))
            {
                TargetObject.ToggleState();
                if (state == TriggerState.Hold) UpdateVisuals(true);
                else UpdateVisuals(!isActive);
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (TargetObject == null) return;

            if (triggerType == TriggerType.Weight)
            {
                if (((1 << other.gameObject.layer) & weightLayers) != 0)
                {
                    objectsOnTrigger = Mathf.Max(0, objectsOnTrigger - 1);
                    if (objectsOnTrigger == 0)
                    {
                        if (state == TriggerState.Hold)
                        {
                            TargetObject.SetState(false);
                            UpdateVisuals(false);
                        }
                    }
                }
                return;
            }

            if (!triggerType.Equals(TriggerType.Collider3D)) return;
            if (other.CompareTag("Player"))
            {
                if (state.Equals(TriggerState.Hold))
                {
                    TargetObject.ToggleState();
                    UpdateVisuals(false);
                }
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (TargetObject == null) return;

            if (triggerType == TriggerType.Weight)
            {
                if (((1 << other.gameObject.layer) & weightLayers) != 0)
                {
                    objectsOnTrigger++;
                    if (objectsOnTrigger == 1)
                    {
                        if (state == TriggerState.Hold)
                        {
                            TargetObject.SetState(true);
                            UpdateVisuals(true);
                        }
                        else
                        {
                            TargetObject.ToggleState();
                            UpdateVisuals(!isActive);
                        }
                    }
                }
                return;
            }

            if (!triggerType.Equals(TriggerType.Collider2D)) return;
            if (other.CompareTag("Player"))
            {
                TargetObject.ToggleState();
                if (state == TriggerState.Hold) UpdateVisuals(true);
                else UpdateVisuals(!isActive);
            }
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (TargetObject == null) return;

            if (triggerType == TriggerType.Weight)
            {
                if (((1 << other.gameObject.layer) & weightLayers) != 0)
                {
                    objectsOnTrigger = Mathf.Max(0, objectsOnTrigger - 1);
                    if (objectsOnTrigger == 0)
                    {
                        if (state == TriggerState.Hold)
                        {
                            TargetObject.SetState(false);
                            UpdateVisuals(false);
                        }
                    }
                }
                return;
            }

            if (!triggerType.Equals(TriggerType.Collider2D)) return;
            if (other.CompareTag("Player"))
            {
                if (state == TriggerState.Hold)
                {
                    TargetObject.ToggleState();
                    UpdateVisuals(false);
                }
            }
        }
    }
}