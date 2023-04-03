// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.BulkInstall.DeployClient.Tests;

public static class TestHelpers
{
    public static DeployInput CreateDeployInput(string? targetUri = null, string? apiKey = null, string? encryptionKey = null, int? installationStatusTimeout = null, string? packagesDirectoryPath = null, LogLevel? logLevel = null)
    {
        return new DeployInput
        {
            TargetUri = targetUri ?? "https://test.com",
            ApiKey = apiKey ?? A.Dummy<string>(),
            EncryptionKey = encryptionKey ?? A.Dummy<string>(),
            InstallationStatusTimeout = installationStatusTimeout ?? 0,
            PackagesDirectoryPath = packagesDirectoryPath ?? A.Dummy<string>(),
            LogLevel = logLevel ?? A.Dummy<LogLevel>()
        };
    }
}
