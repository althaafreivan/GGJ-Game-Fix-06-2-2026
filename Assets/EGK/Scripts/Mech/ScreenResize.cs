using UnityEngine;
using System;
using System.Runtime.InteropServices;

namespace EvanGameKits.UniqueMechanic
{
    public class ScreenResize : MonoBehaviour
    {
        #region DLL_Imports
        [DllImport("user32.dll")]
        private static extern IntPtr GetActiveWindow();
        [DllImport("user32.dll")]
        private static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);
        [DllImport("user32.dll")]
        private static extern bool MoveWindow(IntPtr hWnd, int x, int y, int nWidth, int nHeight, bool bRepaint);
        [DllImport("user32.dll")]
        static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, short X, short Y, short cx, short cy, uint uFlags);
        [DllImport("user32.dll")]
        static extern IntPtr SetWindowLong(IntPtr hWnd, int nIndex, uint dwNewLong);
        [DllImport("user32.dll")]
        static extern IntPtr GetWindowLong(IntPtr hWnd, int nIndex);

        private const int GWL_STYLE = -16;
        private const uint WS_CAPTION = 0x00C00000;
        private const uint WS_SIZEBOX = 0x00040000;
        private const uint SWP_NOMOVE = 0x0002;
        private const uint SWP_NOSIZE = 0x0001;
        private const uint SWP_SHOWWINDOW = 0x0040;
        private static readonly IntPtr HWND_TOPMOST = new IntPtr(-1);
        private static readonly IntPtr HWND_NOTOPMOST = new IntPtr(-2);
        #endregion

        public struct RECT { public int Left, Top, Right, Bottom; }

        [Header("Window Dimensions")]
        public Vector2 initialSize = new Vector2(1280, 720);
        public Vector2 minSize = new Vector2(320, 180);

        [Header("Interpolation Settings")]
        [Range(0.01f, 0.5f)] public float smoothTime = 0.15f;
        public float pixelsPerUnit = 100f;
        public bool hideOnStart = true;

        private IntPtr _hWnd;
        private Vector2 _currentSize;
        private Vector2 _targetSize;
        private Vector2 _velocity;
        private Camera _mainCam;
        private bool _isResizing = false;
        private Vector2 _screenCenter;

        void Start()
        {
            _hWnd = GetActiveWindow();
            _mainCam = Camera.main;

            _screenCenter = new Vector2(Screen.currentResolution.width * 0.5f, Screen.currentResolution.height * 0.5f);

            _currentSize = initialSize;
            _targetSize = initialSize;
            _isResizing = true;
            ApplyPhysicalCrop();

            if (hideOnStart) ShowWindowBorders(false);
        }

        void LateUpdate()
        {
            if (!_isResizing) return;

            _currentSize = Vector2.SmoothDamp(_currentSize, _targetSize, ref _velocity, smoothTime);

            if (Vector2.SqrMagnitude(_currentSize - _targetSize) < 0.25f)
            {
                _currentSize = _targetSize;
                _isResizing = false;
            }

            /*
            if (_mainCam != null && _mainCam.orthographic)
            {
                _mainCam.orthographicSize = _currentSize.y / (2f * pixelsPerUnit);
            }
            */

            ApplyWindowTransform();

        }

        private void ApplyWindowTransform()
        {
            float newX = _screenCenter.x - (_currentSize.x * 0.5f);
            float newY = _screenCenter.y - (_currentSize.y * 0.5f);

            MoveWindow(_hWnd, Mathf.RoundToInt(newX), Mathf.RoundToInt(newY), Mathf.RoundToInt(_currentSize.x), Mathf.RoundToInt(_currentSize.y), false);
        }

        #region Public API for Modules

        public void ResizeWidth(float normalizedValue)
        {
            float targetX = Mathf.Max(initialSize.x * normalizedValue, minSize.x);
            _targetSize = new Vector2(targetX, _currentSize.y);
            _isResizing = true;
        }

        public void ResizeHeight(float normalizedValue)
        {
            float targetY = Mathf.Max(initialSize.y * normalizedValue, minSize.y);
            _targetSize = new Vector2(_currentSize.x, targetY);
            _isResizing = true;
        }

        public void ResizeWindow(float x, float y)
        {
            _targetSize = new Vector2(Mathf.Max(x, minSize.x), Mathf.Max(y, minSize.y));
            _isResizing = true;
        }

        private void ApplyPhysicalCrop()
        {
            _mainCam.sensorSize = new Vector2(_currentSize.x / pixelsPerUnit, _currentSize.y / pixelsPerUnit);
        }

        public void ShowWindowBorders(bool value)
        {
            if (Application.isEditor) return;

            int style = GetWindowLong(_hWnd, GWL_STYLE).ToInt32();

            if (value)
            {
                SetWindowLong(_hWnd, GWL_STYLE, (uint)(style | WS_CAPTION | WS_SIZEBOX));
                SetWindowPos(_hWnd, HWND_NOTOPMOST, 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE | SWP_SHOWWINDOW);
            }
            else
            {
                SetWindowLong(_hWnd, GWL_STYLE, (uint)(style & ~(WS_CAPTION | WS_SIZEBOX)));
                SetWindowPos(_hWnd, HWND_TOPMOST, 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE | SWP_SHOWWINDOW);
            }
        }
        #endregion
    }
}