// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Services.UserRequest;

using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Web;

using DotNetNuke.Abstractions.Application;
using DotNetNuke.Common;
using DotNetNuke.Entities.Controllers;
using DotNetNuke.Framework;

/// <summary>Utilities to handle IP address of user making request to application.</summary>
public class UserRequestIPAddressController : ServiceLocator<IUserRequestIPAddressController, UserRequestIPAddressController>, IUserRequestIPAddressController
{
    /// <inheritdoc/>
    public string GetUserRequestIPAddress(HttpRequestBase request)
    {
        return this.GetUserRequestIPAddress(request, IPAddressFamily.IPv4);
    }

    /// <inheritdoc/>
    public string GetUserRequestIPAddress(HttpRequestBase request, IPAddressFamily ipFamily)
    {
        var userRequestIPHeader = HostController.Instance.GetString("UserRequestIPHeader");
        var userIPAddress = string.Empty;

        if (!string.IsNullOrEmpty(userRequestIPHeader) && request.Headers.AllKeys.Contains(userRequestIPHeader))
        {
            userIPAddress = request.Headers[userRequestIPHeader];
            userIPAddress = userIPAddress.Split(',')[0];
            if (ipFamily == IPAddressFamily.IPv4 && userIPAddress.Contains(':'))
            {
                userIPAddress = userIPAddress.Split(':')[0];
            }
            else if (ipFamily == IPAddressFamily.IPv6
                     && userIPAddress.StartsWith("[") && userIPAddress.Contains(']'))
            {
                userIPAddress = userIPAddress.Split(']')[0].Substring(1);
            }
        }

        if (string.IsNullOrEmpty(userIPAddress))
        {
            var remoteAddrVariable = "REMOTE_ADDR";
            if (request.ServerVariables.AllKeys.Contains(remoteAddrVariable))
            {
                userIPAddress = request.ServerVariables[remoteAddrVariable];
            }
        }

        if (string.IsNullOrEmpty(userIPAddress))
        {
            userIPAddress = request.UserHostAddress;
        }

        if (string.IsNullOrEmpty(userIPAddress) || userIPAddress.Trim() == "::1")
        {
            userIPAddress = string.Empty;
        }

        if (!string.IsNullOrEmpty(userIPAddress) && !this.ValidateIP(userIPAddress, ipFamily))
        {
            userIPAddress = string.Empty;
        }

        return userIPAddress;
    }

    /// <inheritdoc/>
    protected override Func<IUserRequestIPAddressController> GetFactory()
    {
        return () => new UserRequestIPAddressController();
    }

    private bool ValidateIP(string ipString, IPAddressFamily ipFamily)
    {
        IPAddress address;
        if (IPAddress.TryParse(ipString, out address))
        {
            if (ipFamily == IPAddressFamily.IPv4 &&
                address.AddressFamily == AddressFamily.InterNetwork &&
                ipString.Split('.').Length == 4)
            {
                return true;
            }

            if (ipFamily == IPAddressFamily.IPv6 &&
                address.AddressFamily == AddressFamily.InterNetworkV6)
            {
                return true;
            }
        }

        return false;
    }
}
