using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ComPort.Scanner.Extensions;
using ComPort.Scanner.Services.EventArguments;
using ComPort.Scanner.Watchers.EventArguments;
using Octokit;

namespace ComPort.Scanner.Services
{
    internal class VersionService
    {
        public event EventHandler<NewVersionEventArgs> NewVersionAvailible = delegate { };
        public event EventHandler<VersionEventArgs> NoUpdate = delegate { };

        private TaskScheduler taskScheduler;

        private readonly Version appVersion;
        private Version gitVersion;
        private bool manualCheck = false;

        public VersionService(string productVersion)
        {
            var versionFull = new Version(productVersion);
            appVersion = new Version(versionFull.Major, versionFull.Minor, versionFull.Build);
            NewVersion = false;
        }

        public void CheckGitHubVersion(bool manualCheck = false)
        {
            this.manualCheck = manualCheck;

            taskScheduler = TaskScheduler.Current;

            Task.Factory.StartNew(CheckGitHubNewerVersion, CancellationToken.None, TaskCreationOptions.None, taskScheduler);            
        }

        private async Task CheckGitHubNewerVersion()
        {
            var client = new GitHubClient(new ProductHeaderValue("ComPorts"));
            try
            {
                
                IReadOnlyList<Release> releases = await client.Repository.Release.GetAll("Tsessebe", "ComPorts");

                //Setup the versions
                gitVersion = new Version(releases[0].TagName.GetVersionNumbers());
                int versionComparison = appVersion.CompareTo(gitVersion);

                if (versionComparison < 0)
                {
                    //The version on GitHub is more up to date than this local release.
                    NewVersion = true;
                    OnNewVersionAvailible();
                }
                else if (versionComparison > 0)
                {
                    //This local version is greater than the release version on GitHub.
                    OnNoUpdate();
                }
                else
                {
                    //This local Version and the Version on GitHub are equal.
                    OnNoUpdate();
                }
            }
            catch (Exception)
            {
                // TODO: Log Maybee...
            }            
        }

        public bool NewVersion { get; private set; }

        private void OnNewVersionAvailible()
        {
            var handler = NewVersionAvailible;
            handler?.Invoke(this, new NewVersionEventArgs(gitVersion));
        }

        private void OnNoUpdate()
        {
            if (this.manualCheck)
            {
                var handler = NoUpdate;
                handler?.Invoke(this, new VersionEventArgs(gitVersion));
            }
        }
    }
}
