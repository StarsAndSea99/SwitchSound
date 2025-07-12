# SwitchSound - 音频设备快捷键切换工具

[English](#english) | [中文](#中文)

## 中文

### 📖 简介

SwitchSound 是一个轻量级的 Windows 音频设备快捷键切换工具，让您可以通过自定义热键快速在不同的音频播放设备之间切换。

### ✨ 特性

- 🎯 **轻量级**：单文件仅 178KB，无需安装
- ⚡ **快速切换**：支持自定义全局热键（默认 Ctrl+Alt+S）
- 🔄 **循环切换**：按顺序在所有可用音频设备间切换
- 🚀 **开机自启**：支持开机自动启动
- 🎛️ **系统托盘**：常驻系统托盘，右键显示设备列表
- ⚙️ **配置管理**：设置自动保存，支持热键自定义
- 🛡️ **单实例运行**：防止重复启动冲突
- 💬 **通知提示**：切换时显示气泡通知（可选）

### 🎮 使用方法

1. 运行 `SwitchSound.exe`
2. 程序将自动最小化到系统托盘
3. 使用默认热键 `Ctrl+Alt+S` 切换音频设备
4. 右键托盘图标可手动选择设备或打开设置

### ⚙️ 系统要求

- Windows 10/11
- .NET 6.0 Runtime

### 🔧 自定义设置

- 双击托盘图标打开设置界面
- 可自定义热键组合
- 可开启/关闭切换通知
- 可设置开机自启动

### 📦 下载

从 [Releases](../../releases) 页面下载最新版本。

### 🛠️ 技术栈

- C# / .NET 6.0
- Windows Forms
- Core Audio API
- Win32 API

---

## English

### 📖 Description

SwitchSound is a lightweight Windows audio device hotkey switcher that allows you to quickly switch between different audio playback devices using customizable hotkeys.

### ✨ Features

- 🎯 **Lightweight**: Single file only 178KB, no installation required
- ⚡ **Quick Switch**: Support custom global hotkeys (default Ctrl+Alt+S)
- 🔄 **Cycle Through**: Switch between all available audio devices in sequence
- 🚀 **Auto Start**: Support automatic startup with Windows
- 🎛️ **System Tray**: Resides in system tray with right-click device list
- ⚙️ **Configuration**: Auto-save settings with hotkey customization
- 🛡️ **Single Instance**: Prevent duplicate startup conflicts
- 💬 **Notifications**: Show bubble notifications when switching (optional)

### 🎮 Usage

1. Run `SwitchSound.exe`
2. The program will automatically minimize to system tray
3. Use default hotkey `Ctrl+Alt+S` to switch audio devices
4. Right-click tray icon to manually select devices or open settings

### ⚙️ System Requirements

- Windows 10/11
- .NET 6.0 Runtime

### 🔧 Customization

- Double-click tray icon to open settings
- Customize hotkey combinations
- Enable/disable switch notifications
- Configure auto-start with Windows

### 📦 Download

Download the latest version from the [Releases](../../releases) page.

### 🛠️ Tech Stack

- C# / .NET 6.0
- Windows Forms
- Core Audio API
- Win32 API

### 🤝 Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

### 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

### 🙏 Acknowledgments

- Thanks to the Core Audio API documentation
- Inspired by the need for quick audio device switching 