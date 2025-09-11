// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.MvcPipeline.Tokens
{
    using System.Globalization;
    using System.Web.Mvc;

    using DotNetNuke.Entities.Users;
    using DotNetNuke.Services.Tokens;

    /// <summary>Replaces tokens related to the current HTTP request in MVC context.</summary>
    public class MvcRequestPropertyAccess : IPropertyAccess
    {
        private readonly ControllerContext controllerContext;

        /// <summary>Initializes a new instance of the <see cref="MvcRequestPropertyAccess"/> class.</summary>
        /// <param name="controllerContext">The controller context.</param>
        public MvcRequestPropertyAccess(ControllerContext controllerContext)
        {
            this.controllerContext = controllerContext;
        }

        /// <inheritdoc/>
        public virtual CacheLevel Cacheability
        {
            get { return CacheLevel.notCacheable; }
        }

        /// <inheritdoc/>
        public string GetProperty(string propertyName, string format, CultureInfo formatProvider, UserInfo accessingUser, Scope accessLevel, ref bool propertyNotFound)
        {
            var request = this.controllerContext.HttpContext.Request;
            
            switch (propertyName.ToLowerInvariant())
            {
                case "querystring":
                    return request.QueryString.ToString();
                case "applicationpath":
                    return request.ApplicationPath;
                case "relativeapppath":
                    // RelativeAppPath is like ApplicationPath, but will always end with a forward slash (/)
                    return request.ApplicationPath.EndsWith("/")
                        ? request.ApplicationPath
                        : $"{request.ApplicationPath}/";
            }

            propertyNotFound = true;
            return string.Empty;
        }
    }
}
