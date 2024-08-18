using System;

namespace ComPort.Scanner.Services.EventArguments
{
    public class VersionEventArgs : EventArgs
    {
        public VersionEventArgs(Version gitVersion)
        {
            this.GitVersion = gitVersion;
        }

        public Version GitVersion { get; }
    }

    public class NewVersionEventArgs : VersionEventArgs
    {
        public NewVersionEventArgs(Version gitVersion)
            : base(gitVersion)
        {

        }
    }
}
