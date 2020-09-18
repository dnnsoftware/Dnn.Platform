// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.Mvc.Framework.Modules
{
    using System.IO;

    public interface IModuleExecutionEngine
    {
        ModuleRequestResult ExecuteModule(ModuleRequestContext moduleRequestContext);

        void ExecuteModuleResult(ModuleRequestResult moduleResult, TextWriter writer);
    }
}
