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
using System.Web.UI;
using System.Web.UI.HtmlControls;
using DotNetNuke.Common;
using DotNetNuke.Framework;
using DotNetNuke.HttpModules.Config;
using DotNetNuke.Instrumentation;
using DotNetNuke.Services.Analytics;
using DotNetNuke.Services.Log.EventLog;

#endregion

namespace DotNetNuke.HttpModules.Analytics
{
    /// -----------------------------------------------------------------------------
    /// <summary>
    /// This module contains functionality for injecting web analytics scripts into the page
    /// </summary>
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

        private static void OnPreRequestHandlerExecute(object sender, EventArgs e)
        {
            try
            {
                //First check if we are upgrading/installing or if it is a non-page request
                var app = (HttpApplication) sender;
                var request = app.Request;

                if (!Initialize.ProcessHttpModule(request, false, false)) return;

                if (HttpContext.Current == null) return;
                var context = HttpContext.Current;
                
                if (context == null) return;
                
                var page = context.Handler as CDefault;
                if (page == null) return;

				page.PreRender += OnPagePreRender;
            }
            catch (Exception ex)
            {
                var log = new LogInfo {LogTypeKey = EventLogController.EventLogType.HOST_ALERT.ToString()};
                log.AddProperty("Analytics.AnalyticsModule", "OnPreRequestHandlerExecute");
                log.AddProperty("ExceptionMessage", ex.Message);
                LogController.Instance.AddLog(log);
                Logger.Error(log);

            }
        }

		private static void OnPagePreRender(object sender, EventArgs e)
        {
            try
            {
                var  analyticsEngines = AnalyticsEngineConfiguration.GetConfig().AnalyticsEngines;
                if (analyticsEngines == null || analyticsEngines.Count == 0) return;
                
                var page = (Page) sender;
                if (page == null) return;
                
                foreach (AnalyticsEngine engine in analyticsEngines)
                {
                    if ((string.IsNullOrEmpty(engine.ElementId))) continue;
                    
                    AnalyticsEngineBase objEngine;
                    if ((!string.IsNullOrEmpty(engine.EngineType)))
                    {
                        var engineType = Type.GetType(engine.EngineType);
                        if (engineType == null) 
                            objEngine = new GenericAnalyticsEngine();
                        else
                            objEngine = (AnalyticsEngineBase) Activator.CreateInstance(engineType);
                    }
                    else
                    {
                        objEngine = new GenericAnalyticsEngine();
                    }
                    if (objEngine == null) continue;
                        
                    var script = engine.ScriptTemplate;
                    if ((string.IsNullOrEmpty(script))) continue;
                            
                    script = objEngine.RenderScript(script);
                    if ((string.IsNullOrEmpty(script))) continue;

                    var element = (HtmlContainerControl) page.FindControl(engine.ElementId);
                    if (element == null) continue;

                    var scriptControl = new LiteralControl {Text = script};
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
            catch (Exception ex)
            {
                var log = new LogInfo {LogTypeKey = EventLogController.EventLogType.HOST_ALERT.ToString()};
				log.AddProperty("Analytics.AnalyticsModule", "OnPagePreRender");
                log.AddProperty("ExceptionMessage", ex.Message);
                LogController.Instance.AddLog(log);
                Logger.Error(ex);

            }
        }
    }
}