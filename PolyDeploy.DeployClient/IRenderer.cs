namespace PolyDeploy.DeployClient
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public interface IRenderer
    {
        void Welcome(LogLevel level);
        void RenderListOfFiles(LogLevel level, IEnumerable<string> files);
        Task RenderFileUploadsAsync(LogLevel level, IEnumerable<(string file, Task uploadTask)> uploads);
        void RenderInstallationOverview(LogLevel level, SortedList<int, SessionResponse?> packageFiles);
        void RenderInstallationStatus(LogLevel level, SortedList<int, SessionResponse?> packageFiles);
        void RenderCriticalError(LogLevel level, string message, Exception exception);
    }
}
