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

        private void Start()
        {
            player = GetComponent<Player>();
        }

        public override bool isGrounded()
        {
            Collider[] colliders = Physics.OverlapSphere(transform.position + offset, detectionRadius, groundLayer);
            
            foreach (var col in colliders)
            {
                // If the collider is not part of this player's hierarchy, we found valid ground
                if (!col.transform.IsChildOf(transform))
                {
                    OnCollide?.Invoke(player);
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
