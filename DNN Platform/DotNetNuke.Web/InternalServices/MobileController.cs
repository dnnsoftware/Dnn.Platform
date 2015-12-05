#region Copyright
//
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2016
// by DotNetNuke Corporation
//
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated
// documentation files (the "Software"), to deal in the Software without restriction, including without limitation
// the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and
// to permit persons to whom the Software is furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all copies or substantial portions
// of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED
// TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL
// THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF
// CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
// DEALINGS IN THE SOFTWARE.
#endregion

using System.Threading;
using System.Web.Http;
using DotNetNuke.Web.Api;
using DotNetNuke.Web.Api.Internal.Auth.Jwt;

namespace DotNetNuke.Web.InternalServices
{
    [DnnAuthorize]
    public class MobileController : DnnApiController
    {
        /// <summary>
        /// Clients that want to go cookie-less should call this API to login and receive
        /// a Json Web Token (JWT) that allows them to authenticate the users to other
        ///  API methods afterwards.
        /// </summary>
        /// <param name="loginData"></param>
        /// <returns></returns>
        [HttpPost]
        [AllowAnonymous]
        public IHttpActionResult Login(LoginData loginData)
        {
            var tuple = JwtUtil.LoginUser(Request, loginData);

            return tuple == null
                ? (IHttpActionResult)Unauthorized()
                : Ok(new { name = tuple.Item1, token = tuple.Item2 });
        }

        // Test API Method 1
        [HttpGet]
        [RequireJwt]
        public IHttpActionResult Hello()
        {
            var identity = Thread.CurrentPrincipal.Identity;
            var reply = $"Hello {identity.Name}! " +
                        $"You are authenticated through {identity.AuthenticationType}";
            return Ok(new { reply });
        }

        // Test API Method 2
        [HttpPost]
        [RequireJwt]
        [ValidateAntiForgeryToken]
        public IHttpActionResult HelloPost(PostDate something)
        {
            var identity = Thread.CurrentPrincipal.Identity;
            var reply = $"Hello {identity.Name}! " +
                        $"You are authenticated through {identity.AuthenticationType}" +
                        $"You said: ({something.Text})";
            return Ok(new { reply });
        }
    }

    public class PostDate
    {
        public string Text;
    }
}
