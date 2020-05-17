﻿// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
using System;
using System.Net.Http;
using System.Security.Principal;
using System.Threading;
using Dnn.AuthServices.Jwt.Components.Common.Controllers;
using DotNetNuke.Instrumentation;
using DotNetNuke.Web.Api.Auth;
using DotNetNuke.Web.ConfigSection;

namespace Dnn.AuthServices.Jwt.Auth
{
    /// <summary>
    /// This class implements Json Web Token (JWT) authentication scheme.
    /// For detailed description of JWT refer to:
    /// <para>- JTW standard https://tools.ietf.org/html/rfc7519 </para>
    /// <para>- Introduction to JSON Web Tokens http://jwt.io/introduction/ </para>
    /// </summary>
    public class JwtAuthMessageHandler : AuthMessageHandlerBase
    {
        #region constants, properties, etc.

        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(JwtAuthMessageHandler));

        public override string AuthScheme => _jwtController.SchemeType;
        public override bool BypassAntiForgeryToken => true;

        internal static bool IsEnabled { get; set; }
        private readonly IJwtController _jwtController = JwtController.Instance;

        #endregion

        #region constructor

        public JwtAuthMessageHandler(bool includeByDefault, bool forceSsl)
            : base(includeByDefault, forceSsl)
        {
            // Once an instance is enabled and gets registered in
            // ServicesRoutingManager.RegisterAuthenticationHandlers()
            // this scheme gets marked as enabled.
            IsEnabled = true;
        }

        #endregion

        #region implementation

        public override HttpResponseMessage OnInboundRequest(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (NeedsAuthentication(request))
            {
                TryToAuthenticate(request);
            }

            return base.OnInboundRequest(request, cancellationToken);
        }

        private void TryToAuthenticate(HttpRequestMessage request)
        {
            try
            {
                var username = _jwtController.ValidateToken(request);
                if (!string.IsNullOrEmpty(username))
                {
                    if (Logger.IsTraceEnabled) Logger.Trace($"Authenticated user '{username}'");
                    SetCurrentPrincipal(new GenericPrincipal(new GenericIdentity(username, AuthScheme), null), request);
                }
            }
            catch (Exception ex)
            {
                Logger.Error("Unexpected error in authenticating the user. " + ex);
            }
        }

        #endregion
    }
}
