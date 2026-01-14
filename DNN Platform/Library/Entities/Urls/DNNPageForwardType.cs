// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Entities.Urls
{
    using System.Net;

    public enum DNNPageForwardType
    {
        /// <summary>Do not forward.</summary>
        NoForward = 0,

        /// <summary>Respond with <see cref="HttpStatusCode.Found"/>.</summary>
        Redirect302 = 1,

        /// <summary>Respond with <see cref="HttpStatusCode.MovedPermanently"/>.</summary>
        Redirect301 = 2,
    }
}
