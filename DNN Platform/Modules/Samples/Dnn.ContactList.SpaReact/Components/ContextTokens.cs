// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

using System.Globalization;
using System.Web;
using DotNetNuke.Entities.Users;
using DotNetNuke.Services.Tokens;
using DotNetNuke.UI.Modules;
using Newtonsoft.Json;

namespace Dnn.ContactList.SpaReact.Components
{
    public class ContextTokens : IPropertyAccess
    {
        public CacheLevel Cacheability => CacheLevel.notCacheable;
        private readonly SecurityContext security;
        private readonly ModuleInstanceContext moduleContext;

        public ContextTokens(ModuleInstanceContext moduleContext, UserInfo user)
        {
            this.moduleContext = moduleContext;
            this.security = new SecurityContext(moduleContext.Configuration, user);
        }

        public string GetProperty(string propertyName, string format, CultureInfo formatProvider, UserInfo accessingUser, Scope accessLevel, ref bool propertyNotFound)
        {
            switch (propertyName.ToLower())
            {
                case "security":
                    return HttpUtility.HtmlAttributeEncode(JsonConvert.SerializeObject(this.security));
                case "module":
                    return HttpUtility.HtmlAttributeEncode(JsonConvert.SerializeObject(new
                    {
                        this.moduleContext.ModuleId,
                        this.moduleContext.TabId,
                        this.moduleContext.TabModuleId,
                        this.moduleContext.PortalId,
                    }));
                default:
                    propertyNotFound = true;
                    return string.Empty;
            }
        }
    }
}
