﻿// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
using System.Net.Http.Headers;
using System.Web.Http;
using Dnn.AuthServices.Jwt.Components.Common.Controllers;
using Dnn.AuthServices.Jwt.Components.Entity;
using DotNetNuke.Web.Api;
using Newtonsoft.Json;

namespace Dnn.AuthServices.Jwt.Services
{
    [DnnAuthorize(AuthTypes = "JWT")]
    public class MobileController : DnnApiController
    {
        #region API methods

        /// <summary>
        /// Clients that used JWT login should use this API call to logout and invalidate the tokens.
        /// </summary>
        [HttpGet]
        public IHttpActionResult Logout()
        {
            return JwtController.Instance.LogoutUser(Request) ? (IHttpActionResult)Ok(new { success = true}) : Unauthorized();
        }

        /// <summary>
        /// Clients that want to go cookie-less should call this API to login and receive
        /// a Json Web Token (JWT) that allows them to authenticate the users to other
        /// secure API endpoints afterwards.
        /// </summary>
        /// <remarks>AllowAnonymous attribute must stay in this call even though the
        /// DnnAuthorize attribute is present at a class level.</remarks>
        [HttpPost]
        [AllowAnonymous]
        public IHttpActionResult Login(LoginData loginData)
        {
            var result = JwtController.Instance.LoginUser(Request, loginData);
            return ReplyWith(result);
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
        [HttpPost]
        [AllowAnonymous]
        public IHttpActionResult ExtendToken(RenewalDto rtoken)
        {
            var result = JwtController.Instance.RenewToken(Request, rtoken.RenewalToken);
            return ReplyWith(result);
        }

        #endregion

        #region helpers

        private IHttpActionResult ReplyWith(LoginResultData result)
        {
            if (result == null)
            {
                return Unauthorized();
            }

            if (!string.IsNullOrEmpty(result.Error))
            {
                //HACK: this will return the scheme with the error message as a challenge; non-standard method
                return Unauthorized(new AuthenticationHeaderValue(JwtController.AuthScheme, result.Error));
            }

            return Ok(result);
        }

        #endregion

        #region Testing APIs

        // Test API Method 1
        [HttpGet]
        public IHttpActionResult TestGet()
        {
            var identity = System.Threading.Thread.CurrentPrincipal.Identity;
            var reply = $"Hello {identity.Name}! You are authenticated through {identity.AuthenticationType}.";
            return Ok(new { reply });
        }

        // Test API Method 2
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IHttpActionResult TestPost(TestPostData something)
        {
            var identity = System.Threading.Thread.CurrentPrincipal.Identity;
            var reply = $"Hello {identity.Name}! You are authenticated through {identity.AuthenticationType}." +
                        $" You said: ({something.Text})";
            return Ok(new { reply });
        }

        [JsonObject]
        public class TestPostData
        {
            [JsonProperty("text")]
            public string Text;
        }

        #endregion
    }
}
