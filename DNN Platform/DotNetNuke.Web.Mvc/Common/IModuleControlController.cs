// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

using System.Collections.Generic;
using DotNetNuke.Entities.Modules;

namespace DotNetNuke.Web.Mvc.Common
{
    public interface IModuleControlController
    {
        ModuleControlInfo GetModuleControlByControlKey(string controlKey, int moduleDefID);
    }
}
