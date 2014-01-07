#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2014
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
using System.Net.Http;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;
using DotNetNuke.Common;

namespace DotNetNuke.Web.Api.Internal.Auth
{
    public abstract class AuthMessageHandlerBase : DelegatingHandler
    {
        private const string RequestedWithHeader = "X-REQUESTED-WITH";

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var response = OnInboundRequest(request, cancellationToken);
            if(response != null)
            {
                response.RequestMessage = response.RequestMessage ?? request; //if someone returns new HttpResponseMessage(), fill in the requestMessage for other handlers in the chain
                return Task<HttpResponseMessage>.Factory.StartNew(() => response);
            }

            return base.SendAsync(request, cancellationToken).ContinueWith(x => OnOutboundResponse(x.Result, cancellationToken));
        }

        /// <summary>
        /// A chance to process inbound requests
        /// </summary>
        /// <param name="request">the request message</param>
        /// <param name="cancellationToken">a cancellationtoken</param>
        /// <returns>null normally, if a response is returned all inbound processing is terminated and the resposne is returned</returns>
        public virtual HttpResponseMessage OnInboundRequest(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            return null;
        }

        /// <summary>
        /// A change to process outbound responses
        /// </summary>
        /// <param name="response">The response message</param>
        /// <param name="cancellationToken">a cancellationtoken</param>
        /// <returns>the responsemessage</returns>
        public virtual HttpResponseMessage OnOutboundResponse(HttpResponseMessage response, CancellationToken cancellationToken)
        {
            return response;
        }

        protected bool NeedsAuthentication()
        {
            return !Thread.CurrentPrincipal.Identity.IsAuthenticated;
        }

        protected static bool IsXmlHttpRequest(HttpRequestMessage request)
        {
            if (request.Headers.Contains(RequestedWithHeader))
            {
                string header = request.Headers.GetValues(RequestedWithHeader).FirstOrDefault();
                return !String.IsNullOrEmpty(header) &&
                       header.Equals("XmlHttpRequest", StringComparison.InvariantCultureIgnoreCase);
            }

            return false;
        }

        protected static void SetCurrentPrincipal(IPrincipal principal, HttpRequestMessage request)
        {
            Thread.CurrentPrincipal = principal;
            request.GetHttpContext().User = principal;
        }
    }
}