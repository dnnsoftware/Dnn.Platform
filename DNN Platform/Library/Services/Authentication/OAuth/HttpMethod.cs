// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Services.Authentication.OAuth
{
    public enum HttpMethod
    {
        /// <summary><see cref="System.Net.Http.HttpMethod.Get"/>.</summary>
        GET = 0,

        /// <summary><see cref="System.Net.Http.HttpMethod.Post"/>.</summary>
        POST = 1,

        /// <summary><see cref="System.Net.Http.HttpMethod.Put"/>.</summary>
        PUT = 2,

        /// <summary><see cref="System.Net.Http.HttpMethod.Delete"/>.</summary>
        DELETE = 3,
    }
}
