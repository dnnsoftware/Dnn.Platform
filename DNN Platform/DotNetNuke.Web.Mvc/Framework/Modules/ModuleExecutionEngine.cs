using System;
using System.IO;
using DotNetNuke.Common;
using DotNetNuke.Web.Mvc.Framework.ActionResults;

namespace DotNetNuke.Web.Mvc.Framework.Modules
{
    public class ModuleExecutionEngine : IModuleExecutionEngine
    {
        public ModuleRequestResult ExecuteModule(ModuleRequestContext moduleRequestContext)
        {
            Requires.NotNull("moduleRequestContext", moduleRequestContext);

            if (moduleRequestContext.ModuleApplication != null)
            {
                //Run the module
                return moduleRequestContext.ModuleApplication.ExecuteRequest(moduleRequestContext);
            }
            return null;
        }

        public virtual void ExecuteModuleResult(ModuleRequestResult moduleResult, TextWriter writer)
        {
            var result = moduleResult.ActionResult as IDnnViewResult;
            if (result != null)
            {
                result.ExecuteResult(moduleResult.ControllerContext, writer);
            }
            else
            {
                moduleResult.ActionResult.ExecuteResult(moduleResult.ControllerContext);
            }
        }
    }
}
