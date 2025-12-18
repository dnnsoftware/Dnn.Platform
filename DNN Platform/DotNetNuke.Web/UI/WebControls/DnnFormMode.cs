// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.UI.WebControls;

using System.Diagnostics.CodeAnalysis;

public enum DnnFormMode
{
    /// <summary>Inherit.</summary>
    Inherit = 0,

    /// <summary>Short.</summary>
    [SuppressMessage("Microsoft.Naming", "CA1720:IdentifiersShouldNotContainTypeNames", Justification = "Breaking change")]
    Short = 1,

    /// <summary>Long.</summary>
    [SuppressMessage("Microsoft.Naming", "CA1720:IdentifiersShouldNotContainTypeNames", Justification = "Breaking change")]
    Long = 2,
}
