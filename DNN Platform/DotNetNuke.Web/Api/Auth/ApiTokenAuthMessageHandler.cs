// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.Api.Auth
{
    using System;
    using System.Net.Http;
    using System.Security.Principal;
    using System.Threading;

    using DotNetNuke.Instrumentation;
    using DotNetNuke.Web.Api.Auth.ApiTokens;

    /// <summary>
    /// Authentication message handler that authorizes an HTTP request based on a valid API token.
    /// </summary>
    public class ApiTokenAuthMessageHandler : AuthMessageHandlerBase
    {
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(ApiTokenAuthMessageHandler));

        private readonly IApiTokenController apiTokenController = ApiTokenController.Instance;

        /// <summary>
        /// Initializes a new instance of the <see cref="ApiTokenAuthMessageHandler"/> class.
        /// </summary>
        /// <param name="includeByDefault">A value that indicates whether the handler is included by default in the authentication pipeline.</param>
        /// <param name="forceSsl">A value that indicates whether SSL is required.</param>
        public ApiTokenAuthMessageHandler(bool includeByDefault, bool forceSsl)
            : base(includeByDefault, forceSsl)
        {
            IsEnabled = true;
        }

        /// <inheritdoc/>
        public override string AuthScheme => this.apiTokenController.SchemeType;

        /// <inheritdoc/>
        public override bool BypassAntiForgeryToken => true;

        /// <summary>
        /// Gets or sets a value indicating whether the authentication message handler is enabled in the web.config.
        /// </summary>
        internal static bool IsEnabled { get; set; }

        /// <inheritdoc/>
        public override HttpResponseMessage OnInboundRequest(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (this.NeedsAuthentication(request))
            {
                this.TryToAuthenticate(request);
            }

            return base.OnInboundRequest(request, cancellationToken);
        }

        private void TryToAuthenticate(HttpRequestMessage request)
        {
            try
            {
                var (token, user) = this.apiTokenController.ValidateToken(request);
                if (token != null)
                {
                    if (Logger.IsTraceEnabled)
                    {
                        Logger.Trace($"Authenticated using API token {token.ApiTokenId}");
                    }

                    this.apiTokenController.SetApiTokenForRequest(token);

                    if (user != null)
                    {
                        SetCurrentPrincipal(new GenericPrincipal(new GenericIdentity(user.Username, this.AuthScheme), null), request);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error("Unexpected error authenticating API Token. " + ex);
            }
        }
    }
}
