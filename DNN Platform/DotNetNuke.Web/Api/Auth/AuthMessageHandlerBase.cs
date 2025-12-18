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

    /// <summary>Base class for authentication providers message handlers.</summary>
    public abstract class AuthMessageHandlerBase : DelegatingHandler
    {
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(AuthMessageHandlerBase));

        /// <summary>Initializes a new instance of the <see cref="AuthMessageHandlerBase"/> class.</summary>
        /// <param name="includeByDefault">A value indicating whether this handler should be included by default in all API endpoints.</param>
        /// <param name="forceSsl">A value indicating whether this handler should enforce SSL usage.</param>
        protected AuthMessageHandlerBase(bool includeByDefault, bool forceSsl)
        {
            this.DefaultInclude = includeByDefault;
            this.ForceSsl = forceSsl;
        }

        /// <summary>Gets the name of the authentication scheme.</summary>
        public abstract string AuthScheme { get; }

        /// <summary>Gets a value indicating whether this handler should bypass the anti-forgery token check.</summary>
        public virtual bool BypassAntiForgeryToken => false;

        /// <summary>Gets a value indicating whether this handler should be included by default on all API endpoints.</summary>
        public bool DefaultInclude { get; }

        /// <summary>Gets a value indicating whether this handler should enforce SSL usage on it's endpoints.</summary>
        public bool ForceSsl { get; }

        /// <summary>A chance to process inbound requests.</summary>
        /// <param name="request">The request message.</param>
        /// <param name="cancellationToken">A cancellationtoken.</param>
        /// <returns>null normally, if a response is returned all inbound processing is terminated and the resposne is returned.</returns>
        public virtual HttpResponseMessage OnInboundRequest(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            return null;
        }

        /// <summary>A change to process outbound responses.</summary>
        /// <param name="response">The response message.</param>
        /// <param name="cancellationToken">A cancellationtoken.</param>
        /// <returns>The responsemessage.</returns>
        public virtual HttpResponseMessage OnOutboundResponse(HttpResponseMessage response, CancellationToken cancellationToken)
        {
            return response;
        }

        /// <summary>Checks if the current request is an XmlHttpRequest.</summary>
        /// <param name="request">The HTTP Request.</param>
        /// <returns>A value indicating whether the request is an XmlHttpRequest.</returns>
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

        /// <summary>Sets the current principal for the request.</summary>
        /// <param name="principal">The principal to set.</param>
        /// <param name="request">The current request.</param>
        protected static void SetCurrentPrincipal(IPrincipal principal, HttpRequestMessage request)
        {
            Thread.CurrentPrincipal = principal;
            request.GetHttpContext().User = principal;
        }

        /// <summary>Asynchronously sends a response.</summary>
        /// <param name="request">The current request.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>An HttpResponseMessage Task.</returns>
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

        /// <summary>Checks if the current request requires authentication.</summary>
        /// <param name="request">The current request.</param>
        /// <returns>A value indication whether the current request needs authentication.</returns>
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

        /// <summary>Validated the <see cref="ForceSsl"/> setting of the instane against the HTTP(S) request.</summary>
        /// <returns>True if <see cref="ForceSsl"/> matcher the request scheme; false otherwise.</returns>
        private bool MustEnforceSslInRequest(HttpRequestMessage request)
        {
            return !this.ForceSsl || request.RequestUri.Scheme.Equals("HTTPS", StringComparison.InvariantCultureIgnoreCase);
        }
    }
}
