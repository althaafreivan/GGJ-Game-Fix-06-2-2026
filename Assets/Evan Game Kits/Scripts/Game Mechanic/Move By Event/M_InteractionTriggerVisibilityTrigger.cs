using UnityEngine;
using EvanGameKits.Entity;
using EvanGameKits.Entity.Module;

namespace EvanGameKits.Mechanic
{
    public class M_InteractionTriggerVisibilityTrigger : MonoBehaviour
    {
        private InteractionTrigger trigger;
        private Collider targetCollider;
        private bool isCurrentlyChecking = false;

        private void Awake()
        {
            trigger = GetComponent<InteractionTrigger>();
            targetCollider = GetComponent<Collider>();
            
            if (targetCollider == null) targetCollider = GetComponentInChildren<Collider>();
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
            lastVisibleState = false;
        }

        private void Update()
        {
            if (isCurrentlyChecking && M_FrustumDetect.instance != null && targetCollider != null)
            {
                M_FrustumDetect.instance.CheckVisibility(targetCollider, (bool visibleNow) => HandleVisibilityUpdate(visibleNow));
            }
        }

        private bool lastVisibleState = false;
        private bool isPlayerInside = false;

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                isPlayerInside = true;
                // Re-check visibility when entering to trigger notification if already looking
                CheckAndNotify();
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                isPlayerInside = false;
            }
        }

        private void HandleVisibilityUpdate(bool isVisible)
        {
            if (trigger != null)
            {
                trigger.SetFrozen(!isVisible);
                CheckAndNotify();
            }
        }

        private void CheckAndNotify()
        {
            if (Camera.main == null) return;

            var activePlayer = Player.ActivePlayer;
            if (activePlayer == null) return;

            var identity = activePlayer.GetComponent<M_CatIdentity>();
            if (identity == null) identity = activePlayer.GetComponentInParent<M_CatIdentity>();

            if (identity == null || identity.catType != CatType.White) 
            {
                lastVisibleState = false; 
                return;
            }
            
            if (targetCollider == null) return;

            Plane[] planes = GeometryUtility.CalculateFrustumPlanes(Camera.main);
            bool actuallyInFrustum = GeometryUtility.TestPlanesAABB(planes, targetCollider.bounds);

            if (actuallyInFrustum && isPlayerInside && !lastVisibleState)
            {
                NotificationController.instance?.ShowNotification("Expected can't activate a thing in its view, try to move away your window and check from map");
            }
            lastVisibleState = actuallyInFrustum;
        }

        private void OnBecameVisible()
        {
            isCurrentlyChecking = true;
        }

        private void OnBecameInvisible()
        {
            isCurrentlyChecking = false;
            bool isReverse = M_FrustumDetect.instance != null && M_FrustumDetect.instance.isReverse;
            HandleVisibilityUpdate(isReverse);
        }
    }
}
