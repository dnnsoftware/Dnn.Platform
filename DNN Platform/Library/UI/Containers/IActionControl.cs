// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
#region Usings

using DotNetNuke.Entities.Modules.Actions;
using DotNetNuke.UI.Modules;

#endregion

namespace DotNetNuke.UI.Containers
{
    /// -----------------------------------------------------------------------------
    /// Project	 : DotNetNuke
    /// Namespace: DotNetNuke.UI.Containers
    /// Class	 : IActionControl
    /// -----------------------------------------------------------------------------
    /// <summary>
    /// IActionControl provides a common INterface for Action Controls
    /// </summary>
    /// -----------------------------------------------------------------------------
    public interface IActionControl
    {
        ActionManager ActionManager { get; }
        IModuleControl ModuleControl { get; set; }
        event ActionEventHandler Action;
    }
}
