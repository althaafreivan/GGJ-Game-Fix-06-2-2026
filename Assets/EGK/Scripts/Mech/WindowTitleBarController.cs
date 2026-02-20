using UnityEngine;
using System;
using System.Runtime.InteropServices;

namespace EvanGameKits.Core
{
    public class WindowTitleBarController : MonoBehaviour
    {
        [Header("Window Settings")]
        [SerializeField] private bool showTitleBarOnStart = true;
        [SerializeField] private bool centerWindow = true;
        [SerializeField] private Vector2Int windowSize = new Vector2Int(1280, 720);

        [DllImport("user32.dll")]
        private static extern IntPtr GetActiveWindow();

        [DllImport("user32.dll")]
        private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        [DllImport("user32.dll")]
        private static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

        [DllImport("user32.dll")]
        private static extern int GetSystemMetrics(int nIndex);

        private const int GWL_STYLE = -16;
        private const int WS_CAPTION = 0x00C00000;
        private const int WS_THICKFRAME = 0x00040000;
        private const int WS_OVERLAPPEDWINDOW = 0x00CF0000;

        private const uint SWP_NOMOVE = 0x0002;
        private const uint SWP_NOSIZE = 0x0001;
        private const uint SWP_NOZORDER = 0x0004;
        private const uint SWP_FRAMECHANGED = 0x0020;

        private const int SM_CXSCREEN = 0;
        private const int SM_CYSCREEN = 1;

        private void Start()
        {
            #if !UNITY_EDITOR && UNITY_STANDALONE_WIN
            ApplyWindowSettings();
            #endif
        }

        public void ApplyWindowSettings()
        {
            IntPtr hwnd = GetActiveWindow();
            if (hwnd == IntPtr.Zero) return;

            if (showTitleBarOnStart)
            {
                // Restore standard window decorations (Caption, Border, etc.)
                int style = GetWindowLong(hwnd, GWL_STYLE);
                SetWindowLong(hwnd, GWL_STYLE, style | WS_CAPTION | WS_THICKFRAME);
            }

            int x = 0;
            int y = 0;
            uint flags = SWP_FRAMECHANGED | SWP_NOZORDER;

            if (centerWindow)
            {
                int screenWidth = GetSystemMetrics(SM_CXSCREEN);
                int screenHeight = GetSystemMetrics(SM_CYSCREEN);
                x = (screenWidth - windowSize.x) / 2;
                y = (screenHeight - windowSize.y) / 2;
            }
            else
            {
                flags |= SWP_NOMOVE;
            }

            // Apply size and position (and trigger frame change to show title bar)
            SetWindowPos(hwnd, IntPtr.Zero, x, y, windowSize.x, windowSize.y, flags);
        }
    }
}
