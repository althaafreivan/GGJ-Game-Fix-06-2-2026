using UnityEngine;
using UnityEngine.Events;

namespace EvanGameKits.Entity.Module
{
    [RequireComponent(typeof(Base))]
    public class M_MultipleJump : Upforce
    {
        public float jumpForce = 3f;
        public float fallMultiplier = 2.5f;
        public int jumpAmount = 2;
        public UnityEvent onJump;
        private int _jumpAmount = 0;
        GroundDetector groundDetector;

        protected override void Start()
        {
            base.Start();
            _jumpAmount = jumpAmount;
            groundDetector = GetComponent<GroundDetector>();
        }

        public override void ProcessJump(bool isPressed)
        {
            if (groundDetector == null && isPressed) jump();
            if (groundDetector != null && groundDetector.isActiveAndEnabled && groundDetector.isGrounded() && isPressed) { _jumpAmount = jumpAmount-1; onJump?.Invoke(); jump(); }
            else if(groundDetector != null && groundDetector.isActiveAndEnabled && !groundDetector.isGrounded() && isPressed && _jumpAmount>0)
            {
                _jumpAmount--;
                onJump?.Invoke();
                jump();
            }
        }

        private void jump()
        {
            player.rb.linearVelocity = new Vector3(player.rb.linearVelocity.x, jumpForce, player.rb.linearVelocity.z);
        }


    }
}


