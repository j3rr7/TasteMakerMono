using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;

namespace TasteMakerMono
{
    public class HotkeyManager
    {
        private readonly Dictionary<KeyCode, Action> hotkeys = new Dictionary<KeyCode, Action>();
        private readonly Dictionary<KeyCode, float> lastActivationTime = new Dictionary<KeyCode, float>();

        private const float debounceDelay = 0.3f;

        public void RegisterHotkey(KeyCode key, Action action)
        {
            if (hotkeys.ContainsKey(key))
            {
                throw new ArgumentException($"Hotkey for key '{key}' already exists.");
            }

            hotkeys[key] = action;
            lastActivationTime.Add(key, 0f);
        }

        public void HandleHotkeys()
        {
            foreach (KeyCode key in hotkeys.Keys)
            {
                if (Input.GetKeyDown(key))
                {
                    float currentTime = Time.time;
                    if (currentTime - lastActivationTime[key] > debounceDelay)
                    {
                        hotkeys[key].Invoke();
                        lastActivationTime[key] = currentTime;
                    }
                }
            }
            //if (Input.anyKeyDown)
            //{
            //    string inputString = Input.inputString;
            //    if (string.IsNullOrEmpty(inputString))
            //    {
            //        Debug.LogWarning("Input string is empty or null.");
            //        return;
            //    }

            //    KeyCode key;
            //    if (!Enum.TryParse(inputString, out key))
            //    {
            //        Debug.LogWarning($"Invalid key code: '{inputString}'");
            //        return;
            //    }

            //    if (hotkeys.ContainsKey(key))
            //    {
            //        hotkeys[key].Invoke();
            //    }
            //    else
            //    {
            //        Debug.LogWarning($"No action registered for key '{key}'.");
            //    }
            //}
        }
        public void HandleSinglePressHotkeys()
        {
            EventSystem.current.sendNavigationEvents = false;

            foreach (KeyCode key in hotkeys.Keys)
            {
                if (Input.GetKeyDown(key))
                {
                    float currentTime = Time.time;
                    if (currentTime - lastActivationTime[key] >= debounceDelay)
                    {
                        hotkeys[key].Invoke();
                        lastActivationTime[key] = currentTime;
                    }
                }
            }

            EventSystem.current.sendNavigationEvents = true;
        }
    }
}
