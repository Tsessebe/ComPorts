using System;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using ComPort.Scanner.Properties;
using ComPort.Scanner.Watchers;
using ComPort.Scanner.Watchers.EventArguments;

namespace ComPort.Scanner
{
    public class ScannerContext : ApplicationContext
    {
        private NotifyIcon trayIcon;
        private SerialPortWatcher serialPortWatcher;

        public ScannerContext()
        {
            // constructor is within the OnApplicationIdle method
            // due to UI thread handling and preventing duplicates when having events
            Application.ApplicationExit += new EventHandler(OnExit);
            Application.Idle += new EventHandler(OnIdle);
        }

        private void OnExit(object sender, EventArgs e)
        {
            if (this.serialPortWatcher != null)
            {
                this.serialPortWatcher.Stop();
                this.serialPortWatcher.Changed -= OnComPortsChanged;
                this.serialPortWatcher.Added -= OnComPortsAdded;
            }

            if (this.trayIcon != null)
            {
                this.trayIcon.Visible = false;
                this.trayIcon.MouseClick -= OnTrayIconClick;
                this.trayIcon.MouseDoubleClick -= OnTrayIconMouseDoubleClick;
            }

            Application.Exit();
        }

        private void OnIdle(object sender, EventArgs e)
        {
            if (serialPortWatcher == null)
            {
                this.serialPortWatcher = new SerialPortWatcher();
                this.serialPortWatcher.Changed += OnComPortsChanged;
                this.serialPortWatcher.Added += OnComPortsAdded;
                this.serialPortWatcher.Start();

                this.trayIcon = new NotifyIcon()
                {
                    Text = "Com Port Monitor",
                    ContextMenu = new ContextMenu(
                        new MenuItem[]
                        {
                            new MenuItem("Show", OnShowForm),
                            new MenuItem("-"),
                            new MenuItem("Exit", OnExit)
                        }
                    ),
                    Icon = Icon.FromHandle(Resources.USB0.Handle),
                    Visible = true,
                };
                this.trayIcon.MouseClick += OnTrayIconClick;
                this.trayIcon.MouseDoubleClick += OnTrayIconMouseDoubleClick;
            }
        }

        private void OnComPortsAdded(object sender, AddedEventArgs e)
        {
            if (e.Ports.Count > 0)
            {
                var sb = new StringBuilder();
                foreach (var item in e.Ports)
                {
                    sb.AppendLine(item.DeviceName);
                }
                this.trayIcon.BalloonTipIcon = ToolTipIcon.Info;
                this.trayIcon.BalloonTipTitle = "Com Ports Added";
                this.trayIcon.BalloonTipText = sb.ToString();
                this.trayIcon.ShowBalloonTip(TimeSpan.FromSeconds(1).Milliseconds);
            }
        }

        private void OnComPortsChanged(object sender, ChangedEventArgs e)
        {
            
        }

        private void OnShowForm(object sender, EventArgs e)
        {
            var frm = new FormMain(this.serialPortWatcher.ComPorts.Values);
            this.serialPortWatcher.Changed += frm.OnComPortsChanged;
            frm.Show();
        }

        private void OnTrayIconClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {

            }
        }

        private void OnTrayIconMouseDoubleClick(object sender, MouseEventArgs e)
        {
            OnShowForm(sender, e);
        }
    }
}
