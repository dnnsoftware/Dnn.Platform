// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.UI.WebControls
{
    using System;
    using System.Collections;

    public class SettingNameComparer : IComparer
    {
        public int Compare(object x, object y)
        {
            if (x is SettingInfo && y is SettingInfo)
            {
                return string.Compare(((SettingInfo)x).Name, ((SettingInfo)y).Name);
            }
            else
            {
                throw new ArgumentException("Object is not of type SettingInfo");
            }
        }
    }
}
