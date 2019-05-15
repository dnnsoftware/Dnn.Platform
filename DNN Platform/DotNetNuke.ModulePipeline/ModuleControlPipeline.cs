using System;
using System.IO;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Instrumentation;
using DotNetNuke.Services.Localization;
using DotNetNuke.UI.Modules.Html5;
using DotNetNuke.UI.Modules;
using DotNetNuke.Web.Razor;
using System.Collections.Generic;
using DotNetNuke.Web.Mvc;

#if NET472
using System.Web.UI;
#endif

namespace DotNetNuke.ModulePipeline
{
    /// <summary>
    /// The Module Pipeline that determines which Module pattern
    /// to invoke based on the input module type.
    /// </summary>
    public class ModuleControlPipeline
        // MULTI-TARGETTING PIPELINE
        // -------------------------
        // This file multi-targets .NET Framework and .NET Standard,
        // which is needed as DNN migrates to .NET Core. The 'NET472'
        // pre-processor directives are to fully support Legacy DNN.
        // As the Pipeline is upgraded to be more complaint with 
        // .NET Standard 2.0 use the apprioprate pre-processor directives.
#if NET472
        : IModuleControlPipeline
#endif
    {
        private static readonly ILog TraceLogger = LoggerSource.Instance.GetLogger("DNN.Trace");
        private Dictionary<string, IModuleControlFactory> _controlFactories;
        public ModuleControlPipeline(
            WebFormsModuleControlFactory webforms,
            Html5ModuleControlFactory html5,
            RazorModuleControlFactory razor3,
            MvcModuleControlFactory mvc,
            ReflectedModuleControlFactory fallthrough)
        {
            _controlFactories = new Dictionary<string, IModuleControlFactory>(StringComparer.OrdinalIgnoreCase);
            _controlFactories.Add(".ascx", webforms);
            _controlFactories.Add(".htm", html5);
            _controlFactories.Add(".html", html5);
            _controlFactories.Add(".cshtml", razor3);
            _controlFactories.Add(".vbhtml", razor3);
            _controlFactories.Add(".mvc", mvc);
            _controlFactories.Add("default", fallthrough);
        }

#if NET472
        private IModuleControlFactory GetModuleControlFactory(string controlSrc)
        {
            string extension = Path.GetExtension(controlSrc);
            _controlFactories.TryGetValue(extension, out IModuleControlFactory factory);

            return factory ?? _controlFactories["default"];
        }

        public Control LoadModuleControl(TemplateControl containerControl, ModuleInfo moduleConfiguration, string controlKey, string controlSrc)
        {
            if (TraceLogger.IsDebugEnabled)
                TraceLogger.Debug($"ModuleControlFactory.LoadModuleControl Start (TabId:{moduleConfiguration.TabID},ModuleId:{moduleConfiguration.ModuleID}): ModuleControlSource:{moduleConfiguration.ModuleControl.ControlSrc}");

            Control control = null;
            IModuleControlFactory controlFactory = GetModuleControlFactory(controlSrc);

            if (controlFactory != null)
            {
                control = controlFactory.CreateControl(containerControl, controlKey, controlSrc);
            }

            // set the control ID to the resource file name ( ie. controlname.ascx = controlname )
            // this is necessary for the Localization in PageBase
            if (control != null)
            {
                control.ID = Path.GetFileNameWithoutExtension(controlSrc);

                var moduleControl = control as IModuleControl;

                if (moduleControl != null)
                {
                    moduleControl.ModuleContext.Configuration = moduleConfiguration;
                }
            }

            if (TraceLogger.IsDebugEnabled)
                TraceLogger.Debug($"ModuleControlFactory.LoadModuleControl End (TabId:{moduleConfiguration.TabID},ModuleId:{moduleConfiguration.ModuleID}): ModuleControlSource:{moduleConfiguration.ModuleControl.ControlSrc}");
            return control;
        }

        public Control LoadModuleControl(TemplateControl containerControl, ModuleInfo moduleConfiguration)
        {
            if (TraceLogger.IsDebugEnabled)
                TraceLogger.Debug($"ModuleControlFactory.LoadModuleControl Start (TabId:{moduleConfiguration.TabID},ModuleId:{moduleConfiguration.ModuleID}): ModuleControlSource:{moduleConfiguration.ModuleControl.ControlSrc}");
            Control control = null;
            IModuleControlFactory controlFactory = GetModuleControlFactory(moduleConfiguration.ModuleControl.ControlSrc);

            if (controlFactory != null)
            {
                control = controlFactory.CreateModuleControl(containerControl, moduleConfiguration);
            }

            // set the control ID to the resource file name ( ie. controlname.ascx = controlname )
            // this is necessary for the Localization in PageBase
            if (control != null)
            {
                control.ID = Path.GetFileNameWithoutExtension(moduleConfiguration.ModuleControl.ControlSrc);

                var moduleControl = control as IModuleControl;

                if (moduleControl != null)
                {
                    moduleControl.ModuleContext.Configuration = moduleConfiguration;
                }
            }

            if (TraceLogger.IsDebugEnabled)
                TraceLogger.Debug($"ModuleControlFactory.LoadModuleControl End (TabId:{moduleConfiguration.TabID},ModuleId:{moduleConfiguration.ModuleID}): ModuleControlSource:{moduleConfiguration.ModuleControl.ControlSrc}");
            return control;
        }

        public Control LoadSettingsControl(TemplateControl containerControl, ModuleInfo moduleConfiguration, string controlSrc)
        {
            if (TraceLogger.IsDebugEnabled)
                TraceLogger.Debug($"ModuleControlFactory.LoadSettingsControl Start (TabId:{moduleConfiguration.TabID},ModuleId:{moduleConfiguration.ModuleID}): ModuleControlSource:{moduleConfiguration.ModuleControl.ControlSrc}");

            Control control = null;
            IModuleControlFactory controlFactory = GetModuleControlFactory(controlSrc);

            if (controlFactory != null)
            {
                control = controlFactory.CreateSettingsControl(containerControl, moduleConfiguration, controlSrc);
            }

            // set the control ID to the resource file name ( ie. controlname.ascx = controlname )
            // this is necessary for the Localization in PageBase
            if (control != null)
            {
                var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(controlSrc);
                if (fileNameWithoutExtension != null)
                {
                    control.ID = fileNameWithoutExtension.Replace('.', '-');
                }

                var settingsControl = control as ISettingsControl;

                if (settingsControl != null)
                {
                    settingsControl.ModuleContext.Configuration = moduleConfiguration;
                }
            }

            if (TraceLogger.IsDebugEnabled)
                TraceLogger.Debug($"ModuleControlFactory.LoadSettingsControl End (TabId:{moduleConfiguration.TabID},ModuleId:{moduleConfiguration.ModuleID}): ModuleControlSource:{moduleConfiguration.ModuleControl.ControlSrc}");
            return control;
        }

        public Control CreateCachedControl(string cachedContent, ModuleInfo moduleConfiguration)
        {
            var moduleControl = new CachedModuleControl(cachedContent);
            moduleControl.ModuleContext.Configuration = moduleConfiguration;
            return moduleControl;
        }

        public Control CreateModuleControl(ModuleInfo moduleConfiguration)
        {
            IModuleControlFactory factory = GetModuleControlFactory(moduleConfiguration.ModuleControl.ControlSrc);
            return factory.CreateModuleControl(moduleConfiguration);
        }
#endif
    }
}
