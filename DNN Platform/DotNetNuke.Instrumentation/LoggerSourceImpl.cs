// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Instrumentation
{
    using System;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Security;
    using System.Web;

    using log4net;
    using log4net.Config;
    using log4net.Core;
    using log4net.Repository;
    using log4net.Util;

    public class LoggerSourceImpl : ILoggerSource
    {
        public ILog GetLogger(Type type)
        {
            return new Logger(LogManager.GetLogger(type).Logger, type);
        }

        public ILog GetLogger(string name)
        {
            return new Logger(LogManager.GetLogger(name).Logger, null);
        }

        private class Logger : LoggerWrapperImpl, ILog
        {
            private const string ConfigFile = "DotNetNuke.log4net.config";
            private static readonly object ConfigLock = new object();
            private static Level _levelTrace;
            private static Level _levelDebug;
            private static Level _levelInfo;
            private static Level _levelWarn;
            private static Level _levelError;
            private static Level _levelFatal;
            private static bool _configured;

            // add custom logging levels (below trace value of 20000)
            //            internal static Level LevelLogInfo = new Level(10001, "LogInfo");
            //            internal static Level LevelLogError = new Level(10002, "LogError");
            private readonly Type _stackBoundary = typeof(DnnLogger);

            internal Logger(ILogger logger, Type type)
                : base(logger)
            {
                this._stackBoundary = type ?? typeof(Logger);
                EnsureConfig();
                ReloadLevels(logger.Repository);
            }

            public bool IsDebugEnabled
            {
                get { return this.Logger.IsEnabledFor(_levelDebug); }
            }

            public bool IsInfoEnabled
            {
                get { return this.Logger.IsEnabledFor(_levelInfo); }
            }

            public bool IsTraceEnabled
            {
                get { return this.Logger.IsEnabledFor(_levelTrace); }
            }

            public bool IsWarnEnabled
            {
                get { return this.Logger.IsEnabledFor(_levelWarn); }
            }

            public bool IsErrorEnabled
            {
                get { return this.Logger.IsEnabledFor(_levelError); }
            }

            public bool IsFatalEnabled
            {
                get { return this.Logger.IsEnabledFor(_levelFatal); }
            }

            public void Debug(object message)
            {
                this.Debug(message, null);
            }

            public void Debug(object message, Exception exception)
            {
                this.Logger.Log(this._stackBoundary, _levelDebug, message, exception);
            }

            public void DebugFormat(string format, params object[] args)
            {
                this.DebugFormat(CultureInfo.InvariantCulture, format, args);
            }

            public void DebugFormat(IFormatProvider provider, string format, params object[] args)
            {
                this.Logger.Log(this._stackBoundary, _levelDebug, new SystemStringFormat(provider, format, args), null);
            }

            public void Info(object message)
            {
                this.Info(message, null);
            }

            public void Info(object message, Exception exception)
            {
                this.Logger.Log(this._stackBoundary, _levelInfo, message, exception);
            }

            public void InfoFormat(string format, params object[] args)
            {
                this.InfoFormat(CultureInfo.InvariantCulture, format, args);
            }

            public void InfoFormat(IFormatProvider provider, string format, params object[] args)
            {
                this.Logger.Log(this._stackBoundary, _levelInfo, new SystemStringFormat(provider, format, args), null);
            }

            public void Trace(object message)
            {
                this.Trace(message, null);
            }

            public void Trace(object message, Exception exception)
            {
                this.Logger.Log(this._stackBoundary, _levelTrace, message, exception);
            }

            public void TraceFormat(string format, params object[] args)
            {
                this.TraceFormat(CultureInfo.InvariantCulture, format, args);
            }

            public void TraceFormat(IFormatProvider provider, string format, params object[] args)
            {
                this.Logger.Log(this._stackBoundary, _levelTrace, new SystemStringFormat(provider, format, args), null);
            }

            public void Warn(object message)
            {
                this.Warn(message, null);
            }

            public void Warn(object message, Exception exception)
            {
                this.Logger.Log(this._stackBoundary, _levelWarn, message, exception);
            }

            public void WarnFormat(string format, params object[] args)
            {
                this.WarnFormat(CultureInfo.InvariantCulture, format, args);
            }

            public void WarnFormat(IFormatProvider provider, string format, params object[] args)
            {
                this.Logger.Log(this._stackBoundary, _levelWarn, new SystemStringFormat(provider, format, args), null);
            }

            public void Error(object message)
            {
                this.Error(message, null);
            }

            public void Error(object message, Exception exception)
            {
                this.Logger.Log(this._stackBoundary, _levelError, message, exception);
            }

            public void ErrorFormat(string format, params object[] args)
            {
                this.ErrorFormat(CultureInfo.InvariantCulture, format, args);
            }

            public void ErrorFormat(IFormatProvider provider, string format, params object[] args)
            {
                this.Logger.Log(this._stackBoundary, _levelError, new SystemStringFormat(provider, format, args), null);
            }

            public void Fatal(object message)
            {
                this.Fatal(message, null);
            }

            public void Fatal(object message, Exception exception)
            {
                this.Logger.Log(this._stackBoundary, _levelFatal, message, exception);
            }

            public void FatalFormat(string format, params object[] args)
            {
                this.FatalFormat(CultureInfo.InvariantCulture, format, args);
            }

            public void FatalFormat(IFormatProvider provider, string format, params object[] args)
            {
                this.Logger.Log(this._stackBoundary, _levelFatal, new SystemStringFormat(provider, format, args), null);
            }

            private static void EnsureConfig()
            {
                if (!_configured)
                {
                    lock (ConfigLock)
                    {
                        if (!_configured)
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

                            _configured = true;
                        }
                    }
                }
            }

            private static void ReloadLevels(ILoggerRepository repository)
            {
                LevelMap levelMap = repository.LevelMap;

                _levelTrace = levelMap.LookupWithDefault(Level.Trace);
                _levelDebug = levelMap.LookupWithDefault(Level.Debug);
                _levelInfo = levelMap.LookupWithDefault(Level.Info);
                _levelWarn = levelMap.LookupWithDefault(Level.Warn);
                _levelError = levelMap.LookupWithDefault(Level.Error);
                _levelFatal = levelMap.LookupWithDefault(Level.Fatal);

                // LevelLogError = levelMap.LookupWithDefault(LevelLogError);
                //                LevelLogInfo = levelMap.LookupWithDefault(LevelLogInfo);

                //// Register custom logging levels with the default LoggerRepository
                //                LogManager.GetRepository().LevelMap.Add(LevelLogInfo);
                //                LogManager.GetRepository().LevelMap.Add(LevelLogError);
            }

            private static void AddGlobalContext()
            {
                try
                {
                    GlobalContext.Properties["appdomain"] = AppDomain.CurrentDomain.Id.ToString("D");

                    // bool isFullTrust = false;
                    // try
                    // {
                    //    CodeAccessPermission securityTest = new AspNetHostingPermission(AspNetHostingPermissionLevel.Unrestricted);
                    //    securityTest.Demand();
                    //    isFullTrust = true;
                    // }
                    // catch
                    // {
                    //    //code access security error
                    //    isFullTrust = false;
                    // }
                    // if (isFullTrust)
                    // {
                    //    GlobalContext.Properties["processid"] = Process.GetCurrentProcess().Id.ToString("D");
                    // }
                }

                // ReSharper disable EmptyGeneralCatchClause
                catch

                    // ReSharper restore EmptyGeneralCatchClause
                {
                    // do nothing but just make sure no exception here.
                }
            }
        }
    }
}
