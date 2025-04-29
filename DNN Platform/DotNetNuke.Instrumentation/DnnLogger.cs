// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Instrumentation;

using System;
using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using System.Web.Compilation;
using System.Web.UI;

using DotNetNuke.Internal.SourceGenerators;
using log4net;
using log4net.Core;
using log4net.Repository;
using log4net.Util;

/// <summary>Obsolete, use <see cref="LoggerSource"/> instead.</summary>
[DnnDeprecated(9, 13, 7, "Use LoggerSource.Instance", RemovalVersion = 11)]
public sealed partial class DnnLogger : LoggerWrapperImpl
{
    // add custom logging levels (below trace value of 20000)
    private static Level levelLogInfo = new Level(10001, "LogInfo");
    private static Level levelLogError = new Level(10002, "LogError");

    private readonly Type dnnExceptionType = BuildManager.GetType("DotNetNuke.Services.Exceptions.Exceptions", false);
    private readonly Type stackBoundary = typeof(DnnLogger);

    private DnnLogger(ILogger logger)
        : base(logger)
    {
        StackFrame[] stack = new StackTrace().GetFrames();

        if (stack != null)
        {
            int frameDepth = 0;
            Type methodType = stack[frameDepth].GetMethod().ReflectedType;
#pragma warning disable 612, 618
            while (methodType == this.dnnExceptionType || methodType == typeof(DnnLogger) || methodType == typeof(DnnLog) || methodType == typeof(Control))
#pragma warning restore 612, 618
            {
                frameDepth++;
                methodType = stack[frameDepth].GetMethod().ReflectedType;
            }

            this.stackBoundary = new StackTrace().GetFrame(frameDepth - 1).GetMethod().DeclaringType;
        }
        else
        {
            this.stackBoundary = typeof(DnnLogger);
        }

        ReloadLevels(logger.Repository);
    }

    /// <summary>Gets the trace log level.</summary>
    internal static Level LevelTrace { get; private set; }

    /// <summary>Gets the debug log level.</summary>
    internal static Level LevelDebug { get; private set; }

    /// <summary>Gets the info log level.</summary>
    internal static Level LevelInfo { get; private set; }

    /// <summary>Gets the warn log level.</summary>
    internal static Level LevelWarn { get; private set; }

    /// <summary>Gets the error log level.</summary>
    internal static Level LevelError { get; private set; }

    /// <summary>Gets the fatal log level.</summary>
    internal static Level LevelFatal { get; private set; }

    /// <summary>Gets the logger for the given <paramref name="type"/>.</summary>
    /// <param name="type">The type for which to get a logger instance.</param>
    /// <returns>A new <see cref="DnnLogger"/> instance.</returns>
    public static DnnLogger GetClassLogger(Type type)
    {
        return new DnnLogger(LogManager.GetLogger(Assembly.GetCallingAssembly(), type).Logger);
    }

    /// <summary>Gets the logger for the given <paramref name="name"/>.</summary>
    /// <param name="name">The name for the logger instance.</param>
    /// <returns>A new <see cref="DnnLogger"/> instance.</returns>
    public static DnnLogger GetLogger(string name)
    {
        return new DnnLogger(LogManager.GetLogger(name).Logger);
    }

    /// <summary>  Logs a message object with the <c>DEBUG</c> level.</summary>
    /// <param name="message">The message object to log.</param>
    /// <remarks>
    ///   <para>
    ///     This method first checks if this logger is <c>DEBUG</c>
    ///     enabled by comparing the level of this logger with the
    ///     <c>DEBUG</c> level. If this logger is
    ///     <c>DEBUG</c> enabled, then it converts the message object
    ///     (passed as parameter) to a string by invoking the appropriate
    ///     <see cref = "log4net.ObjectRenderer.IObjectRenderer" />. It then
    ///     proceeds to call all the registered appenders in this logger
    ///     and also higher in the hierarchy depending on the value of the
    ///     additivity flag.
    ///   </para>
    ///   <para>
    ///     <b>WARNING</b> Note that passing an <see cref = "Exception" />
    ///     to this method will print the name of the <see cref = "Exception" />
    ///     but no stack trace. To print a stack trace use the
    ///     <see cref = "Error(object,Exception)" /> form instead.
    ///   </para>
    /// </remarks>
    public void Debug(object message)
    {
        this.Logger.Log(this.stackBoundary, LevelDebug, message, null);
    }

