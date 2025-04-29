// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DNNConnect.CKEditorProvider.Objects;

using DotNetNuke.Internal.SourceGenerators;

/// <summary>Toolbar Class.</summary>
[DnnDeprecated(7, 0, 0, "This class is phasing out please use ToolbarSet Class instead", RemovalVersion = 11)]
public partial class Toolbar
{
#pragma warning disable SA1300 // Element should begin with upper-case letter
    /// <summary>Gets or sets The Name of the Toolbar Set.</summary>
    public string sToolbarName { get; set; }

    /// <summary>Gets or sets The Hole Toolbar Set.</summary>
    public string sToolbarSet { get; set; }

    /// <summary>Gets or sets Toolbar Priority from 1-20.</summary>
    public int iPriority { get; set; }
#pragma warning restore SA1300 // Element should begin with upper-case letter
}
