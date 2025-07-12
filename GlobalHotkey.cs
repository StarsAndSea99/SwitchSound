using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace SwitchSound
{
    public class GlobalHotkey : IDisposable
    {
        // Windows API 声明
        [DllImport("user32.dll")]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

        [DllImport("user32.dll")]
        private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        [DllImport("kernel32.dll")]
        private static extern uint GetLastError();

        // 修饰键常量
        public enum ModifierKeys : uint
        {
            None = 0,
            Alt = 1,
            Control = 2,
            Shift = 4,
            Windows = 8
        }

        // 消息常量
        private const int WM_HOTKEY = 0x0312;

        // 错误代码
        private const uint ERROR_HOTKEY_ALREADY_REGISTERED = 1409;

        // 私有字段
        private IntPtr windowHandle;
        private Dictionary<int, Action> hotkeyActions;
        private Dictionary<int, (ModifierKeys modifiers, Keys key)> hotkeyInfo;
        private int nextHotkeyId = 1;

        public GlobalHotkey(IntPtr windowHandle)
        {
            this.windowHandle = windowHandle;
            this.hotkeyActions = new Dictionary<int, Action>();
            this.hotkeyInfo = new Dictionary<int, (ModifierKeys, Keys)>();
        }

        public int RegisterHotkey(ModifierKeys modifiers, Keys key, Action action)
        {
            int hotkeyId = nextHotkeyId++;
            
            if (RegisterHotKey(windowHandle, hotkeyId, (uint)modifiers, (uint)key))
            {
                hotkeyActions[hotkeyId] = action;
                hotkeyInfo[hotkeyId] = (modifiers, key);
                return hotkeyId;
            }
            else
            {
                uint error = GetLastError();
                string errorMessage = GetErrorMessage(error, modifiers, key);
                throw new InvalidOperationException(errorMessage);
            }
        }

        public bool UnregisterHotkey(int hotkeyId)
        {
            if (hotkeyActions.ContainsKey(hotkeyId))
            {
                bool result = UnregisterHotKey(windowHandle, hotkeyId);
                if (result)
                {
                    hotkeyActions.Remove(hotkeyId);
                    hotkeyInfo.Remove(hotkeyId);
                }
                return result;
            }
            return false;
        }

        public void UnregisterAllHotkeys()
        {
            var hotkeyIds = new List<int>(hotkeyActions.Keys);
            foreach (int hotkeyId in hotkeyIds)
            {
                UnregisterHotkey(hotkeyId);
            }
        }

        public void ProcessMessage(ref Message m)
        {
            if (m.Msg == WM_HOTKEY)
            {
                int hotkeyId = m.WParam.ToInt32();
                if (hotkeyActions.ContainsKey(hotkeyId))
                {
                    hotkeyActions[hotkeyId]?.Invoke();
                }
            }
        }

        private string GetErrorMessage(uint error, ModifierKeys modifiers, Keys key)
        {
            string hotkeyString = $"{modifiers} + {key}";
            
            switch (error)
            {
                case ERROR_HOTKEY_ALREADY_REGISTERED:
                    return $"热键 {hotkeyString} 已被其他程序占用，请选择其他组合键";
                case 0:
                    return $"无法注册热键 {hotkeyString}，请检查组合键是否有效";
                default:
                    return $"注册热键 {hotkeyString} 时发生错误 (错误代码: {error})";
            }
        }

        public void Dispose()
        {
            UnregisterAllHotkeys();
        }
    }

    // 热键配置类
    public class HotkeyConfig
    {
        public GlobalHotkey.ModifierKeys ModifierKeys { get; set; }
        public Keys Key { get; set; }

        public HotkeyConfig()
        {
            // 默认热键: Ctrl + Alt + S
            ModifierKeys = GlobalHotkey.ModifierKeys.Control | GlobalHotkey.ModifierKeys.Alt;
            Key = Keys.S;
        }

        public HotkeyConfig(GlobalHotkey.ModifierKeys modifiers, Keys key)
        {
            ModifierKeys = modifiers;
            Key = key;
        }

        public override string ToString()
        {
            var parts = new List<string>();
            
            if (ModifierKeys.HasFlag(GlobalHotkey.ModifierKeys.Control))
                parts.Add("Ctrl");
            if (ModifierKeys.HasFlag(GlobalHotkey.ModifierKeys.Alt))
                parts.Add("Alt");
            if (ModifierKeys.HasFlag(GlobalHotkey.ModifierKeys.Shift))
                parts.Add("Shift");
            if (ModifierKeys.HasFlag(GlobalHotkey.ModifierKeys.Windows))
                parts.Add("Win");
            
            parts.Add(Key.ToString());
            
            return string.Join(" + ", parts);
        }

        public static HotkeyConfig FromString(string hotkeyString)
        {
            var config = new HotkeyConfig();
            
            if (string.IsNullOrEmpty(hotkeyString))
                return config;

            var parts = hotkeyString.Split(new[] { " + " }, StringSplitOptions.RemoveEmptyEntries);
            
            GlobalHotkey.ModifierKeys modifiers = GlobalHotkey.ModifierKeys.None;
            Keys key = Keys.None;

            foreach (var part in parts)
            {
                switch (part.ToLower())
                {
                    case "ctrl":
                        modifiers |= GlobalHotkey.ModifierKeys.Control;
                        break;
                    case "alt":
                        modifiers |= GlobalHotkey.ModifierKeys.Alt;
                        break;
                    case "shift":
                        modifiers |= GlobalHotkey.ModifierKeys.Shift;
                        break;
                    case "win":
                        modifiers |= GlobalHotkey.ModifierKeys.Windows;
                        break;
                    default:
                        if (Enum.TryParse(part, true, out Keys parsedKey))
                        {
                            key = parsedKey;
                        }
                        break;
                }
            }

            config.ModifierKeys = modifiers;
            config.Key = key;
            
            return config;
        }

        public bool IsValid()
        {
            // 热键必须有修饰键和主键
            return ModifierKeys != GlobalHotkey.ModifierKeys.None && Key != Keys.None;
        }
    }
} 