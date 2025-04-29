// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DNNConnect.CKEditorProvider.Objects;

using System.Collections.Generic;

/// <summary>Toolbar Set Class.</summary>
public class ToolbarSet
{
    /// <summary>Initializes a new instance of the <see cref="ToolbarSet" /> class.</summary>
    public ToolbarSet()
    {
        this.ToolbarGroups = new List<ToolbarGroup>();
    }

    /// <summary>Initializes a new instance of the <see cref="ToolbarSet" /> class.</summary>
    /// <param name="name">The name.</param>
    public ToolbarSet(string name)
    {
        this.Name = name;
        this.ToolbarGroups = new List<ToolbarGroup>();
    }

    /// <summary>Initializes a new instance of the <see cref="ToolbarSet" /> class.</summary>
    /// <param name="name">The name.</param>
    /// <param name="priority">The priority.</param>
    public ToolbarSet(string name, int priority)
    {
        this.Name = name;
        this.Priority = priority;
        this.ToolbarGroups = new List<ToolbarGroup>();
    }

    /// <summary>Gets or sets The Name of the Toolbar Set.</summary>
    public string Name { get; set; }

    /// <summary>Gets or sets Toolbar Priority from 1-20.</summary>
    public int Priority { get; set; }

    /// <summary>Gets or sets the toolbar group.</summary>
    /// <value>
    /// The toolbar group.
    /// </value>
    public List<ToolbarGroup> ToolbarGroups { get; set; }
}
