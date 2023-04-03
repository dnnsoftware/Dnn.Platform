// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.BulkInstall.DeployClient
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
