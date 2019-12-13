// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
using System.IO;

namespace DotNetNuke.Web.Mvc.Framework.Modules
{
    public interface IModuleExecutionEngine
    {
        ModuleRequestResult ExecuteModule(ModuleRequestContext moduleRequestContext);

        void ExecuteModuleResult(ModuleRequestResult moduleResult, TextWriter writer);
    }
}
