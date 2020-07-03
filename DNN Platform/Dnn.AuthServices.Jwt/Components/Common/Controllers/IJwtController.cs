// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.AuthServices.Jwt.Components.Common.Controllers
{
    using System.Net.Http;

    using Dnn.AuthServices.Jwt.Components.Entity;

    public interface IJwtController
    {
        string SchemeType { get; }

        string ValidateToken(HttpRequestMessage request);

        bool LogoutUser(HttpRequestMessage request);

        LoginResultData LoginUser(HttpRequestMessage request, LoginData loginData);

        LoginResultData RenewToken(HttpRequestMessage request, string renewalToken);
    }
}
