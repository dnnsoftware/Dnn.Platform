// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Instrumentation;

using System;
using System.Collections.Generic;

using Microsoft.Extensions.Logging;

/// <summary>
/// Provides a context for adding properties to the current log scope.
/// </summary>
public partial class LogRequestContext : IDisposable
{
    private readonly ILogger<LogRequestContext> logger;
    private Dictionary<string, IDisposable> logContext = new Dictionary<string, IDisposable>();

    /// <summary>
    /// Initializes a new instance of the <see cref="LogRequestContext"/> class.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    public LogRequestContext(ILogger<LogRequestContext> logger)
    {
        this.logger = logger;
    }

    /// <summary>
    /// Adds a property to the current log context.
    /// </summary>
    /// <param name="key">The property key.</param>
    /// <param name="value">The property value.</param>
    public void AddToLogContext(string key, object value)
    {
        if (this.logContext.TryGetValue(key, out var value1))
        {
            value1.Dispose();
        }

        this.logContext[key] = Serilog.Context.LogContext.PushProperty(key, value);
        LogAddingProperty(this.logger, key, value);
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        foreach (var context in this.logContext.Values)
        {
            context.Dispose();
        }

        this.logContext.Clear();
        GC.SuppressFinalize(this);
    }

    [LoggerMessage(Level = LogLevel.Warning, Message = "Adding property {Key} with value {Value} to log context")]
    private static partial void LogAddingProperty(ILogger logger, string key, object value);
}
