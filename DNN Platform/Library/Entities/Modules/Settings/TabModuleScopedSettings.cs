using System.Collections;

namespace DotNetNuke.Entities.Modules.Settings
{
    public class TabModuleScopedSettings : StringBasedSettings
    {
        public TabModuleScopedSettings(int tabModuleId, Hashtable tabModuleSettings)
            : base(
                name => tabModuleSettings[name] as string,
                (name, value) => new ModuleController().UpdateTabModuleSetting(tabModuleId, name, value)
                ) { }
    }
}
