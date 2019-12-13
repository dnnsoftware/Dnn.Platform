// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
#region Usings

using System;
using System.Collections;
using System.ComponentModel;
using System.Reflection;

#endregion

namespace DotNetNuke.UI.WebControls
{
    public class PropertyCategoryComparer : IComparer
    {
        #region IComparer Members

        public int Compare(object x, object y)
        {
            if (x is PropertyInfo && y is PropertyInfo)
            {
                var xProp = (PropertyInfo) x;
                var yProp = (PropertyInfo) y;
                object[] xCategory = xProp.GetCustomAttributes(typeof (CategoryAttribute), true);
                string xCategoryName = string.Empty;
                if (xCategory.Length > 0)
                {
                    xCategoryName = ((CategoryAttribute) xCategory[0]).Category;
                }
                else
                {
                    xCategoryName = CategoryAttribute.Default.Category;
                }
                object[] yCategory = yProp.GetCustomAttributes(typeof (CategoryAttribute), true);
                string yCategoryName = string.Empty;
                if (yCategory.Length > 0)
                {
                    yCategoryName = ((CategoryAttribute) yCategory[0]).Category;
                }
                else
                {
                    yCategoryName = CategoryAttribute.Default.Category;
                }
                if (xCategoryName == yCategoryName)
                {
                    return string.Compare(xProp.Name, yProp.Name);
                }
                else
                {
                    return string.Compare(xCategoryName, yCategoryName);
                }
            }
            else
            {
                throw new ArgumentException("Object is not of type PropertyInfo");
            }
        }

        #endregion
    }
}
