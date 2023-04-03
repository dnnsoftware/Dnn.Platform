// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.BulkInstall.DeployClient;

/// <summary>The session status.</summary>
public enum SessionStatus
{
    /// <summary>The session has not yet started.</summary>
    NotStarted = 0,

    /// <summary>The session is in progress.</summary>
    InProgess = 1,

    /// <summary>The session is complete.</summary>
    Complete = 2,
}
