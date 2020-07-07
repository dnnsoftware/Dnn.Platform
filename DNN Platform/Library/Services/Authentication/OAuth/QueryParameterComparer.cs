// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.Authentication.OAuth
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Comparer class used to perform the sorting of the query parameters.
    /// </summary>
    internal class QueryParameterComparer : IComparer<QueryParameter>
    {
        public int Compare(QueryParameter x, QueryParameter y)
        {
            if (x.Name == y.Name)
            {
                return string.CompareOrdinal(x.Value, y.Value);
            }

            return string.CompareOrdinal(x.Name, y.Name);
        }
    }
}
