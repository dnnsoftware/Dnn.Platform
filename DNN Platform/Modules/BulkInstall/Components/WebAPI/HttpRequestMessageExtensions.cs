// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.Modules.BulkInstall.Components.WebAPI
{
    using System;
    using System.Linq;
    using System.Net.Http;

    /// <summary>Extension methods for <see cref="HttpRequestMessage"/>.</summary>
    internal static class HttpRequestMessageExtensions
    {
        /// <summary>Gets the API key associated with the <paramref name="request"/>.</summary>
        /// <param name="request">The request.</param>
        /// <returns>The API key or <see langword="null"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="request"/> is <see langword="null"/>.</exception>
        public static string GetApiKey(this HttpRequestMessage request)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            // Is there an api key header present?
            if (request.Headers.Contains("x-api-key"))
            {
                // Get the api key from the header.
                return request.Headers.GetValues("x-api-key").FirstOrDefault();
            }

            return null;
        }
    }
}
