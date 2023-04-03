// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.BulkInstall.DeployClient;

/// <summary>The exit code of the application CLI.</summary>
public enum ExitCode
{
    /// <summary>The deployment was successful.</summary>
    Success = 0,

    /// <summary>There was an error interacting with the installer.</summary>
    InstallerError = 1,

    /// <summary>The installer detected errors installing a package.</summary>
    PackageError = 2,

    /// <summary>Any other error.</summary>
    UnexpectedError = int.MaxValue,
}
