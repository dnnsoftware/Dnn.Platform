﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.Api.Internal
{
    public interface IAntiForgery
    {
        string CookieName { get; }

        void Validate(string cookieToken, string headerToken);
    }
}
