#region Usings

using System;
using System.Collections.Generic;

#endregion

namespace DotNetNuke.Services.Authentication.OAuth
{
    /// <summary>
    /// Comparer class used to perform the sorting of the query parameters
    /// </summary>
    internal class QueryParameterComparer : IComparer<QueryParameter>
    {
        #region IComparer<QueryParameter> Members

        public int Compare(QueryParameter x, QueryParameter y)
        {
            if (x.Name == y.Name)
            {
                return String.CompareOrdinal(x.Value, y.Value);
            }
            return String.CompareOrdinal(x.Name, y.Name);
        }

        #endregion
    }
}
