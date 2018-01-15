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
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using DotNetNuke.Common;

namespace DotNetNuke.Web.Api
{
    public abstract class AuthFilterBase : IAuthorizationFilter
    {
        /// <summary>
        /// Tests if the request passes the authorization requirements
        /// </summary>
        /// <param name="context">The auth filter context</param>
        /// <returns>True when authorization is succesful</returns>
        public abstract bool IsAuthorized(AuthFilterContext context);

        /// <summary>
        /// Co-ordinates check of authorization and handles Auth failure.  Should rarely be overridden.
        /// </summary>
        /// <param name="actionContext"></param>
        protected virtual void OnAuthorization(HttpActionContext actionContext)
        {
            Requires.NotNull("actionContext", actionContext);

            const string failureMessage = "Authorization has been denied for this request.";
            var authFilterContext = new AuthFilterContext(actionContext, failureMessage);
            if (!IsAuthorized(authFilterContext))
            {
                authFilterContext.HandleUnauthorizedRequest();
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether more than one instance of the indicated attribute can be specified for a single program element.
        /// </summary>
        public abstract bool AllowMultiple { get; }

        Task<HttpResponseMessage> IAuthorizationFilter.ExecuteAuthorizationFilterAsync(HttpActionContext actionContext, CancellationToken cancellationToken, Func<Task<HttpResponseMessage>> continuation)
        {
            Requires.NotNull("actionContext", actionContext);
            Requires.NotNull("continuation", continuation);

            try
            {
                OnAuthorization(actionContext);
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
    }
}