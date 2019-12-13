﻿// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
#region Usings

using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Web.Compilation;
using log4net.Config;

#endregion

namespace DotNetNuke.Instrumentation
{
    [Obsolete("Deprecated in 7.0.1 due to poor performance, use LoggerSource.Instance. Scheduled removal in v11.0.0.")]
    public static class DnnLog
    {
        private const string ConfigFile = "DotNetNuke.log4net.config";
        private static bool _configured;

        //use a single static logger to avoid the performance impact of type reflection on every call for logging
        private static readonly DnnLogger Logger = DnnLogger.GetClassLogger(typeof(DnnLog));
        
        private static readonly object ConfigLock = new object();

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
                            XmlConfigurator.ConfigureAndWatch(new FileInfo(configPath));
                        }
                        _configured = true;
                    }

                }
            }
        }

        /// <summary>
        ///   Standard method to use on method entry
        /// </summary>
        public static void MethodEntry()
        {
            EnsureConfig();

            if (Logger.Logger.IsEnabledFor(DnnLogger.LevelTrace))
            {
                Logger.TraceFormat("Entering Method [{0}]", CallingFrame.GetMethod().Name);
            }
        }

        /// <summary>
        ///   Standard method to use on method exit
        /// </summary>
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

        /// <summary>
        ///   Standard method to use on method exit
        /// </summary>
        public static void MethodExit()
        {
            EnsureConfig();

            if (Logger.Logger.IsEnabledFor(DnnLogger.LevelTrace))
            {
                Logger.TraceFormat("Method [{0}] Returned", CallingFrame.GetMethod().Name);
            }
        }

        #region Trace

        public static void Trace(string message)
        {
            EnsureConfig();

            if (Logger.Logger.IsEnabledFor(DnnLogger.LevelTrace))
            {
                Logger.Trace(message);
            }
        }

        public static void Trace(string format, params object[] args)
        {
            EnsureConfig();

            if (Logger.Logger.IsEnabledFor(DnnLogger.LevelTrace))
            {
                Logger.TraceFormat(format, args);
            }
        }

        public static void Trace(IFormatProvider provider, string format, params object[] args)
        {
            EnsureConfig();

            if (Logger.Logger.IsEnabledFor(DnnLogger.LevelTrace))
            {
                Logger.TraceFormat(provider, format, args);
            }
        }

        #endregion

        #region Debug

        public static void Debug(object message)
        {
            EnsureConfig();

            if (Logger.Logger.IsEnabledFor(DnnLogger.LevelDebug))
            {
                Logger.Debug(message);
            }
        }

        public static void Debug(string format, params object[] args)
        {
            EnsureConfig();

            if (Logger.Logger.IsEnabledFor(DnnLogger.LevelDebug))
            {
                if(!args.Any())
                {
                    Logger.Debug(format);
                }
                else
                {
                    Logger.DebugFormat(format, args);    
                }
            }
        }

        public static void Debug(IFormatProvider provider, string format, params object[] args)
        {
            EnsureConfig();

            if (Logger.Logger.IsEnabledFor(DnnLogger.LevelDebug))
            {
                Logger.DebugFormat(provider, format, args);
            }
        }

        #endregion

        #region Info

        public static void Info(object message)
        {
            EnsureConfig();
            if (Logger.Logger.IsEnabledFor(DnnLogger.LevelInfo))
            {
                Logger.Info(message);
            }
        }

        public static void Info(IFormatProvider provider, string format, params object[] args)
        {
            EnsureConfig();
            if (Logger.Logger.IsEnabledFor(DnnLogger.LevelInfo))
            {
                Logger.InfoFormat(provider, format, args);
            }
        }

        public static void Info(string format, params object[] args)
        {
            EnsureConfig();
            if (Logger.Logger.IsEnabledFor(DnnLogger.LevelInfo))
            {
                if(!args.Any())
                {
                    Logger.Info(format);
                }
                else
                {
                    Logger.InfoFormat(format, args);    
                }
            }
        }

        #endregion

        #region Warn

        public static void Warn(string message, Exception exception)
        {
            EnsureConfig();
            if (Logger.Logger.IsEnabledFor(DnnLogger.LevelWarn))
            {
                Logger.Warn(message, exception);
            }
        }

        public static void Warn(object message)
        {
            EnsureConfig();
            if (Logger.Logger.IsEnabledFor(DnnLogger.LevelWarn))
            {
                Logger.Warn(message);
            }
        }

        public static void Warn(IFormatProvider provider, string format, params object[] args)
        {
            EnsureConfig();
            if (Logger.Logger.IsEnabledFor(DnnLogger.LevelWarn))
            {
                Logger.WarnFormat(provider, format, args);
            }
        }


        public static void Warn(string format, params object[] args)
        {
            EnsureConfig();
            if (Logger.Logger.IsEnabledFor(DnnLogger.LevelWarn))
            {
                if(!args.Any())
                {
                    Logger.Warn(format);
                }
                else
                {
                    Logger.WarnFormat(format, args);
                }
            }
        }

        #endregion

        #region Error

        public static void Error(string message, Exception exception)
        {
            EnsureConfig();

            if (Logger.Logger.IsEnabledFor(DnnLogger.LevelError))
            {
                Logger.Error(message, exception);
            }  

            
        }

        public static void Error(object message)
        {
            EnsureConfig();
            if (Logger.Logger.IsEnabledFor(DnnLogger.LevelError))
            {
                Logger.Error(message);
            }
        }

        public static void Error(Exception exception)
        {
            EnsureConfig();
            if (Logger.Logger.IsEnabledFor(DnnLogger.LevelError))
            {
                Logger.Error(exception.Message, exception);
            }
        }

        public static void Error(IFormatProvider provider, string format, params object[] args)
        {
            EnsureConfig();
            if (Logger.Logger.IsEnabledFor(DnnLogger.LevelError))
            {
                Logger.ErrorFormat(provider, format, args);
            }
        }

        public static void Error(string format, params object[] args)
        {
            EnsureConfig();
            if (Logger.Logger.IsEnabledFor(DnnLogger.LevelError))
            {
                if (!args.Any())
                {
                    Logger.ErrorFormat(format);
                }
                else
                {
                    Logger.ErrorFormat(format, args);
                }
            }
        }

        #endregion

        #region Fatal

        public static void Fatal(string message, Exception exception)
        {
            EnsureConfig();
            if (Logger.Logger.IsEnabledFor(DnnLogger.LevelFatal))
            {
                Logger.Fatal(message, exception);
            }
        }

        public static void Fatal(object message)
        {
            EnsureConfig();
            if (Logger.Logger.IsEnabledFor(DnnLogger.LevelFatal))
            {
                Logger.Fatal(message);
            }
        }

        public static void Fatal(IFormatProvider provider, string format, params object[] args)
        {
            EnsureConfig();
            if (Logger.Logger.IsEnabledFor(DnnLogger.LevelFatal))
            {
                Logger.FatalFormat(provider, format, args);
            }
        }

        public static void Fatal(string format, params object[] args)
        {
            EnsureConfig();
            if (Logger.Logger.IsEnabledFor(DnnLogger.LevelFatal))
            {
                if (!args.Any())
                {
                    Logger.Fatal(format);
                }
                else
                {
                    Logger.FatalFormat(format, args);
                }
            }
        }

        #endregion
    }
}
