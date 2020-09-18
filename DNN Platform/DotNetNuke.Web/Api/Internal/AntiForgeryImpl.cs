// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.Api.Internal
{
    internal class AntiForgeryImpl : IAntiForgery
    {
        public string CookieName
        {
            get { return System.Web.Helpers.AntiForgeryConfig.CookieName; }
        }

        public void Validate(string cookieToken, string headerToken)
        {
            System.Web.Helpers.AntiForgery.Validate(cookieToken, headerToken);
        }
    }
}
