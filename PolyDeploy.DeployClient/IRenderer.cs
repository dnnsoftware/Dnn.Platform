namespace PolyDeploy.DeployClient
{
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public interface IRenderer
    {
        void Welcome();
        void RenderListOfFiles(IEnumerable<string> files);
        Task RenderFileUploadsAsync(IEnumerable<(string file, Task uploadTask)> uploads);
    }
}