    /// <summary>  Logs a formatted message string with the <c>DEBUG</c> level.</summary>
    /// <param name="format">A String containing zero or more format items.</param>
    /// <param name="args">An Object array containing zero or more objects to format.</param>
    /// <remarks>
    ///   <para>
    ///     The message is formatted using the <see cref = "string.Format(IFormatProvider, string, object[])" /> method. See
    ///     <c>String.Format</c> for details of the syntax of the format string and the behavior
    ///     of the formatting.
    ///   </para>
    ///   <para>
    ///     The string is formatted using the <see cref = "CultureInfo.InvariantCulture" />
    ///     format provider. To specify a localized provider use the
    ///     <see cref = "DebugFormat(IFormatProvider,string,object[])" /> method.
    ///   </para>
    ///   <para>
    ///     This method does not take an <see cref = "Exception" /> object to include in the
    ///     log event. To pass an <see cref = "Exception" /> use one of the <see cref = "Debug(object)" />
    ///     methods instead.
    ///   </para>
    /// </remarks>
    public void DebugFormat(string format, params object[] args)
    {
        this.Logger.Log(this.stackBoundary, LevelDebug, new SystemStringFormat(CultureInfo.InvariantCulture, format, args), null);
    }

    /// <summary>  Logs a formatted message string with the <c>DEBUG</c> level.</summary>
    /// <param name="provider">An <see cref = "IFormatProvider" /> that supplies culture-specific formatting information.</param>
    /// <param name="format">A String containing zero or more format items.</param>
    /// <param name="args">An Object array containing zero or more objects to format.</param>
    /// <remarks>
    ///   <para>
    ///     The message is formatted using the <see cref = "string.Format(IFormatProvider, string, object[])" /> method. See
    ///     <c>String.Format</c> for details of the syntax of the format string and the behavior
    ///     of the formatting.
    ///   </para>
    ///   <para>
    ///     This method does not take an <see cref = "Exception" /> object to include in the
    ///     log event. To pass an <see cref = "Exception" /> use one of the <see cref = "Debug(object)" />
    ///     methods instead.
    ///   </para>
    /// </remarks>
    public void DebugFormat(IFormatProvider provider, string format, params object[] args)
    {
        this.Logger.Log(this.stackBoundary, LevelDebug, new SystemStringFormat(provider, format, args), null);
    }

    /// <summary>  Logs a message object with the <c>INFO</c> level.</summary>
    /// <param name="message">The message object to log.</param>
    /// <remarks>
    ///   <para>
    ///     This method first checks if this logger is <c>INFO</c>
    ///     enabled by comparing the level of this logger with the
    ///     <c>INFO</c> level. If this logger is
    ///     <c>INFO</c> enabled, then it converts the message object
    ///     (passed as parameter) to a string by invoking the appropriate
    ///     <see cref = "log4net.ObjectRenderer.IObjectRenderer" />. It then
    ///     proceeds to call all the registered appenders in this logger
    ///     and also higher in the hierarchy depending on the value of
    ///     the additivity flag.
    ///   </para>
    ///   <para>
    ///     <b>WARNING</b> Note that passing an <see cref = "Exception" />
    ///     to this method will print the name of the <see cref = "Exception" />
    ///     but no stack trace. To print a stack trace use the
    ///     <see cref = "Error(object,Exception)" /> form instead.
    ///   </para>
    /// </remarks>
    public void Info(object message)
    {
        this.Logger.Log(this.stackBoundary, LevelInfo, message, null);
    }

