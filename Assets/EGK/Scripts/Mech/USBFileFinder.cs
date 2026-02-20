using UnityEngine;
using UnityEngine.Events;
using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Linq;
using System.Threading.Tasks;
using DG.Tweening;

namespace EvanGameKits.UniqueMechanic
{
    public class USBFileFinder : MonoBehaviour
    {
        [Header("File Detection Settings")]
        public string targetFileName = "myFile.txt";
        public bool _isFileFound;
        [SerializeField] private float timeTreshold = 10f;
        private float timeElapsedAfterFound = 0f;

        [Header("USB Events")]
        public UnityEvent OnUSBConnected;
        public UnityEvent OnUSBDetached;

        [Header("File Events")]
        public UnityEvent OnFileFound;
        public UnityEvent OnFileLost;

        private const int WM_DEVICECHANGE = 0x0219;
        private const int DBT_DEVICEARRIVAL = 0x8000;
        private const int DBT_DEVICEREMOVECOMPLETE = 0x8004;
        private const int GWL_WNDPROC = -4;

        [DllImport("user32.dll")]
        private static extern IntPtr GetActiveWindow();
        [DllImport("user32.dll")]
        private static extern IntPtr SetWindowLongPtr(IntPtr hWnd, int nIndex, IntPtr dwNewLong);
        [DllImport("user32.dll")]
        private static extern IntPtr CallWindowProc(IntPtr lpPrevWndFunc, IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);

        private delegate IntPtr WndProcDelegate(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);
        private IntPtr _oldWndProcPtr;
        private WndProcDelegate _wndProcDelegate;
        private IntPtr _hWnd;

        private Tween _timerTween;

        public event OnVariableChangeDelegate onVariableChange;
        public delegate void OnVariableChangeDelegate(bool newBool);

        private bool isFileFound
        {
            get { return _isFileFound; }
            set
            {
                if (_isFileFound == value) return;
                _isFileFound = value;
                onVariableChange?.Invoke(_isFileFound);
            }
        }

        void Start()
        {
            _hWnd = GetActiveWindow();
            _wndProcDelegate = WndProc;
            _oldWndProcPtr = SetWindowLongPtr(_hWnd, GWL_WNDPROC, Marshal.GetFunctionPointerForDelegate(_wndProcDelegate));

            onVariableChange += variableChangeHandler;
            RunScan();
        }

        private void variableChangeHandler(bool changedVariable)
        {
            _timerTween?.Kill();

            if (changedVariable)
            {
                OnFileFound?.Invoke();
                _timerTween = DOVirtual.Float(timeTreshold, 0, timeTreshold, val =>
                {
                    timeElapsedAfterFound = val;
                })
                .SetEase(Ease.Linear)
                .OnComplete(() => Debug.Log("Timer Finished"));

                Debug.Log("File Found: Timer Started");
            }
            else
            {
                OnFileLost?.Invoke();
                timeElapsedAfterFound = 0;
                Debug.Log("File Lost: Timer Reset");
            }
        }

        private IntPtr WndProc(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam)
        {
            if (msg == WM_DEVICECHANGE)
            {
                int eventType = wParam.ToInt32();
                if (eventType == DBT_DEVICEARRIVAL)
                {
                    OnUSBConnected?.Invoke();
                    RunScan();
                }
                else if (eventType == DBT_DEVICEREMOVECOMPLETE)
                {
                    OnUSBDetached?.Invoke();
                    RunScan();
                }
            }
            return CallWindowProc(_oldWndProcPtr, hWnd, msg, wParam, lParam);
        }

        private async void RunScan()
        {
            await Task.Delay(500);

            DriveInfo[] drives = DriveInfo.GetDrives()
                .Where(d => d.IsReady && d.DriveType == DriveType.Removable)
                .ToArray();

            bool foundInAny = false;

            foreach (var d in drives)
            {
                try
                {
                    string path = Path.Combine(d.Name, targetFileName);
                    if (File.Exists(path))
                    {
                        foundInAny = true;
                        break;
                    }
                }
                catch (Exception e)
                {
                    Debug.LogWarning($"Drive {d.Name} error: {e.Message}");
                }
            }

            isFileFound = foundInAny;
        }

        void OnDisable()
        {
            Cleanup();
        }

        void OnDestroy()
        {
            Cleanup();
        }

        private void Cleanup()
        {
            _timerTween?.Kill();
            if (_hWnd != IntPtr.Zero && _oldWndProcPtr != IntPtr.Zero)
            {
                SetWindowLongPtr(_hWnd, GWL_WNDPROC, _oldWndProcPtr);
                _oldWndProcPtr = IntPtr.Zero;
                _hWnd = IntPtr.Zero;
            }
        }
    }
}
