using System;
using System.Collections.Generic;
using ComPort.Scanner.Watchers.Models;

namespace ComPort.Scanner.Watchers.EventArguments
{
    public class ChangedEventArgs : EventArgs
    {
        private readonly List<ComPortModel> ports;

        public ChangedEventArgs(List<ComPortModel> ports)
        {
            this.ports = ports;
        }

        public List<ComPortModel> Ports => ports;
    }
}
