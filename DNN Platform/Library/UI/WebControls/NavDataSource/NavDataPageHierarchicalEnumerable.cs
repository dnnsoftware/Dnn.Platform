// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.UI.WebControls;

using System.Collections;
using System.Web.UI;

/// <summary>A collection of PageHierarchyData objects.</summary>
public class NavDataPageHierarchicalEnumerable : ArrayList, IHierarchicalEnumerable
{
    /// <summary>Handles enumeration.</summary>
    /// <param name="enumeratedItem">THe <see cref="IHierarchyData"/> item.</param>
    /// <returns><paramref name="enumeratedItem"/>.</returns>
    public virtual IHierarchyData GetHierarchyData(object enumeratedItem)
    {
        return (IHierarchyData)enumeratedItem;
    }
}
