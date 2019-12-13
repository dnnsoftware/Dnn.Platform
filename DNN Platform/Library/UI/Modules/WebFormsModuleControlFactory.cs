using System;
using System.Web.UI;

using DotNetNuke.Entities.Modules;

namespace DotNetNuke.UI.Modules
{
    public class WebFormsModuleControlFactory : BaseModuleControlFactory
    {
        public override Control CreateControl(TemplateControl containerControl, string controlKey, string controlSrc)
        {
            return ControlUtilities.LoadControl<Control>(containerControl, controlSrc);
        }

        public override Control CreateModuleControl(TemplateControl containerControl, ModuleInfo moduleConfiguration)
        {
            return CreateControl(containerControl, String.Empty, moduleConfiguration.ModuleControl.ControlSrc);
        }

        public override Control CreateSettingsControl(TemplateControl containerControl, ModuleInfo moduleConfiguration, string controlSrc)
        {
            return CreateControl(containerControl, String.Empty, controlSrc);
        }
    }
}
