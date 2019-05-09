using System;
using System.IO;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Instrumentation;
using DotNetNuke.Services.Localization;
using DotNetNuke.UI.Modules.Html5;
using DotNetNuke.UI.Modules;
using DotNetNuke.Web.Razor;
using System.Collections.Generic;
using DotNetNuke.Web.Mvc.Contracts;

#if NET472
using System.Web.UI;
#endif

namespace DotNetNuke.ModulePipeline
{

    public class ModuleControlFactory
#if NET472
        : IModuleControlPipeline
#endif
    {
        private static readonly ILog TracelLogger = LoggerSource.Instance.GetLogger("DNN.Trace");
        private Dictionary<string, IModuleControlFactory> _controlFactories;
        public ModuleControlFactory(
            IWebFormsModuleControlFactory webforms,
            IHtml5ModuleControlFactory html5,
            IRazorModuleControlFactory razor3,
            IMvcModuleControlFactory mvc,
            IReflectedModuleControlFactory fallthrough)
        {
            _controlFactories = new Dictionary<string, IModuleControlFactory>();
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
            string extension = Path.GetExtension(controlSrc.ToLowerInvariant());
            IModuleControlFactory factory = _controlFactories[extension];

            return factory ?? _controlFactories["default"];
        }

        public Control LoadModuleControl(TemplateControl containerControl, ModuleInfo moduleConfiguration, string controlKey, string controlSrc)
        {
            if (TracelLogger.IsDebugEnabled)
                TracelLogger.Debug($"ModuleControlFactory.LoadModuleControl Start (TabId:{moduleConfiguration.TabID},ModuleId:{moduleConfiguration.ModuleID}): ModuleControlSource:{moduleConfiguration.ModuleControl.ControlSrc}");

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
                TracelLogger.Debug($"ModuleControlFactory.LoadModuleControl End (TabId:{moduleConfiguration.TabID},ModuleId:{moduleConfiguration.ModuleID}): ModuleControlSource:{moduleConfiguration.ModuleControl.ControlSrc}");
            return control;
        }

        public Control LoadModuleControl(TemplateControl containerControl, ModuleInfo moduleConfiguration)
        {
            if (TracelLogger.IsDebugEnabled)
                TracelLogger.Debug($"ModuleControlFactory.LoadModuleControl Start (TabId:{moduleConfiguration.TabID},ModuleId:{moduleConfiguration.ModuleID}): ModuleControlSource:{moduleConfiguration.ModuleControl.ControlSrc}");
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
                TracelLogger.Debug($"ModuleControlFactory.LoadModuleControl End (TabId:{moduleConfiguration.TabID},ModuleId:{moduleConfiguration.ModuleID}): ModuleControlSource:{moduleConfiguration.ModuleControl.ControlSrc}");
            return control;
        }

        public Control LoadSettingsControl(TemplateControl containerControl, ModuleInfo moduleConfiguration, string controlSrc)
        {
            if (TracelLogger.IsDebugEnabled)
                TracelLogger.Debug($"ModuleControlFactory.LoadSettingsControl Start (TabId:{moduleConfiguration.TabID},ModuleId:{moduleConfiguration.ModuleID}): ModuleControlSource:{moduleConfiguration.ModuleControl.ControlSrc}");

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
                TracelLogger.Debug($"ModuleControlFactory.LoadSettingsControl End (TabId:{moduleConfiguration.TabID},ModuleId:{moduleConfiguration.ModuleID}): ModuleControlSource:{moduleConfiguration.ModuleControl.ControlSrc}");
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
            string extension = Path.GetExtension(moduleConfiguration.ModuleControl.ControlSrc.ToLowerInvariant());
            var moduleControl = new ModuleControlBase();
            moduleControl.ModuleContext.Configuration = moduleConfiguration;

            switch (extension)
            {
                case ".mvc":
                    var segments = moduleConfiguration.ModuleControl.ControlSrc.Replace(".mvc", "").Split('/');

                    moduleControl.LocalResourceFile = String.Format("~/DesktopModules/MVC/{0}/{1}/{2}.resx",
                                        moduleConfiguration.DesktopModule.FolderName,
                                        Localization.LocalResourceDirectory,
                                        segments[0]);
                    break;
                default:
                    moduleControl.LocalResourceFile = moduleConfiguration.ModuleControl.ControlSrc.Replace(Path.GetFileName(moduleConfiguration.ModuleControl.ControlSrc), "") +
                                        Localization.LocalResourceDirectory + "/" +
                                        Path.GetFileName(moduleConfiguration.ModuleControl.ControlSrc);
                    break;
            }
            return moduleControl;
        }
#endif
    }
}
