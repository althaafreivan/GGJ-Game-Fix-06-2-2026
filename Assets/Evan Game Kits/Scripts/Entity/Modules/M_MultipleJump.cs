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
            if (isPressed)
            {
                bool tryingToJump = false;
                if (groundDetector == null) tryingToJump = true;
                else if (groundDetector.isActiveAndEnabled && groundDetector.isGrounded()) tryingToJump = true;
                else if (groundDetector.isActiveAndEnabled && !groundDetector.isGrounded() && _jumpAmount > 0) tryingToJump = true;

                if (tryingToJump)
                {
                    M_Stamina staminaModule = GetComponent<M_Stamina>();
                    if (staminaModule != null && staminaModule.stamina < staminaModule.singleConsume)
                    {
                        staminaModule.ShowLowStaminaNotification(staminaModule.singleConsume);
                        return;
                    }

                    if (groundDetector == null) jump();
                    else if (groundDetector.isGrounded()) { _jumpAmount = jumpAmount - 1; onJump?.Invoke(); jump(); }
                    else { _jumpAmount--; onJump?.Invoke(); jump(); }
                }
            }
        }

        private void jump()
        {
            player.rb.linearVelocity = new Vector3(player.rb.linearVelocity.x, jumpForce, player.rb.linearVelocity.z);
        }


    }
}


