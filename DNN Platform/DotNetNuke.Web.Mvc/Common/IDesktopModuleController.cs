using DotNetNuke.Entities.Modules;

namespace DotNetNuke.Web.Mvc.Common
{
    public interface IDesktopModuleController
    {
        DesktopModuleInfo GetDesktopModule(int desktopModuleId, int portalId);
    }
}
