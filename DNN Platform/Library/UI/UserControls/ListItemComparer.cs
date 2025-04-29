// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.UI.UserControls;

using System.Collections;
using System.Web.UI.WebControls;

public class ListItemComparer : IComparer
{
    /// <inheritdoc/>
    public int Compare(object x, object y)
    {
        var a = (ListItem)x;
        var b = (ListItem)y;
        var c = new CaseInsensitiveComparer();
        return c.Compare(a.Text, b.Text);
    }
}
