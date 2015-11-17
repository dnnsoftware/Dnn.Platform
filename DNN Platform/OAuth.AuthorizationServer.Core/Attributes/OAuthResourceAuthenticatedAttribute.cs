﻿using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Principal;
using System.Web;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Results;
using System.Web.Http.Routing;

using DotNetNuke.Web.Api;
using DotNetOpenAuth.OAuth2.Messages;
using OAuth.AuthorizationServer.Core.Data.Model;
using OAuth.AuthorizationServer.Core.Data.Repositories;
using OAuth.AuthorizationServer.Core.Server;
using OAuth.AuthorizationServer.Core.Utilities;
using DNOA = DotNetOpenAuth.OAuth2;
using DotNetNuke.Common;

namespace DotNetNuke.Web.Api
{

    /// <summary>
    /// This authorization attribute is applied to the authorization methods in our OAuthController
    /// to ensure the user has been authenticated by the resource being requested
    /// </summary>
    public class OauthResourceAuthenticatedAttribute : AuthorizeAttribute, IOverrideDefaultAuthLevel
    {
        private readonly DNOA.AuthorizationServer _authorizationServer = new DNOA.AuthorizationServer(new AuthorizationServerHost());
       // private readonly ResourceRepository _resourceRepository = new ResourceRepository();
        private Resource _targetResource;

        /// <summary>
        /// retrieves querystring from HttpRequestMessage
        /// </summary>
        /// <param name="request"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static string GetQueryString(HttpRequestMessage request, string key)
        {
            // IEnumerable<KeyValuePair<string,string>> - right!
            var queryStrings = request.GetQueryNameValuePairs();
            if (queryStrings == null)
                return null;

            var match = queryStrings.FirstOrDefault(kv => string.Compare(kv.Key, key, true) == 0);
            if (string.IsNullOrEmpty(match.Value))
                return null;

            return match.Value;
        }
        /// <summary>
        /// retrieve cookies from HttpRequestMessage
        /// </summary>
        /// <param name="request"></param>
        /// <param name="cookieName"></param>
        /// <returns></returns>
        public static string GetCookie( HttpRequestMessage request, string cookieName)
        {
            CookieHeaderValue cookie = request.Headers.GetCookies(cookieName).FirstOrDefault();
            if (cookie != null)
                return cookie[cookieName].Value;

            return null;
        }
        /// <summary>
        /// handle unauthorized request
        /// </summary>
        /// <param name="actionContext"></param>
        protected override void HandleUnauthorizedRequest(HttpActionContext actionContext)
        {
            string domainName = "http://" + Globals.GetDomainName(HttpContext.Current.Request) + _targetResource.AuthenticationUrl;

            string redirect = string.Format("{0}?returnUrl={1}", domainName,
                actionContext.Request.RequestUri.ToString());
           // redirect = redirect.Replace("&state=", "?state=");
            var response = actionContext.Request.CreateResponse(System.Net.HttpStatusCode.Redirect);
            response.Headers.Add("Location", redirect);
            actionContext.Response = response;
        
        }

        /// <summary>
        /// check if request is authorized
        /// </summary>
        /// <param name="actionContext"></param>
        /// <returns></returns>
        protected override bool IsAuthorized(HttpActionContext actionContext)
        {
            HttpContext httpContext = HttpContext.Current;
            
            // Figure out what resource the request is intending to access to see if the
            // user has already authenticated to with it
            EndUserAuthorizationRequest pendingRequest;
            try
            {
                 pendingRequest = _authorizationServer.ReadAuthorizationRequest();
            }
            catch (Exception)
            {

                throw new HttpException(Convert.ToInt32(HttpStatusCode.BadRequest),"Invalid Client");

            }
            
            if (pendingRequest == null)
            {
                throw new HttpException(Convert.ToInt32(HttpStatusCode.BadRequest), "Missing authorization request.");
            }

            try
            {
               // _targetResource = _resourceRepository.FindWithSupportedScopes(pendingRequest.Scope);
                _targetResource  =OAUTHDataController.ResourceRepositoryFindWithSupportedScopes(pendingRequest.Scope);

                // Above will return null if no resource supports all of the requested scopes
                if (_targetResource == null)
                {
                    throw new HttpException(Convert.ToInt32(HttpStatusCode.BadRequest), "Bad authorization request.");
                }
            }
            catch (Exception)
            {
                throw new HttpException(Convert.ToInt32(HttpStatusCode.BadRequest), "Bad authorization request.");
            }

            // User is considered authorized if in possession of token that originated from the resource's login page,
            // Name of token is determined by the resource configuration
            string tokenName = _targetResource.AuthenticationTokenName;
            string encryptedToken = httpContext.Request[tokenName]; //could be in cookie if previously logged in, or querystring if just logged in

           
            
            if (string.IsNullOrWhiteSpace(encryptedToken))
            {
                // No token, so unauthorized
                return false;
            }

            // Validate this thing came from us via shared secret with the resource's login page
            // The implementation here ideally could be generalized a bit better or standardized
            string encryptionKey = _targetResource.AuthenticationKey;
            string decryptedToken = EncodingUtility.Decode(encryptedToken, encryptionKey);
            string[] tokenContentParts = decryptedToken.Split(';');

            string name = tokenContentParts[0];
            // DateTime loginDate = DateTime.Parse(tokenContentParts[1]);
            DateTime loginDate = DateTime.Parse(tokenContentParts[1]).ToUniversalTime();
            
            bool storeCookie = bool.Parse(tokenContentParts[2]);

            if ((DateTime.Now.Subtract(loginDate) > TimeSpan.FromDays(7)))
            {
                // Expired, remove cookie if present and flag user as unauthorized
                httpContext.Response.Cookies.Remove(tokenName);
                
                
                return false;
            }

            // Things look good.  
            // Set principal for the authorization server
            IIdentity identity = new GenericIdentity(name);
            httpContext.User = new GenericPrincipal(identity, null);
            
            // If desired, persist cookie so user doesn't have to authenticate with the resource over and over
            var cookie = new HttpCookie(tokenName, encryptedToken);
            if (storeCookie)
            {
                cookie.Expires = DateTime.Now.AddDays(7); // could parameterize lifetime              
            }
            httpContext.Response.AppendCookie(cookie);
            return true;
        }
    }
}
