// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.Api.Auth
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Http;
    using System.Security.Principal;
    using System.Threading;
    using System.Threading.Tasks;

    using DotNetNuke.Instrumentation;

    public abstract class AuthMessageHandlerBase : DelegatingHandler
    {
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(AuthMessageHandlerBase));

        protected AuthMessageHandlerBase(bool includeByDefault, bool forceSsl)
        {
            this.DefaultInclude = includeByDefault;
            this.ForceSsl = forceSsl;
        }

        public abstract string AuthScheme { get; }

        public virtual bool BypassAntiForgeryToken => false;

        public bool DefaultInclude { get; }

        public bool ForceSsl { get; }

        /// <summary>
        /// A chance to process inbound requests.
        /// </summary>
        /// <param name="request">the request message.</param>
        /// <param name="cancellationToken">a cancellationtoken.</param>
        /// <returns>null normally, if a response is returned all inbound processing is terminated and the resposne is returned.</returns>
        public virtual HttpResponseMessage OnInboundRequest(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            return null;
        }

        /// <summary>
        /// A change to process outbound responses.
        /// </summary>
        /// <param name="response">The response message.</param>
        /// <param name="cancellationToken">a cancellationtoken.</param>
        /// <returns>the responsemessage.</returns>
        public virtual HttpResponseMessage OnOutboundResponse(HttpResponseMessage response, CancellationToken cancellationToken)
        {
            return response;
        }

        protected static bool IsXmlHttpRequest(HttpRequestMessage request)
        {
            string value = null;
            IEnumerable<string> values;
            if (request != null && request.Headers.TryGetValues("X-REQUESTED-WITH", out values))
            {
                value = values.FirstOrDefault();
            }

            return !string.IsNullOrEmpty(value) &&
                   value.Equals("XmlHttpRequest", StringComparison.InvariantCultureIgnoreCase);
        }

        protected static void SetCurrentPrincipal(IPrincipal principal, HttpRequestMessage request)
        {
            Thread.CurrentPrincipal = principal;
            request.GetHttpContext().User = principal;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var response = this.OnInboundRequest(request, cancellationToken);
            if (response != null)
            {
                response.RequestMessage = response.RequestMessage ?? request; // if someone returns new HttpResponseMessage(), fill in the requestMessage for other handlers in the chain
                return Task<HttpResponseMessage>.Factory.StartNew(() => response, cancellationToken);
            }

            return base.SendAsync(request, cancellationToken).ContinueWith(x => this.OnOutboundResponse(x.Result, cancellationToken), cancellationToken);
        }

        protected bool NeedsAuthentication(HttpRequestMessage request)
        {
            if (this.MustEnforceSslInRequest(request))
            {
                return !Thread.CurrentPrincipal.Identity.IsAuthenticated;
            }

            if (Logger.IsTraceEnabled)
            {
                Logger.Trace($"{this.AuthScheme}: Validating request vs. SSL mode ({this.ForceSsl}) failed. ");
            }

            // will let callers to return without authenticating the user
            return false;
        }

        /// <summary>
        /// Validated the <see cref="ForceSsl"/> setting of the instane against the HTTP(S) request.
        /// </summary>
        /// <returns>True if <see cref="ForceSsl"/> matcher the request scheme; false otherwise.</returns>
        private bool MustEnforceSslInRequest(HttpRequestMessage request)
        {
            return !this.ForceSsl || request.RequestUri.Scheme.Equals("HTTPS", StringComparison.InvariantCultureIgnoreCase);
        }
    }
}
