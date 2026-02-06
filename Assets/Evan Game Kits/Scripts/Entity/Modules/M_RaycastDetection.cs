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

        public override bool isGrounded(){
            bool onCollide = Physics.CheckSphere(transform.position + offset, detectionRadius, groundLayer);
            if(onCollide) OnCollide?.Invoke(player);
            return onCollide;
            }

        private void OnDrawGizmos()
        {
            Gizmos.color = isGrounded() ? Color.green : Color.red;
            Gizmos.DrawSphere(transform.position + offset, detectionRadius);
        }
    }

}
