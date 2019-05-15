using DotNetNuke.Entities.Modules;
using DotNetNuke.Services.Localization;
using System.IO;
using System.Web.UI;

namespace DotNetNuke.UI.Modules
{
    public abstract class BaseModuleControlFactory : IModuleControlFactory
    {
        public abstract Control CreateControl(TemplateControl containerControl, string controlKey, string controlSrc);

        public abstract Control CreateModuleControl(TemplateControl containerControl, ModuleInfo moduleConfiguration);

        public virtual ModuleControlBase CreateModuleControl(ModuleInfo moduleConfiguration)
        {
            var moduleControl = new ModuleControlBase();
            moduleControl.ModuleContext.Configuration = moduleConfiguration;

            moduleControl.LocalResourceFile = moduleConfiguration.ModuleControl.ControlSrc.Replace(Path.GetFileName(moduleConfiguration.ModuleControl.ControlSrc), "") +
                                       Localization.LocalResourceDirectory + "/" +
                                       Path.GetFileName(moduleConfiguration.ModuleControl.ControlSrc);

            return moduleControl;
        }

        public abstract Control CreateSettingsControl(TemplateControl containerControl, ModuleInfo moduleConfiguration, string controlSrc);
    }
}
