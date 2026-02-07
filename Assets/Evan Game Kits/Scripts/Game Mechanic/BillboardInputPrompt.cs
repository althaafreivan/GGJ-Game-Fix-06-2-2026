using UnityEngine;
using UnityEngine.InputSystem;
using DG.Tweening;

namespace EvanGameKits.Mechanic
{
    public class BillboardInputPrompt : MonoBehaviour
    {
        [Header("Billboard Settings")]
        [SerializeField] private bool lockYAxis = true;
        
        [Header("Animation Settings")]
        [SerializeField] private Key activationKey = Key.E;
        [SerializeField] private float punchScale = 1.2f;
        [SerializeField] private float animationDuration = 0.2f;
        [SerializeField] private float showHideDuration = 0.3f;
        
        [Header("References")]
        [SerializeField] private GameObject iconObject;

        private Camera mainCam;
        private bool isPlayerInRange = false;
        private Transform iconTransform;

        private void Start()
        {
            mainCam = Camera.main;
            
            if (iconObject == null && transform.childCount > 0) 
                iconObject = transform.GetChild(0).gameObject;

            if (iconObject != null)
            {
                iconTransform = iconObject.transform;
                iconObject.SetActive(false);
                iconTransform.localScale = Vector3.zero;
            }
        }

        private void LateUpdate()
        {
            if (mainCam == null || !isPlayerInRange || iconTransform == null) return;

            // Face the camera
            Vector3 targetDir = iconTransform.position - mainCam.transform.position;
            if (lockYAxis) targetDir.y = 0;
            
            if (targetDir != Vector3.zero)
            {
                // Rotate ONLY the icon, not the root object (the door)
                iconTransform.rotation = Quaternion.LookRotation(targetDir);
            }

            // Check for key press animation
            if (Keyboard.current[activationKey].wasPressedThisFrame)
            {
                AnimateFeedback();
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player") && iconObject != null)
            {
                isPlayerInRange = true;
                iconObject.SetActive(true);
                iconTransform.DOKill();
                iconTransform.DOScale(Vector3.one, showHideDuration).SetEase(Ease.OutBack);
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.CompareTag("Player") && iconObject != null)
            {
                isPlayerInRange = false;
                iconTransform.DOKill();
                iconTransform.DOScale(Vector3.zero, showHideDuration)
                    .SetEase(Ease.InBack)
                    .OnComplete(() => iconObject.SetActive(false));
            }
        }

        public void AnimateFeedback()
        {
            if (iconTransform == null) return;

            iconTransform.DOKill(true);
            iconTransform.DOPunchScale(Vector3.one * (punchScale - 1f), animationDuration, 5, 1);
        }
    }
}
