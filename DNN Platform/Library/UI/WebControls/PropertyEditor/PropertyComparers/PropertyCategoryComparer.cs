#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2018
// by DotNetNuke Corporation
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated 
// documentation files (the "Software"), to deal in the Software without restriction, including without limitation 
// the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and 
// to permit persons to whom the Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all copies or substantial portions 
// of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED 
// TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL 
// THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF 
// CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
// DEALINGS IN THE SOFTWARE.
#endregion
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