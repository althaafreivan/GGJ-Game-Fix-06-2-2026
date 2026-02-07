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
        public UnityEvent<Player> OnCollide;
        public override bool isGrounded() => privGrounded;

        private void Start()
        {
            player = GetComponent<Player>();
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (((1 << collision.gameObject.layer) & groundLayer) != 0)
            {
                privGrounded = true;
                OnCollide?.Invoke(player);

                if (collision.gameObject.CompareTag("Bridge"))
                {
                    transform.SetParent(collision.transform);
                }
            }
        }

        private void OnCollisionStay(Collision collision)
        {
            if (((1 << collision.gameObject.layer) & groundLayer) != 0)
            {
                privGrounded = true;
                // Don't invoke OnCollide every frame, or do if needed by other systems
                // OnCollide?.Invoke(player); 
            }
        }

        private void OnCollisionExit(Collision collision)
        {
            if (((1 << collision.gameObject.layer) & groundLayer) != 0)
            {
                privGrounded = false;
                
                if (collision.gameObject.CompareTag("Bridge") && transform.parent == collision.transform)
                {
                    transform.SetParent(null);
                }
            }
        }
    }

}
