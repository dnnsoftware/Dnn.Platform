using System.Collections;

namespace DotNetNuke.Entities.Modules.Settings
{
    public class ModuleScopedSettings : StringBasedSettings
    {
        public ModuleScopedSettings(int moduleId, Hashtable moduleSettings)
            : base(
                name => moduleSettings[name] as string,
                (name, value) => new ModuleController().UpdateModuleSetting(moduleId, name, value)
                ) { }
    }
}
