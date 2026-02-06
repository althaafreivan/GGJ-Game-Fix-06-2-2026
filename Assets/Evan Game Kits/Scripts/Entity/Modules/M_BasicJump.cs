using UnityEngine;
using UnityEngine.Events;

namespace EvanGameKits.Entity.Module
{
    [RequireComponent(typeof(Player))]
    public class M_BasicJump : Upforce
    {
        public float jumpForce = 3f;
        public float fallMultiplier = 2.5f;
        GroundDetector groundDetector;
        public UnityEvent onJump;

        protected override void Start()
        {
            base.Start();
            groundDetector = GetComponent<GroundDetector>();
        }

        public override void ProcessJump(bool isPressed)
        {
            if (groundDetector == null && isPressed) jump(); 
            if (groundDetector != null && groundDetector.isActiveAndEnabled && groundDetector.isGrounded() && isPressed) jump(); 
        }

        private void jump()
        {
            onJump?.Invoke();
            player.rb.linearVelocity = new Vector3(player.rb.linearVelocity.x, jumpForce, player.rb.linearVelocity.z);
        }
    }
}


