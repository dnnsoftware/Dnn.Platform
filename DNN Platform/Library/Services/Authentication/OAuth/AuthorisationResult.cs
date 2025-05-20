// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Services.Authentication.OAuth
{
    public enum AuthorisationResult
    {
        /// <summary>Authorization denied.</summary>
        Denied = 0,

        /// <summary>Authorization succeeded.</summary>
        Authorized = 1,

        /// <summary>Need a code to complete authorization.</summary>
        RequestingCode = 2,
    }
}
