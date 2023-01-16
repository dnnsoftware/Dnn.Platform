﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

// ReSharper disable ConvertPropertyToExpressionBody
namespace DotNetNuke.UI.Modules.Html5
{
    using System.Globalization;

    using DotNetNuke.Entities.Users;
    using DotNetNuke.Services.Tokens;

    public class ModuleContextPropertyAccess : IPropertyAccess
    {
        private readonly ModuleInstanceContext moduleContext;

        /// <summary>Initializes a new instance of the <see cref="ModuleContextPropertyAccess"/> class.</summary>
        /// <param name="moduleContext"></param>
        public ModuleContextPropertyAccess(ModuleInstanceContext moduleContext)
        {
            this.moduleContext = moduleContext;
        }

        /// <inheritdoc/>
        public virtual CacheLevel Cacheability
        {
            get { return CacheLevel.notCacheable; }
        }

        /// <inheritdoc/>
        public string GetProperty(string propertyName, string format, CultureInfo formatProvider, UserInfo accessingUser, Scope accessLevel, ref bool propertyNotFound)
        {
            switch (propertyName.ToLowerInvariant())
            {
                case "moduleid":
                    return this.moduleContext.ModuleId.ToString();
                case "tabmoduleid":
                    return this.moduleContext.TabModuleId.ToString();
                case "tabid":
                    return this.moduleContext.TabId.ToString();
                case "portalid":
                    return this.moduleContext.Configuration.OwnerPortalID.ToString();
                case "issuperuser":
                    return this.moduleContext.PortalSettings.UserInfo.IsSuperUser.ToString();
                case "editmode":
                    return this.moduleContext.EditMode.ToString();
                default:
                    if (this.moduleContext.Settings.ContainsKey(propertyName))
                    {
                        return (string)this.moduleContext.Settings[propertyName];
                    }

                    break;
            }

            propertyNotFound = true;
            return string.Empty;
        }
    }
}
