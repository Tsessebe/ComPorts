using System;
using System.Text.RegularExpressions;

namespace ComPort.Scanner.Watchers.Models
{
    public class ComPortModel
    {
        private static readonly Regex portNoRegex = new Regex("COM(?<portNo>[0-9]+)", RegexOptions.Compiled);

        private ComPortModel()
        {
            No = -1;
        }

        public int No { get; private set; }

        public string PortName { get; private set; }

        public string Caption { get; private set; }

        public string Name { get; private set; }

        public string DeviceId { get; private set; }

        public string PnPDeviceId { get; private set; }

        public string Description { get; private set; }

        public override string ToString()
        {
            return $"{Caption} ({PortName})";
        }

        public static ComPortModel Create(string name, string caption, string deviceId, string pnPDeviceId, string description)
        {
            var result = new ComPortModel()
            {
                Name = name,
                Caption = caption,
                DeviceId = deviceId,
                PnPDeviceId = pnPDeviceId,
                Description = description,
            };

            //result.PortName = portName;
            var m = portNoRegex.Match(result.Caption);
            if (m.Success)
            {
                result.No = Convert.ToInt32(m.Groups["portNo"].Value);
                result.PortName = $"COM{result.No}";
            }

            return result;
        }
    }
}
