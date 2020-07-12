// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Services.FileSystem
{
    using System;

    using DotNetNuke.ComponentModel;
    using DotNetNuke.Entities.Icons;

    internal class IconControllerWrapper : ComponentBase<IIconController, IconControllerWrapper>, IIconController
    {
        public string IconURL(string key)
        {
            return IconController.IconURL(key);
        }

        public string IconURL(string key, string size)
        {
            return IconController.IconURL(key, size);
        }
    }
}
