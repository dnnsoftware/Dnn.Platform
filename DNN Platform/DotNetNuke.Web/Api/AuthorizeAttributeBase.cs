// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.Api
{
    using System.Linq;
    using System.Web.Http;
    using System.Web.Http.Controllers;
    using System.Web.Http.Filters;

    using DotNetNuke.Common;

    /// <summary>Provides Dnn specific details for authorization filter.</summary>
    public abstract class AuthorizeAttributeBase : AuthorizationFilterAttribute
    {
        /// <summary>Checks if the <see cref="AllowAnonymousAttribute"/> is present.</summary>
        /// <param name="actionContext">The HTTP action context.</param>
        /// <returns>A value indicating whether the <see cref="AllowAnonymousAttribute"/> is present.</returns>
        public static bool IsAnonymousAttributePresent(HttpActionContext actionContext)
        {
            return actionContext.ActionDescriptor.GetCustomAttributes<AllowAnonymousAttribute>().Any()
                   || (actionContext.ControllerContext.ControllerDescriptor.GetCustomAttributes<AllowAnonymousAttribute>().Any()
                         && actionContext.ActionDescriptor.GetCustomAttributes<AuthorizeAttributeBase>().All(t => t is SupportedModulesAttribute));
        }

        /// <summary>Tests if the request passes the authorization requirements.</summary>
        /// <param name="context">The auth filter context.</param>
        /// <returns>True when authorization is successful.</returns>
        public abstract bool IsAuthorized(AuthFilterContext context);

        /// <summary>Called by framework at start of Auth process, check if auth should be skipped and handles auth failure.  Should rarely need to be overridden.</summary>
        /// <param name="actionContext">The HTTP action context.</param>
        public override void OnAuthorization(HttpActionContext actionContext)
        {
            Requires.NotNull("actionContext", actionContext);

            if (this.SkipAuthorization(actionContext))
            {
                return;
            }

            const string failureMessage = "Authorization has been denied for this request.";
            var authFilterContext = new AuthFilterContext(actionContext, failureMessage);
            if (!this.IsAuthorized(authFilterContext))
            {
                authFilterContext.HandleUnauthorizedRequest();
            }
        }

        /// <summary>Skips this authorization step if anonymous attribute is applied, override if auth should never be skipped, or other conditions are required.</summary>
        /// <param name="actionContext">The HTTP Action Context.</param>
        /// <returns>A value indicating whether to skip the authorization.</returns>
        protected virtual bool SkipAuthorization(HttpActionContext actionContext)
        {
            return IsAnonymousAttributePresent(actionContext);
        }
    }
}
