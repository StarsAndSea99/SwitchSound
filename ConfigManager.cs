using System;
using System.IO;
using System.Text.Json;
using System.Windows.Forms;

namespace SwitchSound
{
    public class ConfigManager
    {
        private readonly string configPath;
        private AppConfig config;

        public ConfigManager()
        {
            string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string configDir = Path.Combine(appDataPath, "SwitchSound");
            
            if (!Directory.Exists(configDir))
            {
                Directory.CreateDirectory(configDir);
            }
            
            configPath = Path.Combine(configDir, "config.json");
            LoadConfig();
        }

        public HotkeyConfig Hotkey 
        { 
            get => config.Hotkey; 
            set 
            { 
                config.Hotkey = value; 
                SaveConfig(); 
            } 
        }

        public bool AutoStart 
        { 
            get => config.AutoStart; 
            set 
            { 
                config.AutoStart = value; 
                SaveConfig(); 
            } 
        }

        public bool ShowNotifications 
        { 
            get => config.ShowNotifications; 
            set 
            { 
                config.ShowNotifications = value; 
                SaveConfig(); 
            } 
        }

        private void LoadConfig()
        {
            try
            {
                if (File.Exists(configPath))
                {
                    string json = File.ReadAllText(configPath);
                    var options = new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true,
                        WriteIndented = true
                    };
                    
                    config = JsonSerializer.Deserialize<AppConfig>(json, options) ?? new AppConfig();
                }
                else
                {
                    config = new AppConfig();
                    SaveConfig();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"加载配置文件失败: {ex.Message}\n将使用默认配置。", "警告", 
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                config = new AppConfig();
            }
        }

        private void SaveConfig()
        {
            try
            {
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    WriteIndented = true
                };
                
                string json = JsonSerializer.Serialize(config, options);
                File.WriteAllText(configPath, json);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"保存配置文件失败: {ex.Message}", "错误", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public void ResetToDefaults()
        {
            config = new AppConfig();
            SaveConfig();
        }
    }

    public class AppConfig
    {
        public HotkeyConfig Hotkey { get; set; }
        public bool AutoStart { get; set; }
        public bool ShowNotifications { get; set; }

        public AppConfig()
        {
            Hotkey = new HotkeyConfig();
            AutoStart = true;
            ShowNotifications = true;
        }
    }
} 