using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotNetNuke.Entities.Modules.Settings
{
    public interface ISettingsRepository<T> where T : class
    {
        T GetSettings(ModuleInfo moduleContext);
        void SaveSettings(ModuleInfo moduleContext, T settings);
    }
}
