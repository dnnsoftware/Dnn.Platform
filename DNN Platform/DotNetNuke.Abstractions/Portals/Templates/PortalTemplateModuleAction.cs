// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Abstractions.Portals.Templates;

/// <summary>Specify what should happen during the application of a portal template when modules collide with existing modules.</summary>
public enum PortalTemplateModuleAction
{
    /// <summary>Ignore.</summary>
    Ignore = 0,

    /// <summary>Add the module to the page next to the old module.</summary>
    Merge = 1,

    /// <summary>Replace the old module.</summary>
    Replace = 2,
}
