// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
using System.IO;

namespace Dnn.PersonaBar.Pages.Components
{
    public class Localization
    {
        private static string LocalResourcesFile => Path.Combine(Library.Constants.PersonaBarRelativePath, "Modules/Dnn.Pages/App_LocalResources/Pages.resx");

        public static string GetString(string key)
        {
            return DotNetNuke.Services.Localization.Localization.GetString(key, LocalResourcesFile);
        }
    }
}
