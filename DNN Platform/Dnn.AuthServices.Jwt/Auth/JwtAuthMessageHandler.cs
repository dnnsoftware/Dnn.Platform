// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.AuthServices.Jwt.Auth
{
    using System;
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Security.Principal;
    using System.Threading;

    using Dnn.AuthServices.Jwt.Components.Common.Controllers;
    using DotNetNuke.Instrumentation;
    using DotNetNuke.Web.Api.Auth;
    using DotNetNuke.Web.ConfigSection;

    /// <summary>
    /// This class implements Json Web Token (JWT) authentication scheme.
    /// For detailed description of JWT refer to:
    /// <para>- JTW standard https://tools.ietf.org/html/rfc7519. </para>
    /// <para>- Introduction to JSON Web Tokens http://jwt.io/introduction/. </para>
    /// </summary>
    public class JwtAuthMessageHandler : AuthMessageHandlerBase
    {
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(JwtAuthMessageHandler));

        private readonly IJwtController jwtController = JwtController.Instance;

        /// <summary>
        /// Initializes a new instance of the <see cref="JwtAuthMessageHandler"/> class.
        /// </summary>
        /// <param name="includeByDefault">Should this handler be included by default on all routes.</param>
        /// <param name="forceSsl">Should SSL be enforced on this handler.</param>
        [Obsolete("Deprecated in v9.9.0, use the overload that takes accessControlAllowOrigins instead, scheduled removal in v11.")]
        public JwtAuthMessageHandler(bool includeByDefault, bool forceSsl)
            : base(includeByDefault, forceSsl)
        {
            // Once an instance is enabled and gets registered in
            // ServicesRoutingManager.RegisterAuthenticationHandlers()
            // this scheme gets marked as enabled.
            IsEnabled = true;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="JwtAuthMessageHandler"/> class.
        /// </summary>
        /// <param name="includeByDefault">Should this handler be included by default on all routes.</param>
        /// <param name="forceSsl">Should SSL be enforced for this handler.</param>
        /// <param name="accessControlAllowHeaders">A comma separated list of allowed HTTP headers for CORS support.</param>
        /// <param name="accesscontrolAllowMethods">A comma separated list of allowed HTTP methods for CORS support.</param>
        /// <param name="accessControlAllowOrigins">A list of allowed origins for CORS support.</param>
        public JwtAuthMessageHandler(
            bool includeByDefault,
            bool forceSsl,
            string accessControlAllowHeaders,
            string accesscontrolAllowMethods,
            IReadOnlyCollection<string> accessControlAllowOrigins)
            : base(
                  includeByDefault,
                  forceSsl,
                  accessControlAllowHeaders,
                  accesscontrolAllowMethods,
                  accessControlAllowOrigins)
        {
            // Once an instance is enabled and gets registered in
            // ServicesRoutingManager.RegisterAuthenticationHandlers()
            // this scheme gets marked as enabled.
            IsEnabled = true;
        }

        /// <inheritdoc/>
        public override string AuthScheme => this.jwtController.SchemeType;

        /// <inheritdoc/>
        public override bool BypassAntiForgeryToken => true;

        /// <summary>
        /// Gets or sets a value indicating whether this handler is enabled.
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
                var username = this.jwtController.ValidateToken(request);
                if (!string.IsNullOrEmpty(username))
                {
                    if (Logger.IsTraceEnabled)
                    {
                        Logger.Trace($"Authenticated user '{username}'");
                    }

                    SetCurrentPrincipal(new GenericPrincipal(new GenericIdentity(username, this.AuthScheme), null), request);
                }
            }
            catch (Exception ex)
            {
                Logger.Error("Unexpected error in authenticating the user. " + ex);
            }
        }
    }
}
