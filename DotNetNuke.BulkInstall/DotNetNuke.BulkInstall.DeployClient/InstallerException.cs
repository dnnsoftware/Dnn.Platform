// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.BulkInstall.DeployClient;

using System;
public class InstallerException : Exception
{
    public InstallerException(string message, Exception innerException)
        : base(message, innerException)
    {}
}
