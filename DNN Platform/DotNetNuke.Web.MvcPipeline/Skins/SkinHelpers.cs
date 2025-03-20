// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.MvcPipeline.Skins
{
    using System;
    using System.Web;

    using DotNetNuke.Services.Localization;

    public static partial class SkinHelpers
    {
        public static string GetResourceFile(string templateSourceDirectory, string fileName)
        {
            return templateSourceDirectory + "/" + Localization.LocalResourceDirectory + "/" + fileName;
        }

        public static string GetSkinsResourceFile(string fileName)
        {
            return GetResourceFile("/admin/Skins", fileName);
        }
    }
}
