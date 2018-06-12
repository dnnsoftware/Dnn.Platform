#region Copyright
// 
// DotNetNuke® - http://www.dnnsoftware.com
// Copyright (c) 2002-2018
// by DNN Corporation
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
