
namespace PolyDeploy.DeployClient
{
    using System;

    public interface IStopwatch
    {
        void StartNew();

        TimeSpan Elapsed { get; }

    }
}