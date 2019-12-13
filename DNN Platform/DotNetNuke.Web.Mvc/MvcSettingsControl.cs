using DotNetNuke.Entities.Modules;
using DotNetNuke.UI.Modules;

namespace DotNetNuke.Web.Mvc
{
    public class MvcSettingsControl : MvcHostControl, ISettingsControl
    {
        public MvcSettingsControl() : base("Settings")
        {
            ExecuteModuleImmediately = false;
        }

        public void LoadSettings()
        {
            ExecuteModule();
        }

        public void UpdateSettings()
        {
            ExecuteModule();

            ModuleController.Instance.UpdateModule(ModuleContext.Configuration);
        }
    }
}
