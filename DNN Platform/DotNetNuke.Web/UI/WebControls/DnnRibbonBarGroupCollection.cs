// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Web.UI.WebControls;

using System;
using System.Web.UI;

public class DnnRibbonBarGroupCollection : ControlCollection
{
    /// <summary>Initializes a new instance of the <see cref="DnnRibbonBarGroupCollection"/> class.</summary>
    /// <param name="owner">The owner control.</param>
    public DnnRibbonBarGroupCollection(Control owner)
        : base(owner)
    {
    }

    public new DnnRibbonBarGroup this[int index]
    {
        get
        {
            return (DnnRibbonBarGroup)base[index];
        }
    }

    /// <inheritdoc/>
    public override void Add(Control child)
    {
        if (child is DnnRibbonBarGroup)
        {
            base.Add(child);
        }
        else
        {
            throw new ArgumentException("DnnRibbonBarGroupCollection must contain controls of type DnnRibbonBarGroup");
        }
    }

    /// <inheritdoc/>
    public override void AddAt(int index, Control child)
    {
        if (child is DnnRibbonBarGroup)
        {
            base.AddAt(index, child);
        }
        else
        {
            throw new ArgumentException("DnnRibbonBarGroupCollection must contain controls of type DnnRibbonBarGroup");
        }
    }
}
