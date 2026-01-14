// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Entities.Urls
{
    public enum DeletedTabHandlingType
    {
        /// <summary>Respond with a permanent redirect to the home URL.</summary>
        Do301RedirectToPortalHome = 0,

        /// <summary>Respond with a 404 error response.</summary>
        Do404Error = 1,
    }
}
