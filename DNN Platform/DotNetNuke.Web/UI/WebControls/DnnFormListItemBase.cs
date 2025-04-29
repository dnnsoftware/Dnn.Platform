// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Web.UI.WebControls;

using System.Collections;

public abstract class DnnFormListItemBase : DnnFormItemBase
{
    private IEnumerable listSource;

    public string DefaultValue { get; set; }

    public IEnumerable ListSource
    {
        get
        {
            return this.listSource;
        }

        set
        {
            var changed = !Equals(this.listSource, value);
            if (changed)
            {
                this.listSource = value;
                this.BindList();
            }
        }
    }

    public string ListTextField { get; set; }

    public string ListValueField { get; set; }

    protected virtual void BindList()
    {
    }
}
