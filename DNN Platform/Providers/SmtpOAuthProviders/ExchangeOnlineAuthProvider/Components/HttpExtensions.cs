// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.ExchangeOnlineAuthProvider.Components
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Http;
    using System.Text;
    using System.Threading.Tasks;
    using System.Web;

    /// <summary>
    /// Http extensions class.
    /// </summary>
    public static class HttpExtensions
    {
        /// <summary>
        /// Returns an individual querystring value as integer.
        /// </summary>
        /// <param name="request">The http request.</param>
        /// <param name="key">The query string name.</param>
        /// <returns>The integer value of the query string.</returns>
        public static int GetQueryStringAsInteger(this HttpRequest request, string key)
        {
            // IEnumerable<KeyValuePair<string,string>> - right!
            var value = request[key];
            int intValue;
            if (!string.IsNullOrEmpty(value) && int.TryParse(value, out intValue))
            {
                return intValue;
            }

            return -1;
        }
    }
}
