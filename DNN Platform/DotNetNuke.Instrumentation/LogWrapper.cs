// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Instrumentation;

using System;

using DotNetNuke.Internal.SourceGenerators;

using Microsoft.Extensions.Logging;

/// <summary>A wrapper to convert an <see cref="ILog"/> instance to an <see cref="ILogger"/> instance.</summary>
/// <param name="log">An <see cref="ILog"/> implementation.</param>
[DnnDeprecated(10, 4, 0, "Use Microsoft.Extensions.Logging.ILogger<T>")]
public partial class LogWrapper(ILog log) : ILogger
{
    private static readonly IDisposable ScopeInstance = new Scope();
    private readonly ILog log = log;

    /// <inheritdoc />
    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
    {
        if (formatter == null)
        {
            throw new ArgumentNullException(nameof(formatter));
        }

        switch (logLevel)
        {
            case LogLevel.Trace:
                LogWithLevel(this.log.Trace, eventId, state, exception, formatter);
                return;
            case LogLevel.Debug:
                LogWithLevel(this.log.Debug, eventId, state, exception, formatter);
                return;
            case LogLevel.Information:
                LogWithLevel(this.log.Info, eventId, state, exception, formatter);
                return;
            case LogLevel.Warning:
                LogWithLevel(this.log.Warn, eventId, state, exception, formatter);
                return;
            case LogLevel.Error:
                LogWithLevel(this.log.Error, eventId, state, exception, formatter);
                return;
            case LogLevel.Critical:
                LogWithLevel(this.log.Fatal, eventId, state, exception, formatter);
                return;
            case LogLevel.None:
                return;
            default:
                throw new ArgumentOutOfRangeException(nameof(logLevel), logLevel, FormattableString.Invariant($"Unexpected log level: {logLevel}"));
        }

        static void LogWithLevel(
            Action<object, Exception> logException,
            EventId eventId,
            TState state,
            Exception exception,
            Func<TState, Exception, string> formatter)
        {
            logException($"[{eventId}] {formatter(state, exception)}", exception);
        }
    }

    /// <inheritdoc />
    public bool IsEnabled(LogLevel logLevel)
    {
        return logLevel switch
        {
            LogLevel.Trace => this.log.IsTraceEnabled,
            LogLevel.Debug => this.log.IsDebugEnabled,
            LogLevel.Information => this.log.IsInfoEnabled,
            LogLevel.Warning => this.log.IsWarnEnabled,
            LogLevel.Error => this.log.IsErrorEnabled,
            LogLevel.Critical => this.log.IsFatalEnabled,
            _ => false,
        };
    }

    /// <inheritdoc />
    public IDisposable BeginScope<TState>(TState state)
        where TState : notnull
    {
        return ScopeInstance;
    }

    private sealed class Scope : IDisposable
    {
        /// <inheritdoc />
        public void Dispose()
        {
        }
    }
}
