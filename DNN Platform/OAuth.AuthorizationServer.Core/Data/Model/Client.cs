
using DotNetOpenAuth.Messaging;
using DotNetOpenAuth.OAuth2;
using System;
using System.Collections.Generic;
using OAuth.AuthorizationServer.Core.Data.Extensions;

namespace OAuth.AuthorizationServer.Core.Data.Model
{
    // Represents a client (external app) that is recognized by the authorization server
    public class Client : IClientDescription
    {
        public Client()
        {
            Authorizations = new List<Authorization>();
            Scopes = new List<Scope>();
        }
    
        // Identifier, used in requests from client
        public string Id { get; set; }

        // Secret used to verify client
        public string ClientSecret { get; set; }

        // Optional, if specified enforces a match during the code/implicit flows
        // Matching logic is in IsCallbackAllowed method below
        public string Callback { get; set; } 

        // Display name of client, used when user authorizes the client
        public string Name { get; set; }

        // Internally used by DotNetOpenAuth
        public int ClientType { get; set; }

        // Scopes this client is allowed to use
        public virtual ICollection<Scope> Scopes { get; set; }

        // Authorizations in force for this client
        public virtual ICollection<Authorization> Authorizations { get; set; }

        // Helper to correspond with how scopes are represented in DotNetOpenAuth
        private HashSet<string> _supportedScopes;
       
        public HashSet<string> SupportedScopes
        {
            get
            {
                if (_supportedScopes == null)
                {
                    lock (this)
                    {
                        _supportedScopes = Scopes.ToScopeIdentifierSet();
                    }
                }
                return _supportedScopes;
            }
        }

        // These members are used internally by DotNetOpenAuth
        #region IClientDescription        

        Uri IClientDescription.DefaultCallback
        {
            get { return string.IsNullOrEmpty(Callback) ? null : new Uri(Callback); }
        }

        ClientType IClientDescription.ClientType
        {
            get { return (ClientType)ClientType; }
        }
               
        bool IClientDescription.HasNonEmptySecret
        {
            get { return !string.IsNullOrEmpty(ClientSecret); }
        }

        // Determines whether a callback URI included in a client's authorization request
        // is among those allowed callbacks for the registered client.        
        bool IClientDescription.IsCallbackAllowed(Uri cbUri)
        {
            if (string.IsNullOrEmpty(Callback))
            {
                // No callback rules have been set up for this client.
                return true;
            }

            // For security purposes, we're requiring an identical match to what was configured for the client
            if (cbUri.ToString() == Callback)
            {
                return true;
            }

            return false;
        }

        // Checks whether the specified client secret is correct.
        // Returns true if the secret matches the one in the authorization server's record for the client, false otherwise
        bool IClientDescription.IsValidClientSecret(string secret)
        {
            // Could hash this to avoid storing the secret in db
            return MessagingUtilities.EqualsConstantTime(secret, ClientSecret);
        }

        #endregion
    }
}
