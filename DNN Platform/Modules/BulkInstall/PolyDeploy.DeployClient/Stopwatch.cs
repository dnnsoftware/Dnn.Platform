
namespace PolyDeploy.DeployClient
{
    using System;

    public class Stopwatch : IStopwatch
    {
        public void StartNew()
        {
            this.stopwatch = System.Diagnostics.Stopwatch.StartNew();
        }

        public TimeSpan Elapsed { get { return this.stopwatch?.Elapsed ?? TimeSpan.Zero; } }

        private System.Diagnostics.Stopwatch? stopwatch;

    }
}