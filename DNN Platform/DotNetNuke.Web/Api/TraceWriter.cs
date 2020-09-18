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

    internal sealed class TraceWriter : ITraceWriter
    {
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(TraceWriter));
        private readonly bool _enabled;

        public TraceWriter(bool isTracingEnabled)
        {
            this._enabled = isTracingEnabled;
        }

        public void Trace(HttpRequestMessage request, string category, TraceLevel level, Action<TraceRecord> traceAction)
        {
            if (!this._enabled || level == TraceLevel.Off)
            {
                return;
            }

            var rec = new TraceRecord(request, category, level);
            traceAction(rec);
            this.Log(rec);
        }

        private void Log(TraceRecord rec)
        {
            var message = new StringBuilder();

            if (rec.Request != null)
            {
                if (rec.Request.Method != null)
                {
                    message.Append(" ").Append(rec.Request.Method.Method);
                }

                if (rec.Request.RequestUri != null)
                {
                    message.Append(" ").Append(rec.Request.RequestUri.AbsoluteUri);
                }
            }

            if (!string.IsNullOrEmpty(rec.Category))
            {
                message.Append(" ").Append(rec.Category);
            }

            if (!string.IsNullOrEmpty(rec.Message))
            {
                message.Append(" ").Append(rec.Message);
            }

            string output = message.ToString();

            if (!string.IsNullOrEmpty(output))
            {
                switch (rec.Level)
                {
                    case TraceLevel.Debug:
                        Logger.Debug(output);
                        break;
                    case TraceLevel.Info:
                        Logger.Info(output);
                        break;
                    case TraceLevel.Warn:
                        Logger.Warn(output);
                        break;
                    case TraceLevel.Error:
                        Logger.Error(output);
                        break;
                    case TraceLevel.Fatal:
                        Logger.Fatal(output);
                        break;
                }
            }
        }
    }
}
