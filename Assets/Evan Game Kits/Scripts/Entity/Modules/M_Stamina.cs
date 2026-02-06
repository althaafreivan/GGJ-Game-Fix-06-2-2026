using UnityEngine;
using UnityEngine.Events;

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
        [SerializeField] private float maxStamina = 100f;

        private M_LocomotionBoost boost;
        private M_BasicJump upforce;
        private M_MultipleJump mUpforce;

        [SerializeField] private float stamina;
        private bool continuousConsumeActive;

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

        private void FixedUpdate()
        {
            Regenerate();
            HandleContinuousConsumption();
            UpdateComponentStates();
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