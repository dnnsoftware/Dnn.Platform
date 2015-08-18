using System;
using System.Collections.Generic;
using System.Linq;
using DotNetOpenAuth.Messaging.Bindings;
using DotNetOpenAuth.OAuth2;
using DotNetOpenAuth.OAuth2.ChannelElements;
using DotNetOpenAuth.OAuth2.Messages;
using OAuth.AuthorizationServer.Core.Data.Model;
using OAuth.AuthorizationServer.Core.Data.Repositories;

namespace OAuth.AuthorizationServer.Core.Server
{
    // Our implementation of the authorization server, which is passed to the ctor of the 
    // DotNetOpenAuth AuthroizationServer class.  The DotNetOpenAuth impl calls methods in this
    // class when it sees fit.
    public class AuthorizationServerHost : IAuthorizationServerHost
    {
        // Ideally, IOC these dependencies.  OK, ideally, refactor this a bit more, although
        // we're our hands are tied somewhat by the IAuthorizationServerHost interface
       // private readonly ClientRepository _clientRepository = new ClientRepository();
        //private readonly ResourceRepository _resourceRepository = new ResourceRepository();
        private readonly ICryptoKeyStore _cryptoKeyRepository = new SymmetricCryptoKeyRepository();
        private readonly INonceStore _nonceRepository = new NonceRepository();
        //private readonly AuthorizationRepository _authorizationRepository = new AuthorizationRepository();
        private readonly AuthorizationServerSigningKeyManager _tokenSigner = new AuthorizationServerSigningKeyManager();

        public ICryptoKeyStore CryptoKeyStore { get { return _cryptoKeyRepository; } }
        public INonceStore NonceStore { get { return _nonceRepository; } }

        // Generate an access token, given parameters in request that tell use what scopes to include,
        // and thus what resource's encryption key to use in addition to the authroization server key
        public AccessTokenResult CreateAccessToken(IAccessTokenRequest accessTokenRequestMessage)
        {
            var accessToken = new AuthorizationServerAccessToken {Lifetime = TimeSpan.FromMinutes(10)}; // could parameterize lifetime

            //var targetResource = _resourceRepository.FindWithSupportedScopes(accessTokenRequestMessage.Scope);
            var targetResource = OAUTHDataController.ResourceRepositoryFindWithSupportedScopes(accessTokenRequestMessage.Scope);
            accessToken.ResourceServerEncryptionKey = targetResource.PublicTokenEncrypter;
            accessToken.AccessTokenSigningKey = _tokenSigner.GetSigner();

            var result = new AccessTokenResult(accessToken);
            return result;
        }

        // Lookup client given an identifier
        public IClientDescription GetClient(string clientIdentifier)
        {
            //IClientDescription client = _clientRepository.GetById(clientIdentifier);
            IClientDescription client = OAUTHDataController.ClientRepositoryGetById(clientIdentifier);
            if (client == null)
            {
                throw new ArgumentOutOfRangeException("clientIdentifier");
            }
            return client;
        }

        // Determine whether the given authorization is still ok
        public bool IsAuthorizationValid(IAuthorizationDescription authorization)
        {
            // If db precision exceeds token time precision (which is common), the following query would
            // often disregard a token that is minted immediately after the authorization record is stored in the db.
            // To compensate for this, we'll increase the timestamp on the token's issue date by 1 second.
            //var grantedAuths = _authorizationRepository.FindCurrent(authorization.ClientIdentifier, authorization.User,
            //                                                        authorization.UtcIssued + TimeSpan.FromSeconds(1)).ToList();
            var grantedAuths = OAUTHDataController.AuthorizationRepositoryFindCurrent(authorization.ClientIdentifier, authorization.User,
                                                                   authorization.UtcIssued + TimeSpan.FromSeconds(1)).ToList();

            if (!grantedAuths.Any())
            {
                // No granted authorizations prior to the issuance of this token, so it must have been revoked.
                // Even if later authorizations restore this client's ability to call in, we can't allow
                // access tokens issued before the re-authorization because the revoked authorization should
                // effectively and permanently revoke all access and refresh tokens.
                return false;
            }

            // Determine the set of all scopes the user has authorized for this client
            var grantedScopes = new HashSet<string>(OAuthUtilities.ScopeStringComparer);
            foreach (var auth in grantedAuths)
            {
                grantedScopes.UnionWith(OAuthUtilities.SplitScopes(auth.Scope));
            }

            // See if what's requested is authorized
            return authorization.Scope.IsSubsetOf(grantedScopes);
        }

        // Used during client credentials flow.  Before we get here, the client and secret will already have been verified
        // We're also ensuring the scopes requested are ok to give the client
        public AutomatedAuthorizationCheckResponse CheckAuthorizeClientCredentialsGrant(IAccessTokenRequest accessRequest)
        {
            // Find the client
            //var client = _clientRepository.GetById(accessRequest.ClientIdentifier);
            var client = OAUTHDataController.ClientRepositoryGetById(accessRequest.ClientIdentifier);
            
            // Determine the scopes the client is authorized for
            var scopesClientIsAuthorizedFor = client.SupportedScopes;

            // Check if the scopes that are being requested are a subset of the scopes the user is authorized for.
            // If not, that means that the user has requested at least one scope it is not authorized for
            var clientIsAuthorizedForRequestedScopes = accessRequest.Scope.IsSubsetOf(scopesClientIsAuthorizedFor);

            // The token request is approved when the client is authorized for the requested scopes
            var isApproved = clientIsAuthorizedForRequestedScopes;

            return new AutomatedAuthorizationCheckResponse(accessRequest, isApproved);
        }

        public AutomatedUserAuthorizationCheckResponse CheckAuthorizeResourceOwnerCredentialGrant(string userName, string password, IAccessTokenRequest accessRequest)
        {
            // Not supporting this flow, as it's not normally a good idea to have user give their credentials directly to the client
            throw new NotImplementedException();
        }       
    }
}