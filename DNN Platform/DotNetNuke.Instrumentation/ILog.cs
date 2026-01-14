// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Instrumentation
{
    using System;
    using System.Diagnostics.CodeAnalysis;

    /// <summary>A contract specifying the ability to log information.</summary>
    public interface ILog
    {
        /// <summary>Gets a value indicating whether the Debug log level is enabled.</summary>
        bool IsDebugEnabled { get; }

        /// <summary>Gets a value indicating whether the Info log level is enabled.</summary>
        bool IsInfoEnabled { get; }

        /// <summary>Gets a value indicating whether the Trace log level is enabled.</summary>
        bool IsTraceEnabled { get; }

        /// <summary>Gets a value indicating whether the Warn log level is enabled.</summary>
        bool IsWarnEnabled { get; }

        /// <summary>Gets a value indicating whether the Error log level is enabled.</summary>
        bool IsErrorEnabled { get; }

        /// <summary>Gets a value indicating whether the Fatal log level is enabled.</summary>
        bool IsFatalEnabled { get; }

        /// <summary>Log a message at the Debug log level.</summary>
        /// <param name="message">An object to display as the message.</param>
        void Debug(object message);

        /// <summary>Log a message and exception details at the Debug log level.</summary>
        /// <param name="message">An object to display as the message.</param>
        /// <param name="exception">An exception to include in the log.</param>
        void Debug(object message, Exception exception);

        /// <summary>Log a message at the Debug log level.</summary>
        /// <param name="format">A <see href="https://docs.microsoft.com/en-us/dotnet/standard/base-types/composite-formatting">composite format string</see>.</param>
        /// <param name="args">An object array that contains zero or more objects to format.</param>
        void DebugFormat(string format, params object[] args);

        /// <summary>Log a message at the Debug log level.</summary>
        /// <param name="provider">An object that supplies culture-specific formatting information.</param>
        /// <param name="format">A <see href="https://docs.microsoft.com/en-us/dotnet/standard/base-types/composite-formatting">composite format string</see>.</param>
        /// <param name="args">An object array that contains zero or more objects to format.</param>
        void DebugFormat(IFormatProvider provider, string format, params object[] args);

        /// <summary>Log a message at the Info log level.</summary>
        /// <param name="message">An object to display as the message.</param>
        void Info(object message);

        /// <summary>Log a message and exception details at the Info log level.</summary>
        /// <param name="message">An object to display as the message.</param>
        /// <param name="exception">An exception to include in the log.</param>
        void Info(object message, Exception exception);

        /// <summary>Log a message at the Info log level.</summary>
        /// <param name="format">A <see href="https://docs.microsoft.com/en-us/dotnet/standard/base-types/composite-formatting">composite format string</see>.</param>
        /// <param name="args">An object array that contains zero or more objects to format.</param>
        void InfoFormat(string format, params object[] args);

        /// <summary>Log a message at the Info log level.</summary>
        /// <param name="provider">An object that supplies culture-specific formatting information.</param>
        /// <param name="format">A <see href="https://docs.microsoft.com/en-us/dotnet/standard/base-types/composite-formatting">composite format string</see>.</param>
        /// <param name="args">An object array that contains zero or more objects to format.</param>
        void InfoFormat(IFormatProvider provider, string format, params object[] args);

        /// <summary>Log a message at the Trace log level.</summary>
        /// <param name="message">An object to display as the message.</param>
        void Trace(object message);

        /// <summary>Log a message and exception details at the Trace log level.</summary>
        /// <param name="message">An object to display as the message.</param>
        /// <param name="exception">An exception to include in the log.</param>
        void Trace(object message, Exception exception);

        /// <summary>Log a message at the Trace log level.</summary>
        /// <param name="format">A <see href="https://docs.microsoft.com/en-us/dotnet/standard/base-types/composite-formatting">composite format string</see>.</param>
        /// <param name="args">An object array that contains zero or more objects to format.</param>
        void TraceFormat(string format, params object[] args);

        /// <summary>Log a message at the Trace log level.</summary>
        /// <param name="provider">An object that supplies culture-specific formatting information.</param>
        /// <param name="format">A <see href="https://docs.microsoft.com/en-us/dotnet/standard/base-types/composite-formatting">composite format string</see>.</param>
        /// <param name="args">An object array that contains zero or more objects to format.</param>
        void TraceFormat(IFormatProvider provider, string format, params object[] args);

        /// <summary>Log a message at the Warn log level.</summary>
        /// <param name="message">An object to display as the message.</param>
        void Warn(object message);

        /// <summary>Log a message and exception details at the Warn log level.</summary>
        /// <param name="message">An object to display as the message.</param>
        /// <param name="exception">An exception to include in the log.</param>
        void Warn(object message, Exception exception);

        /// <summary>Log a message at the Warn log level.</summary>
        /// <param name="format">A <see href="https://docs.microsoft.com/en-us/dotnet/standard/base-types/composite-formatting">composite format string</see>.</param>
        /// <param name="args">An object array that contains zero or more objects to format.</param>
        void WarnFormat(string format, params object[] args);

        /// <summary>Log a message at the Warn log level.</summary>
        /// <param name="provider">An object that supplies culture-specific formatting information.</param>
        /// <param name="format">A <see href="https://docs.microsoft.com/en-us/dotnet/standard/base-types/composite-formatting">composite format string</see>.</param>
        /// <param name="args">An object array that contains zero or more objects to format.</param>
        void WarnFormat(IFormatProvider provider, string format, params object[] args);

        /// <summary>Log a message at the Error log level.</summary>
        /// <param name="message">An object to display as the message.</param>
        [SuppressMessage("Microsoft.Naming", "CA1716:IdentifiersShouldNotMatchKeywords", Justification = "Breaking change")]
        void Error(object message);

        /// <summary>Log a message and exception details at the Error log level.</summary>
        /// <param name="message">An object to display as the message.</param>
        /// <param name="exception">An exception to include in the log.</param>
        [SuppressMessage("Microsoft.Naming", "CA1716:IdentifiersShouldNotMatchKeywords", Justification = "Breaking change")]
        void Error(object message, Exception exception);

        /// <summary>Log a message at the Error log level.</summary>
        /// <param name="format">A <see href="https://docs.microsoft.com/en-us/dotnet/standard/base-types/composite-formatting">composite format string</see>.</param>
        /// <param name="args">An object array that contains zero or more objects to format.</param>
        void ErrorFormat(string format, params object[] args);

        /// <summary>Log a message at the Error log level.</summary>
        /// <param name="provider">An object that supplies culture-specific formatting information.</param>
        /// <param name="format">A <see href="https://docs.microsoft.com/en-us/dotnet/standard/base-types/composite-formatting">composite format string</see>.</param>
        /// <param name="args">An object array that contains zero or more objects to format.</param>
        void ErrorFormat(IFormatProvider provider, string format, params object[] args);

        /// <summary>Log a message at the Fatal log level.</summary>
        /// <param name="message">An object to display as the message.</param>
        void Fatal(object message);

        /// <summary>Log a message and exception details at the Fatal log level.</summary>
        /// <param name="message">An object to display as the message.</param>
        /// <param name="exception">An exception to include in the log.</param>
        void Fatal(object message, Exception exception);

        /// <summary>Log a message at the Fatal log level.</summary>
        /// <param name="format">A <see href="https://docs.microsoft.com/en-us/dotnet/standard/base-types/composite-formatting">composite format string</see>.</param>
        /// <param name="args">An object array that contains zero or more objects to format.</param>
        void FatalFormat(string format, params object[] args);

        /// <summary>Log a message at the Fatal log level.</summary>
        /// <param name="provider">An object that supplies culture-specific formatting information.</param>
        /// <param name="format">A <see href="https://docs.microsoft.com/en-us/dotnet/standard/base-types/composite-formatting">composite format string</see>.</param>
        /// <param name="args">An object array that contains zero or more objects to format.</param>
        void FatalFormat(IFormatProvider provider, string format, params object[] args);
    }
}
