using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace EvanGameKits.Entity.Module
{
    public class M_VisibilityTrigger : MonoBehaviour
    {
        private Collider targetCollider;
        private Rigidbody rb;
        private RigidbodyConstraints originalConstraints;
        private Locomotion locomotion;
        private Player player;
        private Upforce upforce;
        private List<AIBehaviourModule> aiBehaviourModule;
        private EvanGameKits.Mechanic.TransformStateTweener tweener;
        private bool isCurrentlyChecking = false;

        private void Awake()
        {
            player = GetComponent<Player>();
        }

        private void OnEnable()
        {
            Player.onPlayerChange += OnPlayerChange;
            if (Player.ActivePlayer != null) OnPlayerChange(Player.ActivePlayer);
        }

        private void OnDisable()
        {
            Player.onPlayerChange -= OnPlayerChange;
        }

        private bool isRegistered = false;

        private void OnPlayerChange(Player newPlayer)
        {
            // Force re-registration as the manager instance likely changed with the player
            isRegistered = false;

            if(player != null && player == newPlayer)
            {
                isCurrentlyChecking = false;
                HandleVisibilityUpdate(true);
            }
            else
            {
                isCurrentlyChecking = true;
            }
        }

        private void Start()
        {
            targetCollider = GetComponent<Collider>();
            if (targetCollider == null) targetCollider = GetComponentInChildren<Collider>();

            rb = GetComponent<Rigidbody>();
            if (rb != null)
            {
                originalConstraints = rb.constraints;
                rb.isKinematic = false;
            }

            locomotion = GetComponent<Locomotion>();
            upforce = GetComponent<Upforce>();
            aiBehaviourModule = GetComponents<AIBehaviourModule>().ToList();
            tweener = GetComponent<EvanGameKits.Mechanic.TransformStateTweener>();
        }

        private void OnBecameVisible()
        {
            if(player == null || player != Player.ActivePlayer)
            isCurrentlyChecking = true;
        }

        private void OnBecameInvisible()
        {
            if (player == null || player != Player.ActivePlayer)
            {
                isCurrentlyChecking = false;
                // If we stop checking, we should also handle the visibility state?
                // Original code relied on CheckVisibility returning true/false.
                // Here we might just unregister, but we should probably update state first?
                // Actually, if we stop checking, we assume it's NOT visible (or handle it).
                // But the original code called HandleVisibilityUpdate in Update loop.
                // Wait, if isCurrentlyChecking becomes false, Update didn't run.
                // So HandleVisibilityUpdate wasn't called.
                // But OnBecameInvisible handles the "became invisible" event itself?
                bool isReverse = M_FrustumDetect.instance != null && M_FrustumDetect.instance.isReverse;
                HandleVisibilityUpdate(isReverse);
            }
        }

        private void Update()
        {
            // Manage registration state based on requirements
            if (isCurrentlyChecking)
            {
                if (!isRegistered)
                {
                    if (M_FrustumDetect.instance != null && targetCollider != null)
                    {
                        M_FrustumDetect.instance.Register(targetCollider, HandleVisibilityUpdate);
                        isRegistered = true;
                    }
                }
            }
            else
            {
                if (isRegistered)
                {
                    if (M_FrustumDetect.instance != null && targetCollider != null)
                    {
                        M_FrustumDetect.instance.Unregister(targetCollider);
                    }
                    isRegistered = false;
                }
            }
        }

        private void HandleVisibilityUpdate(bool isVisible)
        {
            if (rb != null) rb.constraints = isVisible ? originalConstraints : RigidbodyConstraints.FreezeAll;
            if (locomotion != null) locomotion.enabled = isVisible;
            if (upforce != null) upforce.enabled = isVisible;
            if (aiBehaviourModule != null)
            {
                foreach(AIBehaviourModule module in aiBehaviourModule)
                {
                    module.enabled = isVisible;
                }
            }
            if (tweener != null) tweener.SetFrozen(!isVisible);

            // Toggle collider to force OnTriggerExit when frozen, and OnTriggerEnter when thawed.
            // This ensures pressure plates (InteractionTrigger) reset correctly when the object is culled.
            if (targetCollider != null) targetCollider.enabled = isVisible;
        }
    }
}
