using UnityEngine;
using DG.Tweening;
using EvanGameKits.Entity;
using System.Collections.Generic;

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
            if (isFrozen) return;

            if (isPlayerInRange && Input.GetKeyDown(interactionKey))
            {
                ToggleState();
            }
        }

        public void ToggleState()
        {
            isOpen = !isOpen;

            if (isOpen)
            {
                GrabMapContent();
            }

            Animate(isOpen);
        }

        private void GrabMapContent()
        {
            GameObject mapContainer = GameObject.Find(mapContainerName);
            if (mapContainer == null)
            {
                Debug.LogWarning($"DoorMapController: Could not find map container named '{mapContainerName}'");
                return;
            }

            List<Transform> children = new List<Transform>();
            foreach (Transform child in mapContainer.transform)
            {
                if (IsChildOf(doorTransform, child)) continue;
                children.Add(child);
            }

            foreach (Transform child in children)
            {
                child.SetParent(doorTransform, true);
            }
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

        private void Animate(bool open)
        {
            currentSequence?.Kill();
            currentSequence = DOTween.Sequence();

            Vector3 targetDoorRot = open ? doorOpenRotation : doorClosedRotation;
            currentSequence.Join(doorTransform.DOLocalRotate(targetDoorRot, duration).SetEase(easeType));

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
