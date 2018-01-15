#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2018
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
using System;
using System.Security.Cryptography;
using System.Security.Principal;
using System.Text;
using DotNetNuke.Entities.Users;
using DotNetNuke.Security.Membership;

namespace DotNetNuke.Web.Api.Internal.Auth
{
    internal class DigestAuthentication
    {
        internal const string AuthenticationScheme = "Digest";
        private static readonly MD5 Md5 = new MD5CryptoServiceProvider();
        private DigestAuthenticationRequest _request;
        private string _password;
        private readonly int _portalId;
        private readonly string _ipAddress;

        public DigestAuthenticationRequest Request
        {
            get { return _request; }
            set { _request = value; }
        }

        public bool IsValid { get; private set; }

        public bool IsNonceStale { get; private set; }

        public string CalculateHashedDigest()
        {
            return CreateMd5HashBinHex(GenerateUnhashedDigest());
        }

        public IPrincipal User { get; private set; }

        public DigestAuthentication(DigestAuthenticationRequest request, int portalId, string ipAddress)
        {
            _request = request;
            _portalId = portalId;
            _ipAddress = ipAddress ?? "";
            AuthenticateRequest();
        }

        private void AuthenticateRequest()
        {
            _password = GetPassword(Request);
            if(_password != null)
            {
                IsNonceStale = ! (IsNonceValid(_request.RequestParams["nonce"]));
                //Services.Logging.LoggingController.SimpleLog(String.Format("Request hash: {0} - Response Hash: {1}", _request.RequestParams("response"), HashedDigest))
                if ((! IsNonceStale) && _request.RequestParams["response"] == CalculateHashedDigest())
                {
                    IsValid = true;
                    User = new GenericPrincipal(new GenericIdentity(_request.RawUsername, AuthenticationScheme), null);
                }
            }
        }

        private string GetPassword(DigestAuthenticationRequest request)
        {
            UserInfo user = UserController.GetUserByName(_portalId, request.CleanUsername);
            if (user == null)
            {
                user = UserController.GetUserByName(_portalId, request.RawUsername);
            }
            if (user == null)
            {
                return null;
            }
            var password = UserController.GetPassword(ref user, "");
            
            //Try to validate user
            var loginStatus = UserLoginStatus.LOGIN_FAILURE;
            user = UserController.ValidateUser(_portalId, user.Username, password, "DNN", "", _ipAddress, ref loginStatus);

            return user != null ? password : null;
        }

        private string GenerateUnhashedDigest()
        {
            string a1 = String.Format("{0}:{1}:{2}", _request.RequestParams["username"].Replace("\\\\", "\\"),
                                      _request.RequestParams["realm"], _password);
            string ha1 = CreateMd5HashBinHex(a1);
            string a2 = String.Format("{0}:{1}", _request.HttpMethod, _request.RequestParams["uri"]);
            string ha2 = CreateMd5HashBinHex(a2);
            string unhashedDigest;
            if (_request.RequestParams["qop"] != null)
            {
                unhashedDigest = String.Format("{0}:{1}:{2}:{3}:{4}:{5}", ha1, _request.RequestParams["nonce"],
                                               _request.RequestParams["nc"], _request.RequestParams["cnonce"],
                                               _request.RequestParams["qop"], ha2);
            }
            else
            {
                unhashedDigest = String.Format("{0}:{1}:{2}", ha1, _request.RequestParams["nonce"], ha2);
            }
            //Services.Logging.LoggingController.SimpleLog(A1, HA1, A2, HA2, unhashedDigest)
            return unhashedDigest;
        }

        private static string CreateMd5HashBinHex(string val)
        {
            //Services.Logging.LoggingController.SimpleLog(String.Format("Creating Hash for {0}", val))
            //Services.Logging.LoggingController.SimpleLog(String.Format("Back and forth: {0}", Encoding.Default.GetString(Encoding.Default.GetBytes(val))))
            byte[] bha1 = Md5.ComputeHash(Encoding.Default.GetBytes(val));
            string ha1 = "";
            for (int i = 0; i <= 15; i++)
            {
                ha1 += String.Format("{0:x02}", bha1[i]);
            }
            return ha1;
        }

        //the nonce is created in DotNetNuke.Web.Api.DigestAuthMessageHandler
        private static bool IsNonceValid(string nonce)
        {
            DateTime expireTime;

            int numPadChars = nonce.Length%4;
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

            return (DateTime.Now <= expireTime);
        }
    }
}