    /// <summary>  Logs a formatted message string with the <c>INFO</c> level.</summary>
    /// <param name="format">A String containing zero or more format items.</param>
    /// <param name="args">An Object array containing zero or more objects to format.</param>
    /// <remarks>
    ///   <para>
    ///     The message is formatted using the <see cref = "string.Format(IFormatProvider, string, object[])" /> method. See
    ///     <c>String.Format</c> for details of the syntax of the format string and the behavior
    ///     of the formatting.
    ///   </para>
    ///   <para>
    ///     The string is formatted using the <see cref = "CultureInfo.InvariantCulture" />
    ///     format provider. To specify a localized provider use the
    ///     <see cref = "InfoFormat(IFormatProvider,string,object[])" /> method.
    ///   </para>
    ///   <para>
    ///     This method does not take an <see cref = "Exception" /> object to include in the
    ///     log event. To pass an <see cref = "Exception" /> use one of the <see cref = "Info(object)" />
    ///     methods instead.
    ///   </para>
    /// </remarks>
    public void InfoFormat(string format, params object[] args)
    {
        this.Logger.Log(this.stackBoundary, LevelInfo, new SystemStringFormat(CultureInfo.InvariantCulture, format, args), null);
    }

    /// <summary>  Logs a formatted message string with the <c>INFO</c> level.</summary>
    /// <param name="provider">An <see cref = "IFormatProvider" /> that supplies culture-specific formatting information.</param>
    /// <param name="format">A String containing zero or more format items.</param>
    /// <param name="args">An Object array containing zero or more objects to format.</param>
    /// <remarks>
    ///   <para>
    ///     The message is formatted using the <see cref = "string.Format(IFormatProvider, string, object[])" /> method. See
    ///     <c>String.Format</c> for details of the syntax of the format string and the behavior
    ///     of the formatting.
    ///   </para>
    ///   <para>
    ///     This method does not take an <see cref = "Exception" /> object to include in the
    ///     log event. To pass an <see cref = "Exception" /> use one of the <see cref = "Info(object)" />
    ///     methods instead.
    ///   </para>
    /// </remarks>
    public void InfoFormat(IFormatProvider provider, string format, params object[] args)
    {
        this.Logger.Log(this.stackBoundary, LevelInfo, new SystemStringFormat(provider, format, args), null);
    }

    /// <summary>  Logs a message object with the <c>WARN</c> level.</summary>
    /// <param name="message">the message object to log.</param>
    /// <remarks>
    ///   <para>
    ///     This method first checks if this logger is <c>WARN</c>
    ///     enabled by comparing the level of this logger with the
    ///     <c>WARN</c> level. If this logger is
    ///     <c>WARN</c> enabled, then it converts the message object
    ///     (passed as parameter) to a string by invoking the appropriate
    ///     <see cref = "log4net.ObjectRenderer.IObjectRenderer" />. It then
    ///     proceeds to call all the registered appenders in this logger and
    ///     also higher in the hierarchy depending on the value of the
    ///     additivity flag.
    ///   </para>
    ///   <para>
    ///     <b>WARNING</b> Note that passing an <see cref = "Exception" /> to this
    ///     method will print the name of the <see cref = "Exception" /> but no
    ///     stack trace. To print a stack trace use the
    ///     <see cref = "Warn(object,Exception)" /> form instead.
    ///   </para>
    /// </remarks>
    public void Warn(object message)
    {
        this.Logger.Log(this.stackBoundary, LevelWarn, message, null);
    }

    /// <summary>  Logs a message object with the <c>WARN</c> level.</summary>
    /// <param name="message">The message object to log.</param>
    /// <param name="exception">The exception to log, including its stack trace.</param>
    /// <remarks>
    ///   <para>
    ///     Logs a message object with the <c>WARN</c> level including
    ///     the stack trace of the <see cref = "Exception" /> <paramref name = "exception" />
    ///     passed as a parameter.
    ///   </para>
    ///   <para>
    ///     See the <see cref = "Warn(object)" /> form for more detailed information.
    ///   </para>
    /// </remarks>
    /// <seealso cref = "Warn(object)" />
    public void Warn(object message, Exception exception)
    {
        this.Logger.Log(this.stackBoundary, LevelWarn, message, exception);
    }

