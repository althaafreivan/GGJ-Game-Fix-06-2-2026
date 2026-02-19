using UnityEngine;
using DG.Tweening;
using EvanGameKits.Entity;
using System.Collections.Generic;
using EvanGameKits.Entity.Module;
using EvanGameKits.GameMechanic;

namespace EvanGameKits.Mechanic
{
    public class DoorMapController : MonoBehaviour
    {
        [Header("Door Settings")]
        [SerializeField] private Transform doorTransform;
        [SerializeField] private Vector3 doorOpenRotation;
        [SerializeField] private Vector3 doorClosedRotation;
        
        [Header("World Mapping")]
        [Tooltip("The name of the root GameObject containing all map pieces.")]
        [SerializeField] private string mapContainerName = "Map";

        [Header("Animation Settings")]
        [SerializeField] private float duration = 1.0f;
        [SerializeField] private Ease easeType = Ease.InOutCubic;

        [Header("Interaction Settings")]
        [SerializeField] private string playerTag = "Player";
        [SerializeField] private KeyCode interactionKey = KeyCode.E;

        private bool isOpen = false;
        private bool isPlayerInRange = false;
        private bool isFrozen = false;
                private Sequence currentSequence;
                private List<Transform> grabbedContent = new List<Transform>();
                private Transform playerOriginalParent;
                private bool playerWasKinematic;
        
                private void Start()
                {
                    if (doorTransform == null) doorTransform = transform;
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
        
                private void Update()
                {
                    if (!isPlayerInRange) return;

                    if (Input.GetKeyDown(interactionKey))
                    {
                        if (IsWhiteCatActive())
                        {
                            NotificationController.instance?.ShowNotification("The door is opened, but maybe the time freeze?");
                            ToggleState();
                        }
                        else if (!isFrozen)
                        {
                            ToggleState();
                        }
                    }
                }
        
                private bool IsWhiteCatActive()
                {
                    if (Player.ActivePlayer == null) return false;
                    var identity = Player.ActivePlayer.GetComponent<M_CatIdentity>();
                    return identity != null && identity.catType == CatType.White;
                }
        
                        public void ToggleState()
                        {
                            if (currentSequence != null && currentSequence.IsActive() && currentSequence.IsPlaying()) return;
                
                            isOpen = !isOpen;
                
                            GrabMapContent();
                            Animate(isOpen, () => ReleaseMapContent());
                        }
                
                        private void GrabMapContent()
                        {
                            GameObject mapContainer = GameObject.Find(mapContainerName);
                            if (mapContainer == null)
                            {
                                Debug.LogWarning($"DoorMapController: Could not find map container named '{mapContainerName}'");
                                return;
                            }
                
                            grabbedContent.Clear();
                            List<Transform> childrenToMove = new List<Transform>();
                            
                            foreach (Transform child in mapContainer.transform)
                            {
                                if (IsChildOf(doorTransform, child)) continue;
                                childrenToMove.Add(child);
                            }
                
                            foreach (Transform child in childrenToMove)
                            {
                                child.SetParent(doorTransform, true);
                                grabbedContent.Add(child);
                            }
                        }
                
                        private void ReleaseMapContent()
                        {
                            GameObject mapContainer = GameObject.Find(mapContainerName);
                            if (mapContainer == null) return;
                
                            foreach (Transform child in grabbedContent)
                            {
                                if (child != null && child.parent == doorTransform)
                                {
                                    child.SetParent(mapContainer.transform, true);
                                }
                            }
                            grabbedContent.Clear();
                        }
        private bool IsChildOf(Transform target, Transform potentialParent)
        {
            Transform current = target;
            while (current != null)
            {
                if (current == potentialParent) return true;
                current = current.parent;
            }
            return false;
        }

        private void Animate(bool open, System.Action onComplete = null)
        {
            currentSequence?.Kill();
            currentSequence = DOTween.Sequence();

            Vector3 targetDoorRot = open ? doorOpenRotation : doorClosedRotation;
            Quaternion targetQuat = Quaternion.Euler(targetDoorRot);
            
            // Using DOLocalRotateQuaternion ensures we always take the shortest path (nearest rotating direction)
            // as it uses Quaternion interpolation (Slerp) which is immune to Euler wrap-around issues.
            currentSequence.Join(doorTransform.DOLocalRotateQuaternion(targetQuat, duration).SetEase(easeType));

            if (onComplete != null)
                currentSequence.OnComplete(() => onComplete());

            if (isFrozen) currentSequence.Pause();
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag(playerTag))
            {
                isPlayerInRange = true;
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.CompareTag(playerTag))
            {
                isPlayerInRange = false;
            }
        }

        [ContextMenu("Set Open State From Current")]
        private void SetOpenState()
        {
            if (doorTransform) doorOpenRotation = doorTransform.localEulerAngles;
        }

        [ContextMenu("Set Closed State From Current")]
        private void SetClosedState()
        {
            if (doorTransform) doorClosedRotation = doorTransform.localEulerAngles;
        }
    }
}
