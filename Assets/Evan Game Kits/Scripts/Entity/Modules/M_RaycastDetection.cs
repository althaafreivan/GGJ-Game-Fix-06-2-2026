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
        private Rigidbody lastGroundRigidbody;

        private void Start()
        {
            player = GetComponent<Player>();
        }

        public override Rigidbody GetGroundRigidbody() => lastGroundRigidbody;

        public override bool isGrounded()
        {
            Collider[] colliders = Physics.OverlapSphere(transform.position + offset, detectionRadius, groundLayer);
            
            lastGroundRigidbody = null;
            foreach (var col in colliders)
            {
                // If the collider is not part of this player's hierarchy, we found valid ground
                if (!col.transform.IsChildOf(transform))
                {
                    OnCollide?.Invoke(player);
                    lastGroundRigidbody = col.attachedRigidbody;
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
