// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.HttpModules.Exceptions
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Web;

    using DotNetNuke.Common;
    using DotNetNuke.Instrumentation;
    using DotNetNuke.Services.Log.EventLog;

    /// <summary>Handles the exception that occur with http modules.</summary>
    public class ExceptionModule : IHttpModule
    {
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(ExceptionModule));

        /// <summary>Gets the name of the module.</summary>
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Breaking change")]
        public string ModuleName => "ExceptionModule";

        /// <summary>Initializes the error handling for the specified application.</summary>
        /// <param name="application">The application.</param>
        public void Init(HttpApplication application)
        {
            application.Error += this.OnErrorRequest;
        }

        /// <inheritdoc/>
        public void Dispose()
        {
        }

        /// <summary>Called when error handling is requested.</summary>
        /// <param name="s">The object with the error.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        public void OnErrorRequest(object s, EventArgs e)
        {
            try
            {
                if (HttpContext.Current == null)
                {
                    return;
                }

                HttpContext context = HttpContext.Current;
                HttpServerUtility server = context.Server;
                HttpRequest request = context.Request;

                if (!Initialize.ProcessHttpModule(request, false, false))
                {
                    return;
                }

                Exception lastException = server.GetLastError();
                if (lastException is HttpException)
                {
                    // HttpExceptions are logged elsewhere
                    return;
                }

                var lex = new UnhandledException("Unhandled Error: ", server.GetLastError());
                var objExceptionLog = new ExceptionLogController();
                try
                {
                    objExceptionLog.AddLog(lex);
                }
                catch (Exception ex)
                {
                    Logger.Error(ex);
                }
            }
            catch (Exception exc)
            {
                // it is possible when terminating the request for the context not to exist
                // in this case we just want to exit since there is nothing else we can do
                Logger.Error(exc);
            }
        }
    }
}
