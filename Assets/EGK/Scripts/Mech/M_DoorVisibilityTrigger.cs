using UnityEngine;
using EvanGameKits.Entity;
using EvanGameKits.Entity.Module;

namespace EvanGameKits.Mechanic
{
    [RequireComponent(typeof(DoorMapController))]
    public class M_DoorVisibilityTrigger : MonoBehaviour
    {
        private DoorMapController doorController;
        private Collider targetCollider;
        private bool isCurrentlyChecking = false;

        private void Awake()
        {
            doorController = GetComponent<DoorMapController>();
            targetCollider = GetComponent<Collider>();
            
            if (targetCollider == null)
            {
                // Try to find a collider in children if the root doesn't have one
                targetCollider = GetComponentInChildren<Collider>();
            }
        }

        private void OnEnable()
        {
            Player.onPlayerChange += OnPlayerChange;
            if (Player.ActivePlayer != null)
            {
                OnPlayerChange(Player.ActivePlayer);
            }
        }

        private void OnDisable()
        {
            Player.onPlayerChange -= OnPlayerChange;
        }

        private void OnPlayerChange(Player newPlayer)
        {
            isCurrentlyChecking = true;
        }

        private void Update()
        {
            if (isCurrentlyChecking && M_FrustumDetect.instance != null && targetCollider != null)
            {
                M_FrustumDetect.instance.CheckVisibility(targetCollider, (bool visibleNow) => HandleVisibilityUpdate(visibleNow));
            }
        }

        private void HandleVisibilityUpdate(bool isVisible)
        {
            if (doorController != null)
            {
                // When isVisible is true, it should NOT be frozen (isFrozen = false)
                // When isVisible is false, it should BE frozen (isFrozen = true)
                doorController.SetFrozen(!isVisible);
            }
        }

        private void OnBecameVisible()
        {
            isCurrentlyChecking = true;
        }

        private void OnBecameInvisible()
        {
            isCurrentlyChecking = false;
            HandleVisibilityUpdate(false);
        }
    }
}
