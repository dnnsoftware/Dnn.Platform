// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.UI.Containers
{
    using DotNetNuke.Entities.Modules.Actions;
    using DotNetNuke.UI.Modules;

    /// -----------------------------------------------------------------------------
    /// Project  : DotNetNuke
    /// Namespace: DotNetNuke.UI.Containers
    /// Class    : IActionControl
    /// -----------------------------------------------------------------------------
    /// <summary>
    /// IActionControl provides a common INterface for Action Controls.
    /// </summary>
    /// -----------------------------------------------------------------------------
    public interface IActionControl
    {
        event ActionEventHandler Action;

        ActionManager ActionManager { get; }

        IModuleControl ModuleControl { get; set; }
    }
}
