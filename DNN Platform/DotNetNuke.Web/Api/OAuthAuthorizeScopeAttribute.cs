using System;
using System.Collections.Generic;
using System.Configuration;
using System.IdentityModel.Tokens;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Security.Permissions;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using DotNetOpenAuth.Messaging;
using DotNetOpenAuth.OAuth2;
using System.ServiceModel.Channels;
using System.Web.Http;
using System.Web.Http.Controllers;
using DNOA = DotNetOpenAuth.OAuth2;

namespace DotNetNuke.Web.Api
{

    class OAuthAuthorizeScopeAttribute : AuthorizeAttribute, IOverrideDefaultAuthLevel
    {
        private static readonly RSACryptoServiceProvider Decrypter;
        private static readonly RSACryptoServiceProvider SignatureVerifier;

        // Get the keys from wherever they are stored
        static OAuthAuthorizeScopeAttribute()
        {
            var decrypter = new RSACryptoServiceProvider();
            var base64EncodedKey = ConfigurationManager.AppSettings["ResourceServerDecryptionKey"];
            decrypter.FromXmlString(Encoding.UTF8.GetString(Convert.FromBase64String(base64EncodedKey)));
            Decrypter = decrypter;

            var verifier = new RSACryptoServiceProvider();
            var base64VerifierEncodedKey = ConfigurationManager.AppSettings["AuthorizationServerVerificationKey"];
            verifier.FromXmlString(Encoding.UTF8.GetString(Convert.FromBase64String(base64VerifierEncodedKey)));
            SignatureVerifier = verifier;
        }

        // Which scopes are required to gain access
        public string[] RequiredScopes { get; set; }

        public OAuthAuthorizeScopeAttribute(params string[] requiredScopes)
        {
            RequiredScopes = requiredScopes;
        }
        //public override bool IsAuthorized(AuthFilterContext context)
        //{
        //    try
        //    {
        //      //  base.OnAuthorization(actionContext);
                
        //        // Bail if no auth header or the header isn't bearing a token for us
        //        var authHeader = context.ActionContext.Request.Headers.FirstOrDefault(x => x.Key == "Authorization");
        //        if (authHeader.Value == null || !authHeader.Value.Any())
        //        {
        //            context.ActionContext.Response = new HttpResponseMessage(HttpStatusCode.Unauthorized);
        //            return false;
        //        }
        //        var authHeaderValue = authHeader.Value.FirstOrDefault(x => x.StartsWith("Bearer "));
        //        if (authHeaderValue == null)
        //        {
        //            context.ActionContext.Response = new HttpResponseMessage(HttpStatusCode.Unauthorized);
        //            return false;
        //        }

        //         //Have the DotNetOpenAuth resource server inspect the provided request using the configured keys
        //         //This checks both that the token is ok and that the token grants the scope required by
        //         //the required scope parameters to this attribute
        //        var resourceServer = new DNOA.ResourceServer(new StandardAccessTokenAnalyzer(SignatureVerifier, Decrypter));
        //        var principal = resourceServer.GetPrincipal(context.ActionContext.Request, RequiredScopes);
        //        if (principal != null)
        //        {
        //            // Things look good.  Set principal for the resource to use in identifying the user so it can act accordingly
        //            Thread.CurrentPrincipal = principal;
        //            HttpContext.Current.User = principal;
        //            // Dont understand why the call to GetPrincipal is setting actionContext.Response to be unauthorized
        //            // even when the principal returned is non-null
        //            // If I do this code the same way in a delegating handler, that doesn't happen
        //            context.ActionContext.Response = null;
        //        }
        //        else
        //        {
        //            context.ActionContext.Response = new HttpResponseMessage(HttpStatusCode.Unauthorized);
        //        }
        //    }
        //    catch (SecurityTokenValidationException)
        //    {
        //        context.ActionContext.Response = new HttpResponseMessage(HttpStatusCode.Unauthorized);
        //    }
        //    catch (ProtocolFaultResponseException)
        //    {
        //        context.ActionContext.Response = new HttpResponseMessage(HttpStatusCode.Unauthorized);
        //    }
        //    catch (Exception)
        //    {
        //        context.ActionContext.Response = new HttpResponseMessage(HttpStatusCode.BadRequest);
        //    }
        //    return true;
        //}

        protected override bool IsAuthorized(HttpActionContext context)
        {
            try
            {
                
                //  base.OnAuthorization(actionContext);
                HttpContext httpContext = HttpContext.Current;
                // Bail if no auth header or the header isn't bearing a token for us
                var authHeader = context.Request.Headers.FirstOrDefault(x => x.Key == "Authorization");
                if (authHeader.Value == null || !authHeader.Value.Any())
                {
                    context.Response = new HttpResponseMessage(HttpStatusCode.Unauthorized);
                    return false;
                }
                var authHeaderValue = authHeader.Value.FirstOrDefault(x => x.StartsWith("Bearer "));
                if (authHeaderValue == null)
                {
                    context.Response = new HttpResponseMessage(HttpStatusCode.Unauthorized);
                    return false;
                }

                //Have the DotNetOpenAuth resource server inspect the provided request using the configured keys
                //This checks both that the token is ok and that the token grants the scope required by
                //the required scope parameters to this attribute
                var resourceServer = new DNOA.ResourceServer(new StandardAccessTokenAnalyzer(SignatureVerifier, Decrypter));
                if (resourceServer != null)
                {
                    //valid token, no need to verify scopes
                    return true;
                }

                //var principal = resourceServer.GetPrincipal(context.Request, RequiredScopes);
                
                //if (principal != null)
                //{
                //    // Things look good.  Set principal for the resource to use in identifying the user so it can act accordingly
                //    Thread.CurrentPrincipal = principal;
                //    HttpContext.Current.User = principal;
                //    // Dont understand why the call to GetPrincipal is setting actionContext.Response to be unauthorized
                //    // even when the principal returned is non-null
                //    // If I do this code the same way in a delegating handler, that doesn't happen
                //    context.Response = null;
                //}
                //else
                //{
                //    context.Response = new HttpResponseMessage(HttpStatusCode.Unauthorized);
                //}
            }
            catch (SecurityTokenValidationException)
            {
                context.Response = new HttpResponseMessage(HttpStatusCode.Unauthorized);
            }
            catch (ProtocolFaultResponseException)
            {
                context.Response = new HttpResponseMessage(HttpStatusCode.Unauthorized);
            }
            catch (Exception)
            {
                context.Response = new HttpResponseMessage(HttpStatusCode.BadRequest);
            }
            return true;
        }
       
    }
}
