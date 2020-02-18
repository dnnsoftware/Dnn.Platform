// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotNetNuke.Services.Localization;

namespace Dnn.EditBar.UI.Helpers
{
    public static class LocalizationHelper
    {
        private const string ResourcesFile = "~/DesktopModules/admin/Dnn.EditBar/App_LocalResources/EditBar.resx";

        public static string GetString(string key, string resourcesFile = ResourcesFile)
        {
            return Localization.GetString(key, resourcesFile);
        }
    }
}
