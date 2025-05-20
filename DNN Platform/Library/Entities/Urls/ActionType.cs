// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Entities.Urls
{
    using System.Net;

    public enum ActionType
    {
        /// <summary>Do nothing to the request.</summary>
        IgnoreRequest = 0,

        /// <summary>Allow the request to continue.</summary>
        Continue = 1,

        /// <summary>Do an immediate <see cref="HttpStatusCode.Found"/> redirect.</summary>
        Redirect302Now = 2,

        /// <summary>Plan to do a <see cref="HttpStatusCode.MovedPermanently"/> redirect.</summary>
        Redirect301 = 3,

        /// <summary>Check to see is a <see cref="HttpStatusCode.MovedPermanently"/> redirect should be done.</summary>
        CheckFor301 = 4,

        /// <summary>Plan to do a <see cref="HttpStatusCode.Found"/> redirect.</summary>
        Redirect302 = 5,

        /// <summary>Response with a <see cref="HttpStatusCode.NotFound"/> response.</summary>
        Output404 = 6,

        /// <summary>Response with a <see cref="HttpStatusCode.InternalServerError"/> response.</summary>
        Output500 = 7,
    }
}
