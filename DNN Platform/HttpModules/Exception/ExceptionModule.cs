#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2018
// by DotNetNuke Corporation
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated 
// documentation files (the "Software"), to deal in the Software without restriction, including without limitation 
// the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and 
// to permit persons to whom the Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all copies or substantial portions 
// of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED 
// TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL 
// THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF 
// CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
// DEALINGS IN THE SOFTWARE.
#endregion
#region Usings

using System;
using System.Web;
using DotNetNuke.Common;
using DotNetNuke.Instrumentation;
using DotNetNuke.Services.Log.EventLog;

#endregion

namespace DotNetNuke.HttpModules.Exceptions
{
    /// <summary>
    /// Handles the exception that occur with http modules
    /// </summary>
    public class ExceptionModule : IHttpModule
    {
    	private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof (ExceptionModule));
        /// <summary>
        /// Gets the name of the module.
        /// </summary>
        /// <value>
        /// The name of the module: "ExceptionModule"
        /// </value>
        public string ModuleName
        {
            get
            {
                return "ExceptionModule";
            }
        }

        #region IHttpModule Members

        /// <summary>
        /// Initializes the error handling for the specified application.
        /// </summary>
        /// <param name="application">The application.</param>
        public void Init(HttpApplication application)
        {
            application.Error += OnErrorRequest;
        }

        public void Dispose()
        {
        }

        #endregion

        /// <summary>
        /// Called when error handling is requested.
        /// </summary>
        /// <param name="s">The object with the error</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        public void OnErrorRequest(object s, EventArgs e)
        {
            try
            {
				if(HttpContext.Current == null)
				{
					return;
				}

                HttpContext contxt = HttpContext.Current;
                HttpServerUtility srver = contxt.Server;
                HttpRequest request = contxt.Request;

                if (!Initialize.ProcessHttpModule(request, false, false))
                {
                    return;
                }

                Exception lastException = srver.GetLastError();

                //HttpExceptions are logged elsewhere
                if (!(lastException is HttpException))
                {
                    var lex = new Exception("Unhandled Error: ", srver.GetLastError());
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
            }
            catch (Exception exc)
            {
                //it is possible when terminating the request for the context not to exist
                //in this case we just want to exit since there is nothing else we can do
				Logger.Error(exc);
            }
        }
    }
}