﻿// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
#region Usings

using System.Collections.Generic;

#endregion

/* This code file contains helper classes used for Url Rewriting / Friendly Url generation */

namespace DotNetNuke.Entities.Urls
{
    /// <summary>
    /// The StringLengthComparer class is a comparer override used for sorting portal aliases by length
    /// </summary>
    internal class StringLengthComparer : Comparer<string>
    {
        public override int Compare(string x, string y)
        {
            if (x == null && y == null)
            {
                return 0;
            }
            if (x == null)
            {
                return -1;
            }
            if (y == null)
            {
                return 1;
            }
            if (x.Length < y.Length)
            {
                return -1;
            }
            if (x.Length > y.Length)
            {
                return 1;
            }
            if (x.Length == y.Length)
            {
                return 0;
            }
            return -1; //should never reach here
        }
    }
}
