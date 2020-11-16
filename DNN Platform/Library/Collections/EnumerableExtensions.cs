// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Collections
{
    using System.Collections.Generic;
    using System.Data;
    using System.Linq;

    /// <summary>
    /// Provides extensions to IEnumerable.
    /// </summary>
    public static class EnumerableExtensions
    {
        /// <summary>
        /// Converts dynamic data to a DataTable. Useful for databinding.
        /// </summary>
        /// <param name="items">The items in the IEnumerable.</param>
        /// <returns>A DataTable with the copied dynamic data.</returns>
        public static DataTable ToDataTable(this IEnumerable<dynamic> items)
        {
            var data = items.ToArray();

            if (!data.Any())
            {
                return null;
            }

            var dt = new DataTable();

            // Create the columns
            foreach (var key in ((IDictionary<string, object>)data[0]).Keys)
            {
                dt.Columns.Add(key);
            }

            // Add data rows to the datatable
            foreach (var d in data)
            {
                dt.Rows.Add(((IDictionary<string, object>)d).Values.ToArray());
            }

            return dt;
        }
    }
}
