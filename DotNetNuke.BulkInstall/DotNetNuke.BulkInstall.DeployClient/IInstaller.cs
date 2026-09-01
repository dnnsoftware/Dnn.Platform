// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.BulkInstall.DeployClient;

using System.IO;
using System.Threading.Tasks;

/// <summary>A contact specifying the ability to communicate with the installer APIs on a server.</summary>
public interface IInstaller
{
    /// <summary>Creates a new deployment session.</summary>
    /// <param name="options">The input options.</param>
    /// <returns>A <see cref="Task{TResult}"/> which resolves to the session ID.</returns>
    Task<string> StartSessionAsync(DeployInput options);

    /// <summary>Gets details about a session.</summary>
    /// <param name="options">The input options.</param>
    /// <param name="sessionId">The session ID.</param>
    /// <returns>A <see cref="Task{TResult}"/> which resolves to a <see cref="Session"/>.</returns>
    Task<Session> GetSessionAsync(DeployInput options, string sessionId);

    /// <summary>Uploads an encrypted package to a deployment session.</summary>
    /// <param name="options">The input options.</param>
    /// <param name="sessionId">The session ID.</param>
    /// <param name="encryptedPackage">The encrypted package contents.</param>
    /// <param name="packageName">The name of the package file.</param>
    /// <returns>A <see cref="Task"/> which resolves upon completion of the upload.</returns>
    UploadPackageResult UploadPackage(
        DeployInput options,
        string sessionId,
        Stream encryptedPackage,
        string packageName);

    /// <summary>Starts the installation of the packages for a session.</summary>
    /// <param name="options">The input options.</param>
    /// <param name="sessionId">The session ID.</param>
    /// <returns>A <see cref="Task"/> which resolves upon completion of the installation.</returns>
    Task InstallPackagesAsync(DeployInput options, string sessionId);
}
