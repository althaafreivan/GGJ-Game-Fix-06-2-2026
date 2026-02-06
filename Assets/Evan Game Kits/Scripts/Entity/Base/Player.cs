using EvanGameKits.Core;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

namespace EvanGameKits.Entity
{
    [RequireComponent(typeof(Rigidbody), typeof(PlayerInput))]
    public class Player : Base, IState
    {
        public static Player ActivePlayer { get; set; }
        public static UnityAction<Player> onPlayerChange;
        private List<IState> states = new List<IState>();

        private Vector3 startPosition;
        private Quaternion startRotation;

        protected override void Awake()
        {
            base.Awake();
            
            states.AddRange(GetComponents<IState>());
        }

        private void Start()
        {
            startPosition = transform.position;
            startRotation = transform.rotation;
        }

        public void Respawn()
        {
            if (rb != null)
            {
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }
            transform.position = startPosition;
            transform.rotation = startRotation;
        }

        public void MuteInput(bool value)
        {
            PlayerInput input = GetComponent<PlayerInput>();
            if (input != null)
            {
                input.enabled = !value;
            }
        }

        protected virtual void OnEnable()
        {
            if (ActivePlayer != this)
            {
                ActivePlayer = this;
                Player.onPlayerChange?.Invoke(this);
            }
        }


        public void OnMove(InputValue context) => MoveInput = context.Get<Vector2>();
        public void OnJump(InputValue context) => IsJumpPressed?.Invoke(context.isPressed);
        public void OnRun(InputValue context) => IsRunPressed?.Invoke(context.Get<float>() >= 0.5f ? true : false);
        public void OnHover(InputValue context) => HoverInput = context.Get<float>();
    }
}

/* public abstract class test
{
    [Header("Player Locomotion Properties")]
    public float speed = 1f;
    public float runMultiplier = 1.1f;
    public float speedSmooth = 0.015f;
    public bool jump = true;
    public float jumpForce = 1f;
    public float fallMultiplier = 2.5f;
    public bool isRotationControlled = true;
    public float rotationSmooth = 1f;

    [Header("Locomotion Event")]
    public UnityEvent onMoveStart;
    public UnityEvent onMoveEnd;
    public UnityEvent onRunStart;
    public UnityEvent onRunEnd;
    public UnityEvent onJump;

    [Header("Locomotion Direction")]
    public bool xDirection = true;
    public bool yDirection = true;

    private float baseSpeed = 1f;
    private Vector2 movement;
    private Vector3 currentVelocity;
    private Rigidbody rb;
    private Camera mainCam;

    private bool isGrounded = true;
    private bool isMoving = false;
    private bool isRunning = false;



    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        if (rb == null) rb = gameObject.AddComponent<Rigidbody>();
        baseSpeed = speed;
        mainCam = Camera.main;
    }

    public void OnJump(InputValue context) { if (isGrounded && jump) rb.linearVelocity = new Vector3(rb.linearVelocity.x, jumpForce, rb.linearVelocity.z); Debug.Log(isGrounded); onJump.Invoke(); }

    public void OnRun(InputValue context)
    {
        isRunning = context.Get<float>() >= 0.5f ? true : false;
        if (isRunning)
        {
            speed = baseSpeed * runMultiplier;
            onRunStart.Invoke();
            Debug.Log("on this");
        }
        else if (!isRunning)
        {
            onRunEnd.Invoke();
            speed = baseSpeed;
            Debug.Log("on this not run");
        }
    }

    public void OnMove(InputValue context)
    {
        Vector2 oldMovement = movement;
        movement = context.Get<Vector2>();

        if (movement.magnitude > 0 && !isMoving)
        {
            isMoving = true;
            onMoveStart.Invoke();
        }
        else if (movement.magnitude == 0 && isMoving)
        {
            isMoving = false;
            onMoveEnd.Invoke();
        }
    }

    private void FixedUpdate()
    {
        Vector3 direction = Quaternion.Euler(0, mainCam.transform.eulerAngles.y, 0) * new Vector3((xDirection ? (movement.x * speed) : 0), rb.linearVelocity.y, (yDirection ? (movement.y * speed) : 0));
        rb.linearVelocity = Vector3.SmoothDamp(rb.linearVelocity, direction, ref currentVelocity, speedSmooth);

        Vector3 horizontalDirection = new Vector3(direction.x, 0, direction.z);

        if (horizontalDirection.sqrMagnitude > .01f && isRotationControlled)
        {
            Quaternion targetRotation = Quaternion.LookRotation(horizontalDirection);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSmooth * Time.fixedDeltaTime);
        }

        // agar tidak floatyy pas lompat wkwkwk
        if (rb.linearVelocity.y < 0)
        {
            rb.linearVelocity += Vector3.up * Physics.gravity.y * (fallMultiplier - 1) * Time.fixedDeltaTime;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = true;
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = false;
        }
    }

    public virtual void Move()
    {

    }
}
*/