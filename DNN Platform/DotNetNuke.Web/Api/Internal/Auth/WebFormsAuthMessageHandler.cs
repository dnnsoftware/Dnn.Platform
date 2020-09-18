// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.Api.Internal.Auth
{
    using System.Net.Http;
    using System.Threading;

    using DotNetNuke.HttpModules.Membership;

    public class WebFormsAuthMessageHandler : MessageProcessingHandler
    {
        public string AuthScheme => "Forms";

        protected override HttpRequestMessage ProcessRequest(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            MembershipModule.AuthenticateRequest(request.GetHttpContext(), allowUnknownExtensions: true);

            return request;
        }

        protected override HttpResponseMessage ProcessResponse(HttpResponseMessage response, CancellationToken cancellationToken)
        {
            return response;
        }
    }
}
