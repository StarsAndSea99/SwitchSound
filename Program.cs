using System;
using System.Diagnostics;
using System.Threading;
using System.Windows.Forms;

namespace SwitchSound
{
    static class Program
    {
        private static Mutex mutex = null;
        private const string MutexName = "SwitchSound_SingleInstance_Mutex";

        [STAThread]
        static void Main()
        {
            // 检查是否已有实例在运行
            bool createdNew;
            mutex = new Mutex(true, MutexName, out createdNew);

            if (!createdNew)
            {
                // 程序已在运行，显示提示并退出
                MessageBox.Show("SwitchSound 已在运行中！\n\n请检查系统托盘图标。", 
                    "程序已运行", MessageBoxButtons.OK, MessageBoxIcon.Information);
                
                // 尝试激活已运行的实例
                BringExistingInstanceToFront();
                return;
            }

            try
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new MainForm());
            }
            finally
            {
                // 释放互斥锁
                mutex?.ReleaseMutex();
                mutex?.Dispose();
            }
        }

        private static void BringExistingInstanceToFront()
        {
            try
            {
                // 查找已运行的 SwitchSound 进程
                Process current = Process.GetCurrentProcess();
                Process[] processes = Process.GetProcessesByName(current.ProcessName);

                foreach (Process process in processes)
                {
                    if (process.Id != current.Id && process.MainWindowHandle != IntPtr.Zero)
                    {
                        // 激活已运行的实例窗口
                        ShowWindow(process.MainWindowHandle, SW_RESTORE);
                        SetForegroundWindow(process.MainWindowHandle);
                        break;
                    }
                }
            }
            catch
            {
                // 忽略激活失败的错误
            }
        }

        // Windows API 声明
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

        private const int SW_RESTORE = 9;
    }
} 