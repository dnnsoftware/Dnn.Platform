#region Copyright

// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2017
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

//Based on the work of:

// Base oAuth Class for Twitter and LinkedIn
// Author: Eran Sandler
// Code Url: http://oauth.net/code/
// Author Url: http://eran.sandler.co.il/
//
// Some modifications by Shannon Whitley
// Author Url: http://voiceoftech.com/
//
// Additional modifications by Evan Smith (DNN-4143 & DNN-6265)
// Author Url: http://skydnn.com


#endregion

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using DotNetNuke.Common;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Data;
using DotNetNuke.Entities.Users;
using DotNetNuke.Security.Membership;
using DotNetNuke.Instrumentation;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Services.Localization;

namespace DotNetNuke.Services.Authentication.OAuth
{
    public abstract class OAuthClientBase
    {
        #region Private Members
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(OAuthClientBase));
        private const string HMACSHA1SignatureType = "HMAC-SHA1";

        //oAuth 1
        private const string OAuthParameterPrefix = "oauth_";
        private const string OAuthConsumerKeyKey = "oauth_consumer_key";
        private const string OAuthCallbackKey = "oauth_callback";
        private const string OAuthVersionKey = "oauth_version";
        private const string OAuthSignatureKey = "oauth_signature";
        private const string OAuthSignatureMethodKey = "oauth_signature_method";
        private const string OAuthTimestampKey = "oauth_timestamp";
        private const string OAuthNonceKey = "oauth_nonce";
        private const string OAuthTokenSecretKey = "oauth_token_secret";
        private const string OAuthVerifierKey = "oauth_verifier";
        private const string OAuthCallbackConfirmedKey = "oauth_callback_confirmed";

        //oAuth 2
        private const string OAuthClientIdKey = "client_id";
        private const string OAuthClientSecretKey = "client_secret";
        private const string OAuthRedirectUriKey = "redirect_uri";
        private const string OAuthGrantTyepKey = "grant_type";
        private const string OAuthCodeKey = "code";

        private readonly Random random = new Random();

        private const string UnreservedChars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-_.~";
        
        //DNN-6265 - Support OAuth V2 optional parameter resource, which is required by Microsoft Azure Active
        //Directory implementation of OAuth V2
        private const string OAuthResourceKey = "resource";

        #endregion

        protected OAuthClientBase(int portalId, AuthMode mode, string service)
        {
            //Set default Expiry to 14 days 
            //oAuth v1 tokens do not expire
            //oAuth v2 tokens have an expiry
            AuthTokenExpiry = new TimeSpan(14, 0, 0, 0);
            Service = service;

            APIKey = OAuthConfigBase.GetConfig(Service, portalId).APIKey;
            APISecret = OAuthConfigBase.GetConfig(Service, portalId).APISecret;
            Mode = mode;

            CallbackUri = Mode == AuthMode.Login
                                    ? new Uri(Globals.LoginURL(String.Empty, false))
                                    : new Uri(Globals.RegisterURL(String.Empty, String.Empty));
        }

        #region Protected Properties

        protected const string OAuthTokenKey = "oauth_token";

        protected virtual string UserGuidKey 
        {
            get { return String.Empty; }
        }

        protected string APIKey { get; set; }
        protected string APISecret { get; set; }
        protected AuthMode Mode { get; set; }
        protected string OAuthVersion { get; set; }
        protected HttpMethod TokenMethod { get; set; }

        //oAuth 1
        protected string OAuthVerifier
        {
            get { return HttpContext.Current.Request.Params[OAuthVerifierKey]; }
        }
        protected Uri RequestTokenEndpoint { get; set; }
        protected HttpMethod RequestTokenMethod { get; set; }
        protected string TokenSecret { get; set; }
        protected string UserGuid { get; set; }

        //oAuth 1 and 2
        protected Uri AuthorizationEndpoint { get; set; }
        protected string AuthToken { get; set; }
        protected TimeSpan AuthTokenExpiry { get; set; }
        protected Uri MeGraphEndpoint { get; set; }
        protected Uri TokenEndpoint { get; set; }
        protected string OAuthHeaderCode { get; set; }

