using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using UnityEngine.Events;

namespace EvanGameKits.Entity.Module
{
    public class M_SwapPlayer : MonoBehaviour
    {
        [SerializeField] public bool MainPlayer = false;
        [SerializeField] private GameObject targetPlayerObject;
        public UnityEvent OnPlayerDeactivate;
        public UnityEvent OnPlayerActivate;
        [HideInInspector] Player player;
        private M_FrustumDetect frustumModule;
        private CinemachineVirtualCameraBase cineCam;
        private PlayerInput input;

        private void Awake()
        {
            input = GetComponent<PlayerInput>();
            player = GetComponent<Player>();
            frustumModule = GetComponent<M_FrustumDetect>();

            if (MainPlayer && player != null)
            {
                Player.ActivePlayer = player;
            }

            if (!MainPlayer && player!=null)
            {
                if (input.user.valid) input.user.UnpairDevices();
                input.enabled = false;
                player.enabled = false;
                if (frustumModule!=null) frustumModule.enabled = false;
            }
        }

        private void Start()
        {
            var brain = Camera.main.GetComponent<CinemachineBrain>();
            if (brain != null)
            {
                cineCam = brain.ActiveVirtualCamera as CinemachineVirtualCameraBase;
            }
        }

        public void Swap()
        {
            if (!input.enabled) return;

            StartCoroutine(ExecuteSwap());
        }

        private IEnumerator ExecuteSwap()
        {
            if (input.user.valid)
            {
                input.user.UnpairDevices();
            }

            if (input!=null) input.enabled = false;
            if (player!=null) player.enabled = false;
            if (frustumModule != null) frustumModule.enabled = false;
            OnPlayerDeactivate?.Invoke();

            yield return new WaitForEndOfFrame();

            if (targetPlayerObject == null) yield break;

            // Reset portal freezes on swap to ensure immediate teleportation capability
            EvanGameKits.Mechanic.Portal.ResetAllPortalsFreeze();

            PlayerInput targetInput = targetPlayerObject.GetComponent<PlayerInput>();
            Player targetPlayer = targetPlayerObject.GetComponent<Player>();
            M_FrustumDetect targetModule = targetPlayerObject.GetComponent<M_FrustumDetect>();
            M_SwapPlayer targetSwapPlayer = targetPlayerObject.GetComponent<M_SwapPlayer>();

            if (targetInput != null)
            {
                targetInput.enabled = true;
                if (targetPlayer != null) targetPlayer.enabled = true;
                if (targetModule != null) targetModule.enabled = true;
                if (targetSwapPlayer != null) targetSwapPlayer.OnPlayerActivate?.Invoke();

                if (cineCam != null)
                {
                    cineCam.Follow = targetPlayerObject.transform;
                    cineCam.LookAt = targetPlayerObject.transform;
                }
            }
        }

        private void Update()
        {
            if (Keyboard.current.leftCtrlKey.wasPressedThisFrame)
            {
                Swap();
            }
        }
    }
}
