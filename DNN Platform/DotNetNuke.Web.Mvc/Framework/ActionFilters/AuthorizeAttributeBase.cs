// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.Mvc.Framework.ActionFilters
{
    using System;
    using System.Web;
    using System.Web.Mvc;

    using DotNetNuke.Common;

    public abstract class AuthorizeAttributeBase : FilterAttribute, IAuthorizationFilter
    {
        public static bool IsAnonymousAttributePresent(AuthorizationContext filterContext)
        {
            return filterContext.ActionDescriptor.IsDefined(typeof(AllowAnonymousAttribute), inherit: true)
                                     || filterContext.ActionDescriptor.ControllerDescriptor.IsDefined(typeof(AllowAnonymousAttribute), inherit: true);
        }

        public virtual void OnAuthorization(AuthorizationContext filterContext)
        {
            Requires.NotNull("filterContext", filterContext);

            if (this.SkipAuthorization(filterContext))
            {
                return;
            }

            if (this.AuthorizeCore(filterContext.HttpContext))
            {
                this.HandleAuthorizedRequest(filterContext);
            }
            else
            {
                this.HandleUnauthorizedRequest(filterContext);
            }
        }

        protected virtual bool AuthorizeCore(HttpContextBase httpContext)
        {
            return true;
        }

        protected virtual void HandleAuthorizedRequest(AuthorizationContext filterContext)
        {
            HttpCachePolicyBase cachePolicy = filterContext.HttpContext.Response.Cache;
            cachePolicy.SetProxyMaxAge(new TimeSpan(0));
            cachePolicy.AddValidationCallback(this.CacheValidateHandler, null /* data */);
        }

        protected virtual void HandleUnauthorizedRequest(AuthorizationContext filterContext)
        {
            const string failureMessage = "Authorization has been denied for this request.";
            var authFilterContext = new AuthFilterContext(filterContext, failureMessage);
            authFilterContext.HandleUnauthorizedRequest();

            // filterContext.HttpContext.Response.Redirect(Globals.AccessDeniedURL());
        }

        protected virtual HttpValidationStatus OnCacheAuthorization(HttpContextBase httpContext)
        {
            Requires.NotNull("httpContext", httpContext);

            bool isAuthorized = this.AuthorizeCore(httpContext);
            return isAuthorized ? HttpValidationStatus.Valid : HttpValidationStatus.IgnoreThisRequest;
        }

        /// <summary>
        /// Skips this authorization step if anonymous attribute is applied, override if auth should never be skipped, or other conditions are required.
        /// </summary>
        /// <param name="actionContext"></param>
        /// <returns></returns>
        protected virtual bool SkipAuthorization(AuthorizationContext filterContext)
        {
            return IsAnonymousAttributePresent(filterContext);
        }

        private void CacheValidateHandler(HttpContext context, object data, ref HttpValidationStatus validationStatus)
        {
            validationStatus = this.OnCacheAuthorization(new HttpContextWrapper(context));
        }
    }
}