        //oAuth 2
        protected string AuthTokenName { get; set; }        
        protected string Scope { get; set; }
		protected string AccessToken { get; set; }
        protected string VerificationCode
        {
            get { return HttpContext.Current.Request.Params[OAuthCodeKey]; }
        }
        
        //DNN-6265 Support "Optional" Resource Parameter required by Azure AD Oauth V2 Solution
        protected string APIResource { get; set; }

        #endregion

        #region Public Properties

        public Uri CallbackUri { get; set; }
        public string Service { get; set; }

        public virtual bool PrefixServiceToUserName
        {
            get { return true; }
        }

        public virtual bool AutoMatchExistingUsers
        {
            get { return false; }
        }

        #endregion

        #region Private Methods

        private AuthorisationResult AuthorizeV1()
        {
            if (!IsCurrentUserAuthorized())
            {
                if (!HaveVerificationCode())
                {
                    string ret = null;

                    string response = RequestToken();

                    if (response.Length > 0)
                    {
                        //response contains token and token secret. We only need the token.
                        NameValueCollection qs = HttpUtility.ParseQueryString(response);

                        if (qs[OAuthCallbackConfirmedKey] != null)
                        {
                            if (qs[OAuthCallbackConfirmedKey] != "true")
                            {
                                throw new Exception("OAuth callback not confirmed.");
                            }
                        }

                        if (qs[OAuthTokenKey] != null)
                        {
                            ret = AuthorizationEndpoint + "?" + OAuthTokenKey + "=" + qs[OAuthTokenKey];

                            AuthToken = qs[OAuthTokenKey];
                            TokenSecret = qs[OAuthTokenSecretKey];
                            SaveTokenCookie("_request");
                        }
                    }

                    if (ret != null)
                    {
                        HttpContext.Current.Response.Redirect(ret, true);
                    }

                    return AuthorisationResult.RequestingCode;
                }

                ExchangeRequestTokenForToken();
            }

            return AuthorisationResult.Authorized;
        }

        private AuthorisationResult AuthorizeV2()
        {
            string errorReason = HttpContext.Current.Request.Params["error_reason"];
            bool userDenied = (errorReason != null);
            if (userDenied)
            {
                return AuthorisationResult.Denied;
            }

            if (!HaveVerificationCode())
            {
                var parameters = new List<QueryParameter>
                                        {
                                            new QueryParameter("scope", Scope),
                                            new QueryParameter(OAuthClientIdKey, APIKey),
                                            new QueryParameter(OAuthRedirectUriKey, HttpContext.Current.Server.UrlEncode(CallbackUri.ToString())),
                                            new QueryParameter("state", Service),
                                            new QueryParameter("response_type", "code")
                                        };

                HttpContext.Current.Response.Redirect(AuthorizationEndpoint + "?" + parameters.ToNormalizedString(), true);
                return AuthorisationResult.RequestingCode;
            }

            ExchangeCodeForToken();

            return String.IsNullOrEmpty(AuthToken) ? AuthorisationResult.Denied : AuthorisationResult.Authorized;
        }

        private string ComputeHash(HashAlgorithm hashAlgorithm, string data)
        {
            if (hashAlgorithm == null)
            {
                throw new ArgumentNullException("hashAlgorithm");
            }

            if (string.IsNullOrEmpty(data))
            {
                throw new ArgumentNullException("data");
            }

            byte[] dataBuffer = Encoding.ASCII.GetBytes(data);
            byte[] hashBytes = hashAlgorithm.ComputeHash(dataBuffer);

            return Convert.ToBase64String(hashBytes);
        }

