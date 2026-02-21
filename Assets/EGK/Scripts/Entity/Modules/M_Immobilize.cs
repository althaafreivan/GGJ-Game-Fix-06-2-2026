using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using EvanGameKits.Entity;
using UnityEngine.InputSystem;

namespace EvanGameKits.Entity.Module
{
    /// <summary>
    /// Module to immobilize the player.
    /// It disables locomotion modules, mutes input, and stops the Rigidbody.
    /// Designed to handle player swapping correctly.
    /// </summary>
    [RequireComponent(typeof(Player))]
    public class M_Immobilize : MonoBehaviour
    {
        private Player player;
        private PlayerInput playerInput;
        private bool isImmobilized;
        private List<MonoBehaviour> modulesToReEnable = new List<MonoBehaviour>();
        private Coroutine activeTimerCoroutine;

        [Header("Settings")]
        [Tooltip("If true, the player will be completely frozen in place (no gravity).")]
        [SerializeField] private bool freezeInPlace = true;

        private bool wasUsingGravity;
        private bool wasKinematic;

        private void Awake()
        {
            player = GetComponent<Player>();
            playerInput = GetComponent<PlayerInput>();
        }

        /// <summary>
        /// Immobilizes the player for the given duration in seconds.
        /// </summary>
        /// <param name="duration">How long to immobilize.</param>
        public void Immobilize(float duration)
        {
            if (duration <= 0) return;
            
            if (activeTimerCoroutine != null)
            {
                StopCoroutine(activeTimerCoroutine);
            }
            
            activeTimerCoroutine = StartCoroutine(ImmobilizeRoutine(duration));
        }

        /// <summary>
        /// Manually starts immobilization.
        /// </summary>
        public void StartImmobilize()
        {
            if (isImmobilized) return;
            isImmobilized = true;

            // 1. Disable movement-related modules
            modulesToReEnable.Clear();
            
            // Disable Locomotion (Walk, Fly, etc.)
            var locomotionModules = GetComponents<Locomotion>();
            foreach (var module in locomotionModules)
            {
                if (module != null && module.enabled)
                {
                    module.enabled = false;
                    modulesToReEnable.Add(module);
                }
            }

            // Disable Upforce (Jump, Jetpack, etc.)
            var upforceModules = GetComponents<Upforce>();
            foreach (var module in upforceModules)
            {
                if (module != null && module.enabled)
                {
                    module.enabled = false;
                    modulesToReEnable.Add(module);
                }
            }

            // 2. Disable Input directly to be sure
            if (playerInput != null)
            {
                playerInput.enabled = false;
            }

            // 3. Handle Rigidbody
            if (player.rb != null)
            {
                player.rb.linearVelocity = Vector3.zero;
                player.rb.angularVelocity = Vector3.zero;

                if (freezeInPlace)
                {
                    wasUsingGravity = player.rb.useGravity;
                    wasKinematic = player.rb.isKinematic;

                    player.rb.useGravity = false;
                    player.rb.isKinematic = true;
                }
            }
        }

        /// <summary>
        /// Manually stops immobilization and restores previous state.
        /// </summary>
        public void StopImmobilize()
        {
            if (!isImmobilized) return;

            if (activeTimerCoroutine != null)
            {
                StopCoroutine(activeTimerCoroutine);
                activeTimerCoroutine = null;
            }

            // 1. Restore Rigidbody state
            if (player.rb != null && freezeInPlace)
            {
                player.rb.isKinematic = wasKinematic;
                player.rb.useGravity = wasUsingGravity;
            }

            // 2. Restore movement modules
            foreach (var module in modulesToReEnable)
            {
                if (module != null) module.enabled = true;
            }
            modulesToReEnable.Clear();

            // 3. Restore Input ONLY if the player is active.
            // This prevents an inactive cat from being re-enabled if a swap happened during immobilization.
            if (player.enabled && playerInput != null)
            {
                playerInput.enabled = true;
            }
            
            isImmobilized = false;
        }

        private void Update()
        {
            // Enforce immobilization state.
            // This prevents other modules (like M_SwapPlayer) from re-enabling input 
            // on this entity while it is supposed to be immobilized.
            if (isImmobilized)
            {
                if (playerInput != null && playerInput.enabled)
                {
                    playerInput.enabled = false;
                }

                if (freezeInPlace && player.rb != null && !player.rb.isKinematic)
                {
                    player.rb.isKinematic = true;
                    player.rb.useGravity = false;
                }
            }
        }

        private IEnumerator ImmobilizeRoutine(float duration)
        {
            StartImmobilize();
            yield return new WaitForSeconds(duration);
            activeTimerCoroutine = null;
            StopImmobilize();
        }
    }
}
