using DotNetNuke.Entities.Modules;
using System;
using System.Web.UI;

namespace DotNetNuke.UI.Modules
{
    public interface IModuleControlPipeline
    {
        Control LoadModuleControl(TemplateControl containerControl, ModuleInfo moduleConfiguration, string controlKey, string controlSrc);
        Control LoadModuleControl(TemplateControl containerControl, ModuleInfo moduleConfiguration);
        Control LoadSettingsControl(TemplateControl containerControl, ModuleInfo moduleConfiguration, string controlSrc);
        Control CreateCachedControl(string cachedContent, ModuleInfo moduleConfiguration);
        Control CreateModuleControl(ModuleInfo moduleConfiguration);
    }
}
