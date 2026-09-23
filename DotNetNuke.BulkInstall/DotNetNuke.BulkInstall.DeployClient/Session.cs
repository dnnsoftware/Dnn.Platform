// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.BulkInstall.DeployClient;

/// <summary>Information about a deployment session.</summary>
public class Session
{
    /// <summary>Gets the session status.</summary>
    public SessionStatus Status { get; init; }

    /// <summary>Gets or sets the session responses.</summary>
    public SortedList<int, SessionResponse?>? Responses { get; set; }
}
