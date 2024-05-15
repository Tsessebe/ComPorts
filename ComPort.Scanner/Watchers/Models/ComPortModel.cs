using System;
using System.Text.RegularExpressions;

namespace ComPort.Scanner.Watchers.Models
{
    public class ComPortModel
    {
        private static readonly Regex portNo = new Regex("[a-zA-Z]+(?<portNo>[0-9]+)", RegexOptions.Compiled);

        public ComPortModel(string name, string deviceName)
        {
            No = -1;
            Name = name;
            DeviceName = string.IsNullOrEmpty(deviceName) ? name : deviceName;
            var m = portNo.Match(Name);
            if (m.Success)
            {
                No = Convert.ToInt32(m.Groups["portNo"].Value);
            }
        }

        public int No { get; }

        public string Name { get; }

        public string DeviceName { get; }

        public override string ToString()
        {
            return $"{DeviceName} ({Name})";
        }
    }
}
