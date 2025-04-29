// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DNNConnect.CKEditorProvider.Objects;

using System.Collections.Generic;

/// <summary>Toolbar Group Class.</summary>
public class ToolbarGroup
{
    /// <summary>Initializes a new instance of the <see cref="ToolbarGroup" /> class.</summary>
    public ToolbarGroup()
    {
        this.items = new List<string>();
    }

#pragma warning disable SA1300 // Element should begin with upper-case letter
    /// <summary>Gets or sets the toolbar buttons.</summary>
    /// <value>
    /// The toolbar buttons.
    /// </value>
    public List<string> items { get; set; }

    /// <summary>Gets or sets the name of the group.</summary>
    /// <value>
    /// The name of the group.
    /// </value>
    public string name { get; set; }
#pragma warning restore SA1300 // Element should begin with upper-case letter
}