        private void ExchangeCodeForToken()
        {
            IList<QueryParameter> parameters = new List<QueryParameter>();
            parameters.Add(new QueryParameter(OAuthClientIdKey, APIKey));
            parameters.Add(new QueryParameter(OAuthRedirectUriKey, HttpContext.Current.Server.UrlEncode(CallbackUri.ToString())));
            //DNN-6265 Support for OAuth V2 Secrets which are not URL Friendly
            parameters.Add(new QueryParameter(OAuthClientSecretKey, HttpContext.Current.Server.UrlEncode(APISecret.ToString())));
            parameters.Add(new QueryParameter(OAuthGrantTyepKey, "authorization_code"));
            parameters.Add(new QueryParameter(OAuthCodeKey, VerificationCode));

            //DNN-6265 Support for OAuth V2 optional parameter
            if (!String.IsNullOrEmpty(APIResource))
            {
                parameters.Add(new QueryParameter("resource", APIResource));
            }

            string responseText = ExecuteWebRequest(TokenMethod, TokenEndpoint, parameters.ToNormalizedString(), String.Empty);

            AuthToken = GetToken(responseText);
            AuthTokenExpiry = GetExpiry(responseText);
        }

        private void ExchangeRequestTokenForToken()
        {
            LoadTokenCookie("_request");

            string response = ExecuteAuthorizedRequest(HttpMethod.POST, TokenEndpoint);

            if (response.Length > 0)
            {
                //Store the Token and Token Secret
                NameValueCollection qs = HttpUtility.ParseQueryString(response);
                if (qs[OAuthTokenKey] != null)
                {
                    AuthToken = qs[OAuthTokenKey];
                }
                if (qs[OAuthTokenSecretKey] != null)
                {
                    TokenSecret = qs[OAuthTokenSecretKey];
                }
                if (qs[UserGuidKey] != null)
                {
                    UserGuid = qs[UserGuidKey];
                }
            }
        }

        private string ExecuteAuthorizedRequest(HttpMethod method, Uri uri)
        {
            string outUrl;
            List<QueryParameter> requestParameters;

            string nonce = GenerateNonce();
            string timeStamp = GenerateTimeStamp();

            string verifier = (uri == TokenEndpoint) ? OAuthVerifier: String.Empty;
            //Generate Signature
            string sig = GenerateSignature(uri,
                                            AuthToken,
                                            TokenSecret,
                                            String.Empty,
                                            verifier,
                                            method.ToString(),
                                            timeStamp,
                                            nonce,
                                            out outUrl,
                                            out requestParameters);


            var headerParameters = new List<QueryParameter>
                                       {
                                           new QueryParameter(OAuthConsumerKeyKey, APIKey),
                                           new QueryParameter(OAuthNonceKey, nonce),
                                           new QueryParameter(OAuthSignatureKey, sig),
                                           new QueryParameter(OAuthSignatureMethodKey, HMACSHA1SignatureType),
                                           new QueryParameter(OAuthTimestampKey, timeStamp),
                                           new QueryParameter(OAuthTokenKey, AuthToken),
                                           new QueryParameter(OAuthVersionKey, OAuthVersion)
                                       };
            if (uri == TokenEndpoint)
            {
                headerParameters.Add(new QueryParameter(OAuthVerifierKey, OAuthVerifier));
            }

            string ret = ExecuteWebRequest(method, uri, String.Empty, headerParameters.ToAuthorizationString());

            return ret;
        }

        private string ExecuteWebRequest(HttpMethod method, Uri uri, string parameters, string authHeader)
        {
            WebRequest request;

            if (method == HttpMethod.POST)
            {
                byte[] byteArray = Encoding.UTF8.GetBytes(parameters);

                request = WebRequest.CreateDefault(uri);
                request.Method = "POST";
                request.ContentType = "application/x-www-form-urlencoded";
                //request.ContentType = "text/xml";
                request.ContentLength = byteArray.Length;
				
				if (!String.IsNullOrEmpty(OAuthHeaderCode))
				{ 
					byte[] API64 = Encoding.UTF8.GetBytes(APIKey + ":" + APISecret); 
					string Api64Encoded = System.Convert.ToBase64String(API64); 
					//Authentication providers needing an "Authorization: Basic/bearer base64(clientID:clientSecret)" header. OAuthHeaderCode might be: Basic/Bearer/empty.
					request.Headers.Add("Authorization: " + OAuthHeaderCode + " " + Api64Encoded); 
				}

                if (!String.IsNullOrEmpty(parameters))
                {
                    Stream dataStream = request.GetRequestStream();
                    dataStream.Write(byteArray, 0, byteArray.Length);
                    dataStream.Close();
                }
            }
            else
            {
                request = WebRequest.CreateDefault(GenerateRequestUri(uri.ToString(), parameters));
            }

            //Add Headers
            if (!String.IsNullOrEmpty(authHeader))
            {
                request.Headers.Add(HttpRequestHeader.Authorization, authHeader);
            }

            try
            {
                using (WebResponse response = request.GetResponse())
                {
                    using (Stream responseStream = response.GetResponseStream())
                    {
                        if (responseStream != null)
                        {
                            using (var responseReader = new StreamReader(responseStream))
                            {
                                return responseReader.ReadToEnd();
                            }
                        }
                    }
                }
            }
            catch (WebException ex)
            {
                using (Stream responseStream = ex.Response.GetResponseStream())
                {
                    if (responseStream != null)
                    {
                        using (var responseReader = new StreamReader(responseStream))
                        {
                            Logger.ErrorFormat("WebResponse exception: {0}", responseReader.ReadToEnd());
                        }
                    }
                }
            }
            return null;
        }

