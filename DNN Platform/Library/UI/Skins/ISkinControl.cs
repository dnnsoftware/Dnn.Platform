﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
#region Usings

using DotNetNuke.UI.Modules;

#endregion

namespace DotNetNuke.UI.Skins
{
    public interface ISkinControl
    {
        IModuleControl ModuleControl { get; set; }
    }
}
