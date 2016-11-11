﻿#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2016
// by DotNetNuke Corporation
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated 
// documentation files (the "Software"), to deal in the Software without restriction, including without limitation 
// the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and 
// to permit persons to whom the Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all copies or substantial portions 
// of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED 
// TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL 
// THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF 
// CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
// DEALINGS IN THE SOFTWARE.
#endregion
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