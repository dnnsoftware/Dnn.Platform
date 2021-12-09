namespace PolyDeploy.DeployClient
{
    using System.Collections.Generic;

    public interface IRenderer
    {
        void RenderListOfFiles(IEnumerable<string> files);
        void RenderFileUploadStarted(string file);
        void RenderFileUploadComplete(string file);
    }
}