using UnityEngine;
using EvanGameKits.Entity;
using EvanGameKits.Entity.Module;

namespace EvanGameKits.Mechanic
{
    public class M_PortalVisibilityTrigger : MonoBehaviour
    {
        private Portal portal;
        private Collider targetCollider;
        private bool isCurrentlyChecking = false;

        private void Awake()
        {
            portal = GetComponent<Portal>();
            targetCollider = GetComponent<Collider>();
        }

        private void OnEnable()
        {
            Player.onPlayerChange += OnPlayerChange;
            // Initialize checking state based on current active player
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
            // Portals are not the player, so we always want to check their visibility 
            // when they are being observed by the active player's camera frustum.
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
            if (portal != null)
            {
                // If it is visible in the camera (or isReverse logic is applied in M_FrustumDetect), 
                // and we want it to work only when looking at it, then it should NOT be frozen when visible.
                // If the player "freezes" things by looking away, then:
                // isVisible = true -> isFrozen = false
                // isVisible = false -> isFrozen = true
                portal.SetFrozen(!isVisible);
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
