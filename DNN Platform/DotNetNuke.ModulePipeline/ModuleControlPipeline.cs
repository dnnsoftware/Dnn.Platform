// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.ModulePipeline
{
    using System;
    using System.Collections.Generic;
    using System.IO;
#if NET472
    using System.Web.UI;
#endif

    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Instrumentation;
    using DotNetNuke.UI.Modules;
    using DotNetNuke.UI.Modules.Html5;
    using DotNetNuke.Web.Mvc;
    using DotNetNuke.Web.Razor;

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
    // .NET Standard 2.0 use the appropriate pre-processor directives.
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
            this._controlFactories = new Dictionary<string, IModuleControlFactory>(StringComparer.OrdinalIgnoreCase);
            this._controlFactories.Add(".ascx", webforms);
            this._controlFactories.Add(".htm", html5);
            this._controlFactories.Add(".html", html5);
            this._controlFactories.Add(".cshtml", razor3);
            this._controlFactories.Add(".vbhtml", razor3);
            this._controlFactories.Add(".mvc", mvc);
            this._controlFactories.Add("default", fallthrough);
        }

#if NET472
        /// <inheritdoc />
        public Control LoadModuleControl(TemplateControl containerControl, ModuleInfo moduleConfiguration, string controlKey, string controlSrc)
        {
            if (TraceLogger.IsDebugEnabled)
            {
                TraceLogger.Debug($"ModuleControlFactory.LoadModuleControl Start (TabId:{moduleConfiguration.TabID},ModuleId:{moduleConfiguration.ModuleID}): ModuleControlSource:{moduleConfiguration.ModuleControl.ControlSrc}");
            }

            Control control = null;
            IModuleControlFactory controlFactory = this.GetModuleControlFactory(controlSrc);

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
            {
                TraceLogger.Debug($"ModuleControlFactory.LoadModuleControl End (TabId:{moduleConfiguration.TabID},ModuleId:{moduleConfiguration.ModuleID}): ModuleControlSource:{moduleConfiguration.ModuleControl.ControlSrc}");
            }

            return control;
        }

        /// <inheritdoc />
        public Control LoadModuleControl(TemplateControl containerControl, ModuleInfo moduleConfiguration)
        {
            if (TraceLogger.IsDebugEnabled)
            {
                TraceLogger.Debug($"ModuleControlFactory.LoadModuleControl Start (TabId:{moduleConfiguration.TabID},ModuleId:{moduleConfiguration.ModuleID}): ModuleControlSource:{moduleConfiguration.ModuleControl.ControlSrc}");
            }

            Control control = null;
            IModuleControlFactory controlFactory = this.GetModuleControlFactory(moduleConfiguration.ModuleControl.ControlSrc);

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
            {
                TraceLogger.Debug($"ModuleControlFactory.LoadModuleControl End (TabId:{moduleConfiguration.TabID},ModuleId:{moduleConfiguration.ModuleID}): ModuleControlSource:{moduleConfiguration.ModuleControl.ControlSrc}");
            }

            return control;
        }

        /// <inheritdoc />
        public Control LoadSettingsControl(TemplateControl containerControl, ModuleInfo moduleConfiguration, string controlSrc)
        {
            if (TraceLogger.IsDebugEnabled)
            {
                TraceLogger.Debug($"ModuleControlFactory.LoadSettingsControl Start (TabId:{moduleConfiguration.TabID},ModuleId:{moduleConfiguration.ModuleID}): ModuleControlSource:{moduleConfiguration.ModuleControl.ControlSrc}");
            }

            Control control = null;
            IModuleControlFactory controlFactory = this.GetModuleControlFactory(controlSrc);

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
            {
                TraceLogger.Debug($"ModuleControlFactory.LoadSettingsControl End (TabId:{moduleConfiguration.TabID},ModuleId:{moduleConfiguration.ModuleID}): ModuleControlSource:{moduleConfiguration.ModuleControl.ControlSrc}");
            }

            return control;
        }

        /// <inheritdoc />
        public Control CreateCachedControl(string cachedContent, ModuleInfo moduleConfiguration)
        {
            var moduleControl = new CachedModuleControl(cachedContent);
            moduleControl.ModuleContext.Configuration = moduleConfiguration;
            return moduleControl;
        }

        /// <inheritdoc />
        public Control CreateModuleControl(ModuleInfo moduleConfiguration)
        {
            IModuleControlFactory factory = this.GetModuleControlFactory(moduleConfiguration.ModuleControl.ControlSrc);
            return factory.CreateModuleControl(moduleConfiguration);
        }

        private IModuleControlFactory GetModuleControlFactory(string controlSrc)
        {
            string extension = Path.GetExtension(controlSrc);
            this._controlFactories.TryGetValue(extension, out IModuleControlFactory factory);

            return factory ?? this._controlFactories["default"];
        }
#endif
    }
}
