// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.AuthServices.Jwt.Services
{
    using System.Net.Http.Headers;
    using System.Web.Http;

    using Dnn.AuthServices.Jwt.Components.Common.Controllers;
    using Dnn.AuthServices.Jwt.Components.Entity;
    using DotNetNuke.Web.Api;
    using Newtonsoft.Json;

    [DnnAuthorize(AuthTypes = "JWT")]
    public class MobileController : DnnApiController
    {
        /// <summary>
        /// Clients that used JWT login should use this API call to logout and invalidate the tokens.
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public IHttpActionResult Logout()
        {
            return JwtController.Instance.LogoutUser(this.Request) ? (IHttpActionResult)this.Ok(new { success = true }) : this.Unauthorized();
        }

        /// <summary>
        /// Clients that want to go cookie-less should call this API to login and receive
        /// a Json Web Token (JWT) that allows them to authenticate the users to other
        /// secure API endpoints afterwards.
        /// </summary>
        /// <remarks>AllowAnonymous attribute must stay in this call even though the
        /// DnnAuthorize attribute is present at a class level.</remarks>
        /// <returns></returns>
        [HttpPost]
        [AllowAnonymous]
        public IHttpActionResult Login(LoginData loginData)
        {
            var result = JwtController.Instance.LoginUser(this.Request, loginData);
            return this.ReplyWith(result);
        }

        /// <summary>
        /// Extends the token expiry. A new JWT is returned to the caller which must be used in
        /// new API requests. The caller must pass the renewal token received at the login time.
        /// The header still needs to pass the current token for validation even when it is expired.
        /// </summary>
        /// <remarks>The access token is allowed to get renewed one time only.<br />
        /// AllowAnonymous attribute must stay in this call even though the
        /// DnnAuthorize attribute is present at a class level.
        /// </remarks>
        /// <returns></returns>
        [HttpPost]
        [AllowAnonymous]
        public IHttpActionResult ExtendToken(RenewalDto rtoken)
        {
            var result = JwtController.Instance.RenewToken(this.Request, rtoken.RenewalToken);
            return this.ReplyWith(result);
        }

        // Test API Method 1
        [HttpGet]
        public IHttpActionResult TestGet()
        {
            var identity = System.Threading.Thread.CurrentPrincipal.Identity;
            var reply = $"Hello {identity.Name}! You are authenticated through {identity.AuthenticationType}.";
            return this.Ok(new { reply });
        }

        // Test API Method 2
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IHttpActionResult TestPost(TestPostData something)
        {
            var identity = System.Threading.Thread.CurrentPrincipal.Identity;
            var reply = $"Hello {identity.Name}! You are authenticated through {identity.AuthenticationType}." +
                        $" You said: ({something.Text})";
            return this.Ok(new { reply });
        }

        private IHttpActionResult ReplyWith(LoginResultData result)
        {
            if (result == null)
            {
                return this.Unauthorized();
            }

            if (!string.IsNullOrEmpty(result.Error))
            {
                // HACK: this will return the scheme with the error message as a challenge; non-standard method
                return this.Unauthorized(new AuthenticationHeaderValue(JwtController.AuthScheme, result.Error));
            }

            return this.Ok(result);
        }

        [JsonObject]
        public class TestPostData
        {
            [JsonProperty("text")]
            public string Text;
        }
    }
}
