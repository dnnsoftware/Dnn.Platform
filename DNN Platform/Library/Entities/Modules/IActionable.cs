﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
#region Usings

using DotNetNuke.Entities.Modules.Actions;

#endregion

namespace DotNetNuke.Entities.Modules
{
    public interface IActionable
    {
        ModuleActionCollection ModuleActions { get; }
    }
}
