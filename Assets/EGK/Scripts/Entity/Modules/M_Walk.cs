using UnityEngine;
using UnityEngine.Events;
using EvanGameKits.Entity.Module.Interface;

namespace EvanGameKits.Entity.Module
{
    [RequireComponent(typeof(Base))]
    public class M_Walk : Locomotion, IHasSpeed
    {
        public float movementSpeed = 5f;
        
        public float speed
        {
            get => movementSpeed;
            set => movementSpeed = value;
        }

        public float speedInterpolation = .15f;
        public float rotationInterpolation = .15f;
        public float fallMultiplier = 2f;
        public bool isRotationControlled = true;
        public bool xDirection = true, yDirection = true;
        public UnityEvent<float> onVelocity;
        public UnityEvent<bool> onMove;
        private float smoothInput;
        private Vector3 currentVelocity;
        private Camera cam;
        private GroundDetector groundDetector;

        protected override void Start()
        {
            base.Start();
            cam = Camera.main;
            groundDetector = GetComponent<GroundDetector>();
        }

        public override void ProcessMovement()
        {
            Vector3 targetLinearVelocity = Quaternion.Euler(0, cam.transform.eulerAngles.y, 0) * new Vector3((xDirection ? (entity.MoveInput.x * movementSpeed) : 0), 0, (yDirection ? (entity.MoveInput.y * movementSpeed) : 0));
            
            // Get platform velocity if grounded
            Vector3 platformVelocity = Vector3.zero;
            if (groundDetector != null && groundDetector.isGrounded())
            {
                Rigidbody platformRb = groundDetector.GetGroundRigidbody();
                if (platformRb != null)
                {
                    platformVelocity = platformRb.linearVelocity;
                }
            }

            // Combine desired movement with platform movement, preserving vertical velocity for jumping/falling
            Vector3 finalVelocity = targetLinearVelocity + platformVelocity;
            finalVelocity.y = entity.rb.linearVelocity.y;

            // Apply smooth damp to the combined velocity
            entity.rb.linearVelocity = Vector3.SmoothDamp(entity.rb.linearVelocity, finalVelocity, ref currentVelocity, speedInterpolation);

            float vel = entity.MoveInput.magnitude;
            if (vel>0 || vel<0)
            {
                onMove?.Invoke(true);
            }
            else
            {
                onMove?.Invoke(false);
            }
            smoothInput = Mathf.MoveTowards(smoothInput, vel, 1f * Time.fixedDeltaTime);
            onVelocity.Invoke((smoothInput < .01f) ? 0f: smoothInput);

            Vector3 horizontalDirection = new Vector3(targetLinearVelocity.x, 0, targetLinearVelocity.z);

            if (horizontalDirection.sqrMagnitude > .01f && isRotationControlled)
            {
                Quaternion targetRotation = Quaternion.LookRotation(horizontalDirection);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationInterpolation * Time.fixedDeltaTime);
            }

            // agar tidak floatyy pas lompat wkwkwk
            if (entity.rb.linearVelocity.y < 0)
            {
                entity.rb.linearVelocity += Vector3.up * Physics.gravity.y * (fallMultiplier - 1) * Time.fixedDeltaTime;

            }
        }
    }
}


