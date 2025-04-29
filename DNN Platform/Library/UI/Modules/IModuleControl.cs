// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.UI.Modules;

using System.Web.UI;

/// <summary>IModuleControl provides a common Interface for Module Controls.</summary>
public interface IModuleControl
{
    /// <summary>Gets the control.</summary>
    Control Control { get; }

    /// <summary>Gets the control path.</summary>
    string ControlPath { get; }

    /// <summary>Gets the control name.</summary>
    string ControlName { get; }

    /// <summary>Gets the module context.</summary>
    ModuleInstanceContext ModuleContext { get; }

    /// <summary>Gets or sets the local resource localization file for the control.</summary>
    string LocalResourceFile { get; set; }
}
