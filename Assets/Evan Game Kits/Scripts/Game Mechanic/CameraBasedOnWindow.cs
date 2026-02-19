using UnityEngine;
using System;
using System.Runtime.InteropServices;
using Unity.Cinemachine;
using System.Threading;
using System.Diagnostics;

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
        [SerializeField] private float minSmoothing = 25f;
        [SerializeField] private float maxSmoothing = 5f;
        [SerializeField] private float velocityThreshold = 1000f;
        [SerializeField, Range(60, 4000)] private int threadUpdateRate = 1000;

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

        [DllImport("user32.dll")]
        private static extern bool GetCursorPos(out POINT lpPoint);

        [DllImport("user32.dll")]
        private static extern short GetAsyncKeyState(int vKey);

        [StructLayout(LayoutKind.Sequential)]
        public struct RECT { public int Left, Top, Right, Bottom; }

        [StructLayout(LayoutKind.Sequential)]
        public struct POINT { public int X; public int Y; }

        private const int GWL_STYLE = -16;
        private const int WS_CAPTION = 0x00C00000;
        private const int WS_THICKFRAME = 0x00040000;
        private const uint SWP_NOSIZE = 0x0001;
        private const uint SWP_NOZORDER = 0x0004;
        private const uint SWP_FRAMECHANGED = 0x0020;
        private const int VK_LBUTTON = 0x01;

        private Vector2 initialWindowPos;
        private Vector2 currentWindowPos;
        private Vector2 targetWindowPos;
        private bool initialized;
        private IntPtr hwnd;

        // Threading fields
        private Thread windowThread;
        private bool threadRunning;
        private readonly object posLock = new object();
        private Vector2 lastGlobalMousePos;
        private bool isDragging;
        private int lastUpdateFrame = -1;
        private Vector2 interpolatedLensShift;
        private float currentVelocity;

        protected override void Awake()
        {
            base.Awake();
            instance = this;
            mainCam = Camera.main;

            hwnd = GetActiveWindow();

            #if !UNITY_EDITOR && UNITY_STANDALONE_WIN
            // Force 1280x720 and center window
            int windowWidth = 1280;
            int windowHeight = 720;
            int screenWidth = GetSystemMetrics(SM_CXSCREEN);
            int screenHeight = GetSystemMetrics(SM_CYSCREEN);
            int centerX = (screenWidth - windowWidth) / 2;
            int centerY = (screenHeight - windowHeight) / 2;

            // Remove SWP_NOSIZE so we can actually set the size
            SetWindowPos(hwnd, IntPtr.Zero, centerX, centerY, windowWidth, windowHeight, SWP_NOZORDER);
            #endif

            if (hideTitleBar && !Application.isEditor)
            {
                int style = GetWindowLong(hwnd, GWL_STYLE);
                SetWindowLong(hwnd, GWL_STYLE, style & ~WS_CAPTION & ~WS_THICKFRAME);
                // Force a frame change to apply the style immediately
                SetWindowPos(hwnd, IntPtr.Zero, 0, 0, 0, 0, SWP_NOSIZE | SWP_NOZORDER | SWP_FRAMECHANGED);
            }
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            AutoCalibrate();
            
            #if !UNITY_EDITOR && UNITY_STANDALONE_WIN
            // Start the window thread only in builds
            threadRunning = true;
            windowThread = new Thread(WindowThreadLoop);
            windowThread.Priority = System.Threading.ThreadPriority.Highest;
            windowThread.IsBackground = true; 
            windowThread.Start();
            #endif

            Application.onBeforeRender += SyncPortalToWindow;
        }

        private void OnDisable()
        {
            threadRunning = false;
            #if !UNITY_EDITOR && UNITY_STANDALONE_WIN
            if (windowThread != null && windowThread.IsAlive)
            {
                windowThread.Join(100);
            }
            #endif
            Application.onBeforeRender -= SyncPortalToWindow;
        }

        private void Update()
        {
            // Logic moved to SyncPortalToWindow for better synchronization
        }

        public void AutoCalibrate()
        {
            #if UNITY_EDITOR
            RECT editorRect;
            if (GetWindowRect(hwnd, out editorRect))
            {
                initialWindowPos = new Vector2(editorRect.Left, editorRect.Top);
                currentWindowPos = initialWindowPos;
                targetWindowPos = initialWindowPos;
            }
            else
            {
                initialWindowPos = new Vector2(Screen.width / 2f, Screen.height / 2f);
                currentWindowPos = initialWindowPos;
                targetWindowPos = initialWindowPos;
            }
            initialized = true;
            #endif

            if (mainCam != null)
            {
                float vFov = mainCam.fieldOfView;
                sensitivity = (3.0f * Mathf.Tan(vFov * 0.5f * Mathf.Deg2Rad)) / Screen.height;
            }

            #if !UNITY_EDITOR
            RECT rect;
            if (GetWindowRect(hwnd, out rect))
            {
                int windowWidth = rect.Right - rect.Left;
                int windowHeight = rect.Bottom - rect.Top;

                int screenWidth = GetSystemMetrics(SM_CXSCREEN);
                int screenHeight = GetSystemMetrics(SM_CYSCREEN);

                int centerX = (screenWidth - windowWidth) / 2;
                int centerY = (screenHeight - windowHeight) / 2;

                lock (posLock)
                {
                    currentWindowPos = new Vector2(centerX, centerY);
                    targetWindowPos = currentWindowPos;
                    initialWindowPos = currentWindowPos;
                }

                SetWindowPos(hwnd, IntPtr.Zero, centerX, centerY, 0, 0, SWP_NOSIZE | SWP_NOZORDER);
                initialized = true;
            }
            #endif
        }

        private void WindowThreadLoop()
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();
            long lastTicks = sw.ElapsedTicks;

            while (threadRunning)
            {
                long currentTicks = sw.ElapsedTicks;
                float dt = (float)(currentTicks - lastTicks) / Stopwatch.Frequency;
                
                // Target time per frame in seconds
                double targetDt = 1.0 / (double)Mathf.Max(threadUpdateRate, 60);

                if (dt >= targetDt)
                {
                    lastTicks = currentTicks;
                    HandleWindowUpdate(dt);
                }
                else
                {
                    // If we are far ahead of schedule, sleep to save CPU.
                    // Otherwise, yield to maintain high precision for high rates.
                    if (targetDt - dt > 0.0015) 
                        Thread.Sleep(1);
                    else
                        Thread.Yield();
                }
            }
        }

        private void HandleWindowUpdate(float dt)
        {
            POINT globalMouse;
            if (!GetCursorPos(out globalMouse)) return;
            Vector2 currentGlobalMouse = new Vector2(globalMouse.X, globalMouse.Y);
            bool lmbPressed = (GetAsyncKeyState(VK_LBUTTON) & 0x8000) != 0;

            lock (posLock)
            {
                if (lmbPressed)
                {
                    if (!isDragging)
                    {
                        isDragging = true;
                        lastGlobalMousePos = currentGlobalMouse;
                    }
                    Vector2 mouseDelta = currentGlobalMouse - lastGlobalMousePos;
                    targetWindowPos += mouseDelta;
                    lastGlobalMousePos = currentGlobalMouse;
                    
                    // Calculate velocity magnitude (pixels per second)
                    if (dt > 0)
                        currentVelocity = mouseDelta.magnitude / dt;
                }
                else
                {
                    isDragging = false;
                    currentVelocity = 0;
                }

                if (isDragging)
                {
                    currentWindowPos = targetWindowPos;
                }
                else if (Vector2.SqrMagnitude(currentWindowPos - targetWindowPos) > 0.0001f)
                {
                    float step = maxDragSpeed * dt;
                    currentWindowPos = Vector2.MoveTowards(currentWindowPos, targetWindowPos, step);
                }
            }
        }

        private void SyncPortalToWindow()
        {
            if (!initialized) return;

            // Ensure window update and HandleWindowUpdate run only once per frame
            if (Time.frameCount != lastUpdateFrame)
            {
                lastUpdateFrame = Time.frameCount;

                #if UNITY_EDITOR
                HandleWindowUpdate(Time.unscaledDeltaTime);
                #endif

                lock (posLock)
                {
                    if (initialized && (Vector2.SqrMagnitude(currentWindowPos - targetWindowPos) > 0.001f || isDragging))
                    {
                        // Move the window on main thread to avoid Win32 thread-affinity deadlocks
                        SetWindowPos(hwnd, IntPtr.Zero, (int)currentWindowPos.x, (int)currentWindowPos.y, 0, 0, SWP_NOSIZE | SWP_NOZORDER);
                    }
                }
            }

            float curX, curY, initX, initY;
            lock (posLock)
            {
                curX = currentWindowPos.x;
                curY = currentWindowPos.y;
                initX = initialWindowPos.x;
                initY = initialWindowPos.y;
            }

            float deltaX = curX - initX;
            float deltaY = curY - initY;

            Vector2 targetLensShift = new Vector2(
                deltaX * sensitivity,
                -deltaY * sensitivity
            );

            // Calculate dynamic smoothing based on velocity. 
            // Higher velocity = lower smoothing value = more interpolation (to hide stutters).
            float t = Mathf.Clamp01(currentVelocity / velocityThreshold);
            float dynamicSmoothing = Mathf.Lerp(minSmoothing, maxSmoothing, t);

            if (dynamicSmoothing > 0)
            {
                interpolatedLensShift = Vector2.Lerp(interpolatedLensShift, targetLensShift, Time.unscaledDeltaTime * dynamicSmoothing);
            }
            else
            {
                interpolatedLensShift = targetLensShift;
            }

            mainCam.lensShift = interpolatedLensShift;
        }

        protected override void PostPipelineStageCallback(CinemachineVirtualCameraBase vcam, CinemachineCore.Stage stage, ref CameraState state, float deltaTime) { }
    }
}
