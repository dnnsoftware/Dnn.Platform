// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
#region Usings

using System.Collections;
using System.Globalization;

#endregion

namespace DotNetNuke.Services.Localization
{
    public class CultureInfoComparer : IComparer
    {
        private readonly string _compare;

        public CultureInfoComparer(string compareBy)
        {
            _compare = compareBy;
        }

        #region IComparer Members

        public int Compare(object x, object y)
        {
            switch (_compare.ToUpperInvariant())
            {
                case "ENGLISH":
                    return ((CultureInfo) x).EnglishName.CompareTo(((CultureInfo) y).EnglishName);
                case "NATIVE":
                    return ((CultureInfo) x).NativeName.CompareTo(((CultureInfo) y).NativeName);
                default:
                    return ((CultureInfo) x).Name.CompareTo(((CultureInfo) y).Name);
            }
        }

        #endregion
    }
}
