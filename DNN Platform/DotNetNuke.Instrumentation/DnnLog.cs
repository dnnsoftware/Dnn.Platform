// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Instrumentation
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Web.Compilation;

    using DotNetNuke.Internal.SourceGenerators;
    using log4net.Config;

    /// <summary>Provides access to logging methods.  Obsolete, use <see cref="LoggerSource"/> instead.</summary>
    [DnnDeprecated(7, 0, 1, "Use LoggerSource.Instance", RemovalVersion = 11)]
    public static partial class DnnLog
    {
        private const string ConfigFile = "DotNetNuke.log4net.config";
        private static readonly DnnLogger Logger = DnnLogger.GetClassLogger(typeof(DnnLog));

        private static readonly object ConfigLock = new object();
        private static bool isConfigured;

        // use a single static logger to avoid the performance impact of type reflection on every call for logging
        private static StackFrame CallingFrame
        {
            get
            {
                StackFrame frame = null;
                StackFrame[] stack = new StackTrace().GetFrames();

                int frameDepth = 0;
                if (stack != null)
                {
                    Type reflectedType = stack[frameDepth].GetMethod().ReflectedType;
                    while (reflectedType == BuildManager.GetType("DotNetNuke.Services.Exceptions.Exceptions", false) || reflectedType == typeof(DnnLogger) || reflectedType == typeof(DnnLog))
                    {
                        frameDepth++;
                        reflectedType = stack[frameDepth].GetMethod().ReflectedType;
                    }

                    frame = stack[frameDepth];
                }

                return frame;
            }
        }

        private static Type CallingType
        {
            get
            {
                return CallingFrame.GetMethod().DeclaringType;
            }
        }

        /// <summary>  Standard method to use on method entry.</summary>
        public static void MethodEntry()
        {
            EnsureConfig();

            if (Logger.Logger.IsEnabledFor(DnnLogger.LevelTrace))
            {
                Logger.TraceFormat("Entering Method [{0}]", CallingFrame.GetMethod().Name);
            }
        }

        /// <summary>Standard method to use on method exit.</summary>
        /// <param name="returnObject">The return value of the method.</param>
        public static void MethodExit(object returnObject)
        {
            EnsureConfig();

            if (Logger.Logger.IsEnabledFor(DnnLogger.LevelTrace))
            {
                if (returnObject == null)
                {
                    returnObject = "NULL";
                }

                Logger.TraceFormat("Method [{0}] Returned [{1}]", CallingFrame.GetMethod().Name, returnObject);
            }
        }

        /// <summary>  Standard method to use on method exit.</summary>
        public static void MethodExit()
        {
            EnsureConfig();

            if (Logger.Logger.IsEnabledFor(DnnLogger.LevelTrace))
            {
                Logger.TraceFormat("Method [{0}] Returned", CallingFrame.GetMethod().Name);
            }
        }

        /// <summary>Logs a message at the Trace log level.</summary>
        /// <param name="message">The message to log.</param>
        public static void Trace(string message)
        {
            EnsureConfig();

            if (Logger.Logger.IsEnabledFor(DnnLogger.LevelTrace))
            {
                Logger.Trace(message);
            }
        }

        /// <summary>Log a message at the Trace log level.</summary>
        /// <param name="format">A <see href="https://docs.microsoft.com/en-us/dotnet/standard/base-types/composite-formatting">composite format string</see>.</param>
        /// <param name="args">An object array that contains zero or more objects to format.</param>
        public static void Trace(string format, params object[] args)
        {
            EnsureConfig();

            if (Logger.Logger.IsEnabledFor(DnnLogger.LevelTrace))
            {
                Logger.TraceFormat(format, args);
            }
        }

        /// <summary>Log a message at the Trace log level.</summary>
        /// <param name="provider">An object that supplies culture-specific formatting information.</param>
        /// <param name="format">A <see href="https://docs.microsoft.com/en-us/dotnet/standard/base-types/composite-formatting">composite format string</see>.</param>
        /// <param name="args">An object array that contains zero or more objects to format.</param>
        public static void Trace(IFormatProvider provider, string format, params object[] args)
        {
            EnsureConfig();

            if (Logger.Logger.IsEnabledFor(DnnLogger.LevelTrace))
            {
                Logger.TraceFormat(provider, format, args);
            }
        }

        /// <summary>Logs a message at the Debug log level.</summary>
        /// <param name="message">The message to log.</param>
        public static void Debug(object message)
        {
            EnsureConfig();

            if (Logger.Logger.IsEnabledFor(DnnLogger.LevelDebug))
            {
                Logger.Debug(message);
            }
        }

        /// <summary>Log a message at the Debug log level.</summary>
        /// <param name="format">A <see href="https://docs.microsoft.com/en-us/dotnet/standard/base-types/composite-formatting">composite format string</see>.</param>
        /// <param name="args">An object array that contains zero or more objects to format.</param>
        public static void Debug(string format, params object[] args)
        {
            EnsureConfig();

            if (Logger.Logger.IsEnabledFor(DnnLogger.LevelDebug))
            {
                if (args.Length == 0)
                {
                    Logger.Debug(format);
                }
                else
                {
                    Logger.DebugFormat(format, args);
                }
            }
        }

        /// <summary>Log a message at the Debug log level.</summary>
        /// <param name="provider">An object that supplies culture-specific formatting information.</param>
        /// <param name="format">A <see href="https://docs.microsoft.com/en-us/dotnet/standard/base-types/composite-formatting">composite format string</see>.</param>
        /// <param name="args">An object array that contains zero or more objects to format.</param>
        public static void Debug(IFormatProvider provider, string format, params object[] args)
        {
            EnsureConfig();

            if (Logger.Logger.IsEnabledFor(DnnLogger.LevelDebug))
            {
                Logger.DebugFormat(provider, format, args);
            }
        }

        /// <summary>Logs a message at the Info log level.</summary>
        /// <param name="message">The message to log.</param>
        public static void Info(object message)
        {
            EnsureConfig();
            if (Logger.Logger.IsEnabledFor(DnnLogger.LevelInfo))
            {
                Logger.Info(message);
            }
        }

        /// <summary>Log a message at the Info log level.</summary>
        /// <param name="provider">An object that supplies culture-specific formatting information.</param>
        /// <param name="format">A <see href="https://docs.microsoft.com/en-us/dotnet/standard/base-types/composite-formatting">composite format string</see>.</param>
        /// <param name="args">An object array that contains zero or more objects to format.</param>
        public static void Info(IFormatProvider provider, string format, params object[] args)
        {
            EnsureConfig();
            if (Logger.Logger.IsEnabledFor(DnnLogger.LevelInfo))
            {
                Logger.InfoFormat(provider, format, args);
            }
        }

        /// <summary>Log a message at the Info log level.</summary>
        /// <param name="format">A <see href="https://docs.microsoft.com/en-us/dotnet/standard/base-types/composite-formatting">composite format string</see>.</param>
        /// <param name="args">An object array that contains zero or more objects to format.</param>
        public static void Info(string format, params object[] args)
        {
            EnsureConfig();
            if (Logger.Logger.IsEnabledFor(DnnLogger.LevelInfo))
            {
                if (args.Length == 0)
                {
                    Logger.Info(format);
                }
                else
                {
                    Logger.InfoFormat(format, args);
                }
            }
        }

        /// <summary>Log a message and exception details at the Warn log level.</summary>
        /// <param name="message">An object to display as the message.</param>
        /// <param name="exception">An exception to include in the log.</param>
        public static void Warn(string message, Exception exception)
        {
            EnsureConfig();
            if (Logger.Logger.IsEnabledFor(DnnLogger.LevelWarn))
            {
                Logger.Warn(message, exception);
            }
        }

        /// <summary>Logs a message at the Warn log level.</summary>
        /// <param name="message">The message to log.</param>
        public static void Warn(object message)
        {
            EnsureConfig();
            if (Logger.Logger.IsEnabledFor(DnnLogger.LevelWarn))
            {
                Logger.Warn(message);
            }
        }

        /// <summary>Log a message at the Warn log level.</summary>
        /// <param name="provider">An object that supplies culture-specific formatting information.</param>
        /// <param name="format">A <see href="https://docs.microsoft.com/en-us/dotnet/standard/base-types/composite-formatting">composite format string</see>.</param>
        /// <param name="args">An object array that contains zero or more objects to format.</param>
        public static void Warn(IFormatProvider provider, string format, params object[] args)
        {
            EnsureConfig();
            if (Logger.Logger.IsEnabledFor(DnnLogger.LevelWarn))
            {
                Logger.WarnFormat(provider, format, args);
            }
        }

        /// <summary>Log a message at the Warn log level.</summary>
        /// <param name="format">A <see href="https://docs.microsoft.com/en-us/dotnet/standard/base-types/composite-formatting">composite format string</see>.</param>
        /// <param name="args">An object array that contains zero or more objects to format.</param>
        public static void Warn(string format, params object[] args)
        {
            EnsureConfig();
            if (Logger.Logger.IsEnabledFor(DnnLogger.LevelWarn))
            {
                if (args.Length == 0)
                {
                    Logger.Warn(format);
                }
                else
                {
                    Logger.WarnFormat(format, args);
                }
            }
        }

        /// <summary>Log a message and exception details at the Error log level.</summary>
        /// <param name="message">An object to display as the message.</param>
        /// <param name="exception">An exception to include in the log.</param>
        public static void Error(string message, Exception exception)
        {
            EnsureConfig();

            if (Logger.Logger.IsEnabledFor(DnnLogger.LevelError))
            {
                Logger.Error(message, exception);
            }
        }

        /// <summary>Logs a message at the Error log level.</summary>
        /// <param name="message">The message to log.</param>
        public static void Error(object message)
        {
            EnsureConfig();
            if (Logger.Logger.IsEnabledFor(DnnLogger.LevelError))
            {
                Logger.Error(message);
            }
        }

        /// <summary>Log exception details at the Error log level.</summary>
        /// <param name="exception">An exception to include in the log.</param>
        public static void Error(Exception exception)
        {
            EnsureConfig();
            if (Logger.Logger.IsEnabledFor(DnnLogger.LevelError))
            {
                Logger.Error(exception.Message, exception);
            }
        }

        /// <summary>Log a message at the Error log level.</summary>
        /// <param name="provider">An object that supplies culture-specific formatting information.</param>
        /// <param name="format">A <see href="https://docs.microsoft.com/en-us/dotnet/standard/base-types/composite-formatting">composite format string</see>.</param>
        /// <param name="args">An object array that contains zero or more objects to format.</param>
        public static void Error(IFormatProvider provider, string format, params object[] args)
        {
            EnsureConfig();
            if (Logger.Logger.IsEnabledFor(DnnLogger.LevelError))
            {
                Logger.ErrorFormat(provider, format, args);
            }
        }

        /// <summary>Log a message at the Error log level.</summary>
        /// <param name="format">A <see href="https://docs.microsoft.com/en-us/dotnet/standard/base-types/composite-formatting">composite format string</see>.</param>
        /// <param name="args">An object array that contains zero or more objects to format.</param>
        public static void Error(string format, params object[] args)
        {
            EnsureConfig();
            if (Logger.Logger.IsEnabledFor(DnnLogger.LevelError))
            {
                if (args.Length == 0)
                {
                    Logger.ErrorFormat(format);
                }
                else
                {
                    Logger.ErrorFormat(format, args);
                }
            }
        }

        /// <summary>Log a message and exception details at the Fatal log level.</summary>
        /// <param name="message">An object to display as the message.</param>
        /// <param name="exception">An exception to include in the log.</param>
        public static void Fatal(string message, Exception exception)
        {
            EnsureConfig();
            if (Logger.Logger.IsEnabledFor(DnnLogger.LevelFatal))
            {
                Logger.Fatal(message, exception);
            }
        }

        /// <summary>Logs a message at the Fatal log level.</summary>
        /// <param name="message">The message to log.</param>
        public static void Fatal(object message)
        {
            EnsureConfig();
            if (Logger.Logger.IsEnabledFor(DnnLogger.LevelFatal))
            {
                Logger.Fatal(message);
            }
        }

        /// <summary>Log a message at the Fatal log level.</summary>
        /// <param name="provider">An object that supplies culture-specific formatting information.</param>
        /// <param name="format">A <see href="https://docs.microsoft.com/en-us/dotnet/standard/base-types/composite-formatting">composite format string</see>.</param>
        /// <param name="args">An object array that contains zero or more objects to format.</param>
        public static void Fatal(IFormatProvider provider, string format, params object[] args)
        {
            EnsureConfig();
            if (Logger.Logger.IsEnabledFor(DnnLogger.LevelFatal))
            {
                Logger.FatalFormat(provider, format, args);
            }
        }

        /// <summary>Log a message at the Fatal log level.</summary>
        /// <param name="format">A <see href="https://docs.microsoft.com/en-us/dotnet/standard/base-types/composite-formatting">composite format string</see>.</param>
        /// <param name="args">An object array that contains zero or more objects to format.</param>
        public static void Fatal(string format, params object[] args)
        {
            EnsureConfig();
            if (Logger.Logger.IsEnabledFor(DnnLogger.LevelFatal))
            {
                if (args.Length == 0)
                {
                    Logger.Fatal(format);
                }
                else
                {
                    Logger.FatalFormat(format, args);
                }
            }
        }

        private static void EnsureConfig()
        {
            if (!isConfigured)
            {
                lock (ConfigLock)
                {
                    if (!isConfigured)
                    {
                        var configPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ConfigFile);
                        var originalPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Config\\" + ConfigFile);
                        if (!File.Exists(configPath) && File.Exists(originalPath))
                        {
                            File.Copy(originalPath, configPath);
                        }

                        if (File.Exists(configPath))
                        {
                            XmlConfigurator.ConfigureAndWatch(new FileInfo(configPath));
                        }

                        isConfigured = true;
                    }
                }
            }
        }
    }
}
