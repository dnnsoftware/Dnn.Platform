using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;

using System.Web.UI.WebControls;
//using DotNetNuke.Instrumentation;
using DotNetNuke.Web.Api;

using System.Net.Http.Formatting;
using DotNetOpenAuth.Messaging;
using DotNetOpenAuth.OAuth2;
using DotNetOpenAuth.OAuth2.Messages;
using OAuth.AuthorizationServer.API.Attributes;
using OAuth.AuthorizationServer.API.Models;
using OAuth.AuthorizationServer.Core.Data.Model;
using OAuth.AuthorizationServer.Core.Data.Repositories;
using OAuth.AuthorizationServer.Core.Server;
using Authorization = OAuth.AuthorizationServer.Core.Data.Model.Authorization;
using DNOA = DotNetOpenAuth.OAuth2;


namespace OAuth.AuthorizationServer.API.Controllers
{
    // Exposed endpoint by which clients can request access to a resource via the OAuth2 protocol
    public class OAuthController :  DnnApiController
    {
       // private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(OAuthController));

        // Ideally, IOC these dependencies.  
        private readonly DNOA.AuthorizationServer _authorizationServer = new DNOA.AuthorizationServer(new AuthorizationServerHost());        
        //private readonly ClientRepository _clientRepository = new ClientRepository();
       // private readonly AuthorizationRepository _authorizationRepository = new AuthorizationRepository();
       // private readonly ResourceRepository _resourceRepository = new ResourceRepository();
      //  private readonly UserRepository _userRepository = new UserRepository();

        // Provides authorization token to the client based on information in the request
        // DotNetOpenAuth is doing all the heavy lifting here.  Request must contain all of the
        // necessary info to grant a token
        public HttpResponseMessage Token()
        {
           // return _authorizationServer.HandleTokenRequest(Request).AsActionResult();
            return _authorizationServer.HandleTokenRequest(Request).AsHttpResponseMessage();
        }

        // Prompts the user to authorize a client to access the user's private data.
        // If user is not already authenticated by the resource, user will be redirected to login first and then
        // come back here to authorize the client
        //[ResourceAuthenticated, AcceptVerbs(HttpVerbs.Get | HttpVerbs.Post)]
        public HttpResponseMessage Authorize()
        {
            // Have DotNetOpenAuth read the info we need out of the request
            EndUserAuthorizationRequest pendingRequest = _authorizationServer.ReadAuthorizationRequest();
            if (pendingRequest == null)
            {
                throw new HttpException(Convert.ToInt32(HttpStatusCode.BadRequest), "Missing authorization request.");
            }

            // Make sure the client is one we recognize
            //Client requestingClient = _clientRepository.GetById(pendingRequest.ClientIdentifier);
            Client requestingClient = OAUTHDataController.ClientRepositoryGetById(pendingRequest.ClientIdentifier);
            
            if (requestingClient == null)
            {
                throw new HttpException(Convert.ToInt32(HttpStatusCode.BadRequest), "Invalid request");
            }

            // Ensure client is allowed to use the requested scopes
            if (!pendingRequest.Scope.IsSubsetOf(requestingClient.SupportedScopes))
            {
                throw new HttpException(Convert.ToInt32(HttpStatusCode.BadRequest), "Invalid request");
            }

            // Consider auto-approving if safe, so user doesn't have to authorize repeatedly.  
            // Leaving this step out for now            

            // Show user the authorization page by which they can authorize this client to access their
            // data within the resource determined by the requested scopes
            var model = new AccountAuthorizeModel
            {
                Client = requestingClient,
                Scopes = requestingClient.Scopes.Where(x => pendingRequest.Scope.Contains(x.Identifier)).ToList(),
                AuthorizationRequest = pendingRequest
            };
           return  Request.CreateResponse(HttpStatusCode.OK, model);
            //return View(model);
        }

