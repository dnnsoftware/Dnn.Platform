// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Services.UserRequest
{
    using System.Web;

    public interface IUserRequestIPAddressController
    {
        /// <summary>
        ///  To retrieve IPv4 of user making request to application.
        /// </summary>
        /// <param name="request"></param>
        /// <returns>IP address.</returns>
        string GetUserRequestIPAddress(HttpRequestBase request);

        /// <summary>
        ///  To retrieve IPv4/IPv6 of user making request to application.
        /// </summary>
        /// <param name="request"></param>
        /// <param name="ipFamily"></param>
        /// <returns>IP address.</returns>
        string GetUserRequestIPAddress(HttpRequestBase request, IPAddressFamily ipFamily);
    }
}
