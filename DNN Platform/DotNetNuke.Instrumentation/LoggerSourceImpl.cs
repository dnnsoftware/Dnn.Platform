// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Instrumentation
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;

    using DotNetNuke.Internal.SourceGenerators;

    using Serilog;

    /// <summary>An <see cref="ILoggerSource"/> implementation.</summary>
    [SuppressMessage("Microsoft.Design", "CA1711:IdentifiersShouldNotHaveIncorrectSuffix", Justification = "Breaking change")]
    [DnnDeprecated(10, 4, 0, "Use Microsoft.Extensions.Logging.ILogger<T>")]
    public partial class LoggerSourceImpl : ILoggerSource
    {
        /// <inheritdoc />
        public ILog GetLogger(Type type)
        {
            return new Logger(type);
        }

        /// <inheritdoc />
        public ILog GetLogger(string name)
        {
            return new Logger(null);
        }

        private sealed class Logger : ILog
        {
            private readonly ILogger logger;

            internal Logger(Type type)
            {
                if (Log.Logger == null)
                {
                    // initialize Serilog - under normal circumstances this should have already been done by the application startup code, but we need to ensure it's done before we can use it
                    SerilogController.Initialize();
                }

                if (type == null)
                {
                    this.logger = Log.Logger;
                }
                else
                {
                    this.logger = Log.ForContext(type);
                }
            }

            public bool IsDebugEnabled
            {
                get { return this.logger.IsEnabled(Serilog.Events.LogEventLevel.Debug); }
            }

            public bool IsInfoEnabled
            {
                get { return this.logger.IsEnabled(Serilog.Events.LogEventLevel.Information); }
            }

            public bool IsTraceEnabled
            {
                get { return this.logger.IsEnabled(Serilog.Events.LogEventLevel.Verbose); }
            }

            public bool IsWarnEnabled
            {
                get { return this.logger.IsEnabled(Serilog.Events.LogEventLevel.Warning); }
            }

            public bool IsErrorEnabled
            {
                get { return this.logger.IsEnabled(Serilog.Events.LogEventLevel.Error); }
            }

            public bool IsFatalEnabled
            {
                get { return this.logger.IsEnabled(Serilog.Events.LogEventLevel.Fatal); }
            }

            public void Debug(object message)
            {
                this.Debug(message, null);
            }

            public void Debug(object message, Exception exception)
            {
                if (message == null)
                {
                    this.logger.Debug(exception, exception.Message);
                }
                else if (message is string)
                {
                    this.logger.Debug(exception, (string)message);
                }
                else
                {
                    this.logger.Debug(exception, message.ToString());
                }
            }

            public void DebugFormat(string format, params object[] args)
            {
                this.DebugFormat(CultureInfo.InvariantCulture, format, args);
            }

            public void DebugFormat(IFormatProvider provider, string format, params object[] args)
            {
                this.logger.Debug(string.Format(CultureInfo.InvariantCulture, format, args));
            }

            public void Info(object message)
            {
                this.Info(message, null);
            }

            public void Info(object message, Exception exception)
            {
                if (message is string)
                {
                    this.logger.Information(exception, (string)message);
                }
                else
                {
                    this.logger.Information(exception, message.ToString());
                }
            }

            public void InfoFormat(string format, params object[] args)
            {
                this.InfoFormat(CultureInfo.InvariantCulture, format, args);
            }

            public void InfoFormat(IFormatProvider provider, string format, params object[] args)
            {
                this.logger.Information(string.Format(CultureInfo.InvariantCulture, format, args));
            }

            public void Trace(object message)
            {
                this.Trace(message, null);
            }

            public void Trace(object message, Exception exception)
            {
                if (message is string)
                {
                    this.logger.Verbose(exception, (string)message);
                }
                else
                {
                    this.logger.Verbose(exception, message.ToString());
                }
            }

            public void TraceFormat(string format, params object[] args)
            {
                this.TraceFormat(CultureInfo.InvariantCulture, format, args);
            }

            public void TraceFormat(IFormatProvider provider, string format, params object[] args)
            {
                this.logger.Verbose(string.Format(CultureInfo.InvariantCulture, format, args));
            }

            public void Warn(object message)
            {
                this.Warn(message, null);
            }

            public void Warn(object message, Exception exception)
            {
                if (message is string)
                {
                    this.logger.Warning(exception, (string)message);
                }
                else
                {
                    this.logger.Warning(exception, message.ToString());
                }
            }

            public void WarnFormat(string format, params object[] args)
            {
                this.WarnFormat(CultureInfo.InvariantCulture, format, args);
            }

            public void WarnFormat(IFormatProvider provider, string format, params object[] args)
            {
                this.logger.Warning(string.Format(CultureInfo.InvariantCulture, format, args));
            }

            public void Error(object message)
            {
                this.Error(message, null);
            }

            public void Error(object message, Exception exception)
            {
                if (message is string)
                {
                    this.logger.Error(exception, (string)message);
                }
                else
                {
                    this.logger.Error(exception, message.ToString());
                }
            }

            public void ErrorFormat(string format, params object[] args)
            {
                this.ErrorFormat(CultureInfo.InvariantCulture, format, args);
            }

            public void ErrorFormat(IFormatProvider provider, string format, params object[] args)
            {
                this.logger.Error(string.Format(CultureInfo.InvariantCulture, format, args));
            }

            public void Fatal(object message)
            {
                this.Fatal(message, null);
            }

            public void Fatal(object message, Exception exception)
            {
                if (message is string)
                {
                    this.logger.Fatal(exception, (string)message);
                }
                else
                {
                    this.logger.Fatal(exception, message.ToString());
                }
            }

            public void FatalFormat(string format, params object[] args)
            {
                this.FatalFormat(CultureInfo.InvariantCulture, format, args);
            }

            public void FatalFormat(IFormatProvider provider, string format, params object[] args)
            {
                this.logger.Fatal(string.Format(CultureInfo.InvariantCulture, format, args));
            }
        }
    }
}
