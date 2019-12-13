// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
using DotNetNuke.Services.Localization;

namespace DotNetNuke.Web.Components
{
    public class LocalizationHelper
    {
        private const string ResourceFile = "admin/ControlPanel/App_LocalResources/ControlBar";

        public static string GetControlBarString(string key)
        {
            return Localization.GetString(key, ResourceFile);
        }
    }
}
