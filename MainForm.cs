using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Microsoft.Win32;

namespace SwitchSound
{
    public partial class MainForm : Form
    {
        private NotifyIcon notifyIcon;
        private CoreAudioApi audioApi;
        private List<CoreAudioApi.AudioDevice> audioDevices;
        private int currentDeviceIndex = 0;
        private GlobalHotkey globalHotkey;
        private ConfigManager configManager;

        public MainForm()
        {
            InitializeComponent();
            InitializeApplication();
        }

        private void InitializeComponent()
        {
            // 设置窗体属性
            this.Text = "SwitchSound - 音频设备切换工具";
            this.Size = new Size(400, 300);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.ShowInTaskbar = false;
            this.WindowState = FormWindowState.Minimized;
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;

            // 创建系统托盘图标
            notifyIcon = new NotifyIcon();
            notifyIcon.Icon = SystemIcons.Application;
            notifyIcon.Text = "SwitchSound - 音频设备切换工具";
            notifyIcon.Visible = true;
            notifyIcon.DoubleClick += NotifyIcon_DoubleClick;

            // 创建托盘菜单
            CreateTrayMenu();

            // 窗体事件
            this.Resize += MainForm_Resize;
            this.FormClosing += MainForm_FormClosing;
            this.Load += MainForm_Load;
        }

