// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Modules.DigitalAssets.Components.ExtensionPoint
{
    using System;

    using DotNetNuke.Services.Localization;

    public class LocalizationHelper
    {
        private const string ResourceFile = "DesktopModules/DigitalAssets/App_LocalResources/SharedResources";

        public static string GetString(string key)
        {
            return Localization.GetString(key, ResourceFile);
        }
    }
}