        private string GenerateSignatureBase(Uri url, string token, string callbackurl, string oauthVerifier, string httpMethod, string timeStamp, string nonce, out string normalizedUrl, out List<QueryParameter> requestParameters)
        {
            if (token == null)
            {
                token = string.Empty;
            }

            if (string.IsNullOrEmpty(httpMethod))
            {
                throw new ArgumentNullException("httpMethod");
            }

            requestParameters = GetQueryParameters(url.Query);
            requestParameters.Add(new QueryParameter(OAuthVersionKey, OAuthVersion));
            requestParameters.Add(new QueryParameter(OAuthNonceKey, nonce));
            requestParameters.Add(new QueryParameter(OAuthTimestampKey, timeStamp));
            requestParameters.Add(new QueryParameter(OAuthSignatureMethodKey, HMACSHA1SignatureType));
            requestParameters.Add(new QueryParameter(OAuthConsumerKeyKey, APIKey));

            if (!string.IsNullOrEmpty(callbackurl))
            {
                requestParameters.Add(new QueryParameter(OAuthCallbackKey, UrlEncode(callbackurl)));
            }

            if (!string.IsNullOrEmpty(oauthVerifier))
            {
                requestParameters.Add(new QueryParameter(OAuthVerifierKey, oauthVerifier));
            }

            if (!string.IsNullOrEmpty(token))
            {
                requestParameters.Add(new QueryParameter(OAuthTokenKey, token));
            }

            requestParameters.Sort(new QueryParameterComparer());

            normalizedUrl = string.Format("{0}://{1}", url.Scheme, url.Host);
            if (!((url.Scheme == "http" && url.Port == 80) || (url.Scheme == "https" && url.Port == 443)))
            {
                normalizedUrl += ":" + url.Port;
            }
            normalizedUrl += url.AbsolutePath;
            string normalizedRequestParameters = requestParameters.ToNormalizedString();

            var signatureBase = new StringBuilder();
            signatureBase.AppendFormat("{0}&", httpMethod.ToUpper());
            signatureBase.AppendFormat("{0}&", UrlEncode(normalizedUrl));
            signatureBase.AppendFormat("{0}", UrlEncode(normalizedRequestParameters));

            return signatureBase.ToString();
        }

        private string GenerateSignatureUsingHash(string signatureBase, HashAlgorithm hash)
        {
            return ComputeHash(hash, signatureBase);
        }

        private List<QueryParameter> GetQueryParameters(string parameters)
        {
            if (parameters.StartsWith("?"))
            {
                parameters = parameters.Remove(0, 1);
            }

            var result = new List<QueryParameter>();

            if (!string.IsNullOrEmpty(parameters))
            {
                string[] p = parameters.Split('&');
                foreach (string s in p)
                {
                    if (!string.IsNullOrEmpty(s) && !s.StartsWith(OAuthParameterPrefix))
                    {
                        if (s.IndexOf('=') > -1)
                        {
                            string[] temp = s.Split('=');
                            result.Add(new QueryParameter(temp[0], temp[1]));
                        }
                        else
                        {
                            result.Add(new QueryParameter(s, string.Empty));
                        }
                    }
                }
            }

            return result;
        }

