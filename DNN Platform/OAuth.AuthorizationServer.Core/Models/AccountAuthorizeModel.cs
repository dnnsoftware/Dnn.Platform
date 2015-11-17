using System.Collections.Generic;
using DotNetOpenAuth.OAuth2.Messages;
using OAuth.AuthorizationServer.Core.Data.Model;

namespace OAuth.AuthorizationServer.API.Models
{
   
    /// <summary>
    /// Simple model to use in authorize page shown to user
    /// </summary>
    public class AccountAuthorizeModel
    {
        /// <summary>
        /// Get/Set Client
        /// </summary>
        public Client Client { get; set; }
        /// <summary>
        /// Get/Set Scopes
        /// </summary>
        public List<Scope> Scopes { get; set; }
        /// <summary>
        /// Get/Set AuthorizationRequest
        /// </summary>
        public EndUserAuthorizationRequest AuthorizationRequest { get; set; }
    }

}