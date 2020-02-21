// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
using System.Globalization;
using DotNetNuke.Entities.Users;
using DotNetNuke.Services.Tokens;

// ReSharper disable ConvertPropertyToExpressionBody

namespace DotNetNuke.UI.Modules.Html5
{
    public class ModuleContextPropertyAccess : IPropertyAccess
    {
        private readonly ModuleInstanceContext _moduleContext;

        public ModuleContextPropertyAccess(ModuleInstanceContext moduleContext)
        {
            _moduleContext = moduleContext;
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
                    return _moduleContext.ModuleId.ToString();
                case "tabmoduleid":
                    return _moduleContext.TabModuleId.ToString();
                case "tabid":
                    return _moduleContext.TabId.ToString();
                case "portalid":
                    return _moduleContext.Configuration.OwnerPortalID.ToString();
                case "issuperuser":
                    return _moduleContext.PortalSettings.UserInfo.IsSuperUser.ToString();
                case "editmode":
                    return _moduleContext.EditMode.ToString();
                default:
                    if (_moduleContext.Settings.ContainsKey(propertyName))
                    {
                        return (string)_moduleContext.Settings[propertyName];
                    }
                    break;
            }

            propertyNotFound = true;
            return string.Empty;
        }
    }
}
