// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.HttpModules.Analytics
{
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

    /// -----------------------------------------------------------------------------
    /// <summary>
    /// This module contains functionality for injecting web analytics scripts into the page.
    /// </summary>
    /// -----------------------------------------------------------------------------
    public class AnalyticsModule : IHttpModule
    {
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(AnalyticsModule));

        public string ModuleName
        {
            get
            {
                return "AnalyticsModule";
            }
        }

        public void Init(HttpApplication application)
        {
            application.PreRequestHandlerExecute += OnPreRequestHandlerExecute;
        }

        public void Dispose()
        {
        }

        private static void OnPreRequestHandlerExecute(object sender, EventArgs e)
        {
            try
            {
                // First check if we are upgrading/installing or if it is a non-page request
                var app = (HttpApplication)sender;
                var request = app.Request;

                if (!Initialize.ProcessHttpModule(request, false, false))
                {
                    return;
                }

                if (HttpContext.Current == null)
                {
                    return;
                }

                var context = HttpContext.Current;

                if (context == null)
                {
                    return;
                }

                var page = context.Handler as CDefault;
                if (page == null)
                {
                    return;
                }

                page.InitComplete += OnPageInitComplete;
                page.PreRender += OnPagePreRender;
            }
            catch (Exception ex)
            {
                var log = new LogInfo { LogTypeKey = EventLogController.EventLogType.HOST_ALERT.ToString() };
                log.AddProperty("Analytics.AnalyticsModule", "OnPreRequestHandlerExecute");
                log.AddProperty("ExceptionMessage", ex.Message);
                LogController.Instance.AddLog(log);
                Logger.Error(log);
            }
        }

        private static void OnPageInitComplete(object sender, EventArgs e)
        {
            InitializeAnalyticsControls(sender as Page, true);
        }

        private static void OnPagePreRender(object sender, EventArgs e)
        {
            InitializeAnalyticsControls(sender as Page, false);
        }

        private static void InitializeAnalyticsControls(Page page, bool injectTop)
        {
            try
            {
                var analyticsEngines = AnalyticsEngineConfiguration.GetConfig().AnalyticsEngines;
                if (analyticsEngines == null || analyticsEngines.Count == 0)
                {
                    return;
                }

                if (page == null)
                {
                    return;
                }

                foreach (AnalyticsEngine engine in analyticsEngines)
                {
                    if (string.IsNullOrEmpty(engine.ElementId) || engine.InjectTop != injectTop)
                    {
                        continue;
                    }

                    AnalyticsEngineBase objEngine;
                    if (!string.IsNullOrEmpty(engine.EngineType))
                    {
                        var engineType = Type.GetType(engine.EngineType);
                        if (engineType == null)
                        {
                            objEngine = new GenericAnalyticsEngine();
                        }
                        else
                        {
                            objEngine = (AnalyticsEngineBase)Activator.CreateInstance(engineType);
                        }
                    }
                    else
                    {
                        objEngine = new GenericAnalyticsEngine();
                    }

                    if (objEngine == null)
                    {
                        continue;
                    }

                    var script = engine.ScriptTemplate;
                    if (string.IsNullOrEmpty(script))
                    {
                        continue;
                    }

                    script = objEngine.RenderScript(script);
                    if (string.IsNullOrEmpty(script))
                    {
                        continue;
                    }

                    var element = (HtmlContainerControl)page.FindControl(engine.ElementId);
                    if (element == null)
                    {
                        continue;
                    }

                    var scriptControl = new LiteralControl { Text = script };
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
                var log = new LogInfo { LogTypeKey = EventLogController.EventLogType.HOST_ALERT.ToString() };
                log.AddProperty("Analytics.AnalyticsModule", "OnPagePreRender");
                log.AddProperty("ExceptionMessage", ex.Message);
                LogController.Instance.AddLog(log);
                Logger.Error(ex);
            }
        }
    }
}
