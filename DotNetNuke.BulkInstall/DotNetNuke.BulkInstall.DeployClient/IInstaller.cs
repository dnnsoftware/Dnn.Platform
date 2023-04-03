// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.BulkInstall.DeployClient
{
    using System.IO;
    using System.Threading.Tasks;

    public interface IInstaller
    {
        Task<string> StartSessionAsync(DeployInput options);

        Task<Session> GetSessionAsync(DeployInput options, string sessionId);

        Task UploadPackageAsync(DeployInput options, string sessionId, Stream encryptedPackage, string packageName);

        Task InstallPackagesAsync(DeployInput options, string sessionId);
    }
}
