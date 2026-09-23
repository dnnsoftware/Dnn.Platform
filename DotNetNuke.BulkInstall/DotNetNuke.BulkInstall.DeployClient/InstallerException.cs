// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.BulkInstall.DeployClient;

using System;

/// <summary>An exception to be thrown when there are issues in the <see cref="Installer"/>.</summary>
public class InstallerException : Exception
{
    /// <summary>Initializes a new instance of the <see cref="InstallerException"/> class.</summary>
    /// <param name="message">The exception message.</param>
    /// <param name="innerException">The inner exception.</param>
    public InstallerException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
