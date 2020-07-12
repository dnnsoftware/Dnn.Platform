// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.AuthServices.Jwt.Auth
{
    using System;
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

        private readonly IJwtController _jwtController = JwtController.Instance;

        public JwtAuthMessageHandler(bool includeByDefault, bool forceSsl)
            : base(includeByDefault, forceSsl)
        {
            // Once an instance is enabled and gets registered in
            // ServicesRoutingManager.RegisterAuthenticationHandlers()
            // this scheme gets marked as enabled.
            IsEnabled = true;
        }

        public override string AuthScheme => this._jwtController.SchemeType;

        public override bool BypassAntiForgeryToken => true;

        internal static bool IsEnabled { get; set; }

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
                var username = this._jwtController.ValidateToken(request);
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
