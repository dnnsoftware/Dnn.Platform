// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
using System;
using System.Net.Http;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Security;
using DotNetNuke.Security.Permissions;

namespace DotNetNuke.Web.Api
{
    public class DnnModuleAuthorizeAttribute : AuthorizeAttributeBase, IOverrideDefaultAuthLevel
    {
        public DnnModuleAuthorizeAttribute()
        {
            AccessLevel = SecurityAccessLevel.Host;
        }

        public string PermissionKey { get; set; }
        public SecurityAccessLevel AccessLevel { get; set; }

        public override bool IsAuthorized(AuthFilterContext context)
        {
            var activeModule = FindModuleInfo(context.ActionContext.Request);

            if (activeModule != null)
            {
                return ModulePermissionController.HasModuleAccess(AccessLevel, PermissionKey, activeModule);
            }

            return false;
        }

        protected virtual ModuleInfo FindModuleInfo(HttpRequestMessage request)
        {
            return request.FindModuleInfo();
        }
    }
}
