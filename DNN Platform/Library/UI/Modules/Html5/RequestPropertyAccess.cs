// Copyright (c) DNN Software. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

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
            switch (propertyName.ToLower())
            {
                case "querystring":
                    return _request.QueryString.ToString();
            }

            propertyNotFound = true;
            return string.Empty;
        }
    }
}
