#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2014
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
using System.Web.UI;
using System.Web.UI.HtmlControls;

using DotNetNuke.Framework;
using DotNetNuke.HttpModules.Config;
using DotNetNuke.Instrumentation;
using DotNetNuke.Services.Analytics;
using DotNetNuke.Services.Log.EventLog;

#endregion

namespace DotNetNuke.HttpModules.Analytics
{
    /// -----------------------------------------------------------------------------
    /// Namespace:  DotNetNuke.HttpModules.Analytics
    /// Project:    HttpModules
    /// Module:     AnalyticsModule
    /// -----------------------------------------------------------------------------
    /// <summary>
    /// This module contains functionality for injecting web analytics scripts into the page
    /// </summary>
    /// <remarks>
    /// </remarks>
    /// <history>
    ///		[cniknet]	05/03/2009	created
    /// </history>
    /// -----------------------------------------------------------------------------
    public class AnalyticsModule : IHttpModule
    {
    	private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof (AnalyticsModule));
        public string ModuleName
        {
            get
            {
                return "AnalyticsModule";
            }
        }

        #region IHttpModule Members

        public void Init(HttpApplication application)
        {
            application.PreRequestHandlerExecute += OnPreRequestHandlerExecute;
        }

        public void Dispose()
        {
        }

        #endregion

        private void OnPreRequestHandlerExecute(object sender, EventArgs e)
        {
            try
            {
                //First check if we are upgrading/installing or if it is a non-page request
                var app = (HttpApplication) sender;
                HttpRequest request = app.Request;

                //First check if we are upgrading/installing
                if (request.Url.LocalPath.ToLower().EndsWith("install.aspx")
                        || request.Url.LocalPath.ToLower().Contains("upgradewizard.aspx")
                        || request.Url.LocalPath.ToLower().Contains("installwizard.aspx"))
                {
                    return;
                }
				
                //exit if a request for a .net mapping that isn't a content page is made i.e. axd
                if (request.Url.LocalPath.ToLower().EndsWith(".aspx") == false && request.Url.LocalPath.ToLower().EndsWith(".asmx") == false &&
                    request.Url.LocalPath.ToLower().EndsWith(".ashx") == false)
                {
                    return;
                }
                if (HttpContext.Current != null)
                {
                    HttpContext context = HttpContext.Current;
                    if ((context == null))
                    {
                        return;
                    }
                    var page = context.Handler as CDefault;
                    if ((page == null))
                    {
                        return;
                    }
                    page.Load += OnPageLoad;
                }
            }
            catch (Exception ex)
            {
                var objEventLog = new EventLogController();
                var objEventLogInfo = new LogInfo();
                objEventLogInfo.AddProperty("Analytics.AnalyticsModule", "OnPreRequestHandlerExecute");
                objEventLogInfo.AddProperty("ExceptionMessage", ex.Message);
                objEventLogInfo.LogTypeKey = EventLogController.EventLogType.HOST_ALERT.ToString();
                objEventLog.AddLog(objEventLogInfo);
                Logger.Error(objEventLogInfo);

            }
        }

        private void OnPageLoad(object sender, EventArgs e)
        {
            try
            {
                AnalyticsEngineCollection analyticsEngines = AnalyticsEngineConfiguration.GetConfig().AnalyticsEngines;
                if (analyticsEngines == null || analyticsEngines.Count == 0)
                {
                    return;
                }
                var page = (Page) sender;
                if ((page == null))
                {
                    return;
                }
                foreach (AnalyticsEngine engine in analyticsEngines)
                {
                    if ((!String.IsNullOrEmpty(engine.ElementId)))
                    {
                        AnalyticsEngineBase objEngine = null;
                        if ((!String.IsNullOrEmpty(engine.EngineType)))
                        {
                            Type engineType = Type.GetType(engine.EngineType);
                            if (engineType == null) 
                                objEngine = new GenericAnalyticsEngine();
                            else
                                objEngine = (AnalyticsEngineBase) Activator.CreateInstance(engineType);
                        }
                        else
                        {
                            objEngine = new GenericAnalyticsEngine();
                        }
                        if (objEngine != null)
                        {
                            string script = engine.ScriptTemplate;
                            if ((!String.IsNullOrEmpty(script)))
                            {
                                script = objEngine.RenderScript(script);
                                if ((!String.IsNullOrEmpty(script)))
                                {
                                    var element = (HtmlContainerControl) page.FindControl(engine.ElementId);
                                    if (element != null)
                                    {
                                        var scriptControl = new LiteralControl();
                                        scriptControl.Text = script;
                                        if (engine.InjectTop)
                                        {
                                            element.Controls.AddAt(0, scriptControl);
                                        }
                                        else
                                        {
                                            element.Controls.Add(scriptControl);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                var objEventLog = new EventLogController();
                var objEventLogInfo = new LogInfo();
                objEventLogInfo.AddProperty("Analytics.AnalyticsModule", "OnPageLoad");
                objEventLogInfo.AddProperty("ExceptionMessage", ex.Message);
                objEventLogInfo.LogTypeKey = EventLogController.EventLogType.HOST_ALERT.ToString();
                objEventLog.AddLog(objEventLogInfo);
                Logger.Error(ex);

            }
        }
    }
}