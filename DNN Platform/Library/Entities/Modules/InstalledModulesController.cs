// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
#region Usings

using System.Collections.Generic;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Data;

#endregion

namespace DotNetNuke.Entities.Modules
{
    public class InstalledModulesController
    {
        public static List<InstalledModuleInfo> GetInstalledModules()
        {
            return CBO.FillCollection<InstalledModuleInfo>(DataProvider.Instance().GetInstalledModules());
        }
    }
}
