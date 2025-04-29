// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.UI.WebControls;

using System;
using System.Web.UI;

/// <summary>Restricts the client to add only controls of specific type into the control collection.</summary>
/// <typeparam name="T">The type of control.</typeparam>
public sealed class TypedControlCollection<T> : ControlCollection
    where T : Control
{
    /// <summary>Initializes a new instance of the <see cref="TypedControlCollection{T}"/> class.</summary>
    /// <param name="owner">The owner control.</param>
    public TypedControlCollection(Control owner)
        : base(owner)
    {
    }

    /// <inheritdoc/>
    public override void Add(Control child)
    {
        if (!(child is T))
        {
            throw new InvalidOperationException("Not supported");
        }

        base.Add(child);
    }

    /// <inheritdoc/>
    public override void AddAt(int index, Control child)
    {
        if (!(child is T))
        {
            throw new InvalidOperationException("Not supported");
        }

        base.AddAt(index, child);
    }
}
