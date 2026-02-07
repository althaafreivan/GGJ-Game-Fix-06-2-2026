using UnityEngine;
using UnityEngine.Events;
using EvanGameKits.Mechanic;

namespace EvanGameKits.Entity.Module
{
    [RequireComponent(typeof(Base))]
    public class M_Stamina : MonoBehaviour
    {
        [Header("Configuration")]
        [SerializeField] private bool alwaysRegen = true;
        [SerializeField] private float singleConsume = 10f;
        [SerializeField] private float multipleConsume = 20f;
        [SerializeField] private float regenRate = 5f;
        [SerializeField] public float maxStamina = 100f;

        private M_LocomotionBoost boost;
        private M_BasicJump upforce;
        private M_MultipleJump mUpforce;

        [SerializeField] private float stamina;
        private bool continuousConsumeActive;
        private Base entity;

        [Header("Events")]
        public UnityEvent<float> onStaminaChange;

        private void Awake()
        {
            entity = GetComponent<Base>();
        }

        private void Start()
        {
            stamina = maxStamina;

            upforce = GetComponent<M_BasicJump>();
            mUpforce = GetComponent<M_MultipleJump>();
            boost = GetComponent<M_LocomotionBoost>();

            if (upforce != null) upforce.onJump.AddListener(ConsumeOnce);
            if (mUpforce != null) mUpforce.onJump.AddListener(ConsumeOnce);

            if (boost != null)
                boost.onBoost.AddListener((bool val) => { continuousConsumeActive = val; });
        }

        private void OnEnable()
        {
            if (entity != null)
            {
                entity.IsJumpPressed += OnJumpAttempt;
                entity.IsRunPressed += OnRunAttempt;
            }
        }

        private void OnDisable()
        {
            if (entity != null)
            {
                entity.IsJumpPressed -= OnJumpAttempt;
                entity.IsRunPressed -= OnRunAttempt;
            }
        }

        private void OnJumpAttempt(bool isPressed)
        {
            if (isPressed && stamina < singleConsume)
            {
                ShowLowStaminaNotification(singleConsume);
            }
        }

        private void OnRunAttempt(bool isPressed)
        {
            if (isPressed && stamina <= 0)
            {
                // To start boosting, we at least need some stamina
                ShowLowStaminaNotification(multipleConsume * Time.fixedDeltaTime * 5f); // Arbitrary small amount (e.g. 5 frames worth)
            }
        }

        private void ShowLowStaminaNotification(float requiredStamina)
        {
            float needed = requiredStamina - stamina;
            if (needed <= 0) return;

            float timeToRegen = needed / regenRate;
            string formattedTime = timeToRegen.ToString("F1");
            NotificationController.instance?.ShowNotification($"Stamina is too low, regen for {formattedTime}s to do your action");
        }

        private void FixedUpdate()
        {
            Regenerate();
            HandleContinuousConsumption();
            UpdateComponentStates();
            onStaminaChange?.Invoke(stamina);
        }

        private void ConsumeOnce()
        {
            stamina = Mathf.Max(0, stamina - singleConsume);
        }

        private void HandleContinuousConsumption()
        {
            if (continuousConsumeActive && stamina > 0f)
            {
                stamina -= multipleConsume * Time.fixedDeltaTime;
                if (stamina <= 0f)
                {
                    stamina = 0f;
                    continuousConsumeActive = false;
                }
            }
        }

        private void Regenerate()
        {
            if (!continuousConsumeActive && stamina < maxStamina)
            {
                stamina = Mathf.Min(maxStamina, stamina + regenRate * Time.fixedDeltaTime);
            }
        }

        private void UpdateComponentStates()
        {
            bool canJump = stamina >= singleConsume;
            if (upforce != null) upforce.enabled = canJump;
            if (mUpforce != null) mUpforce.enabled = canJump;

            bool canBoost = stamina > 0;
            if (boost != null) boost.enabled = canBoost;
        }
    }
}