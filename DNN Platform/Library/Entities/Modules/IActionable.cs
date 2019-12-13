// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
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
