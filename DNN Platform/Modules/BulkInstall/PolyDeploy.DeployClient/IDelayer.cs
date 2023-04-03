namespace PolyDeploy.DeployClient
{
    using System;
    using System.Threading.Tasks;

    public interface IDelayer
    {
        Task Delay(TimeSpan delay);
    }
}