// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.Api.Internal.Auth
{
    using System.Diagnostics.CodeAnalysis;
    using System.Net.Http;
    using System.Threading;

    using DotNetNuke.HttpModules.Membership;

    /// <summary>A web API message handler for web forms auth.</summary>
    public class WebFormsAuthMessageHandler : MessageProcessingHandler
    {
        /// <summary>Gets the auth scheme.</summary>
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Breaking change")]
        public string AuthScheme => "Forms";

        /// <inheritdoc/>
        protected override HttpRequestMessage ProcessRequest(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            MembershipModule.AuthenticateRequest(request.GetHttpContext(), allowUnknownExtensions: true);

            return request;
        }

        /// <inheritdoc/>
        protected override HttpResponseMessage ProcessResponse(HttpResponseMessage response, CancellationToken cancellationToken)
        {
            return response;
        }
    }
}
