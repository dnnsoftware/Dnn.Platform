// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Instrumentation
{
    using System;
    using System.Globalization;
    using System.IO;

    using DotNetNuke.Internal.SourceGenerators;
    using log4net;
    using log4net.Config;
    using log4net.Core;
    using log4net.Repository;
    using log4net.Util;

    /// <summary>An <see cref="ILoggerSource"/> implementation.</summary>
    public class LoggerSourceImpl : ILoggerSource
    {
        /// <inheritdoc/>
        public ILog GetLogger(Type type)
        {
            return new Logger(LogManager.GetLogger(type).Logger, type);
        }

        /// <inheritdoc/>
        public ILog GetLogger(string name)
        {
            return new Logger(LogManager.GetLogger(name).Logger, null);
        }

        private class Logger : LoggerWrapperImpl, ILog
        {
            private const string ConfigFile = "DotNetNuke.log4net.config";
            private static readonly object ConfigLock = new object();
            private static Level levelTrace;
            private static Level levelDebug;
            private static Level levelInfo;
            private static Level levelWarn;
            private static Level levelError;
            private static Level levelFatal;
            private static bool configured;

            // add custom logging levels (below trace value of 20000)
            //            internal static Level LevelLogInfo = new Level(10001, "LogInfo");
            //            internal static Level LevelLogError = new Level(10002, "LogError");
            private readonly Type stackBoundary = typeof(Logger);

            internal Logger(ILogger logger, Type type)
                : base(logger)
            {
                this.stackBoundary = type ?? typeof(Logger);
                EnsureConfig();
                ReloadLevels(logger.Repository);
            }

            public bool IsDebugEnabled
            {
                get { return this.Logger.IsEnabledFor(levelDebug); }
            }

            public bool IsInfoEnabled
            {
                get { return this.Logger.IsEnabledFor(levelInfo); }
            }

            public bool IsTraceEnabled
            {
                get { return this.Logger.IsEnabledFor(levelTrace); }
            }

            public bool IsWarnEnabled
            {
                get { return this.Logger.IsEnabledFor(levelWarn); }
            }

            public bool IsErrorEnabled
            {
                get { return this.Logger.IsEnabledFor(levelError); }
            }

            public bool IsFatalEnabled
            {
                get { return this.Logger.IsEnabledFor(levelFatal); }
            }

            public void Debug(object message)
            {
                this.Debug(message, null);
            }

            public void Debug(object message, Exception exception)
            {
                this.Logger.Log(this.stackBoundary, levelDebug, message, exception);
            }

            public void DebugFormat(string format, params object[] args)
            {
                this.DebugFormat(CultureInfo.InvariantCulture, format, args);
            }

            public void DebugFormat(IFormatProvider provider, string format, params object[] args)
            {
                this.Logger.Log(this.stackBoundary, levelDebug, new SystemStringFormat(provider, format, args), null);
            }

            public void Info(object message)
            {
                this.Info(message, null);
            }

            public void Info(object message, Exception exception)
            {
                this.Logger.Log(this.stackBoundary, levelInfo, message, exception);
            }

            public void InfoFormat(string format, params object[] args)
            {
                this.InfoFormat(CultureInfo.InvariantCulture, format, args);
            }

            public void InfoFormat(IFormatProvider provider, string format, params object[] args)
            {
                this.Logger.Log(this.stackBoundary, levelInfo, new SystemStringFormat(provider, format, args), null);
            }

            public void Trace(object message)
            {
                this.Trace(message, null);
            }

            public void Trace(object message, Exception exception)
            {
                this.Logger.Log(this.stackBoundary, levelTrace, message, exception);
            }

            public void TraceFormat(string format, params object[] args)
            {
                this.TraceFormat(CultureInfo.InvariantCulture, format, args);
            }

            public void TraceFormat(IFormatProvider provider, string format, params object[] args)
            {
                this.Logger.Log(this.stackBoundary, levelTrace, new SystemStringFormat(provider, format, args), null);
            }

            public void Warn(object message)
            {
                this.Warn(message, null);
            }

            public void Warn(object message, Exception exception)
            {
                this.Logger.Log(this.stackBoundary, levelWarn, message, exception);
            }

            public void WarnFormat(string format, params object[] args)
            {
                this.WarnFormat(CultureInfo.InvariantCulture, format, args);
            }

            public void WarnFormat(IFormatProvider provider, string format, params object[] args)
            {
                this.Logger.Log(this.stackBoundary, levelWarn, new SystemStringFormat(provider, format, args), null);
            }

            public void Error(object message)
            {
                this.Error(message, null);
            }

            public void Error(object message, Exception exception)
            {
                this.Logger.Log(this.stackBoundary, levelError, message, exception);
            }

            public void ErrorFormat(string format, params object[] args)
            {
                this.ErrorFormat(CultureInfo.InvariantCulture, format, args);
            }

            public void ErrorFormat(IFormatProvider provider, string format, params object[] args)
            {
                this.Logger.Log(this.stackBoundary, levelError, new SystemStringFormat(provider, format, args), null);
            }

            public void Fatal(object message)
            {
                this.Fatal(message, null);
            }

            public void Fatal(object message, Exception exception)
            {
                this.Logger.Log(this.stackBoundary, levelFatal, message, exception);
            }

            public void FatalFormat(string format, params object[] args)
            {
                this.FatalFormat(CultureInfo.InvariantCulture, format, args);
            }

            public void FatalFormat(IFormatProvider provider, string format, params object[] args)
            {
                this.Logger.Log(this.stackBoundary, levelFatal, new SystemStringFormat(provider, format, args), null);
            }

            private static void EnsureConfig()
            {
                if (!configured)
                {
                    lock (ConfigLock)
                    {
                        if (!configured)
                        {
                            var configPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ConfigFile);
                            var originalPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Config\\" + ConfigFile);
                            if (!File.Exists(configPath) && File.Exists(originalPath))
                            {
                                File.Copy(originalPath, configPath);
                            }

                            if (File.Exists(configPath))
                            {
                                AddGlobalContext();
                                XmlConfigurator.ConfigureAndWatch(new FileInfo(configPath));
                            }

                            configured = true;
                        }
                    }
                }
            }

            private static void ReloadLevels(ILoggerRepository repository)
            {
                LevelMap levelMap = repository.LevelMap;

                levelTrace = levelMap.LookupWithDefault(Level.Trace);
                levelDebug = levelMap.LookupWithDefault(Level.Debug);
                levelInfo = levelMap.LookupWithDefault(Level.Info);
                levelWarn = levelMap.LookupWithDefault(Level.Warn);
                levelError = levelMap.LookupWithDefault(Level.Error);
                levelFatal = levelMap.LookupWithDefault(Level.Fatal);
            }

            private static void AddGlobalContext()
            {
                try
                {
                    GlobalContext.Properties["appdomain"] = AppDomain.CurrentDomain.Id.ToString("D");
                }
                catch
                {
                    // do nothing but just make sure no exception here.
                }
            }
        }
    }
}