    /// <summary>  Logs a formatted message string with the <c>WARN</c> level.</summary>
    /// <param name="format">A String containing zero or more format items.</param>
    /// <param name="args">An Object array containing zero or more objects to format.</param>
    /// <remarks>
    ///   <para>
    ///     The message is formatted using the <see cref = "string.Format(IFormatProvider, string, object[])" /> method. See
    ///     <c>String.Format</c> for details of the syntax of the format string and the behavior
    ///     of the formatting.
    ///   </para>
    ///   <para>
    ///     The string is formatted using the <see cref = "CultureInfo.InvariantCulture" />
    ///     format provider. To specify a localized provider use the
    ///     <see cref = "WarnFormat(IFormatProvider,string,object[])" /> method.
    ///   </para>
    ///   <para>
    ///     This method does not take an <see cref = "Exception" /> object to include in the
    ///     log event. To pass an <see cref = "Exception" /> use one of the <see cref = "Warn(object)" />
    ///     methods instead.
    ///   </para>
    /// </remarks>
    public void WarnFormat(string format, params object[] args)
    {
        this.Logger.Log(this.stackBoundary, LevelWarn, new SystemStringFormat(CultureInfo.InvariantCulture, format, args), null);
    }

    /// <summary>  Logs a formatted message string with the <c>WARN</c> level.</summary>
    /// <param name="provider">An <see cref = "IFormatProvider" /> that supplies culture-specific formatting information.</param>
    /// <param name="format">A String containing zero or more format items.</param>
    /// <param name="args">An Object array containing zero or more objects to format.</param>
    /// <remarks>
    ///   <para>
    ///     The message is formatted using the <see cref = "string.Format(IFormatProvider, string, object[])" /> method. See
    ///     <c>String.Format</c> for details of the syntax of the format string and the behavior
    ///     of the formatting.
    ///   </para>
    ///   <para>
    ///     This method does not take an <see cref = "Exception" /> object to include in the
    ///     log event. To pass an <see cref = "Exception" /> use one of the <see cref = "Warn(object)" />
    ///     methods instead.
    ///   </para>
    /// </remarks>
    public void WarnFormat(IFormatProvider provider, string format, params object[] args)
    {
        this.Logger.Log(this.stackBoundary, LevelWarn, new SystemStringFormat(provider, format, args), null);
    }

    /// <summary>  Logs a message object with the <c>ERROR</c> level.</summary>
    /// <param name="message">The message object to log.</param>
    /// <remarks>
    ///   <para>
    ///     This method first checks if this logger is <c>ERROR</c>
    ///     enabled by comparing the level of this logger with the
    ///     <c>ERROR</c> level. If this logger is
    ///     <c>ERROR</c> enabled, then it converts the message object
    ///     (passed as parameter) to a string by invoking the appropriate
    ///     <see cref = "log4net.ObjectRenderer.IObjectRenderer" />. It then
    ///     proceeds to call all the registered appenders in this logger and
    ///     also higher in the hierarchy depending on the value of the
    ///     additivity flag.
    ///   </para>
    ///   <para>
    ///     <b>WARNING</b> Note that passing an <see cref = "Exception" /> to this
    ///     method will print the name of the <see cref = "Exception" /> but no
    ///     stack trace. To print a stack trace use the
    ///     <see cref = "Error(object,Exception)" /> form instead.
    ///   </para>
    /// </remarks>
    public void Error(object message)
    {
        this.Logger.Log(this.stackBoundary, LevelError, message, null);
    }

    /// <summary>  Logs a message object with the <c>ERROR</c> level.</summary>
    /// <param name="message">The message object to log.</param>
    /// <param name="exception">The exception to log, including its stack trace.</param>
    /// <remarks>
    ///   <para>
    ///     Logs a message object with the <c>ERROR</c> level including
    ///     the stack trace of the <see cref = "Exception" /> <paramref name = "exception" />
    ///     passed as a parameter.
    ///   </para>
    ///   <para>
    ///     See the <see cref = "Error(object)" /> form for more detailed information.
    ///   </para>
    /// </remarks>
    /// <seealso cref = "Error(object)" />
    public void Error(object message, Exception exception)
    {
        this.Logger.Log(this.stackBoundary, LevelError, message, exception);
    }

