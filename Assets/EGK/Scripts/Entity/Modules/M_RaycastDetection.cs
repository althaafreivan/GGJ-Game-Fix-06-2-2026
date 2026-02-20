using EvanGameKits.Entity.Module;
using EvanGameKits.Entity;
using UnityEngine;
using UnityEngine.Events;

namespace EvanGameKits.Entity.Module
{
    public class M_RaycastDetection : GroundDetector
    {
        public float detectionRadius = .3f;
        public Vector3 offset;
        public LayerMask groundLayer;
        public UnityEvent<Player> OnCollide;
        private Player player;
        private M_SwapPlayer swapPlayer;
        private Rigidbody lastGroundRigidbody;
        private Transform lastGroundTransform;

        private void Start()
        {
            player = GetComponent<Player>();
            swapPlayer = GetComponent<M_SwapPlayer>();
            swapPlayer.OnPlayerDeactivate?.AddListener(playerDeactivate);
            swapPlayer.OnPlayerActivate?.AddListener(playerActivate);
        }

        private void playerActivate()
        {
            transform.parent = null;
        }

        private void playerDeactivate()
        {
            isGrounded();
            if (lastGroundTransform != null)
            {
                if (transform.parent != lastGroundTransform) transform.parent = lastGroundTransform;
            }
            else
            {
                transform.parent = null;
            }
        }

        public override Rigidbody GetGroundRigidbody() => lastGroundRigidbody;

        public override bool isGrounded()
        {
            Collider[] colliders = Physics.OverlapSphere(transform.position + offset, detectionRadius, groundLayer);
            
            lastGroundRigidbody = null;
            lastGroundTransform = null;
            foreach (var col in colliders)
            {
                // If the collider is not part of this player's hierarchy, we found valid ground
                if (!col.transform.IsChildOf(transform))
                {
                    OnCollide?.Invoke(player);
                    lastGroundRigidbody = col.attachedRigidbody;
                    lastGroundTransform = col.transform;
                    return true;
                }
            }

            return false;
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = isGrounded() ? Color.green : Color.red;
            Gizmos.DrawSphere(transform.position + offset, detectionRadius);
        }
    }

}
