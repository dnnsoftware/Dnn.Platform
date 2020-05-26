// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
using System.Collections.Generic;
using DotNetNuke.Entities.Modules;

namespace DotNetNuke.Web.Mvc.Common
{
    public interface IModuleControlController
    {
        ModuleControlInfo GetModuleControlByControlKey(string controlKey, int moduleDefID);
    }
}
