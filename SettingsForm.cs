using System;
using System.Drawing;
using System.Windows.Forms;

namespace SwitchSound
{
    public partial class SettingsForm : Form
    {
        private ConfigManager configManager;
        private GlobalHotkey globalHotkey;
        private HotkeyConfig tempHotkey;
        
        // 控件
        private GroupBox hotkeyGroupBox;
        private Label hotkeyLabel;
        private TextBox hotkeyTextBox;
        private Button setHotkeyButton;
        private Button resetHotkeyButton;
        
        private GroupBox optionsGroupBox;
        private CheckBox autoStartCheckBox;
        private CheckBox showNotificationsCheckBox;
        
        private Button okButton;
        private Button cancelButton;
        private Button applyButton;

        public event EventHandler SettingsChanged;

        public SettingsForm(ConfigManager configManager, GlobalHotkey globalHotkey)
        {
            this.configManager = configManager;
            this.globalHotkey = globalHotkey;
            this.tempHotkey = new HotkeyConfig(configManager.Hotkey.ModifierKeys, configManager.Hotkey.Key);
            
            InitializeComponent();
            LoadSettings();
        }

        private void InitializeComponent()
        {
            this.Text = "设置";
            this.Size = new Size(400, 300);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.ShowIcon = false;

            // 热键设置组
            hotkeyGroupBox = new GroupBox();
            hotkeyGroupBox.Text = "快捷键设置";
            hotkeyGroupBox.Location = new Point(12, 12);
            hotkeyGroupBox.Size = new Size(360, 100);

            hotkeyLabel = new Label();
            hotkeyLabel.Text = "当前快捷键:";
            hotkeyLabel.Location = new Point(12, 25);
            hotkeyLabel.Size = new Size(80, 20);

            hotkeyTextBox = new TextBox();
            hotkeyTextBox.Location = new Point(100, 23);
            hotkeyTextBox.Size = new Size(120, 23);
            hotkeyTextBox.ReadOnly = true;

            setHotkeyButton = new Button();
            setHotkeyButton.Text = "设置";
            setHotkeyButton.Location = new Point(230, 22);
            setHotkeyButton.Size = new Size(60, 25);
            setHotkeyButton.Click += SetHotkeyButton_Click;

            resetHotkeyButton = new Button();
            resetHotkeyButton.Text = "重置";
            resetHotkeyButton.Location = new Point(295, 22);
            resetHotkeyButton.Size = new Size(60, 25);
            resetHotkeyButton.Click += ResetHotkeyButton_Click;

            var hotkeyHelpLabel = new Label();
            hotkeyHelpLabel.Text = "提示: 点击\"设置\"按钮后按下您想要的快捷键组合";
            hotkeyHelpLabel.Location = new Point(12, 55);
            hotkeyHelpLabel.Size = new Size(340, 20);
            hotkeyHelpLabel.ForeColor = Color.Gray;

            hotkeyGroupBox.Controls.AddRange(new Control[] { 
                hotkeyLabel, hotkeyTextBox, setHotkeyButton, resetHotkeyButton, hotkeyHelpLabel 
            });

            // 选项设置组
            optionsGroupBox = new GroupBox();
            optionsGroupBox.Text = "选项设置";
            optionsGroupBox.Location = new Point(12, 125);
            optionsGroupBox.Size = new Size(360, 80);

            autoStartCheckBox = new CheckBox();
            autoStartCheckBox.Text = "开机自动启动";
            autoStartCheckBox.Location = new Point(12, 25);
            autoStartCheckBox.Size = new Size(120, 20);

            showNotificationsCheckBox = new CheckBox();
            showNotificationsCheckBox.Text = "显示切换通知";
            showNotificationsCheckBox.Location = new Point(12, 50);
            showNotificationsCheckBox.Size = new Size(120, 20);

            optionsGroupBox.Controls.AddRange(new Control[] { 
                autoStartCheckBox, showNotificationsCheckBox 
            });

            // 按钮
            okButton = new Button();
            okButton.Text = "确定";
            okButton.Location = new Point(135, 220);
            okButton.Size = new Size(75, 25);
            okButton.Click += OkButton_Click;

            cancelButton = new Button();
            cancelButton.Text = "取消";
            cancelButton.Location = new Point(220, 220);
            cancelButton.Size = new Size(75, 25);
            cancelButton.Click += CancelButton_Click;

            applyButton = new Button();
            applyButton.Text = "应用";
            applyButton.Location = new Point(305, 220);
            applyButton.Size = new Size(75, 25);
            applyButton.Click += ApplyButton_Click;

            this.Controls.AddRange(new Control[] { 
                hotkeyGroupBox, optionsGroupBox, okButton, cancelButton, applyButton 
            });

            this.AcceptButton = okButton;
            this.CancelButton = cancelButton;
        }

