namespace PolyDeploy.DeployClient.Tests
{
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public interface IRenderer
    {
        Task RenderListOfFiles(IEnumerable<string> files);
    }
}