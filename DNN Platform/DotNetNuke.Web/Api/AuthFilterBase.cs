// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.Api
{
    using System;
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Web.Http.Controllers;
    using System.Web.Http.Filters;

    using DotNetNuke.Common;

    public abstract class AuthFilterBase : IAuthorizationFilter
    {
        /// <summary>
        /// Gets a value indicating whether more than one instance of the indicated attribute can be specified for a single program element.
        /// </summary>
        public abstract bool AllowMultiple { get; }

        /// <summary>
        /// Tests if the request passes the authorization requirements.
        /// </summary>
        /// <param name="context">The auth filter context.</param>
        /// <returns>True when authorization is succesful.</returns>
        public abstract bool IsAuthorized(AuthFilterContext context);

        Task<HttpResponseMessage> IAuthorizationFilter.ExecuteAuthorizationFilterAsync(HttpActionContext actionContext, CancellationToken cancellationToken, Func<Task<HttpResponseMessage>> continuation)
        {
            Requires.NotNull("actionContext", actionContext);
            Requires.NotNull("continuation", continuation);

            try
            {
                this.OnAuthorization(actionContext);
            }
            catch (Exception e)
            {
                var tcs = new TaskCompletionSource<HttpResponseMessage>();
                tcs.SetException(e);
                return tcs.Task;
            }

            if (actionContext.Response != null)
            {
                var tcs = new TaskCompletionSource<HttpResponseMessage>();
                tcs.SetResult(actionContext.Response);
                return tcs.Task;
            }

            return continuation();
        }

        /// <summary>
        /// Co-ordinates check of authorization and handles Auth failure.  Should rarely be overridden.
        /// </summary>
        /// <param name="actionContext"></param>
        protected virtual void OnAuthorization(HttpActionContext actionContext)
        {
            Requires.NotNull("actionContext", actionContext);

            const string failureMessage = "Authorization has been denied for this request.";
            var authFilterContext = new AuthFilterContext(actionContext, failureMessage);
            if (!this.IsAuthorized(authFilterContext))
            {
                authFilterContext.HandleUnauthorizedRequest();
            }
        }
    }
}
