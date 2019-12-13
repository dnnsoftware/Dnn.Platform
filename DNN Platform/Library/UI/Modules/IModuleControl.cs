// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
#region Usings

using System.Web.UI;

#endregion

namespace DotNetNuke.UI.Modules
{
    /// -----------------------------------------------------------------------------
    /// Project	 : DotNetNuke
    /// Namespace: DotNetNuke.UI.Modules
    /// Class	 : IModuleControl
    /// -----------------------------------------------------------------------------
    /// <summary>
    /// IModuleControl provides a common Interface for Module Controls
    /// </summary>
    /// -----------------------------------------------------------------------------
    public interface IModuleControl
    {
        Control Control { get; }
        string ControlPath { get; }
        string ControlName { get; }
        string LocalResourceFile { get; set; }
        ModuleInstanceContext ModuleContext { get; }
    }
}
