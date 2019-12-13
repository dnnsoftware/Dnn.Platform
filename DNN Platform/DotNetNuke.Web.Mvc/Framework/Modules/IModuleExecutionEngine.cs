using System.IO;

namespace DotNetNuke.Web.Mvc.Framework.Modules
{
    public interface IModuleExecutionEngine
    {
        ModuleRequestResult ExecuteModule(ModuleRequestContext moduleRequestContext);

        void ExecuteModuleResult(ModuleRequestResult moduleResult, TextWriter writer);
    }
}
