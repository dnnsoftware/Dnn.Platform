using System;
using DotNetNuke.Web.Api.Auth.Jwt;

namespace DotNetNuke.Web.Api
{
    /// <summary>
    /// This attribute should be applied to WEB API services that
    /// must deny access through the JWT authentication scheme.
    /// </summary>
    public class RequireJwtAttribute : AuthorizeAttributeBase
    {
        public override bool IsAuthorized(AuthFilterContext context)
        {
            try
            {
                var headers = context.ActionContext.Request.Headers;
                var authorization = JwtUtil.ValidateAuthHeader(headers.Authorization);
                return !string.IsNullOrEmpty(authorization); // aprove the request only if it has a JWT auth scheme
            }
            catch (Exception e)
            {
                context.AuthFailureMessage = e.Message;
                return false;
            }
        }
    }
}
