// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.UI.Modules
{
    using System.Web.UI;

    /// -----------------------------------------------------------------------------
    /// Project  : DotNetNuke
    /// Namespace: DotNetNuke.UI.Modules
    /// Class    : IModuleControl
    /// -----------------------------------------------------------------------------
    /// <summary>
    /// IModuleControl provides a common Interface for Module Controls.
    /// </summary>
    /// -----------------------------------------------------------------------------
    public interface IModuleControl
    {
        Control Control { get; }

        string ControlPath { get; }

        string ControlName { get; }

        ModuleInstanceContext ModuleContext { get; }

        string LocalResourceFile { get; set; }
    }
}
