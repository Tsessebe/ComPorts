using System.Collections.Generic;
using ComPort.Scanner.Watchers.Models;

namespace ComPort.Scanner.Watchers.EventArguments
{
    public class AddedEventArgs : ChangedEventArgs
    {
        public AddedEventArgs(List<ComPortModel> ports)
            : base(ports)
        {
        }
    }
}
