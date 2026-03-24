// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Instrumentation
{
    using System;

    using Microsoft.Extensions.Logging;

    /// <summary>Provides extension methods to ease transition from <see cref="ILog"/> to <see cref="ILogger"/>.</summary>
    public static class LoggingMigrationExtensions
    {
        extension(ILogger logger)
        {
#pragma warning disable CA1848 // Use the LoggerMessage delegates
#pragma warning disable CA2254 // Template should be a static expression
#pragma warning disable SA1101 // Prefix local calls with this
            /// <inheritdoc cref="ILog.IsDebugEnabled" />
            public bool IsDebugEnabled => logger.IsEnabled(LogLevel.Debug);

            /// <inheritdoc cref="ILog.IsInfoEnabled" />
            public bool IsInfoEnabled => logger.IsEnabled(LogLevel.Information);

            /// <inheritdoc cref="ILog.IsTraceEnabled" />
            public bool IsTraceEnabled => logger.IsEnabled(LogLevel.Trace);

            /// <inheritdoc cref="ILog.IsWarnEnabled" />
            public bool IsWarnEnabled => logger.IsEnabled(LogLevel.Warning);

            /// <inheritdoc cref="ILog.IsErrorEnabled" />
            public bool IsErrorEnabled => logger.IsEnabled(LogLevel.Error);

            /// <inheritdoc cref="ILog.IsFatalEnabled" />
            public bool IsFatalEnabled => logger.IsEnabled(LogLevel.Critical);

            /// <inheritdoc cref="ILog.Debug(object)" />
            public void Debug(object message) => logger.Debug(message?.ToString());

            /// <inheritdoc cref="ILog.Debug(object)" />
            public void Debug(string message) => logger.LogDebug(message);

            /// <inheritdoc cref="ILog.Debug(object, Exception)" />
            public void Debug(object message, Exception exception) => logger.Debug(message?.ToString(), exception);

            /// <inheritdoc cref="ILog.Debug(object, Exception)" />
            public void Debug(string message, Exception exception) => logger.LogDebug(exception, message);

            /// <inheritdoc cref="ILog.DebugFormat(string, object[])" />
            public void DebugFormat(string format, params object[] args) => logger.LogDebug(format, args);

            /// <inheritdoc cref="ILog.DebugFormat(IFormatProvider, string, object[])" />
            public void DebugFormat(IFormatProvider provider, string format, params object[] args) => logger.Debug(string.Format(provider, format, args));

            /// <inheritdoc cref="ILog.Info(object)" />
            public void Info(object message) => logger.Info(message?.ToString());

            /// <inheritdoc cref="ILog.Info(object)" />
            public void Info(string message) => logger.LogInformation(message);

            /// <inheritdoc cref="ILog.Info(object, Exception)" />
            public void Info(object message, Exception exception) => logger.Info(message?.ToString(), exception);

            /// <inheritdoc cref="ILog.Info(object, Exception)" />
            public void Info(string message, Exception exception) => logger.LogInformation(exception, message);

            /// <inheritdoc cref="ILog.InfoFormat(string, object[])" />
            public void InfoFormat(string format, params object[] args) => logger.LogInformation(format, args);

            /// <inheritdoc cref="ILog.InfoFormat(IFormatProvider, string, object[])" />
            public void InfoFormat(IFormatProvider provider, string format, params object[] args) => logger.Info(string.Format(provider, format, args));

            /// <inheritdoc cref="ILog.Trace(object)" />
            public void Trace(object message) => logger.Trace(message?.ToString());

            /// <inheritdoc cref="ILog.Trace(object)" />
            public void Trace(string message) => logger.LogTrace(message);

            /// <inheritdoc cref="ILog.Trace(object, Exception)" />
            public void Trace(object message, Exception exception) => logger.Trace(message?.ToString(), exception);

            /// <inheritdoc cref="ILog.Trace(object, Exception)" />
            public void Trace(string message, Exception exception) => logger.LogTrace(exception, message);

            /// <inheritdoc cref="ILog.TraceFormat(string, object[])" />
            public void TraceFormat(string format, params object[] args) => logger.LogTrace(format, args);

            /// <inheritdoc cref="ILog.TraceFormat(IFormatProvider, string, object[])" />
            public void TraceFormat(IFormatProvider provider, string format, params object[] args) => logger.Trace(string.Format(provider, format, args));

            /// <inheritdoc cref="ILog.Warn(object)" />
            public void Warn(object message) => logger.Warn(message?.ToString());

            /// <inheritdoc cref="ILog.Warn(object)" />
            public void Warn(string message) => logger.LogWarning(message);

            /// <inheritdoc cref="ILog.Warn(object, Exception)" />
            public void Warn(object message, Exception exception) => logger.Warn(message?.ToString(), exception);

            /// <inheritdoc cref="ILog.Warn(object, Exception)" />
            public void Warn(string message, Exception exception) => logger.LogWarning(exception, message);

            /// <inheritdoc cref="ILog.WarnFormat(string, object[])" />
            public void WarnFormat(string format, params object[] args) => logger.LogWarning(format, args);

            /// <inheritdoc cref="ILog.WarnFormat(IFormatProvider, string, object[])" />
            public void WarnFormat(IFormatProvider provider, string format, params object[] args) => logger.Warn(string.Format(provider, format, args));

            /// <inheritdoc cref="ILog.Error(object)" />
            public void Error(object message) => logger.Error(message?.ToString());

            /// <inheritdoc cref="ILog.Error(object)" />
            public void Error(string message) => logger.LogError(message);

            /// <inheritdoc cref="ILog.Error(object, Exception)" />
            public void Error(object message, Exception exception) => logger.Error(message?.ToString(), exception);

            /// <inheritdoc cref="ILog.Error(object, Exception)" />
            public void Error(string message, Exception exception) => logger.LogError(exception, message);

            /// <inheritdoc cref="ILog.ErrorFormat(string, object[])" />
            public void ErrorFormat(string format, params object[] args) => logger.LogError(format, args);

            /// <inheritdoc cref="ILog.ErrorFormat(IFormatProvider, string, object[])" />
            public void ErrorFormat(IFormatProvider provider, string format, params object[] args) => logger.Error(string.Format(provider, format, args));

            /// <inheritdoc cref="ILog.Fatal(object)" />
            public void Fatal(object message) => logger.Fatal(message?.ToString());

            /// <inheritdoc cref="ILog.Fatal(object)" />
            public void Fatal(string message) => logger.LogCritical(message);

            /// <inheritdoc cref="ILog.Fatal(object, Exception)" />
            public void Fatal(object message, Exception exception) => logger.Fatal(message?.ToString(), exception);

            /// <inheritdoc cref="ILog.Fatal(object, Exception)" />
            public void Fatal(string message, Exception exception) => logger.LogCritical(exception, message);

            /// <inheritdoc cref="ILog.FatalFormat(string, object[])" />
            public void FatalFormat(string format, params object[] args) => logger.LogCritical(format, args);

            /// <inheritdoc cref="ILog.FatalFormat(IFormatProvider, string, object[])" />
            public void FatalFormat(IFormatProvider provider, string format, params object[] args) => logger.Fatal(string.Format(provider, format, args));
        }
    }
#pragma warning restore SA1101 // Prefix local calls with this
#pragma warning restore CA2254 // Template should be a static expression
#pragma warning restore CA1848 // Use the LoggerMessage delegates
}