    /// <summary>  Logs a formatted message string with the <c>ERROR</c> level.</summary>
    /// <param name="format">A String containing zero or more format items.</param>
    /// <param name="args">An Object array containing zero or more objects to format.</param>
    /// <remarks>
    ///   <para>
    ///     The message is formatted using the <see cref = "string.Format(IFormatProvider, string, object[])" /> method. See
    ///     <c>String.Format</c> for details of the syntax of the format string and the behavior
    ///     of the formatting.
    ///   </para>
    ///   <para>
    ///     The string is formatted using the <see cref = "CultureInfo.InvariantCulture" />
    ///     format provider. To specify a localized provider use the
    ///     <see cref = "ErrorFormat(IFormatProvider,string,object[])" /> method.
    ///   </para>
    ///   <para>
    ///     This method does not take an <see cref = "Exception" /> object to include in the
    ///     log event. To pass an <see cref = "Exception" /> use one of the <see cref = "Error(object)" />
    ///     methods instead.
    ///   </para>
    /// </remarks>
    public void ErrorFormat(string format, params object[] args)
    {
        this.Logger.Log(this.stackBoundary, LevelError, new SystemStringFormat(CultureInfo.InvariantCulture, format, args), null);
    }

    /// <summary>  Logs a formatted message string with the <c>ERROR</c> level.</summary>
    /// <param name="provider">An <see cref = "IFormatProvider" /> that supplies culture-specific formatting information.</param>
    /// <param name="format">A String containing zero or more format items.</param>
    /// <param name="args">An Object array containing zero or more objects to format.</param>
    /// <remarks>
    ///   <para>
    ///     The message is formatted using the <see cref = "string.Format(IFormatProvider, string, object[])" /> method. See
    ///     <c>String.Format</c> for details of the syntax of the format string and the behavior
    ///     of the formatting.
    ///   </para>
    ///   <para>
    ///     This method does not take an <see cref = "Exception" /> object to include in the
    ///     log event. To pass an <see cref = "Exception" /> use one of the <see cref = "Error(object)" />
    ///     methods instead.
    ///   </para>
    /// </remarks>
    public void ErrorFormat(IFormatProvider provider, string format, params object[] args)
    {
        this.Logger.Log(this.stackBoundary, LevelError, new SystemStringFormat(provider, format, args), null);
    }

    /// <summary>  Logs a message object with the <c>FATAL</c> level.</summary>
    /// <param name="message">The message object to log.</param>
    /// <remarks>
    ///   <para>
    ///     This method first checks if this logger is <c>FATAL</c>
    ///     enabled by comparing the level of this logger with the
    ///     <c>FATAL</c> level. If this logger is
    ///     <c>FATAL</c> enabled, then it converts the message object
    ///     (passed as parameter) to a string by invoking the appropriate
    ///     <see cref = "log4net.ObjectRenderer.IObjectRenderer" />. It then
    ///     proceeds to call all the registered appenders in this logger and
    ///     also higher in the hierarchy depending on the value of the
    ///     additivity flag.
    ///   </para>
    ///   <para>
    ///     <b>WARNING</b> Note that passing an <see cref = "Exception" /> to this
    ///     method will print the name of the <see cref = "Exception" /> but no
    ///     stack trace. To print a stack trace use the
    ///     <see cref = "Fatal(object,Exception)" /> form instead.
    ///   </para>
    /// </remarks>
    public void Fatal(object message)
    {
        this.Logger.Log(this.stackBoundary, LevelFatal, message, null);
    }

