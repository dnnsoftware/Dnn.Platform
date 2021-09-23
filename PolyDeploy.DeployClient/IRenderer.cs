namespace PolyDeploy.DeployClient
{
    using System.Collections.Generic;

    public interface IRenderer
    {
        void RenderListOfFiles(IEnumerable<string> files);
    }
}