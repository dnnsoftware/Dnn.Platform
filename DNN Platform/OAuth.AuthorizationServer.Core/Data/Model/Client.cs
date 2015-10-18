
using DotNetOpenAuth.Messaging;
using DotNetOpenAuth.OAuth2;
using System;
using System.Collections.Generic;
using OAuth.AuthorizationServer.Core.Data.Extensions;

namespace OAuth.AuthorizationServer.Core.Data.Model
{
   
    /// <summary>
    /// Represents a client (external app) that is recognized by the authorization server
    /// </summary>
    public class Client : IClientDescription
    {
        /// <summary>
        /// constructor for Client - adds lists for Authorizations and Scopes
        /// </summary>
        public Client()
        {
            Authorizations = new List<Authorization>();
            Scopes = new List<Scope>();
        }
    
        
        /// <summary>
        /// Identifier, used in requests from client
        /// </summary>
        public string Id { get; set; }

        
        /// <summary>
        /// Secret used to verify client
        /// </summary>
        public string ClientSecret { get; set; }

      
        /// <summary>
        /// Optional, if specified enforces a match during the code/implicit flows
        /// Matching logic is in IsCallbackAllowed method below
        /// </summary>
        public string Callback { get; set; } 

        
        /// <summary>
        /// Display name of client, used when user authorizes the client
        /// </summary>
        public string Name { get; set; }

     
        /// <summary>
        /// Gets/Sets ClientType (Internally used by DotNetOpenAuth)
        /// </summary>
        public int ClientType { get; set; }

 
        /// <summary>
        /// Gets/Sets Scopes this client is allowed to use
        /// </summary>
        public virtual ICollection<Scope> Scopes { get; set; }

        
        /// <summary>
        /// Gets/Sets Authorizations in force for this client
        /// </summary>
        public virtual ICollection<Authorization> Authorizations { get; set; }

        // Helper to correspond with how scopes are represented in DotNetOpenAuth
        private HashSet<string> _supportedScopes;

        /// <summary>
        /// Gets/Sets SupportedScopes
        /// </summary>
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
