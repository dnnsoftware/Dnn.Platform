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

using Newtonsoft.Json;

namespace DotNetNuke.Web.Api.Internal.Auth.Jwt
{
    // ReSharper disable InconsistentNaming

    /// <summary>
    /// The supported algorithms for this implementation Json Web Token (JWT).
    /// </summary>
    public enum JwtSupportedAlgorithms
    {
        Unsupported,
        HS256,
        /*
        HS384,
        HS512
         */
    }

    /// <summary>
    /// Structure used for the Login to obtain a Json Web Token (JWT).
    /// </summary>
    [JsonObject]
    public struct LoginData
    {
        [JsonProperty("u")]
        public string Username;
        [JsonProperty("p")]
        public string Password;
    }

    /// <summary>
    /// Standard Json Web Token (JWT) header
    /// </summary>
    [JsonObject]
    public class JwtHeader
    {
        /// <summary>
        /// The type of the token, which is JWT.
        /// </summary>
        [JsonProperty("type")]
        public string Type { get; set; }

        /// <summary>
        /// The hashing algorithm used to encrypt this message. Currently we are supporting the algorithms
        /// defined by <see cref="JwtSupportedAlgorithms"/> only.
        /// </summary>
        [JsonProperty("alg")]
        public string Algorithm { get; set; }
    }

    /// <summary>
    /// DNN specific JWT payload
    /// </summary>
    [JsonObject]
    public class DnnJwtPayload
    {
        /// <summary>
        /// The user name this token issued for.
        /// </summary>
        [JsonProperty("sub")]
        public string Username { get; set; }

        /// <summary>
        /// Represents the Authorization server (Token Issuer) party.
        /// </summary>
        [JsonProperty("iss")]
        public string Issuer { get; set; }

        /// <summary>
        /// The time which this JWT must not be used before, this claim contains UNIX time vale.
        /// </summary>
        [JsonProperty("nbf")]
        public long NotBefore { get; set; }

        /// <summary>
        /// The expiration time of the JWT, this claim contains UNIX time value.
        /// </summary>
        [JsonProperty("exp")]
        public long Expiration { get; set; }

        /// <summary>
        /// The Portal ID which this user is validated under
        /// </summary>
        [JsonProperty("portal")]
        public int PortalId { get; set; }

        /// <summary>
        /// This is used as a random value which is part of the algorithm's "secret" value.
        /// See token validation in <see cref="JwtAuthMessageHandler"/>
        /// </summary>
        [JsonProperty("session")]
        public string SessionId { get; set; }
    }
}