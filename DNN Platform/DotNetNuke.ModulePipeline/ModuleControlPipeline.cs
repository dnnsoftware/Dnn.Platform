// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.ModulePipeline
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Web.UI;

    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Instrumentation;
    using DotNetNuke.UI.Modules;

    /// <summary>
    /// The Module Pipeline that determines which Module pattern
    /// to invoke based on the input module type.
    /// </summary>
    public class ModuleControlPipeline : IModuleControlPipeline
    {
        private static readonly ILog TraceLogger = LoggerSource.Instance.GetLogger("DNN.Trace");
        private readonly IModuleControlFactory[] factories;

        /// <summary>Initializes a new instance of the <see cref="ModuleControlPipeline"/> class.</summary>
        /// <param name="factories">The control factories.</param>
        public ModuleControlPipeline(IEnumerable<IModuleControlFactory> factories)
        {
            this.factories = factories.OrderByDescending(f => f.Priority).ToArray();
        }

        /// <inheritdoc />
        public Control LoadModuleControl(TemplateControl containerControl, ModuleInfo moduleConfiguration, string controlKey, string controlSrc)
        {
            if (TraceLogger.IsDebugEnabled)
            {
                TraceLogger.Debug($"ModuleControlFactory.LoadModuleControl Start (TabId:{moduleConfiguration.TabID},ModuleId:{moduleConfiguration.ModuleID}): ModuleControlSource:{moduleConfiguration.ModuleControl.ControlSrc}");
            }

            Control control = null;
            IModuleControlFactory controlFactory = this.GetModuleControlFactory(moduleConfiguration, controlSrc);

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
            IModuleControlFactory controlFactory = this.GetModuleControlFactory(moduleConfiguration, moduleConfiguration.ModuleControl.ControlSrc);

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
            IModuleControlFactory controlFactory = this.GetModuleControlFactory(moduleConfiguration, controlSrc);

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
            IModuleControlFactory factory = this.GetModuleControlFactory(moduleConfiguration, moduleConfiguration.ModuleControl.ControlSrc);
            return factory.CreateModuleControl(moduleConfiguration);
        }

        private IModuleControlFactory GetModuleControlFactory(ModuleInfo moduleConfiguration, string controlSrc)
        {
            var length = this.factories.Length;

            for (var i = 0; i < length; i++)
            {
                var factory = this.factories[i];

                if (factory.SupportsControl(moduleConfiguration, controlSrc))
                {
                    return factory;
                }
            }

            // The following exception should never be thrown, as the default factory should always be able to create a control
            throw new NotSupportedException($"No module control factory found for module {moduleConfiguration.ModuleID} with control source {controlSrc}");
        }
    }
}
