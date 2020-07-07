// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.UI.WebControls
{
    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.Reflection;

    public class PropertyCategoryComparer : IComparer
    {
        public int Compare(object x, object y)
        {
            if (x is PropertyInfo && y is PropertyInfo)
            {
                var xProp = (PropertyInfo)x;
                var yProp = (PropertyInfo)y;
                object[] xCategory = xProp.GetCustomAttributes(typeof(CategoryAttribute), true);
                string xCategoryName = string.Empty;
                if (xCategory.Length > 0)
                {
                    xCategoryName = ((CategoryAttribute)xCategory[0]).Category;
                }
                else
                {
                    xCategoryName = CategoryAttribute.Default.Category;
                }

                object[] yCategory = yProp.GetCustomAttributes(typeof(CategoryAttribute), true);
                string yCategoryName = string.Empty;
                if (yCategory.Length > 0)
                {
                    yCategoryName = ((CategoryAttribute)yCategory[0]).Category;
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
    }
}