    /// <summary>  Logs a message object with the <c>FATAL</c> level.</summary>
    /// <param name="message">The message object to log.</param>
    /// <param name="exception">The exception to log, including its stack trace.</param>
    /// <remarks>
    ///   <para>
    ///     Logs a message object with the <c>FATAL</c> level including
    ///     the stack trace of the <see cref = "Exception" /> <paramref name = "exception" />
    ///     passed as a parameter.
    ///   </para>
    ///   <para>
    ///     See the <see cref = "Fatal(object)" /> form for more detailed information.
    ///   </para>
    /// </remarks>
    /// <seealso cref = "Fatal(object)" />
    public void Fatal(object message, Exception exception)
    {
        this.Logger.Log(this.stackBoundary, LevelFatal, message, exception);
    }

    /// <summary>  Logs a formatted message string with the <c>FATAL</c> level.</summary>
    /// <param name="format">A String containing zero or more format items.</param>
    /// <param name="args">An Object array containing zero or more objects to format.</param>
    /// <remarks>
    ///   <para>
    ///     The message is formatted using the <see cref = "string.Format(IFormatProvider, string, object[])" /> method. See
    ///     <c>String.Format</c> for details of the syntax of the format string and the behavior
    ///     of the formatting.
    ///   </para>
    ///   <para>
    ///     The string is formatted using the <see cref = "CultureInfo.InvariantCulture" />
    ///     format provider. To specify a localized provider use the
    ///     <see cref = "FatalFormat(IFormatProvider,string,object[])" /> method.
    ///   </para>
    ///   <para>
    ///     This method does not take an <see cref = "Exception" /> object to include in the
    ///     log event. To pass an <see cref = "Exception" /> use one of the <see cref = "Fatal(object)" />
    ///     methods instead.
    ///   </para>
    /// </remarks>
    public void FatalFormat(string format, params object[] args)
    {
        this.Logger.Log(this.stackBoundary, LevelFatal, new SystemStringFormat(CultureInfo.InvariantCulture, format, args), null);
    }

    /// <summary>  Logs a formatted message string with the <c>FATAL</c> level.</summary>
    /// <param name="provider">An <see cref = "IFormatProvider" /> that supplies culture-specific formatting information.</param>
    /// <param name="format">A String containing zero or more format items.</param>
    /// <param name="args">An Object array containing zero or more objects to format.</param>
    /// <remarks>
    ///   <para>
    ///     The message is formatted using the <see cref = "string.Format(IFormatProvider, string, object[])" /> method. See
    ///     <c>String.Format</c> for details of the syntax of the format string and the behavior
    ///     of the formatting.
    ///   </para>
    ///   <para>
    ///     This method does not take an <see cref = "Exception" /> object to include in the
    ///     log event. To pass an <see cref = "Exception" /> use one of the <see cref = "Fatal(object)" />
    ///     methods instead.
    ///   </para>
    /// </remarks>
    public void FatalFormat(IFormatProvider provider, string format, params object[] args)
    {
        this.Logger.Log(this.stackBoundary, LevelFatal, new SystemStringFormat(provider, format, args), null);
    }

    /// <summary>Logs an install message at the Error log level.</summary>
    /// <param name="message">The message to log.</param>
    public void InstallLogError(object message)
    {
        this.Logger.Log(this.stackBoundary, levelLogError, message, null);
    }

    /// <summary>Log an install message and exception details at the Error log level.</summary>
    /// <param name="message">An object to display as the message.</param>
    /// <param name="exception">An exception to include in the log.</param>
    public void InstallLogError(string message, Exception exception)
    {
        this.Logger.Log(this.stackBoundary, levelLogError, message, exception);
    }

    /// <summary>Log an install message at the Error log level.</summary>
    /// <param name="format">A <see href="https://docs.microsoft.com/en-us/dotnet/standard/base-types/composite-formatting">composite format string</see>.</param>
    /// <param name="args">An object array that contains zero or more objects to format.</param>
    public void InstallLogErrorFormat(string format, params object[] args)
    {
        this.Logger.Log(this.stackBoundary, levelLogError, new SystemStringFormat(CultureInfo.InvariantCulture, format, args), null);
    }

