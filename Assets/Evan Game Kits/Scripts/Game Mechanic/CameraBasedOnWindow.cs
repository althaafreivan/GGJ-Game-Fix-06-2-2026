using UnityEngine;
using System;
using System.Runtime.InteropServices;
using Unity.Cinemachine;

namespace EvanGameKits.Core
{
    public class M_WindowPortalCamera : CinemachineExtension
    {
        public static M_WindowPortalCamera instance;
        private Camera mainCam;

        [Header("Portal Settings")]
        [SerializeField] private float sensitivity = 0.001f;

        [Header("Movement Settings")]
        [SerializeField] private bool hideTitleBar = true;
        [SerializeField] private float maxDragSpeed = 5000f;

        [DllImport("user32.dll")]
        private static extern int GetSystemMetrics(int nIndex);

        private const int SM_CXSCREEN = 0;
        private const int SM_CYSCREEN = 1;
        [DllImport("user32.dll")]
        private static extern IntPtr GetActiveWindow();

        [DllImport("user32.dll")]
        private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        [DllImport("user32.dll")]
        private static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

        [DllImport("user32.dll")]
        private static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

        [StructLayout(LayoutKind.Sequential)]
        public struct RECT { public int Left, Top, Right, Bottom; }

        private const int GWL_STYLE = -16;
        private const int WS_CAPTION = 0x00C00000;
        private const int WS_THICKFRAME = 0x00040000;
        private const uint SWP_NOSIZE = 0x0001;
        private const uint SWP_NOZORDER = 0x0004;

        private Vector2 initialWindowPos;
        private Vector2 currentWindowPos;
        private Vector2 targetWindowPos;
        private Vector2 lastMousePos;
        private bool isDragging;
        private bool initialized;
        private IntPtr hwnd;

        protected override void Awake()
        {
            base.Awake();
            instance = this;
            mainCam = Camera.main;

            Application.targetFrameRate = 165;
            QualitySettings.vSyncCount = 1;

            hwnd = GetActiveWindow();
            if (hideTitleBar)
            {
                int style = GetWindowLong(hwnd, GWL_STYLE);
                SetWindowLong(hwnd, GWL_STYLE, style & ~WS_CAPTION & ~WS_THICKFRAME);
            }
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            AutoCalibrate();
            Application.onBeforeRender += SyncPortalToWindow;
        }

        private void OnDisable()
        {
            Application.onBeforeRender -= SyncPortalToWindow;
        }

        public void AutoCalibrate()
        {
            float vFov = mainCam.fieldOfView;
            sensitivity = (3.0f * Mathf.Tan(vFov * 0.5f * Mathf.Deg2Rad)) / Screen.height;

            RECT rect;
            if (GetWindowRect(hwnd, out rect))
            {
                int windowWidth = rect.Right - rect.Left;
                int windowHeight = rect.Bottom - rect.Top;

                int screenWidth = GetSystemMetrics(SM_CXSCREEN);
                int screenHeight = GetSystemMetrics(SM_CYSCREEN);

                int centerX = (screenWidth - windowWidth) / 2;
                int centerY = (screenHeight - windowHeight) / 2;

                currentWindowPos = new Vector2(centerX, centerY);
                targetWindowPos = currentWindowPos;
                initialWindowPos = currentWindowPos;

                SetWindowPos(hwnd, IntPtr.Zero, centerX, centerY, 0, 0, SWP_NOSIZE | SWP_NOZORDER);

                initialized = true;
            }
        }

        private void Update()
        {
            HandleWindowDrag();
        }

        [DllImport("user32.dll")]
        private static extern bool GetCursorPos(out POINT lpPoint);

        [StructLayout(LayoutKind.Sequential)]
        public struct POINT { public int X; public int Y; }

        private Vector2 lastGlobalMousePos;

        private void HandleWindowDrag()
        {
            POINT globalMouse;
            GetCursorPos(out globalMouse);
            Vector2 currentGlobalMouse = new Vector2(globalMouse.X, globalMouse.Y);

            if (Input.GetMouseButtonDown(0))
            {
                isDragging = true;
                lastGlobalMousePos = currentGlobalMouse;
            }

            if (Input.GetMouseButtonUp(0))
            {
                isDragging = false;
            }

            if (isDragging)
            {
                Vector2 mouseDelta = currentGlobalMouse - lastGlobalMousePos;
                targetWindowPos += mouseDelta;
                lastGlobalMousePos = currentGlobalMouse;
            }

            float step = maxDragSpeed * Time.unscaledDeltaTime;
            currentWindowPos = Vector2.MoveTowards(currentWindowPos, targetWindowPos, step);

            SetWindowPos(hwnd, IntPtr.Zero, (int)currentWindowPos.x, (int)currentWindowPos.y, 0, 0, SWP_NOSIZE | SWP_NOZORDER);
        }

        private void SyncPortalToWindow()
        {
            if (!initialized) return;

            double deltaX = (double)currentWindowPos.x - (double)initialWindowPos.x;
            double deltaY = (double)currentWindowPos.y - (double)initialWindowPos.y;

            mainCam.lensShift = new Vector2(
                (float)(deltaX * sensitivity),
                -(float)(deltaY * sensitivity)
            );
        }

        protected override void PostPipelineStageCallback(CinemachineVirtualCameraBase vcam, CinemachineCore.Stage stage, ref CameraState state, float deltaTime) { }
    }
}