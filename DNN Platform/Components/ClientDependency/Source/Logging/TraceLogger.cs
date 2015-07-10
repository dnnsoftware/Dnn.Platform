using System;
using System.Web;

namespace ClientDependency.Core.Logging
{
    internal class TraceLogger : ILogger
    {
        private HttpContextBase GetHttpContext()
        {
            if (HttpContext.Current == null)
                return null;
            return new HttpContextWrapper(HttpContext.Current);
        }

        private void Trace(string msg, bool isWarn = false, string category = "ClientDependency")
        {
            var http = GetHttpContext();
            if (http == null) return;
            if (isWarn)
            {
                http.Trace.Warn(category, msg);
            }
            else
            {
                http.Trace.Write(category, msg);
            }
        }

        public void Debug(string msg)
        {
            Trace(msg);
        }

        public void Info(string msg)
        {
            Trace(msg);
        }

        public void Warn(string msg)
        {
            Trace(msg, true);
        }

        public void Error(string msg, Exception ex)
        {
            Trace(msg, true);
        }

        public void Fatal(string msg, Exception ex)
        {
            Trace(msg, true);
        }
    }
}