// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
/* This code file contains helper classes used for Url Rewriting / Friendly Url generation */

namespace DotNetNuke.Entities.Urls
{
    using System.Collections.Generic;

    /// <summary>
    /// The StringLengthComparer class is a comparer override used for sorting portal aliases by length.
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

            return -1; // should never reach here
        }
    }
}
