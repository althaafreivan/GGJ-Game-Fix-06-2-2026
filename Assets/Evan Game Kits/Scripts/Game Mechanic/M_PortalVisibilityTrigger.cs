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
            if (portal == null) portal = GetComponentInParent<Portal>();
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
            if (this == null) return;
            // Portals are not the player, so we always want to check their visibility 
            // when they are being observed by the active player's camera frustum.
            isCurrentlyChecking = true;
        }

        private void Update()
        {
            if (this == null || portal == null) return;

            if (isCurrentlyChecking && M_FrustumDetect.instance != null && targetCollider != null)
            {
                M_FrustumDetect.instance.CheckVisibility(targetCollider, (bool visibleNow) => {
                    if (this != null) HandleVisibilityUpdate(visibleNow);
                });
            }
        }

        private void HandleVisibilityUpdate(bool isVisible)
        {
            if (portal != null)
            {
                // M_FrustumDetect already accounts for isReverse (White cat looking = false).
                // So if isVisible is false, it means it should be frozen.
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
