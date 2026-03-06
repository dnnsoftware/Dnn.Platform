// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Services.Tokens
{
    using System;
    using System.Globalization;

    using DotNetNuke.Common;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Framework;

    using Microsoft.Extensions.DependencyInjection;

    public class AntiForgeryTokenPropertyAccess : IPropertyAccess
    {
        private readonly IServicesFramework servicesFramework;

        /// <summary>Initializes a new instance of the <see cref="AntiForgeryTokenPropertyAccess"/> class.</summary>
        [Obsolete("Deprecated in DotNetNuke 10.2.2. Please use overload with IServicesFramework. Scheduled removal in v12.0.0.")]
        public AntiForgeryTokenPropertyAccess()
            : this(null)
        {
        }

        /// <summary>Initializes a new instance of the <see cref="AntiForgeryTokenPropertyAccess"/> class.</summary>
        /// <param name="servicesFramework">The web API service framework.</param>
        public AntiForgeryTokenPropertyAccess(IServicesFramework servicesFramework)
        {
            this.servicesFramework = servicesFramework ?? Globals.GetCurrentServiceProvider().GetRequiredService<IServicesFramework>();
        }

        /// <inheritdoc />
        public CacheLevel Cacheability
        {
            get { return CacheLevel.notCacheable; }
        }

        /// <inheritdoc />
        public string GetProperty(string propertyName, string format, CultureInfo formatProvider, UserInfo accessingUser, Scope accessLevel, ref bool propertyNotFound)
        {
            this.servicesFramework.RequestAjaxAntiForgerySupport();

            return string.Empty;
        }
    }
}
