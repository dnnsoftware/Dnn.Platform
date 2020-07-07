// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.UI.Modules
{
    using System;
    using System.IO;
    using System.Web.UI;

    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Framework;
    using DotNetNuke.Instrumentation;
    using DotNetNuke.Services.Localization;
    using DotNetNuke.UI.Modules.Html5;

    [Obsolete("This implementation has moved to DotNetNuke.ModulePipeline.ModuleControlFactory. Scheduled removal in v11.0.0.")]
    public class ModuleControlFactory
    {
        private static readonly ILog TracelLogger = LoggerSource.Instance.GetLogger("DNN.Trace");

        [Obsolete("This implementation has moved to DotNetNuke.ModulePipeline.ModuleControlFactory. Scheduled removal in v11.0.0.")]
        public static Control LoadModuleControl(TemplateControl containerControl, ModuleInfo moduleConfiguration, string controlKey, string controlSrc)
        {
            if (TracelLogger.IsDebugEnabled)
            {
                TracelLogger.Debug($"ModuleControlFactory.LoadModuleControl Start (TabId:{moduleConfiguration.TabID},ModuleId:{moduleConfiguration.ModuleID}): ModuleControlSource:{moduleConfiguration.ModuleControl.ControlSrc}");
            }

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

            if (TracelLogger.IsDebugEnabled)
            {
                TracelLogger.Debug($"ModuleControlFactory.LoadModuleControl End (TabId:{moduleConfiguration.TabID},ModuleId:{moduleConfiguration.ModuleID}): ModuleControlSource:{moduleConfiguration.ModuleControl.ControlSrc}");
            }

            return control;
        }

        [Obsolete("This implementation has moved to DotNetNuke.ModulePipeline.ModuleControlFactory. Scheduled removal in v11.0.0.")]
        public static Control LoadModuleControl(TemplateControl containerControl, ModuleInfo moduleConfiguration)
        {
            if (TracelLogger.IsDebugEnabled)
            {
                TracelLogger.Debug($"ModuleControlFactory.LoadModuleControl Start (TabId:{moduleConfiguration.TabID},ModuleId:{moduleConfiguration.ModuleID}): ModuleControlSource:{moduleConfiguration.ModuleControl.ControlSrc}");
            }

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

            if (TracelLogger.IsDebugEnabled)
            {
                TracelLogger.Debug($"ModuleControlFactory.LoadModuleControl End (TabId:{moduleConfiguration.TabID},ModuleId:{moduleConfiguration.ModuleID}): ModuleControlSource:{moduleConfiguration.ModuleControl.ControlSrc}");
            }

            return control;
        }

        [Obsolete("This implementation has moved to DotNetNuke.ModulePipeline.ModuleControlFactory. Scheduled removal in v11.0.0.")]
        public static Control LoadSettingsControl(TemplateControl containerControl, ModuleInfo moduleConfiguration, string controlSrc)
        {
            if (TracelLogger.IsDebugEnabled)
            {
                TracelLogger.Debug($"ModuleControlFactory.LoadSettingsControl Start (TabId:{moduleConfiguration.TabID},ModuleId:{moduleConfiguration.ModuleID}): ModuleControlSource:{moduleConfiguration.ModuleControl.ControlSrc}");
            }

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

            if (TracelLogger.IsDebugEnabled)
            {
                TracelLogger.Debug($"ModuleControlFactory.LoadSettingsControl End (TabId:{moduleConfiguration.TabID},ModuleId:{moduleConfiguration.ModuleID}): ModuleControlSource:{moduleConfiguration.ModuleControl.ControlSrc}");
            }

            return control;
        }

        [Obsolete("This implementation has moved to DotNetNuke.ModulePipeline.ModuleControlFactory. Scheduled removal in v11.0.0.")]
        public static Control CreateCachedControl(string cachedContent, ModuleInfo moduleConfiguration)
        {
            var moduleControl = new CachedModuleControl(cachedContent);
            moduleControl.ModuleContext.Configuration = moduleConfiguration;
            return moduleControl;
        }

        [Obsolete("This implementation has moved to DotNetNuke.ModulePipeline.ModuleControlFactory. Scheduled removal in v11.0.0.")]
        public static Control CreateModuleControl(ModuleInfo moduleConfiguration)
        {
            string extension = Path.GetExtension(moduleConfiguration.ModuleControl.ControlSrc.ToLowerInvariant());
            var moduleControl = new ModuleControlBase();
            moduleControl.ModuleContext.Configuration = moduleConfiguration;

            switch (extension)
            {
                case ".mvc":
                    var segments = moduleConfiguration.ModuleControl.ControlSrc.Replace(".mvc", string.Empty).Split('/');

                    moduleControl.LocalResourceFile = string.Format(
                        "~/DesktopModules/MVC/{0}/{1}/{2}.resx",
                        moduleConfiguration.DesktopModule.FolderName,
                        Localization.LocalResourceDirectory,
                        segments[0]);
                    break;
                default:
                    moduleControl.LocalResourceFile = moduleConfiguration.ModuleControl.ControlSrc.Replace(Path.GetFileName(moduleConfiguration.ModuleControl.ControlSrc), string.Empty) +
                                        Localization.LocalResourceDirectory + "/" +
                                        Path.GetFileName(moduleConfiguration.ModuleControl.ControlSrc);
                    break;
            }

            return moduleControl;
        }

        [Obsolete("This implementation has moved to DotNetNuke.ModulePipeline.ModuleControlFactory. Scheduled removal in v11.0.0.")]
        private static IModuleControlFactory GetModuleControlFactory(string controlSrc)
        {
            string extension = Path.GetExtension(controlSrc.ToLowerInvariant());

            IModuleControlFactory controlFactory = null;
            Type factoryType;
            switch (extension)
            {
                case ".ascx":
                    controlFactory = new WebFormsModuleControlFactory();
                    break;
                case ".html":
                case ".htm":
                    controlFactory = new Html5ModuleControlFactory();
                    break;
                case ".cshtml":
                case ".vbhtml":
                    factoryType = Reflection.CreateType("DotNetNuke.Web.Razor.RazorModuleControlFactory");
                    if (factoryType != null)
                    {
                        controlFactory = Reflection.CreateObject(factoryType) as IModuleControlFactory;
                    }

                    break;
                case ".mvc":
                    factoryType = Reflection.CreateType("DotNetNuke.Web.Mvc.MvcModuleControlFactory");
                    if (factoryType != null)
                    {
                        controlFactory = Reflection.CreateObject(factoryType) as IModuleControlFactory;
                    }

                    break;
                default:
                    controlFactory = new ReflectedModuleControlFactory();
                    break;
            }

            return controlFactory;
        }
    }
}
