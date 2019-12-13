﻿// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
using System;
using System.Web;
using System.Web.Mvc;
using DotNetNuke.Common;

namespace DotNetNuke.Web.Mvc.Framework.ActionFilters
{
    public abstract class AuthorizeAttributeBase : FilterAttribute, IAuthorizationFilter
    {
        protected virtual bool AuthorizeCore(HttpContextBase httpContext)
        {
            return true;
        }

        private void CacheValidateHandler(HttpContext context, object data, ref HttpValidationStatus validationStatus)
        {
            validationStatus = OnCacheAuthorization(new HttpContextWrapper(context));
        }

        public virtual void OnAuthorization(AuthorizationContext filterContext)
        {
            Requires.NotNull("filterContext", filterContext);

            if (SkipAuthorization(filterContext))
            {
                return;
            }

            if (AuthorizeCore(filterContext.HttpContext))
            {
                HandleAuthorizedRequest(filterContext);
            }
            else
            {
                HandleUnauthorizedRequest(filterContext);
            }
        }

        protected virtual void HandleAuthorizedRequest(AuthorizationContext filterContext)
        {
            HttpCachePolicyBase cachePolicy = filterContext.HttpContext.Response.Cache;
            cachePolicy.SetProxyMaxAge(new TimeSpan(0));
            cachePolicy.AddValidationCallback(CacheValidateHandler, null /* data */);
        }

        protected virtual void HandleUnauthorizedRequest(AuthorizationContext filterContext)
        {
            const string failureMessage = "Authorization has been denied for this request.";
            var authFilterContext = new AuthFilterContext(filterContext, failureMessage);
            authFilterContext.HandleUnauthorizedRequest();
            //filterContext.HttpContext.Response.Redirect(Globals.AccessDeniedURL());
        }

        protected virtual HttpValidationStatus OnCacheAuthorization(HttpContextBase httpContext)
        {
            Requires.NotNull("httpContext", httpContext);

            bool isAuthorized = AuthorizeCore(httpContext);
            return (isAuthorized) ? HttpValidationStatus.Valid : HttpValidationStatus.IgnoreThisRequest;
        }

        /// <summary>
        /// Skips this authorization step if anonymous attribute is applied, override if auth should never be skipped, or other conditions are required
        /// </summary>
        /// <param name="actionContext"></param>
        /// <returns></returns>
        protected virtual bool SkipAuthorization(AuthorizationContext filterContext)
        {
            return IsAnonymousAttributePresent(filterContext);
        }

        public static bool IsAnonymousAttributePresent(AuthorizationContext filterContext)
        {
            return filterContext.ActionDescriptor.IsDefined(typeof(AllowAnonymousAttribute), inherit: true)
                                     || filterContext.ActionDescriptor.ControllerDescriptor.IsDefined(typeof(AllowAnonymousAttribute), inherit: true);
        }
    }
}
