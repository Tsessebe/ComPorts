using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO.Ports;
using System.Linq;
using System.Management;
using System.Threading;
using System.Threading.Tasks;
using ComPort.Scanner.Watchers.EventArguments;
using ComPort.Scanner.Watchers.Models;

namespace ComPort.Scanner.Watchers
{
    [ExcludeFromCodeCoverage]
    public sealed class SerialPortWatcher : IDisposable
    {
        public event EventHandler<ChangedEventArgs> Changed = delegate { };
        public event EventHandler<AddedEventArgs> Added = delegate { };

        private void OnPortsChanged()
        {
            var handler = Changed;
            handler?.Invoke(this, new ChangedEventArgs(ComPorts.Values.ToList()));
        }

        private void OnPortsAdded(List<ComPortModel> ports)
        {
            var handler = Added;
            handler?.Invoke(this, new AddedEventArgs(ports));
        }

        private ManagementEventWatcher watcher;
        private TaskScheduler taskScheduler;

        public SerialPortWatcher()
        {
            ComPorts = new ConcurrentDictionary<string, ComPortModel>();
        }

        public ConcurrentDictionary<string, ComPortModel> ComPorts { get; private set; }

        public void Start()
        {
            taskScheduler = TaskScheduler.FromCurrentSynchronizationContext();

            WqlEventQuery query = new WqlEventQuery("SELECT * FROM Win32_DeviceChangeEvent");
            watcher = new ManagementEventWatcher(query);
            watcher.EventArrived += (sender, eventArgs) => CheckForNewPorts(eventArgs);

            CheckForNewPorts(null);

            watcher.Start();
        }

        public void Stop()
        {
            watcher?.Stop();
        }

        private void CheckForNewPorts(EventArrivedEventArgs args)
        {
            // do it async so it is performed in the UI thread if this class has been created in the UI thread
            Task.Factory.StartNew(CheckForNewPortsAsync, CancellationToken.None, TaskCreationOptions.None, taskScheduler);
        }

        private void CheckForNewPortsAsync()
        {
            bool newPorts = false;
            bool removedPorts = false;
            var addedPorts = new List<ComPortModel>();

            using (var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_PnPEntity WHERE Caption like '%(COM%'"))
            {
                var portnames = SerialPort.GetPortNames().OrderBy(_ => _).ToList();
                var ports = searcher.Get().Cast<ManagementBaseObject>().ToList().Select(p => p["Caption"].ToString());

                var portList = portnames
                    .Select(n => new ComPortModel(n, ports.FirstOrDefault(s => s.Contains(n))))
                    .Where(_ => !_.DeviceName.StartsWith("Standard Serial"))
                    .OrderBy(_ => _.Name).ToList();


                foreach (var port in portList)
                {
                    if (ComPorts.TryAdd(port.Name, port))
                    {
                        addedPorts.Add(port);
                        newPorts = true;
                    }

                }

                foreach (var comPort in ComPorts.Values)
                {
                    if (!portnames.Contains(comPort.Name))
                    {
                        if (ComPorts.TryRemove(comPort.Name, out var _))
                        {
                            removedPorts = true;
                        }
                    }
                }
            }
            if (newPorts)
            {
                OnPortsAdded(addedPorts);
            }

            if (newPorts || removedPorts)
            {
                OnPortsChanged();
            }
        }

        #region IDisposable Members

        public void Dispose()
        {
            watcher.Stop();
        }

        #endregion

    }
}
