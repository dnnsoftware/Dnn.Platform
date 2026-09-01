// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.BulkInstall.DeployClient;

/// <summary>The level of logging.</summary>
public enum LogLevel
{
    /// <summary>The lowest level, includes all details.</summary>
    Trace = Microsoft.Extensions.Logging.LogLevel.Trace,

    /// <summary>Information to help with debugging issues.</summary>
    Debug = Microsoft.Extensions.Logging.LogLevel.Debug,

    /// <summary>The default level, all informational messages.</summary>
    Information = Microsoft.Extensions.Logging.LogLevel.Information,

    /// <summary>Information which could be an issue but which does not stop execution.</summary>
    Warning = Microsoft.Extensions.Logging.LogLevel.Warning,

    /// <summary>Information which is definitely an issue and will stop execution.</summary>
    Error = Microsoft.Extensions.Logging.LogLevel.Error,

    /// <summary>Unexpected issues which stop execution.</summary>
    Critical = Microsoft.Extensions.Logging.LogLevel.Critical,

    /// <summary>The log level indicating that no logging should occur.</summary>
    None = Microsoft.Extensions.Logging.LogLevel.None,
}
