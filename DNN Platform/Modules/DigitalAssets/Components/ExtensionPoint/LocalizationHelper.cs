// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
using System;

using DotNetNuke.Services.Localization;

namespace DotNetNuke.Modules.DigitalAssets.Components.ExtensionPoint
{
    public class LocalizationHelper
    {
        private const string ResourceFile = "DesktopModules/DigitalAssets/App_LocalResources/SharedResources";

        public static string GetString(string key)
        {
            return Localization.GetString(key, ResourceFile);
        }
    }
}
