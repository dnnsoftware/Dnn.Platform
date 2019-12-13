#region Usings

using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Portals;

#endregion

namespace DotNetNuke.UI.Modules
{
    public interface IModuleInjectionFilter
    {
        bool CanInjectModule(ModuleInfo module, PortalSettings portalSettings);
    }
}
