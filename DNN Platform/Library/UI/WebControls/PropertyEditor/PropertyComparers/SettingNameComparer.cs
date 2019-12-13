// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
#region Usings

using System;
using System.Collections;

#endregion

namespace DotNetNuke.UI.WebControls
{
    public class SettingNameComparer : IComparer
    {
        #region IComparer Members

        public int Compare(object x, object y)
        {
            if (x is SettingInfo && y is SettingInfo)
            {
                return string.Compare(((SettingInfo) x).Name, ((SettingInfo) y).Name);
            }
            else
            {
                throw new ArgumentException("Object is not of type SettingInfo");
            }
        }

        #endregion
    }
}
