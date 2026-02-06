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
        }

        private void OnDisable()
        {
            Player.onPlayerChange -= OnPlayerChange;
        }
        
        private void OnPlayerChange(Player newPlayer)
        {
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
                HandleVisibilityUpdate(false);
            }
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
        }
    }
}