namespace PolyDeploy.DeployClient
{
    using System;
    using System.Threading.Tasks;

    public class Delayer : IDelayer
    {
        public Task Delay(TimeSpan delay)
        {
            return Task.Delay(delay);
        }
    }
}