        private void LoadSettings()
        {
            hotkeyTextBox.Text = configManager.Hotkey.ToString();
            autoStartCheckBox.Checked = configManager.AutoStart;
            showNotificationsCheckBox.Checked = configManager.ShowNotifications;
        }

        private void SetHotkeyButton_Click(object sender, EventArgs e)
        {
            var hotkeyDialog = new HotkeyDialog();
            if (hotkeyDialog.ShowDialog() == DialogResult.OK)
            {
                tempHotkey = hotkeyDialog.SelectedHotkey;
                hotkeyTextBox.Text = tempHotkey.ToString();
            }
        }

        private void ResetHotkeyButton_Click(object sender, EventArgs e)
        {
            tempHotkey = new HotkeyConfig();
            hotkeyTextBox.Text = tempHotkey.ToString();
        }

        private void OkButton_Click(object sender, EventArgs e)
        {
            ApplySettings();
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void CancelButton_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void ApplyButton_Click(object sender, EventArgs e)
        {
            ApplySettings();
        }

        private void ApplySettings()
        {
            try
            {
                // 验证热键配置
                if (!tempHotkey.IsValid())
                {
                    MessageBox.Show("请设置有效的快捷键组合", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // 保存配置到文件
                configManager.Hotkey = tempHotkey;
                configManager.AutoStart = autoStartCheckBox.Checked;
                configManager.ShowNotifications = showNotificationsCheckBox.Checked;
                
                // 通知主窗体更新设置
                SettingsChanged?.Invoke(this, EventArgs.Empty);
                
                MessageBox.Show("设置已保存并生效", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"保存设置失败: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }

    public partial class HotkeyDialog : Form
    {
        public HotkeyConfig SelectedHotkey { get; private set; }
        
        private Label instructionLabel;
        private TextBox hotkeyTextBox;
        private Button okButton;
        private Button cancelButton;
        
        private bool isCapturing = false;
        private GlobalHotkey.ModifierKeys currentModifiers;
        private Keys currentKey;

        public HotkeyDialog()
        {
            InitializeComponent();
            SelectedHotkey = new HotkeyConfig();
        }

        private void InitializeComponent()
        {
            this.Text = "设置快捷键";
            this.Size = new Size(300, 150);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.ShowIcon = false;
            this.KeyPreview = true;

            instructionLabel = new Label();
            instructionLabel.Text = "请按下您想要的快捷键组合:";
            instructionLabel.Location = new Point(12, 15);
            instructionLabel.Size = new Size(260, 20);

            hotkeyTextBox = new TextBox();
            hotkeyTextBox.Location = new Point(12, 40);
            hotkeyTextBox.Size = new Size(260, 23);
            hotkeyTextBox.ReadOnly = true;
            hotkeyTextBox.Text = "按下快捷键...";

            okButton = new Button();
            okButton.Text = "确定";
            okButton.Location = new Point(115, 80);
            okButton.Size = new Size(75, 25);
            okButton.Enabled = false;
            okButton.Click += OkButton_Click;

            cancelButton = new Button();
            cancelButton.Text = "取消";
            cancelButton.Location = new Point(200, 80);
            cancelButton.Size = new Size(75, 25);
            cancelButton.Click += CancelButton_Click;

            this.Controls.AddRange(new Control[] { 
                instructionLabel, hotkeyTextBox, okButton, cancelButton 
            });

            this.AcceptButton = okButton;
            this.CancelButton = cancelButton;
            
            this.KeyDown += HotkeyDialog_KeyDown;
            this.KeyUp += HotkeyDialog_KeyUp;
        }

        private void HotkeyDialog_KeyDown(object sender, KeyEventArgs e)
        {
            currentModifiers = GlobalHotkey.ModifierKeys.None;
            
            if (e.Control) currentModifiers |= GlobalHotkey.ModifierKeys.Control;
            if (e.Alt) currentModifiers |= GlobalHotkey.ModifierKeys.Alt;
            if (e.Shift) currentModifiers |= GlobalHotkey.ModifierKeys.Shift;
            
            // 只有在有修饰键的情况下才接受按键
            if (currentModifiers != GlobalHotkey.ModifierKeys.None && 
                e.KeyCode != Keys.ControlKey && e.KeyCode != Keys.Menu && e.KeyCode != Keys.ShiftKey)
            {
                currentKey = e.KeyCode;
                var tempHotkey = new HotkeyConfig(currentModifiers, currentKey);
                hotkeyTextBox.Text = tempHotkey.ToString();
                okButton.Enabled = true;
                isCapturing = true;
            }
        }

        private void HotkeyDialog_KeyUp(object sender, KeyEventArgs e)
        {
            if (isCapturing)
            {
                SelectedHotkey = new HotkeyConfig(currentModifiers, currentKey);
            }
        }

        private void OkButton_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void CancelButton_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }
    }
} 