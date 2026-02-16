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
            if (isPressed)
            {
                bool canJumpActually = (groundDetector == null) || (groundDetector.isActiveAndEnabled && groundDetector.isGrounded());
                if (canJumpActually)
                {
                    jump();
                }
            }
        }

        private void jump()
        {
            M_Stamina staminaModule = GetComponent<M_Stamina>();
            if (staminaModule != null)
            {
                if (staminaModule.stamina < staminaModule.singleConsume)
                {
                    staminaModule.ShowLowStaminaNotification(staminaModule.singleConsume);
                    return;
                }
            }

            onJump?.Invoke();
            player.rb.linearVelocity = new Vector3(player.rb.linearVelocity.x, jumpForce, player.rb.linearVelocity.z);
        }
    }
}