        /// Processes the user's response as to whether to authorize a Client to access his/her private data.
        //[ResourceAuthenticated(Order = 1), HttpPost, ValidateAntiForgeryToken(Order = 2)]
        public HttpResponseMessage ProcessAuthorization(bool isApproved)
        {
            // Have DotNetOpenAuth read the info we need out of the request
            EndUserAuthorizationRequest pendingRequest = _authorizationServer.ReadAuthorizationRequest();
            if (pendingRequest == null)
            {
                throw new HttpException(Convert.ToInt32(HttpStatusCode.BadRequest), "Missing authorization request.");
            }

            // Make sure the client is one we recognize
            //Client requestingClient = _clientRepository.GetById(pendingRequest.ClientIdentifier);
            Client requestingClient = OAUTHDataController.ClientRepositoryGetById(pendingRequest.ClientIdentifier);
            if (requestingClient == null)
            {
                throw new HttpException(Convert.ToInt32(HttpStatusCode.BadRequest), "Invalid request");
            }

            // Make sure the resource is defined, it definitely should be due to the ResourceAuthenticated attribute
            //Resource requestedResource = _resourceRepository.FindWithSupportedScopes(pendingRequest.Scope);
            Resource requestedResource = OAUTHDataController.ResourceRepositoryFindWithSupportedScopes(pendingRequest.Scope);
            if (requestedResource == null)
            {
                throw new HttpException(Convert.ToInt32(HttpStatusCode.BadRequest), "Invalid request");
            }

            // See if authorization of this client was approved by the user
            // At this point, the user either agrees to the entire scope requested by the client or none of it.  
            // If we gave capability for user to reduce scope to give client less access, some changes would be required here
            IDirectedProtocolMessage authRequest;
            if (isApproved)
            {
                // Add user to our repository if this is their first time
                //var requestingUser = _userRepository.GetById(User.Identity.Name);
                var requestingUser = OAUTHDataController.UserRepositoryGetById(User.Identity.Name);
                if (requestingUser == null)
                {
                    requestingUser = new OAUTHUser { Id = User.Identity.Name, CreateDateUtc = DateTime.UtcNow };
                    OAUTHDataController.OAuthUserInsert(requestingUser);
                    //_userRepository.Insert(requestingUser);
                    //_userRepository.Save();
                }

                // The authorization we file in our database lasts until the user explicitly revokes it.
                // You can cause the authorization to expire by setting the ExpirationDateUTC
                // property in the below created ClientAuthorization.
                //_authorizationRepository.Insert(new Authorization
                //{
                //    ClientId = requestingClient.Id,
                //    Scope = OAuthUtilities.JoinScopes(pendingRequest.Scope),
                //    UserId = requestingUser.Id,
                //    ResourceId = requestedResource.Id,
                //    CreatedOnUtc = DateTime.UtcNow                    
                //});
                //_authorizationRepository.Save();

                var a=new Authorization
                {
                    ClientId = requestingClient.Id,
                    Scope = OAuthUtilities.JoinScopes(pendingRequest.Scope),
                    UserId = requestingUser.Id,
                    ResourceId = requestedResource.Id,
                    CreatedOnUtc = DateTime.UtcNow
                };
                OAUTHDataController.OAuthAuthorizationInsert(a);
                                                
                // Have DotNetOpenAuth generate an approval to send back to the client
                authRequest = _authorizationServer.PrepareApproveAuthorizationRequest(pendingRequest, User.Identity.Name);
            }
            else
            {
                // Have DotNetOpenAuth generate a rejection to send back to the client
                authRequest = _authorizationServer.PrepareRejectAuthorizationRequest(pendingRequest);
                // The PrepareResponse call below is giving an error of "The following required parameters were missing from the DotNetOpenAuth.OAuth2.Messages.EndUserAuthorizationFailedResponse message: error"
                // unless I do this.....
                var msg = (EndUserAuthorizationFailedResponse) authRequest;
                msg.Error = "User denied your request";
            }

            // This will redirect to the client app using their defined callback, so they can handle
            // the approval or rejection as they see fit
            return _authorizationServer.Channel.PrepareResponse(authRequest).AsHttpResponseMessage();
        }
    }
}


