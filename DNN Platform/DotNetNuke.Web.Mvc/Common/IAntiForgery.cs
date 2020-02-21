// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
namespace DotNetNuke.Web.Mvc.Common
{
    public interface IAntiForgery
    {
        string CookieName { get; }
        void Validate(string cookieToken, string headerToken);
    }
}