        private void InitializeApplication()
        {
            try
            {
                // 初始化音频API
                audioApi = new CoreAudioApi();
                
                // 初始化配置管理器
                configManager = new ConfigManager();
                
                // 加载音频设备
                RefreshAudioDevices();
                
                // 初始化全局热键
                InitializeHotkey();
                
                // 设置开机自启动
                SetAutoStart(configManager.AutoStart);

                // 显示启动通知
                notifyIcon.ShowBalloonTip(2000, "SwitchSound 已启动", 
                    $"快捷键: {configManager.Hotkey}\n双击托盘图标打开设置", 
                    ToolTipIcon.Info);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"初始化应用程序失败: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void CreateTrayMenu()
        {
            var contextMenu = new ContextMenuStrip();
            
            // 设备列表菜单项
            var devicesMenuItem = new ToolStripMenuItem("音频设备");
            contextMenu.Items.Add(devicesMenuItem);
            
            contextMenu.Items.Add(new ToolStripSeparator());
            
            // 设置菜单项
            var settingsMenuItem = new ToolStripMenuItem("设置");
            settingsMenuItem.Click += SettingsMenuItem_Click;
            contextMenu.Items.Add(settingsMenuItem);
            
            // 刷新设备列表
            var refreshMenuItem = new ToolStripMenuItem("刷新设备列表");
            refreshMenuItem.Click += RefreshMenuItem_Click;
            contextMenu.Items.Add(refreshMenuItem);
            
            contextMenu.Items.Add(new ToolStripSeparator());

            // 显示主窗口菜单项
            var showMenuItem = new ToolStripMenuItem("显示主窗口");
            showMenuItem.Click += ShowMenuItem_Click;
            contextMenu.Items.Add(showMenuItem);
            
            // 关于菜单项
            var aboutMenuItem = new ToolStripMenuItem("关于");
            aboutMenuItem.Click += AboutMenuItem_Click;
            contextMenu.Items.Add(aboutMenuItem);
            
            // 退出菜单项
            var exitMenuItem = new ToolStripMenuItem("退出");
            exitMenuItem.Click += ExitMenuItem_Click;
            contextMenu.Items.Add(exitMenuItem);
            
            notifyIcon.ContextMenuStrip = contextMenu;
        }

        private void RefreshAudioDevices()
        {
            try
            {
                audioDevices = audioApi.GetAudioDevices();
                
                // 更新托盘菜单中的设备列表
                UpdateDeviceMenu();
                
                // 找到当前默认设备的索引
                var defaultDevice = audioDevices.FirstOrDefault(d => d.IsDefault);
                if (defaultDevice != null)
                {
                    currentDeviceIndex = audioDevices.IndexOf(defaultDevice);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"刷新音频设备失败: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void UpdateDeviceMenu()
        {
            var contextMenu = notifyIcon.ContextMenuStrip;
            var devicesMenuItem = contextMenu.Items[0] as ToolStripMenuItem;
            
            devicesMenuItem.DropDownItems.Clear();
            
            for (int i = 0; i < audioDevices.Count; i++)
            {
                var device = audioDevices[i];
                var menuItem = new ToolStripMenuItem(device.Name);
                menuItem.Checked = device.IsDefault;
                menuItem.Tag = i;
                menuItem.Click += DeviceMenuItem_Click;
                devicesMenuItem.DropDownItems.Add(menuItem);
            }
        }

        private void InitializeHotkey()
        {
            try
            {
                // 先注销旧的热键
                globalHotkey?.Dispose();
                
                // 重新创建热键管理器
                globalHotkey = new GlobalHotkey(this.Handle);
                
                // 注册新的热键
                var hotkey = configManager.Hotkey;
                globalHotkey.RegisterHotkey(hotkey.ModifierKeys, hotkey.Key, SwitchToNextDevice);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"注册热键失败: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void SwitchToNextDevice()
        {
            if (audioDevices == null || audioDevices.Count == 0)
            {
                RefreshAudioDevices();
                return;
            }

            // 切换到下一个设备
            currentDeviceIndex = (currentDeviceIndex + 1) % audioDevices.Count;
            var nextDevice = audioDevices[currentDeviceIndex];
            
            try
            {
                if (audioApi.SetDefaultAudioDevice(nextDevice.Id))
                {
                    // 显示通知
                    if (configManager.ShowNotifications)
                    {
                        notifyIcon.ShowBalloonTip(2000, "音频设备切换", $"已切换到: {nextDevice.Name}", ToolTipIcon.Info);
                    }
                    
                    // 更新设备列表
                    RefreshAudioDevices();
                }
                else
                {
                    notifyIcon.ShowBalloonTip(2000, "切换失败", $"无法切换到: {nextDevice.Name}", ToolTipIcon.Error);
                }
            }
            catch (Exception ex)
            {
                notifyIcon.ShowBalloonTip(2000, "切换失败", ex.Message, ToolTipIcon.Error);
            }
        }

        private void SetAutoStart(bool enabled)
        {
            try
            {
                var key = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", true);
                if (enabled)
                {
                    key?.SetValue("SwitchSound", Application.ExecutablePath);
                }
                else
                {
                    key?.DeleteValue("SwitchSound", false);
                }
                key?.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"设置开机自启动失败: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // 事件处理方法
        private void MainForm_Load(object sender, EventArgs e)
        {
            // 启动时隐藏窗口
            this.Hide();
        }

        private void MainForm_Resize(object sender, EventArgs e)
        {
            if (this.WindowState == FormWindowState.Minimized)
            {
                this.Hide();
            }
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            // 阻止窗口关闭，改为最小化到托盘
            if (e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true;
                this.Hide();
            }
        }

        private void NotifyIcon_DoubleClick(object sender, EventArgs e)
        {
            // 双击托盘图标显示设置窗口
            ShowSettingsWindow();
        }

        private void DeviceMenuItem_Click(object sender, EventArgs e)
        {
            var menuItem = sender as ToolStripMenuItem;
            var deviceIndex = (int)menuItem.Tag;
            var device = audioDevices[deviceIndex];
            
            try
            {
                if (audioApi.SetDefaultAudioDevice(device.Id))
                {
                    currentDeviceIndex = deviceIndex;
                    if (configManager.ShowNotifications)
                    {
                        notifyIcon.ShowBalloonTip(2000, "音频设备切换", $"已切换到: {device.Name}", ToolTipIcon.Info);
                    }
                    RefreshAudioDevices();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"切换设备失败: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void SettingsMenuItem_Click(object sender, EventArgs e)
        {
            ShowSettingsWindow();
        }

        private void RefreshMenuItem_Click(object sender, EventArgs e)
        {
            RefreshAudioDevices();
            notifyIcon.ShowBalloonTip(1000, "刷新完成", "设备列表已刷新", ToolTipIcon.Info);
        }

        private void ShowMenuItem_Click(object sender, EventArgs e)
        {
            ShowMainWindow();
        }

        private void AboutMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show("SwitchSound v1.0\n\n音频设备快捷键切换工具\n\n支持自定义热键快速切换音频播放设备\n\n单实例运行，防止重复启动", 
                "关于", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void ExitMenuItem_Click(object sender, EventArgs e)
        {
            // 真正退出应用程序
            notifyIcon.Visible = false;
            globalHotkey?.Dispose();
            Application.Exit();
        }

        private void ShowSettingsWindow()
        {
            var settingsForm = new SettingsForm(configManager, globalHotkey);
            settingsForm.SettingsChanged += (s, args) =>
            {
                // 更新开机自启动设置
                SetAutoStart(configManager.AutoStart);
                
                // 重新初始化热键（单独处理错误）
                try
                {
                    InitializeHotkey();
                }
                catch (Exception hotkeyEx)
                {
                    // 热键注册失败不影响其他设置
                    notifyIcon.ShowBalloonTip(3000, "热键注册失败", 
                        $"无法注册热键 {configManager.Hotkey}：{hotkeyEx.Message}。其他设置已保存。", 
                        ToolTipIcon.Warning);
                }
            };
            settingsForm.ShowDialog();
        }

        private void ShowMainWindow()
        {
            this.Show();
            this.WindowState = FormWindowState.Normal;
            this.BringToFront();
            this.Activate();
        }

        protected override void WndProc(ref Message m)
        {
            // 处理全局热键消息
            globalHotkey?.ProcessMessage(ref m);
            base.WndProc(ref m);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                notifyIcon?.Dispose();
                globalHotkey?.Dispose();
            }
            base.Dispose(disposing);
        }
    }
} 