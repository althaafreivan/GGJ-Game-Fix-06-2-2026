using UnityEngine;
using EvanGameKits.Entity;
using EvanGameKits.Entity.Module;

namespace EvanGameKits.Mechanic
{
    public class M_TransformStateTweenerVisibilityTrigger : MonoBehaviour
    {
        private TransformStateTweener tweener;
        private Collider targetCollider;
        private bool isCurrentlyChecking = false;

        private void Awake()
        {
            tweener = GetComponent<TransformStateTweener>();
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
            if (tweener != null)
            {
                tweener.SetFrozen(!isVisible);
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