        private string RequestToken()
        {
            string outUrl;
            List<QueryParameter> requestParameters;

            string nonce = GenerateNonce();
            string timeStamp = GenerateTimeStamp();

            string sig = GenerateSignature(RequestTokenEndpoint,
                                            String.Empty,
                                            String.Empty,
                                            CallbackUri.OriginalString,
                                            String.Empty,
                                            RequestTokenMethod.ToString(),
                                            timeStamp,
                                            nonce,
                                            out outUrl,
                                            out requestParameters);

            var headerParameters = new List<QueryParameter>
                                       {
                                           new QueryParameter(OAuthCallbackKey, CallbackUri.OriginalString),
                                           new QueryParameter(OAuthConsumerKeyKey, APIKey),
                                           new QueryParameter(OAuthNonceKey, nonce),
                                           new QueryParameter(OAuthSignatureKey, sig),
                                           new QueryParameter(OAuthSignatureMethodKey, HMACSHA1SignatureType),
                                           new QueryParameter(OAuthTimestampKey, timeStamp),
                                           new QueryParameter(OAuthVersionKey, OAuthVersion)
                                       };

            string ret = ExecuteWebRequest(RequestTokenMethod, new Uri(outUrl), String.Empty, headerParameters.ToAuthorizationString());

            return ret;
        }

        private void SaveTokenCookie(string suffix)
        {
            var authTokenCookie = new HttpCookie(AuthTokenName + suffix) { Path = (!string.IsNullOrEmpty(Globals.ApplicationPath) ? Globals.ApplicationPath : "/") };
            authTokenCookie.Values[OAuthTokenKey] = AuthToken;
            authTokenCookie.Values[OAuthTokenSecretKey] = TokenSecret;
            authTokenCookie.Values[UserGuidKey] = UserGuid;

            authTokenCookie.Expires = DateTime.Now.Add(AuthTokenExpiry);
            HttpContext.Current.Response.SetCookie(authTokenCookie);
        }

        private Uri GenerateRequestUri(string url, string parameters)
        {
            if (string.IsNullOrEmpty(parameters))
            {
                return new Uri(url);
            }

            return new Uri(string.Format("{0}{1}{2}", url, url.Contains("?") ? "&" : "?", parameters));
        }

        #endregion

        #region Protected Methods

