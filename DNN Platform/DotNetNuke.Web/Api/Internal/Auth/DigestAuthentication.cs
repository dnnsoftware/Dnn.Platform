// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.Api.Internal.Auth
{
    using System;
    using System.Security.Cryptography;
    using System.Security.Principal;
    using System.Text;

    using DotNetNuke.Entities.Users;
    using DotNetNuke.Security.Membership;

    internal class DigestAuthentication
    {
        internal const string AuthenticationScheme = "Digest";
        private static readonly MD5 Md5 = new MD5CryptoServiceProvider();
        private readonly int _portalId;
        private readonly string _ipAddress;
        private DigestAuthenticationRequest _request;
        private string _password;

        public DigestAuthentication(DigestAuthenticationRequest request, int portalId, string ipAddress)
        {
            this._request = request;
            this._portalId = portalId;
            this._ipAddress = ipAddress ?? string.Empty;
            this.AuthenticateRequest();
        }

        public DigestAuthenticationRequest Request
        {
            get { return this._request; }
            set { this._request = value; }
        }

        public bool IsValid { get; private set; }

        public bool IsNonceStale { get; private set; }

        public IPrincipal User { get; private set; }

        public string CalculateHashedDigest()
        {
            return CreateMd5HashBinHex(this.GenerateUnhashedDigest());
        }

        private static string CreateMd5HashBinHex(string val)
        {
            // Services.Logging.LoggingController.SimpleLog(String.Format("Creating Hash for {0}", val))
            // Services.Logging.LoggingController.SimpleLog(String.Format("Back and forth: {0}", Encoding.Default.GetString(Encoding.Default.GetBytes(val))))
            byte[] bha1 = Md5.ComputeHash(Encoding.Default.GetBytes(val));
            string ha1 = string.Empty;
            for (int i = 0; i <= 15; i++)
            {
                ha1 += string.Format("{0:x02}", bha1[i]);
            }

            return ha1;
        }

        // the nonce is created in DotNetNuke.Web.Api.DigestAuthMessageHandler
        private static bool IsNonceValid(string nonce)
        {
            DateTime expireTime;

            int numPadChars = nonce.Length % 4;
            if (numPadChars > 0)
            {
                numPadChars = 4 - numPadChars;
            }

            string newNonce = nonce.PadRight(nonce.Length + numPadChars, '=');

            try
            {
                byte[] decodedBytes = Convert.FromBase64String(newNonce);
                string expireStr = Encoding.Default.GetString(decodedBytes);
                expireTime = DateTime.Parse(expireStr);
            }
            catch (FormatException)
            {
                return false;
            }

            return DateTime.Now <= expireTime;
        }

        private void AuthenticateRequest()
        {
            this._password = this.GetPassword(this.Request);
            if (this._password != null)
            {
                this.IsNonceStale = !IsNonceValid(this._request.RequestParams["nonce"]);

                // Services.Logging.LoggingController.SimpleLog(String.Format("Request hash: {0} - Response Hash: {1}", _request.RequestParams("response"), HashedDigest))
                if ((!this.IsNonceStale) && this._request.RequestParams["response"] == this.CalculateHashedDigest())
                {
                    this.IsValid = true;
                    this.User = new GenericPrincipal(new GenericIdentity(this._request.RawUsername, AuthenticationScheme), null);
                }
            }
        }

        private string GetPassword(DigestAuthenticationRequest request)
        {
            UserInfo user = UserController.GetUserByName(this._portalId, request.CleanUsername);
            if (user == null)
            {
                user = UserController.GetUserByName(this._portalId, request.RawUsername);
            }

            if (user == null)
            {
                return null;
            }

            var password = UserController.GetPassword(ref user, string.Empty);

            // Try to validate user
            var loginStatus = UserLoginStatus.LOGIN_FAILURE;
            user = UserController.ValidateUser(this._portalId, user.Username, password, "DNN", string.Empty, this._ipAddress, ref loginStatus);

            return user != null ? password : null;
        }

        private string GenerateUnhashedDigest()
        {
            string a1 = string.Format("{0}:{1}:{2}", this._request.RequestParams["username"].Replace("\\\\", "\\"),
                                      this._request.RequestParams["realm"], this._password);
            string ha1 = CreateMd5HashBinHex(a1);
            string a2 = string.Format("{0}:{1}", this._request.HttpMethod, this._request.RequestParams["uri"]);
            string ha2 = CreateMd5HashBinHex(a2);
            string unhashedDigest;
            if (this._request.RequestParams["qop"] != null)
            {
                unhashedDigest = string.Format("{0}:{1}:{2}:{3}:{4}:{5}", ha1, this._request.RequestParams["nonce"],
                                               this._request.RequestParams["nc"], this._request.RequestParams["cnonce"],
                                               this._request.RequestParams["qop"], ha2);
            }
            else
            {
                unhashedDigest = string.Format("{0}:{1}:{2}", ha1, this._request.RequestParams["nonce"], ha2);
            }

            // Services.Logging.LoggingController.SimpleLog(A1, HA1, A2, HA2, unhashedDigest)
            return unhashedDigest;
        }
    }
}
