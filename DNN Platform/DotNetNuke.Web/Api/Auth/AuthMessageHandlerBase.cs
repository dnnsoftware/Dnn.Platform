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

    /// <summary>
    /// Base class for authentication handlers.
    /// </summary>
    public abstract class AuthMessageHandlerBase : DelegatingHandler
    {
        private const string AccessControlAllowHeadersKey = "Access-Control-Allow-Headers";
        private const string AccessControlAllowMethodsKey = "Access-Control-Allow-Methods";
        private const string AccessControlAllowOriginKey = "Access-Control-Allow-Origin";

        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(AuthMessageHandlerBase));

        private string origin;

        /// <summary>
        /// Initializes a new instance of the <see cref="AuthMessageHandlerBase"/> class.
        /// </summary>
        /// <param name="includeByDefault">Should this handler be included by default on all routes.</param>
        /// <param name="forceSsl">Should this handler enforce ssl usage.</param>
        [Obsolete("Deprecated in v9.9.0, use the overload that takes accessControlAllowOrigins, scheduled removal in v11.")]
        protected AuthMessageHandlerBase(bool includeByDefault, bool forceSsl)
        {
            this.DefaultInclude = includeByDefault;
            this.ForceSsl = forceSsl;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AuthMessageHandlerBase"/> class.
        /// </summary>
        /// <param name="includeByDefault">Should this handler be included by default on all routes.</param>
        /// <param name="forceSsl">Should this handler enforce ssl usage.</param>
        /// <param name="accessControlAllowHeaders">A string of comma delimited allowed HTTP headers for this handler.</param>
        /// <param name="accessControlAllowMethods">A string of comma delimited allowed HTTP methods for this handler.</param>
        /// <param name="accessControlAllowOrigins">
        /// A list of Access-Control-Allow-Origin values, in the web.config file
        /// this is a list separated by semi-colons (;).
        /// </param>
        protected AuthMessageHandlerBase(
            bool includeByDefault,
            bool forceSsl,
            string accessControlAllowHeaders,
            string accessControlAllowMethods,
            IReadOnlyCollection<string> accessControlAllowOrigins)
        {
            this.DefaultInclude = includeByDefault;
            this.ForceSsl = forceSsl;
            this.AccessControlAllowHeaders = accessControlAllowHeaders;
            this.AccessControlAllowMethods = accessControlAllowMethods;
            this.AccessControlAllowOrigins = accessControlAllowOrigins;
        }

        /// <summary>
        /// Gets the authentication scheme.
        /// </summary>
        public abstract string AuthScheme { get; }

        /// <summary>
        /// Gets a value indicating whether to bypass the anti forgery token.
        /// </summary>
        public virtual bool BypassAntiForgeryToken => false;

        /// <summary>
        /// Gets a value indicating whether this handler is included in all routes.
        /// </summary>
        public bool DefaultInclude { get; }

        /// <summary>
        /// Gets a value indicating whether this handler enforces ssl usage.
        /// </summary>
        public bool ForceSsl { get; }

        /// <summary>
        /// Gets a string of comma delimited HTTP headers allowed to be used by this handler for CORS support.
        /// </summary>
        public string AccessControlAllowHeaders { get; }

        /// <summary>
        /// Gets a string of comma delimited HTTP methods allowed to be used by this handler for CORS support.
        /// </summary>
        public string AccessControlAllowMethods { get; }

        /// <summary>
        /// Gets a list of allowed origins for CORS support for this handler.
        /// </summary>
        public IReadOnlyCollection<string> AccessControlAllowOrigins { get; }

        /// <summary>
        /// A chance to process inbound requests.
        /// </summary>
        /// <param name="request">The request message.</param>
        /// <param name="cancellationToken">A cancellationtoken.</param>
        /// <returns>null normally, if a response is returned all inbound processing is terminated and the resposne is returned.</returns>
        public virtual HttpResponseMessage OnInboundRequest(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (request.Headers.Contains("Origin"))
            {
                this.origin = request.Headers.GetValues("Origin").FirstOrDefault();
            }

            return null;
        }

        /// <summary>
        /// A change to process outbound responses.
        /// </summary>
        /// <param name="response">The response message.</param>
        /// <param name="cancellationToken">A cancellationtoken.</param>
        /// <returns>The responsemessage.</returns>
        public virtual HttpResponseMessage OnOutboundResponse(HttpResponseMessage response, CancellationToken cancellationToken)
        {
            if (!string.IsNullOrEmpty(this.AccessControlAllowHeaders))
            {
                if (response.Headers.Contains(AccessControlAllowHeadersKey))
                {
                    response.Headers.Remove(AccessControlAllowHeadersKey);
                }

                response.Headers.Add(AccessControlAllowHeadersKey, this.AccessControlAllowHeaders);
            }

            if (!string.IsNullOrEmpty(this.AccessControlAllowMethods))
            {
                if (response.Headers.Contains(AccessControlAllowMethodsKey))
                {
                    response.Headers.Remove(AccessControlAllowMethodsKey);
                }

                response.Headers.Add(AccessControlAllowMethodsKey, this.AccessControlAllowMethods);
            }

            if (!string.IsNullOrEmpty(this.origin) &&
                this.AccessControlAllowOrigins != null &&
                this.AccessControlAllowOrigins.Count() > 0)
            {
                if (response.Headers.Contains(AccessControlAllowOriginKey))
                {
                    response.Headers.Remove(AccessControlAllowOriginKey);
                }

                if (this.AccessControlAllowOrigins.Contains(this.origin))
                {
                    response.Headers.Add(AccessControlAllowOriginKey, this.origin);
                }

                if (this.AccessControlAllowOrigins.Contains("*"))
                {
                    response.Headers.Add(AccessControlAllowOriginKey, "*");
                }
            }

            return response;
        }

        /// <summary>
        /// Checks if a request was an XmlHttpRequest.
        /// </summary>
        /// <param name="request">The http request message.</param>
        /// <returns>A value indicating whether this request was an XmlHttpRequest.</returns>
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

        /// <summary>
        /// Sets the current principal.
        /// </summary>
        /// <param name="principal">The principal to set.</param>
        /// <param name="request">The http request message.</param>
        protected static void SetCurrentPrincipal(IPrincipal principal, HttpRequestMessage request)
        {
            Thread.CurrentPrincipal = principal;
            request.GetHttpContext().User = principal;
        }

        /// <summary>
        /// Sends an HTTP request asynchronously.
        /// </summary>
        /// <param name="request">The HTTP request message.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>A task of type <see cref="HttpResponseMessage"/>.</returns>
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

        /// <summary>
        /// Checks if the request needs authentication.
        /// </summary>
        /// <param name="request">The HTTP request message.</param>
        /// <returns>A value indicating whether the request needs authentication.</returns>
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
