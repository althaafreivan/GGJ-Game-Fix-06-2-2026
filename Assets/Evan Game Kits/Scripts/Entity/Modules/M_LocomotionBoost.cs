using UnityEngine;
using EvanGameKits.Entity.Module.Interface;
using UnityEngine.Events;

namespace EvanGameKits.Entity.Module
{
    [RequireComponent(typeof(Player))]
    public class M_LocomotionBoost : MonoBehaviour
    {
        protected Player player;
        private IHasSpeed targetModule;

        public float speedMultiplier = 2f;
        public float acceleration = 1f;
        public UnityEvent<bool> onBoost;

        private float targetSpeed;
        private float velocityRef;
        private float baseSpeed;
        private bool isBoosting;

        protected void Awake()
        {
            player = GetComponent<Player>();
            targetModule = GetComponent<IHasSpeed>();
        }

        private void Start()
        {
            if (targetModule != null)
            {
                baseSpeed = targetModule.speed;
                targetSpeed = baseSpeed;
            }
        }

        private void OnEnable()
        {
            if (player != null)
                player.IsRunPressed += Sprint;
        }

        private void OnDisable()
        {
            if (player != null)
                player.IsRunPressed -= Sprint;

            if (targetModule != null)
            {
                targetModule.speed = baseSpeed;
                isBoosting = false;
            }
        }

        protected void Sprint(bool val)
        {
            if (targetModule == null) return;

            isBoosting = val;
            targetSpeed = val ? baseSpeed * speedMultiplier : baseSpeed;
            onBoost?.Invoke(val);

            if (acceleration <= 0f)
            {
                targetModule.speed = targetSpeed;
            }
        }

        private void Update()
        {
            if (targetModule == null || acceleration <= 0f) return;

            if (Mathf.Approximately(targetModule.speed, targetSpeed)) return;

            targetModule.speed = Mathf.SmoothDamp(
                 targetModule.speed,
                 targetSpeed,
                 ref velocityRef,
                 1f / acceleration
            );
        }
    }
}

namespace EvanGameKits.Entity.Module.Interface
{
    public interface IHasSpeed
    {
        float speed { get; set; }
    }
}

