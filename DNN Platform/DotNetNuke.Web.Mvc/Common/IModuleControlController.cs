using System.Collections.Generic;
using DotNetNuke.Entities.Modules;

namespace DotNetNuke.Web.Mvc.Common
{
    public interface IModuleControlController
    {
        ModuleControlInfo GetModuleControlByControlKey(string controlKey, int moduleDefID);
    }
}
