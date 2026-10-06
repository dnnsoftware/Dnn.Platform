// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.Api
{
    using System;
    using System.Net.Http;
    using System.Text;
    using System.Web.Http.Tracing;

    using DotNetNuke.Instrumentation;

    using Microsoft.Extensions.Logging;

    /// <summary>A <see cref="ITraceWriter"/> implementation.</summary>
    internal sealed partial class TraceWriter : ITraceWriter
    {
        private static readonly ILogger Logger = DnnLoggingController.GetLogger<TraceWriter>();
        private readonly bool enabled;

        /// <summary>Initializes a new instance of the <see cref="TraceWriter"/> class.</summary>
        /// <param name="isTracingEnabled">Whether tracing is enabled.</param>
        public TraceWriter(bool isTracingEnabled)
        {
            this.enabled = isTracingEnabled;
        }

        /// <inheritdoc />
        public void Trace(HttpRequestMessage request, string category, TraceLevel level, Action<TraceRecord> traceAction)
        {
            if (!this.enabled || level == TraceLevel.Off)
            {
                return;
            }

            var rec = new TraceRecord(request, category, level);
            traceAction(rec);
            Log(rec);
        }

        private static void Log(TraceRecord rec)
        {
            var message = new StringBuilder();
            if (rec.Request != null)
            {
                if (rec.Request.Method != null)
                {
                    message.Append(' ').Append(rec.Request.Method.Method);
                }

                if (rec.Request.RequestUri != null)
                {
                    message.Append(' ').Append(rec.Request.RequestUri.AbsoluteUri);
                }
            }

            if (!string.IsNullOrEmpty(rec.Category))
            {
                message.Append(' ').Append(rec.Category);
            }

            if (!string.IsNullOrEmpty(rec.Message))
            {
                message.Append(' ').Append(rec.Message);
            }

            var output = message.ToString();
            if (string.IsNullOrEmpty(output))
            {
                return;
            }

            var logLevel = rec.Level switch
            {
                TraceLevel.Debug => LogLevel.Debug,
                TraceLevel.Info => LogLevel.Information,
                TraceLevel.Warn => LogLevel.Warning,
                TraceLevel.Error => LogLevel.Error,
                TraceLevel.Fatal => LogLevel.Critical,
                _ => LogLevel.None,
            };

            Logger.TraceWriterLogMessage(logLevel, output);
        }
    }
}