    /// <summary>Log an install message at the Error log level.</summary>
    /// <param name="provider">An object that supplies culture-specific formatting information.</param>
    /// <param name="format">A <see href="https://docs.microsoft.com/en-us/dotnet/standard/base-types/composite-formatting">composite format string</see>.</param>
    /// <param name="args">An object array that contains zero or more objects to format.</param>
    public void InstallLogErrorFormat(IFormatProvider provider, string format, params object[] args)
    {
        this.Logger.Log(this.stackBoundary, levelLogError, new SystemStringFormat(provider, format, args), null);
    }

    /// <summary>Logs an install message at the Info log level.</summary>
    /// <param name="message">The message to log.</param>
    public void InstallLogInfo(object message)
    {
        this.Logger.Log(this.stackBoundary, levelLogInfo, message, null);
    }

    /// <summary>Log an install message at the Info log level.</summary>
    /// <param name="format">A <see href="https://docs.microsoft.com/en-us/dotnet/standard/base-types/composite-formatting">composite format string</see>.</param>
    /// <param name="args">An object array that contains zero or more objects to format.</param>
    public void InstallLogInfoFormat(string format, params object[] args)
    {
        this.Logger.Log(this.stackBoundary, levelLogInfo, new SystemStringFormat(CultureInfo.InvariantCulture, format, args), null);
    }

    /// <summary>Log an install message at the Info log level.</summary>
    /// <param name="provider">An object that supplies culture-specific formatting information.</param>
    /// <param name="format">A <see href="https://docs.microsoft.com/en-us/dotnet/standard/base-types/composite-formatting">composite format string</see>.</param>
    /// <param name="args">An object array that contains zero or more objects to format.</param>
    public void InstallLogInfoFormat(IFormatProvider provider, string format, params object[] args)
    {
        this.Logger.Log(this.stackBoundary, levelLogInfo, new SystemStringFormat(provider, format, args), null);
    }

    /// <summary>Log a message at the Trace log level.</summary>
    /// <param name="format">A <see href="https://docs.microsoft.com/en-us/dotnet/standard/base-types/composite-formatting">composite format string</see>.</param>
    /// <param name="args">An object array that contains zero or more objects to format.</param>
    internal void TraceFormat(string format, params object[] args)
    {
        this.Logger.Log(this.stackBoundary, LevelTrace, new SystemStringFormat(CultureInfo.InvariantCulture, format, args), null);
    }

    /// <summary>Logs a message at the Trace log level.</summary>
    /// <param name="message">The message to log.</param>
    internal void Trace(string message)
    {
        this.Logger.Log(this.stackBoundary, LevelTrace, message, null);
    }

    /// <summary>Log a message at the Trace log level.</summary>
    /// <param name="provider">An object that supplies culture-specific formatting information.</param>
    /// <param name="format">A <see href="https://docs.microsoft.com/en-us/dotnet/standard/base-types/composite-formatting">composite format string</see>.</param>
    /// <param name="args">An object array that contains zero or more objects to format.</param>
    internal void TraceFormat(IFormatProvider provider, string format, params object[] args)
    {
        this.Logger.Log(this.stackBoundary, LevelTrace, new SystemStringFormat(provider, format, args), null);
    }

    /// <summary>  Virtual method called when the configuration of the repository changes.</summary>
    /// <param name="repository">the repository holding the levels.</param>
    private static void ReloadLevels(ILoggerRepository repository)
    {
        LevelMap levelMap = repository.LevelMap;

        LevelTrace = levelMap.LookupWithDefault(Level.Trace);
        LevelDebug = levelMap.LookupWithDefault(Level.Debug);
        LevelInfo = levelMap.LookupWithDefault(Level.Info);
        LevelWarn = levelMap.LookupWithDefault(Level.Warn);
        LevelError = levelMap.LookupWithDefault(Level.Error);
        LevelFatal = levelMap.LookupWithDefault(Level.Fatal);
        levelLogError = levelMap.LookupWithDefault(levelLogError);
        levelLogInfo = levelMap.LookupWithDefault(levelLogInfo);

        // Register custom logging levels with the default LoggerRepository
        LogManager.GetRepository().LevelMap.Add(levelLogInfo);
        LogManager.GetRepository().LevelMap.Add(levelLogError);
    }
}
