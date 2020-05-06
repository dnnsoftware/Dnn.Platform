// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

using DotNetNuke.Services.Localization;
using Dnn.Modules.ResourceManager.Components;

namespace Dnn.Modules.ResourceManager.Helpers
{
    internal class LocalizationHelper
    {
        private const string ResourceFile = "~/" + Constants.ModulePath + "/App_LocalResources/ResourceManager.resx";

        public static string GetString(string key)
        {
            return Localization.GetString(key, ResourceFile);
        }
    }
}