// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

using System.Globalization;
using System.Web;
using DotNetNuke.Entities.Users;
using DotNetNuke.Services.Tokens;

namespace DotNetNuke.UI.Modules.Html5
{
    public class RequestPropertyAccess : IPropertyAccess
    {
        private readonly HttpRequest _request;

        public RequestPropertyAccess(HttpRequest request)
        {
            _request = request;
        }

        public virtual CacheLevel Cacheability
        {
            get { return CacheLevel.notCacheable; }
        }

        public string GetProperty(string propertyName, string format, CultureInfo formatProvider, UserInfo accessingUser, Scope accessLevel, ref bool propertyNotFound)
        {
            switch (propertyName.ToLowerInvariant())
            {
                case "querystring":
                    return _request.QueryString.ToString();
            }

            propertyNotFound = true;
            return string.Empty;
        }
    }
}
