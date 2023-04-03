// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.BulkInstall.DeployClient;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

/// <summary>A contract specifying the ability to display progress of the deployment process.</summary>
public interface IRenderer
{
    /// <summary>Renders a welcome message.</summary>
    /// <param name="level">The current log level.</param>
    void Welcome(LogLevel level);

    /// <summary>Renders a hierarchical list of <paramref name="files"/>.</summary>
    /// <param name="level">The current log level.</param>
    /// <param name="files">The file paths.</param>
    void RenderListOfFiles(LogLevel level, IEnumerable<string> files);

    /// <summary>Renders the file upload progress.</summary>
    /// <param name="level">The current log level.</param>
    /// <param name="uploads">The files to upload.</param>
    /// <returns>A <see cref="Task"/> which completes when the upload tasks have all completed.</returns>
    Task RenderFileUploadsAsync(LogLevel level, IEnumerable<(string File, Task UploadTask)> uploads);

    /// <summary>Renders the overview of the packages to be installed.</summary>
    /// <param name="level">The current log level.</param>
    /// <param name="packageFiles">The details of the packages to be installed.</param>
    void RenderInstallationOverview(LogLevel level, SortedList<int, SessionResponse?> packageFiles);

    /// <summary>Renders the status of the installation.</summary>
    /// <param name="level">The current log level.</param>
    /// <param name="packageFiles">The details of the packages being installed.</param>
    void RenderInstallationStatus(LogLevel level, SortedList<int, SessionResponse?> packageFiles);

    /// <summary>Renders a critical error.</summary>
    /// <param name="level">The current log level.</param>
    /// <param name="message">The friendly message.</param>
    /// <param name="exception">The exception.</param>
    void RenderCriticalError(LogLevel level, string message, Exception exception);
}
