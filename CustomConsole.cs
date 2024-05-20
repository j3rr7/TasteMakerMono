using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;

namespace TasteMakerMono
{
    public class CustomConsole : MonoBehaviour
    {
        private static CustomConsole _instance = null;
        private static bool _isConsoleInitialized = false;
        private bool _consoleVisible = true;

        private static readonly IntPtr INTPTR_ZERO = IntPtr.Zero;
        private readonly int SW_SHOW = 5;
        private readonly int SW_HIDE = 0;

        [DllImport("kernel32.dll")]
        private static extern bool AllocConsole();

        [DllImport("kernel32.dll")]
        private static extern bool FreeConsole();

        [DllImport("kernel32.dll")]
        private static extern IntPtr GetConsoleWindow();

        [DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        public static CustomConsole Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new GameObject("CustomConsole").AddComponent<CustomConsole>();
                    DontDestroyOnLoad(_instance.gameObject);
                    _instance.Initialize();
                }
                return _instance;
            }
        }

        private void Awake()
        {
            if (_instance == null)
            {
                _instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        public void Initialize()
        {
            if (!_isConsoleInitialized)
            {
                try
                {
                    AllocConsole();
                    ShowWindow(GetConsoleWindow(), SW_SHOW);
                    Application.logMessageReceivedThreaded += HandleLogThreaded;
                    _isConsoleInitialized = true;
                }
                catch (Exception)
                {
                    _isConsoleInitialized = false;
                }
            }
        }

        private void OnDestroy()
        {
            if (_isConsoleInitialized)
            {
                FreeConsole();
                _isConsoleInitialized = false;
            }
            Application.logMessageReceivedThreaded -= HandleLogThreaded;
        }

        public void ToggleVisibility()
        {
            IntPtr consoleWindow = GetConsoleWindow();

            if (_consoleVisible)
            {
                ShowWindow(consoleWindow, SW_HIDE);
                _consoleVisible = false;
            }
            else
            {
                ShowWindow(consoleWindow, SW_SHOW);
                _consoleVisible = true;
            }
        }

        private static void HandleLog(string logString, string stackTrace, LogType type)
        {
            SetConsoleColorBasedOnLogType(type);
            Console.WriteLine(logString + " " + stackTrace);
            Console.ResetColor();
        }

        private static void HandleLogThreaded(string logString, string stackTrace, LogType type)
        {
            SetConsoleColorBasedOnLogType(type);
            Console.WriteLine(logString + " " + stackTrace);
            Console.ResetColor();
        }

        private static void SetConsoleColorBasedOnLogType(LogType type)
        {
            switch (type)
            {
                case LogType.Error:
                case LogType.Assert:
                case LogType.Exception:
                    Console.ForegroundColor = ConsoleColor.Red;
                    break;
                case LogType.Warning:
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    break;
                default:
                    Console.ForegroundColor = ConsoleColor.White;
                    break;
            }
        }
    }
}
