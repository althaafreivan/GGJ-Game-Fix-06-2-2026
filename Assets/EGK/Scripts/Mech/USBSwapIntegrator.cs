using UnityEngine;
using EvanGameKits.UniqueMechanic;
using EvanGameKits.Entity.Module;
using System.IO;
using System.Linq;

namespace EvanGameKits.Mechanic
{
    public class USBSwapIntegrator : MonoBehaviour
    {
        [SerializeField] private USBFileFinder usbFinder;
        
        [Header("Players")]
        [Tooltip("The player that should be active when NO USB is plugged in (e.g. Black).")]
        [SerializeField] private M_SwapPlayer playerA;
        
        [Tooltip("The player that should be active when a USB IS plugged in (e.g. White).")]
        [SerializeField] private M_SwapPlayer playerB;

        private float lastSwapTime;
        private const float swapCooldown = 0.5f;

        private void OnEnable()
        {
            if (usbFinder == null) usbFinder = GetComponent<USBFileFinder>();

            if (usbFinder != null)
            {
                usbFinder.OnUSBConnected.AddListener(HandleUSBConnected);
                usbFinder.OnUSBDetached.AddListener(HandleUSBDetached);
            }
        }

        private void Start()
        {
            // Initial check: determine if a USB is already plugged in at startup
            bool isAlreadyPlugged = CheckInitialUSBState();
            
            if (isAlreadyPlugged)
            {
                // If USB is present but Player A is active, swap to B (White)
                if (IsActive(playerA))
                {
                    playerA.Swap();
                }
            }
            else
            {
                // If USB is NOT present but Player B is active, swap to A (Black)
                if (IsActive(playerB))
                {
                    playerB.Swap();
                }
            }
        }

        private bool CheckInitialUSBState()
        {
            // Simple check for any removable drive on startup
            return DriveInfo.GetDrives().Any(d => d.IsReady && d.DriveType == DriveType.Removable);
        }

        private void OnDisable()
        {
            if (usbFinder != null)
            {
                usbFinder.OnUSBConnected.RemoveListener(HandleUSBConnected);
                usbFinder.OnUSBDetached.RemoveListener(HandleUSBDetached);
            }
        }

        private void HandleUSBConnected()
        {
            if (Time.time < lastSwapTime + swapCooldown) return;

            if (IsActive(playerA))
            {
                lastSwapTime = Time.time;
                playerA.Swap();
            }
        }

        private void HandleUSBDetached()
        {
            if (Time.time < lastSwapTime + swapCooldown) return;

            if (IsActive(playerB))
            {
                lastSwapTime = Time.time;
                playerB.Swap();
            }
        }

        private bool IsActive(M_SwapPlayer module)
        {
            if (module == null) return false;
            var input = module.GetComponent<UnityEngine.InputSystem.PlayerInput>();
            return input != null && input.enabled;
        }
    }
}
