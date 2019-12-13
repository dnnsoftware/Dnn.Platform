// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace DotNetNuke.Collections
{
    /// <summary>
    /// Provides extensions to IEnumerable
    /// </summary>
    public static class EnumerableExtensions
    {
        /// <summary>
        /// Converts dynamic data to a DataTable. Useful for databinding.
        /// </summary>
        /// <param name="items">The items in the IEnumerable</param>
        /// <returns>A DataTable with the copied dynamic data.</returns>
        public static DataTable ToDataTable(this IEnumerable<dynamic> items)
        {
            var data = items.ToArray();

            if (!data.Any()) 
                return null;

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
