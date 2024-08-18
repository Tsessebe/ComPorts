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
        private TaskScheduler taskScheduler;

        private ManagementEventWatcher watcher;

        public SerialPortWatcher()
        {
            ComPorts = new ConcurrentDictionary<string, ComPortModel>();
        }

        public event EventHandler<AddedEventArgs> Added = delegate { };
        public event EventHandler<ChangedEventArgs> Changed = delegate { };

        public ConcurrentDictionary<string, ComPortModel> ComPorts { get; }

        public void Dispose()
        {
            watcher.Stop();
        }

        public void Start()
        {
            taskScheduler = TaskScheduler.FromCurrentSynchronizationContext();            

            var query = new WqlEventQuery(@"SELECT * FROM Win32_DeviceChangeEvent");
            watcher = new ManagementEventWatcher(query);
            watcher.EventArrived += (sender, eventArgs) => CheckForNewPorts(eventArgs);

            CheckForNewPorts(null);

            watcher.Start();
        }

        public void Stop()
        {
            watcher?.Stop();
        }

        // ReSharper disable once UnusedParameter.Local
        private void CheckForNewPorts(EventArrivedEventArgs args)
        {
            // do it async, so it is performed in the UI thread if this class has been created in the UI thread
            Task.Factory.StartNew(CheckForNewPortsAsync, CancellationToken.None, TaskCreationOptions.None, taskScheduler);
        }

        private void CheckForNewPortsAsync()
        {
            var newPorts = false;
            var removedPorts = false;
            var addedPorts = new List<ComPortModel>();

            using (var searcher =
                   new ManagementObjectSearcher(@"SELECT * FROM Win32_PnPEntity WHERE Caption like '%(COM%'"))
            {
                var portNames = SerialPort.GetPortNames().OrderBy(p => p).ToList();
                
                var portObjects = searcher.Get().Cast<ManagementBaseObject>()
                    .Select(p => ComPortModel.Create(name:p["Name"].ToString(),
                        caption:p["Caption"].ToString(),
                        deviceId:p["DeviceId"].ToString(),
                        pnPDeviceId:p["PNPDeviceID"].ToString(),
                        description:p["Description"].ToString()
                    ))
                    .Where(o => !o.Name.Contains("Bluetooth link"))
                    .ToList();


                foreach (var port in portObjects)
                {
                    if (!ComPorts.TryAdd(port.PortName, port))
                    {
                        continue;
                    }

                    addedPorts.Add(port);
                    newPorts = true;
                }

                foreach (var comPort in ComPorts.Values)
                {
                    if (portNames.Contains(comPort.PortName))
                    {
                        continue;
                    }

                    if (ComPorts.TryRemove(comPort.PortName, out var _))
                    {
                        removedPorts = true;
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

        private void OnPortsAdded(List<ComPortModel> ports)
        {
            var handler = Added;
            handler?.Invoke(this, new AddedEventArgs(ports));
        }

        private void OnPortsChanged()
        {
            var handler = Changed;
            handler?.Invoke(this, new ChangedEventArgs(ComPorts.Values.ToList()));
        }
    }
}