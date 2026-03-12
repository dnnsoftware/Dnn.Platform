// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.Api.Internal.Auth
{
    using System.Diagnostics.CodeAnalysis;
    using System.Net.Http;
    using System.Threading;

    using DotNetNuke.Abstractions.Application;
    using DotNetNuke.Abstractions.Logging;
    using DotNetNuke.Common;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.HttpModules.Membership;
    using DotNetNuke.Security.Roles;
    using DotNetNuke.Services.UserRequest;

    using Microsoft.Extensions.DependencyInjection;

    /// <summary>A web API message handler for web forms auth.</summary>
    public class WebFormsAuthMessageHandler : MessageProcessingHandler
    {
        /// <summary>Gets the auth scheme.</summary>
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Breaking change")]
        public string AuthScheme => "Forms";

        /// <inheritdoc />
        protected override HttpRequestMessage ProcessRequest(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            using var scope = Globals.GetOrCreateServiceScope();
            MembershipModule.AuthenticateRequest(
                scope.ServiceProvider.GetRequiredService<IHostSettingsService>(),
                scope.ServiceProvider.GetRequiredService<IPortalController>(),
                scope.ServiceProvider.GetRequiredService<IUserRequestIPAddressController>(),
                scope.ServiceProvider.GetRequiredService<IRoleController>(),
                scope.ServiceProvider.GetRequiredService<IEventLogger>(),
                scope.ServiceProvider.GetRequiredService<IHostSettings>(),
                request.GetHttpContext(),
                allowUnknownExtensions: true);

            return request;
        }

        /// <inheritdoc />
        protected override HttpResponseMessage ProcessResponse(HttpResponseMessage response, CancellationToken cancellationToken) => response;
    }
}
