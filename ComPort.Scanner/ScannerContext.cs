using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using ComPort.Scanner.Properties;
using ComPort.Scanner.Watchers;
using ComPort.Scanner.Watchers.EventArguments;
using Microsoft.Win32;

namespace ComPort.Scanner
{
    public class ScannerContext : ApplicationContext
    {
#if DEBUG
        private const string RegAppName = "ComPorts_Dev";
#else
        private const string RegAppName = "ComPorts";
#endif
        private const string SubKeyPath = "SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run";
        private readonly MenuItem winStartMenu;
        private SerialPortWatcher serialPortWatcher;
        private NotifyIcon trayIcon;
        
        public ScannerContext()
        {
            // constructor is within the OnApplicationIdle method
            // due to UI thread handling and preventing duplicates when having events
            Application.ApplicationExit += OnExit;
            Application.Idle += OnIdle;
            winStartMenu = new MenuItem("Run on Windows Start", OnWinStartClick);
            winStartMenu.Checked = true;

            AddStartup(RegAppName, Application.ExecutablePath);
        }

        private static void AddStartup(string appName, string path)
        {
            using (var key = Registry.CurrentUser.OpenSubKey(SubKeyPath, true))
            {
                key?.SetValue(appName, "\"" + path + "\"", RegistryValueKind.String);
            }
        }

        private static void RemoveStartup(string appName)
        {
            using (var key = Registry.CurrentUser.OpenSubKey(SubKeyPath, true))
            {
                key?.DeleteValue(appName, false);
            }
        }

        private void OnComPortsAdded(object sender, AddedEventArgs e)
        {
            if (e.Ports.Count <= 0)
            {
                return;
            }

            var sb = new StringBuilder();
            foreach (var item in e.Ports)
            {
                sb.AppendLine(item.DeviceName);
            }

            trayIcon.BalloonTipIcon = ToolTipIcon.Info;
            trayIcon.BalloonTipTitle = "Com Ports Added";
            trayIcon.BalloonTipText = sb.ToString();
            trayIcon.ShowBalloonTip(TimeSpan.FromSeconds(1).Milliseconds);
        }

        private void OnDeviceManagerClick(object sender, EventArgs e)
        {
            var processStartInfo = new ProcessStartInfo
            {
                FileName = "mmc.exe",
                Arguments = "devmgmt.msc"
            };

            var process = new Process
            {
                StartInfo = processStartInfo
            };

            process.Start();
        }

        private void OnExit(object sender, EventArgs e)
        {
            if (serialPortWatcher != null)
            {
                serialPortWatcher.Stop();
                serialPortWatcher.Added -= OnComPortsAdded;
            }

            if (trayIcon != null)
            {
                trayIcon.Visible = false;
                trayIcon.MouseDoubleClick -= OnTrayIconMouseDoubleClick;
            }
#if DEBUG
            RemoveStartup(RegAppName);
#endif
            Application.Exit();
        }

        private void OnIdle(object sender, EventArgs e)
        {
            if (serialPortWatcher != null)
            {
                return;
            }

            serialPortWatcher = new SerialPortWatcher();
            serialPortWatcher.Added += OnComPortsAdded;
            serialPortWatcher.Start();

            trayIcon = new NotifyIcon
            {
                Text = "Com Port Monitor",
                ContextMenu = new ContextMenu(
                    new[]
                    {
                        new MenuItem("Show", OnShowFormClick),
                        new MenuItem("-"),
                        new MenuItem("Open Device Manager", OnDeviceManagerClick),
                        new MenuItem("-"),
                        winStartMenu,
                        new MenuItem("About", OnAboutClick),
                        new MenuItem("Exit", OnExit)
                    }
                ),
                Icon = Icon.FromHandle(Resources.USB0.Handle),
                Visible = true
            };
            trayIcon.MouseDoubleClick += OnTrayIconMouseDoubleClick;
        }

        private void OnAboutClick(object sender, EventArgs e)
        {
            var frm = new FormAbout();
            frm.ShowDialog();
        }

        private void OnShowFormClick(object sender, EventArgs e)
        {
            var frm = new FormMain(serialPortWatcher.ComPorts.Values);
            serialPortWatcher.Changed += frm.OnComPortsChanged;
            frm.Closing += OnFormClosing;
            frm.Show();
        }
        
        private void OnFormClosing(object sender, CancelEventArgs cancelEventArgs)
        {
            if (!(sender is FormMain frm))
            {
                return;
            }
            serialPortWatcher.Changed -= frm.OnComPortsChanged;
        }
        
        private void OnTrayIconMouseDoubleClick(object sender, MouseEventArgs e)
        {
            OnShowFormClick(sender, e);
        }

        private void OnWinStartClick(object sender, EventArgs e)
        {
            if (!(sender is MenuItem mnu))
            {
                return;
            }

            mnu.Checked = !mnu.Checked;

            if (mnu.Checked)
            {
                AddStartup(RegAppName, Application.ExecutablePath);
            }
            else
            {
                RemoveStartup(RegAppName);
            }
        }
    }
}