// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
using System.Net.Http;
using System.Threading;
using DotNetNuke.HttpModules.Membership;

namespace DotNetNuke.Web.Api.Internal.Auth
{
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
