using System;

namespace DotNetNuke.Web.Api.Internal.Auth.Jwt
{
    /// <summary>
    /// This attribute should be applied to WEB API services that
    /// must deny access through the JWT authentication scheme.
    /// </summary>
    internal class DenyJwtAttribute : AuthorizeAttributeBase
    {
        public override bool IsAuthorized(AuthFilterContext context)
        {
            try
            {
                var headers = context.ActionContext.Request.Headers;
                var authorization = JwtAuthMessageHandler.ValidateAuthHeader(headers.Authorization);
                return string.IsNullOrEmpty(authorization); // aprove the request only if it has no JWT auth scheme
            }
            catch (Exception e)
            {
                context.AuthFailureMessage = e.Message;
                return false;
            }
        }
    }
}
