using EvanGameKits.Entity;
using UnityEngine;
using UnityEngine.Assertions.Must;
using UnityEngine.Events;

namespace EvanGameKits.Entity.Module
{
    [RequireComponent(typeof(Collider))]
    public class M_ColliderDetection : GroundDetector
    {
        public LayerMask groundLayer;
        private bool privGrounded;
        private Player player;
        private Rigidbody lastGroundRigidbody;
        public UnityEvent<Player> OnCollide;
        public override bool isGrounded() => privGrounded;
        public override Rigidbody GetGroundRigidbody() => lastGroundRigidbody;

        private void Start()
        {
            player = GetComponent<Player>();
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (((1 << collision.gameObject.layer) & groundLayer) != 0)
            {
                privGrounded = true;
                lastGroundRigidbody = collision.rigidbody;
                OnCollide?.Invoke(player);
            }
        }

        private void OnCollisionStay(Collision collision)
        {
            if (((1 << collision.gameObject.layer) & groundLayer) != 0)
            {
                privGrounded = true;
                lastGroundRigidbody = collision.rigidbody;
            }
        }

        private void OnCollisionExit(Collision collision)
        {
            if (((1 << collision.gameObject.layer) & groundLayer) != 0)
            {
                privGrounded = false;
                lastGroundRigidbody = null;
            }
        }
    }

}
