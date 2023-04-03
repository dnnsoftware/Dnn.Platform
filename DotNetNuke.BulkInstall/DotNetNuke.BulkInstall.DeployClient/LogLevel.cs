// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.BulkInstall.DeployClient;

/// <summary>The level of logging.</summary>
public enum LogLevel
{
    /// <summary>The lowest level, includes all details.</summary>
    Trace = 0,

    /// <summary>Information to help with debugging issues.</summary>
    Debug = 1,

    /// <summary>The default level, all informational messages.</summary>
    Information = 2,

    /// <summary>Information which could be an issue but which does not stop execution.</summary>
    Warning = 3,

    /// <summary>Information which is definitely an issue and will stop execution.</summary>
    Error = 4,

    /// <summary>Unexpected issues which stop execution.</summary>
    Critical = 5,

    /// <summary>The log level indicating that no logging should occur.</summary>
    None = 6,
}