        /// <summary>
        /// Generate the timestamp for the signature
        /// </summary>
        /// <returns></returns>
        protected virtual string GenerateTimeStamp()
        {
            // Default implementation of UNIX time of the current UTC time
            TimeSpan ts = DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0);
            return Convert.ToInt64(ts.TotalSeconds).ToString(CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Generate a nonce
        /// </summary>
        /// <returns></returns>
        protected virtual string GenerateNonce()
        {
            // Just a simple implementation of a random number between 123400 and 9999999
            return random.Next(123400, 9999999).ToString(CultureInfo.InvariantCulture);
        }

        protected virtual TimeSpan GetExpiry(string responseText)
        {
            return TimeSpan.MinValue;
        }

        protected virtual string GetToken(string responseText)
        {
            return responseText;
        }

        protected void LoadTokenCookie(string suffix)
        {
            HttpCookie authTokenCookie = HttpContext.Current.Request.Cookies[AuthTokenName + suffix];
            if (authTokenCookie != null)
            {
                if (authTokenCookie.HasKeys)
                {
                    AuthToken = authTokenCookie.Values[OAuthTokenKey];
                    TokenSecret = authTokenCookie.Values[OAuthTokenSecretKey];
                    UserGuid = authTokenCookie.Values[UserGuidKey];
                }
            }
        }

        #endregion

        public virtual void AuthenticateUser(UserData user, PortalSettings settings, string IPAddress, Action<NameValueCollection> addCustomProperties, Action<UserAuthenticatedEventArgs> onAuthenticated)
        {
            var loginStatus = UserLoginStatus.LOGIN_FAILURE;

            string userName = PrefixServiceToUserName ? Service + "-" + user.Id : user.Id;
            string token = Service + "-" + user.Id;

            UserInfo objUserInfo;

            if (AutoMatchExistingUsers)
            {
                objUserInfo = MembershipProvider.Instance().GetUserByUserName(settings.PortalId, userName);
                if (objUserInfo != null)
                {
                    //user already exists... lets check for a token next... 
                    var dnnAuthToken = MembershipProvider.Instance().GetUserByAuthToken(settings.PortalId, token, Service);
                    if (dnnAuthToken == null)
                    {
                        DataProvider.Instance().AddUserAuthentication(objUserInfo.UserID, Service, token, objUserInfo.UserID);
                    }
                }
            }

            objUserInfo = UserController.ValidateUser(settings.PortalId, userName, "",
                                                                Service, token,
                                                                settings.PortalName, IPAddress,
                                                                ref loginStatus);


            //Raise UserAuthenticated Event
            var eventArgs = new UserAuthenticatedEventArgs(objUserInfo, token, loginStatus, Service)
                                            {
                                                AutoRegister = true,
                                                UserName = userName,
                                            };

            var profileProperties = new NameValueCollection();

            if (objUserInfo == null || (string.IsNullOrEmpty(objUserInfo.FirstName) && !string.IsNullOrEmpty(user.FirstName)))
            {
                profileProperties.Add("FirstName", user.FirstName);
            }
            if (objUserInfo == null || (string.IsNullOrEmpty(objUserInfo.LastName) && !string.IsNullOrEmpty(user.LastName)))
            {
                profileProperties.Add("LastName", user.LastName);
            }
            if (objUserInfo == null || (string.IsNullOrEmpty(objUserInfo.Email) && !string.IsNullOrEmpty(user.Email)))
            {
                profileProperties.Add("Email", user.PreferredEmail);
            }
            if (objUserInfo == null || (string.IsNullOrEmpty(objUserInfo.DisplayName) && !string.IsNullOrEmpty(user.DisplayName)))
            {
                profileProperties.Add("DisplayName", user.DisplayName);
            }
            if (objUserInfo == null || (string.IsNullOrEmpty(objUserInfo.Profile.GetPropertyValue("ProfileImage")) && !string.IsNullOrEmpty(user.ProfileImage)))
            {
                profileProperties.Add("ProfileImage", user.ProfileImage);
            }
            if (objUserInfo == null || (string.IsNullOrEmpty(objUserInfo.Profile.GetPropertyValue("Website")) && !string.IsNullOrEmpty(user.Website)))
            {
                profileProperties.Add("Website", user.Website);
            }
            if ((objUserInfo == null || (string.IsNullOrEmpty(objUserInfo.Profile.GetPropertyValue("PreferredLocale")))) && !string.IsNullOrEmpty(user.Locale))
            {
                if (LocaleController.IsValidCultureName(user.Locale.Replace('_', '-')))
                {
                    profileProperties.Add("PreferredLocale", user.Locale.Replace('_', '-'));
                }
                else
                {
                    profileProperties.Add("PreferredLocale", settings.CultureCode);
                }
            }

            if (objUserInfo == null || (string.IsNullOrEmpty(objUserInfo.Profile.GetPropertyValue("PreferredTimeZone"))))
            {
                if (String.IsNullOrEmpty(user.TimeZoneInfo))
                {
                    int timeZone;
                    if (Int32.TryParse(user.Timezone, out timeZone))
                    {
                        var timeZoneInfo = Localization.Localization.ConvertLegacyTimeZoneOffsetToTimeZoneInfo(timeZone);

                        profileProperties.Add("PreferredTimeZone", timeZoneInfo.Id);
                    }
                }
                else
                {
                    profileProperties.Add("PreferredTimeZone", user.TimeZoneInfo);
                }
            }

            addCustomProperties(profileProperties);

            eventArgs.Profile = profileProperties;

            if (Mode == AuthMode.Login)
            {
                SaveTokenCookie(String.Empty);
            }

            onAuthenticated(eventArgs);
        }

        public virtual AuthorisationResult Authorize()
        {
            if (OAuthVersion == "1.0")
            {
                return AuthorizeV1();
            }
            return AuthorizeV2();
        }

        /// <summary>
        /// Generates a signature using the HMAC-SHA1 algorithm
        /// </summary>
        /// <param name="url">The full url that needs to be signed including its non OAuth url parameters</param>
        /// <param name="token">The token, if available. If not available pass null or an empty string</param>
        /// <param name="tokenSecret">The token secret, if available. If not available pass null or an empty string</param>
        /// <param name="callbackurl"> </param>
        /// <param name="oauthVerifier">This value MUST be included when exchanging Request Tokens for Access Tokens. Otherwise pass a null or an empty string</param>
        /// <param name="httpMethod">The http method used. Must be a valid HTTP method verb (POST,GET,PUT, etc)</param>
        /// <param name="timeStamp"> </param>
        /// <param name="nonce"> </param>
        /// <param name="normalizedUrl"> </param>
        /// <param name="requestParameters"> </param>
        /// <returns>A base64 string of the hash value</returns>
        public string GenerateSignature(Uri url, string token, string tokenSecret, string callbackurl, string oauthVerifier, string httpMethod, string timeStamp, string nonce, out string normalizedUrl, out List<QueryParameter> requestParameters)
        {
            string signatureBase = GenerateSignatureBase(url, token, callbackurl, oauthVerifier, httpMethod, timeStamp, nonce, out normalizedUrl, out requestParameters);

            var hmacsha1 = new HMACSHA1
                               {
                                   Key = Encoding.ASCII.GetBytes(string.Format("{0}&{1}", UrlEncode(APISecret),
                                                                             string.IsNullOrEmpty(tokenSecret)
                                                                                 ? ""
                                                                                 : UrlEncode(tokenSecret)))
                               };

            return GenerateSignatureUsingHash(signatureBase, hmacsha1);
        }

        public virtual TUserData GetCurrentUser<TUserData>() where TUserData : UserData
        {
            LoadTokenCookie(String.Empty);

            if (!IsCurrentUserAuthorized())
            {
                return null;
            }

            var accessToken = string.IsNullOrEmpty(AccessToken) ? "access_token=" + AuthToken : AccessToken + "=" + AuthToken;
            string responseText = (OAuthVersion == "1.0")
                            ? ExecuteAuthorizedRequest(HttpMethod.GET, MeGraphEndpoint)
                            : ExecuteWebRequest(HttpMethod.GET, GenerateRequestUri(MeGraphEndpoint.ToString(), accessToken), null, String.Empty);
            var user = Json.Deserialize<TUserData>(responseText);
            return user;
        }

        public bool HaveVerificationCode()
        {
            return (OAuthVersion == "1.0") ? (OAuthVerifier != null) : (VerificationCode != null);
        }

        public bool IsCurrentService()
        {
            string service = HttpContext.Current.Request.Params["state"];
            return !String.IsNullOrEmpty(service) && service == Service;
        }

        public Boolean IsCurrentUserAuthorized()
        {
            return !String.IsNullOrEmpty(AuthToken);
        }

        public void RemoveToken()
        {
            var authTokenCookie = new HttpCookie(AuthTokenName)
            {
                Expires = DateTime.Now.AddDays(-30),
                Path = (!string.IsNullOrEmpty(Globals.ApplicationPath) ? Globals.ApplicationPath : "/")
            };
            HttpContext.Current.Response.SetCookie(authTokenCookie);
        }

        /// <summary>
        /// This is a different Url Encode implementation since the default .NET one outputs the percent encoding in lower case.
        /// While this is not a problem with the percent encoding spec, it is used in upper case throughout OAuth
        /// </summary>
        /// <param name="value">The value to Url encode</param>
        /// <returns>Returns a Url encoded string</returns>
        public static string UrlEncode(string value)
        {
            var result = new StringBuilder();

            foreach (char symbol in value)
            {
                if (UnreservedChars.IndexOf(symbol) != -1)
                {
                    result.Append(symbol);
                }
                else
                {
                    result.Append('%' + String.Format("{0:X2}", (int)symbol));
                }
            }

            return result.ToString();
        }
    }
}
