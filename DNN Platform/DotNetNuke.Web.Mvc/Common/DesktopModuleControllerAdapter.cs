using System;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Framework;

namespace DotNetNuke.Web.Mvc.Common
{
    public class DesktopModuleControllerAdapter : ServiceLocator<IDesktopModuleController, DesktopModuleControllerAdapter>, IDesktopModuleController
    {
        protected override Func<IDesktopModuleController> GetFactory()
        {
            return () => new DesktopModuleControllerAdapter();
        }

        public DesktopModuleInfo GetDesktopModule(int desktopModuleId, int portalId)
        {
            return Entities.Modules.DesktopModuleController.GetDesktopModule(desktopModuleId, portalId);
        }
    }
}
