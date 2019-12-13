// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
using System.Net.Http;
using Dnn.AuthServices.Jwt.Components.Entity;

namespace Dnn.AuthServices.Jwt.Components.Common.Controllers
{
    public interface IJwtController
    {
        string SchemeType { get; }
        string ValidateToken(HttpRequestMessage request);
        bool LogoutUser(HttpRequestMessage request);
        LoginResultData LoginUser(HttpRequestMessage request, LoginData loginData);
        LoginResultData RenewToken(HttpRequestMessage request, string renewalToken);
    }
}
