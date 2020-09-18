// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.UI.WebControls
{
    using System;
    using System.Collections;
    using System.Reflection;

    public class PropertySortOrderComparer : IComparer
    {
        public int Compare(object x, object y)
        {
            if (x is PropertyInfo && y is PropertyInfo)
            {
                var xProp = (PropertyInfo)x;
                var yProp = (PropertyInfo)y;
                object[] xSortOrder = xProp.GetCustomAttributes(typeof(SortOrderAttribute), true);
                int xSortOrderValue;
                if (xSortOrder.Length > 0)
                {
                    xSortOrderValue = ((SortOrderAttribute)xSortOrder[0]).Order;
                }
                else
                {
                    xSortOrderValue = SortOrderAttribute.DefaultOrder;
                }

                object[] ySortOrder = yProp.GetCustomAttributes(typeof(SortOrderAttribute), true);
                int ySortOrderValue;
                if (ySortOrder.Length > 0)
                {
                    ySortOrderValue = ((SortOrderAttribute)ySortOrder[0]).Order;
                }
                else
                {
                    ySortOrderValue = SortOrderAttribute.DefaultOrder;
                }

                if (xSortOrderValue == ySortOrderValue)
                {
                    return string.Compare(xProp.Name, yProp.Name);
                }
                else
                {
                    return xSortOrderValue - ySortOrderValue;
                }
            }
            else
            {
                throw new ArgumentException("Object is not of type PropertyInfo");
            }
        }
    }
}
