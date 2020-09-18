// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.UI.WebControls
{
    using System;
    using System.Collections;
    using System.Reflection;

    public class PropertyNameComparer : IComparer
    {
        public int Compare(object x, object y)
        {
            if (x is PropertyInfo && y is PropertyInfo)
            {
                return string.Compare(((PropertyInfo)x).Name, ((PropertyInfo)y).Name);
            }
            else
            {
                throw new ArgumentException("Object is not of type PropertyInfo");
            }
        }
    }
}
