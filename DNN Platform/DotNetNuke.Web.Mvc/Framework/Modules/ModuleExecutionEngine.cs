// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.Mvc.Framework.Modules
{
    using System;
    using System.IO;

    using DotNetNuke.Common;
    using DotNetNuke.Web.Mvc.Framework.ActionResults;

    public class ModuleExecutionEngine : IModuleExecutionEngine
    {
        public ModuleRequestResult ExecuteModule(ModuleRequestContext moduleRequestContext)
        {
            Requires.NotNull("moduleRequestContext", moduleRequestContext);

            if (moduleRequestContext.ModuleApplication != null)
            {
                // Run the module
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
