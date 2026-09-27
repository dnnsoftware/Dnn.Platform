// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.UI.Modules.Html5
{
    using System.Globalization;
    using System.Web;

    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Services.Tokens;

    /// <summary>Replaces tokens related to the current http request.</summary>
    public class RequestPropertyAccess : IPropertyAccess
    {
        private readonly HttpRequest request;
        private readonly HttpRequestBase requestBase;

        /// <summary>Initializes a new instance of the <see cref="RequestPropertyAccess"/> class.</summary>
        /// <param name="request">The current http request.</param>
        public RequestPropertyAccess(HttpRequest request)
        {
            this.request = request;
        }

        /// <summary>Initializes a new instance of the <see cref="RequestPropertyAccess"/> class.</summary>
        /// <param name="requestBase">The current http request.</param>
        public RequestPropertyAccess(HttpRequestBase requestBase)
        {
            this.requestBase = requestBase;
        }

        /// <inheritdoc />
        public virtual CacheLevel Cacheability
        {
            get { return CacheLevel.notCacheable; }
        }

        /// <inheritdoc />
        public string GetProperty(string propertyName, string format, CultureInfo formatProvider, UserInfo accessingUser, Scope accessLevel, ref bool propertyNotFound)
        {
            switch (propertyName.ToLowerInvariant())
            {
                case "querystring":
                    return this.request != null ? this.request.QueryString.ToString() : this.requestBase.QueryString.ToString();
                case "applicationpath":
                    return this.request != null ? this.request.ApplicationPath : this.requestBase.ApplicationPath;
                case "relativeapppath":
                    // RelativeAppPath is like ApplicationPath, but will always end with a forward slash (/)
                    return this.request != null ? this.request.ApplicationPath.EnsureEndsWith("/") : this.requestBase.ApplicationPath.EnsureEndsWith("/");
            }

            propertyNotFound = true;
            return string.Empty;
        }
    }
}
