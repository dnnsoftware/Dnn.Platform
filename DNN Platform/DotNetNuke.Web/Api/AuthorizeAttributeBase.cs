#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2018
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
using System.Linq;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using DotNetNuke.Common;

namespace DotNetNuke.Web.Api
{
    public abstract class AuthorizeAttributeBase : AuthorizationFilterAttribute
    {
        /// <summary>
        /// Tests if the request passes the authorization requirements
        /// </summary>
        /// <param name="context">The auth filter context</param>
        /// <returns>True when authorization is succesful</returns>
        public abstract bool IsAuthorized(AuthFilterContext context);

        /// <summary>
        /// Called by framework at start of Auth process, check if auth should be skipped and handles auth failure.  Should rarely need to be overridden.
        /// </summary>
        /// <param name="actionContext"></param>
        public override void OnAuthorization(HttpActionContext actionContext)
        {
            Requires.NotNull("actionContext", actionContext);

            if (SkipAuthorization(actionContext))
            {
                return;
            }

            const string failureMessage = "Authorization has been denied for this request.";
            var authFilterContext = new AuthFilterContext(actionContext, failureMessage);
            if (!IsAuthorized(authFilterContext))
            {
                authFilterContext.HandleUnauthorizedRequest();
            }
        }

        /// <summary>
        /// Skips this authorization step if anonymous attribute is applied, override if auth should never be skipped, or other conditions are required
        /// </summary>
        /// <param name="actionContext"></param>
        /// <returns></returns>
        protected virtual bool SkipAuthorization(HttpActionContext actionContext)
        {
            return IsAnonymousAttributePresent(actionContext);
        }

        public static bool IsAnonymousAttributePresent(HttpActionContext actionContext)
        {
            return actionContext.ActionDescriptor.GetCustomAttributes<AllowAnonymousAttribute>().Any()
                   || (actionContext.ControllerContext.ControllerDescriptor.GetCustomAttributes<AllowAnonymousAttribute>().Any()
                         && actionContext.ActionDescriptor.GetCustomAttributes<AuthorizeAttributeBase>().All(t => t is SupportedModulesAttribute));
        }
    }
}