// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.EditBar.UI.Helpers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    using DotNetNuke.Services.Localization;

    public static class LocalizationHelper
    {
        private const string ResourcesFile = "~/DesktopModules/admin/Dnn.EditBar/App_LocalResources/EditBar.resx";

        public static string GetString(string key, string resourcesFile = ResourcesFile)
        {
            return Localization.GetString(key, resourcesFile);
        }
    }
}
