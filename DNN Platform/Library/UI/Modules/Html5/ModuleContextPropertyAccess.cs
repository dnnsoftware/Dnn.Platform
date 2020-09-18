// Licensed to the .NET Foundation under one or more agreements.
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
        private readonly ModuleInstanceContext _moduleContext;

        public ModuleContextPropertyAccess(ModuleInstanceContext moduleContext)
        {
            this._moduleContext = moduleContext;
        }

        public virtual CacheLevel Cacheability
        {
            get { return CacheLevel.notCacheable; }
        }

        public string GetProperty(string propertyName, string format, CultureInfo formatProvider, UserInfo accessingUser, Scope accessLevel, ref bool propertyNotFound)
        {
            switch (propertyName.ToLowerInvariant())
            {
                case "moduleid":
                    return this._moduleContext.ModuleId.ToString();
                case "tabmoduleid":
                    return this._moduleContext.TabModuleId.ToString();
                case "tabid":
                    return this._moduleContext.TabId.ToString();
                case "portalid":
                    return this._moduleContext.Configuration.OwnerPortalID.ToString();
                case "issuperuser":
                    return this._moduleContext.PortalSettings.UserInfo.IsSuperUser.ToString();
                case "editmode":
                    return this._moduleContext.EditMode.ToString();
                default:
                    if (this._moduleContext.Settings.ContainsKey(propertyName))
                    {
                        return (string)this._moduleContext.Settings[propertyName];
                    }

                    break;
            }

            propertyNotFound = true;
            return string.Empty;
        }
    }
}
