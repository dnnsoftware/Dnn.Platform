// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.Api
{
    /// <summary>
    /// Enumeration that contains HTTP Status Codes that are not included in the HttpStatusCode enumeration provided by the .NET framework.
    /// </summary>
    public enum HttpStatusCodeAdditions
    {
        /// <summary>
        /// Equivalent to HTTP Status 422. Unprocessable Entity status code means the server understands the content type of the request entity (hence a
        /// 415 (Unsupported Media Type) status code is inappropriate), and the syntax of the request entity is correct (thus a 400 (Bad Request)
        /// status code is inappropriate) but was unable to process the contained instructions.
        /// <see>
        ///     <cref>http://tools.ietf.org/html/rfc4918</cref>
        /// </see>
        /// </summary>
        UnprocessableEntity = 422,
    }
}
