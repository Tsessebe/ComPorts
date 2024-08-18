using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using ComPort.Scanner.Extensions;
using ComPort.Scanner.Properties;
using ComPort.Scanner.Services;
using ComPort.Scanner.Services.EventArguments;
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
        private VersionService versionService;
        private NotifyIcon trayIcon;

        private FormMain mainForm = null;

        public ScannerContext()
        {
            // constructor is within the OnApplicationIdle method
            // due to UI thread handling and preventing duplicates when having events
            Application.ApplicationExit += OnExit;
            Application.Idle += OnIdle;
            winStartMenu = new MenuItem("Run on Windows Start", OnWinStartClick);
            winStartMenu.Checked = true;

            AddStartup(RegAppName, Application.ExecutablePath);
            versionService = new VersionService(Application.ProductVersion);
            versionService.NewVersionAvailible += OnVersionServiceNewVersionAvailible;
            versionService.NoUpdate += OnVersionServiceNoUpdate;
            versionService.CheckGitHubVersion();

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
                sb.AppendLine(item.Caption);
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
#if DEBUG
                Text = "Com Port Monitor - DEV",
#else
                Text = "Com Port Monitor",
#endif
                ContextMenu = new ContextMenu(
                    new[]
                    {
                        new MenuItem("Show", OnShowFormClick),
                        new MenuItem("-"),
                        new MenuItem("Open Device Manager", OnDeviceManagerClick),
                        new MenuItem("-"),
                        winStartMenu,
                        new MenuItem("About", OnAboutClick),
                        new MenuItem("Check for Update", OnCheckUpdateClick),
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

        private void OnCheckUpdateClick(object sender, EventArgs e)
        {
            versionService.CheckGitHubVersion(true);
        }

        private void OnShowFormClick(object sender, EventArgs e)
        {
            if (mainForm == null)
            {
                mainForm = new FormMain(serialPortWatcher.ComPorts.Values);
                serialPortWatcher.Changed += mainForm.OnComPortsChanged;
                mainForm.Closing += OnFormClosing;
            }
            mainForm.Show();
            mainForm.Activate();
        }

        private void OnFormClosing(object sender, CancelEventArgs cancelEventArgs)
        {
            if (!(sender is FormMain frm))
            {
                return;
            }
            serialPortWatcher.Changed -= mainForm.OnComPortsChanged;
            mainForm.Dispose();
            mainForm = null;
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

        private void OnVersionServiceNewVersionAvailible(object sender, NewVersionEventArgs e)
        {
            trayIcon.BalloonTipIcon = ToolTipIcon.Info;
            trayIcon.BalloonTipTitle = "Com Ports - New Verion";
            trayIcon.BalloonTipText = $"Version: {e.GitVersion} is availible.";
            trayIcon.ShowBalloonTip(TimeSpan.FromSeconds(1).Milliseconds);
        }

        private void OnVersionServiceNoUpdate(object sender, VersionEventArgs e)
        {
            MessageBox.Show("No New Version availible", "Com Ports Version", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }
}