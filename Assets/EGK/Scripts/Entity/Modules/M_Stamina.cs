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
        [SerializeField] public float singleConsume = 10f;
        [SerializeField] public float multipleConsume = 20f;
        [SerializeField] public float regenRate = 5f;
        [SerializeField] public float maxStamina = 100f;

        private M_LocomotionBoost boost;
        private M_BasicJump upforce;
        private M_MultipleJump mUpforce;

        [SerializeField] public float stamina;
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

        public void ShowLowStaminaNotification(float requiredStamina)
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
            onStaminaChange?.Invoke(stamina);
        }

        private void ConsumeOnce()
        {
            stamina = Mathf.Max(0, stamina - singleConsume);
        }

        private void HandleContinuousConsumption()
        {
            bool isMoving = entity.MoveInput.sqrMagnitude > 0.01f;
            if (continuousConsumeActive && isMoving && stamina > 0f)
            {
                stamina -= multipleConsume * Time.fixedDeltaTime;
                if (stamina <= 0f)
                {
                    stamina = 0f;
                }
            }
        }

        private void Regenerate()
        {
            bool isConsuming = continuousConsumeActive && entity.MoveInput.sqrMagnitude > 0.01f && stamina > 0f;
            if (!isConsuming && stamina < maxStamina)
            {
                stamina = Mathf.Min(maxStamina, stamina + regenRate * Time.fixedDeltaTime);
            }
        }
    }
}