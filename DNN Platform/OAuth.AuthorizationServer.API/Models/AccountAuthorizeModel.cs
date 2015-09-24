using System.Collections.Generic;
using DotNetOpenAuth.OAuth2.Messages;
using OAuth.AuthorizationServer.Core.Data.Model;

namespace OAuth.AuthorizationServer.API.Models
{
    // Simple model to use in authorize page shown to user
    public class AccountAuthorizeModel
    {
        public Client Client { get; set; }
        public List<Scope> Scopes { get; set; }
        public EndUserAuthorizationRequest AuthorizationRequest { get; set; }
    }

    public class SimpleAccountAuthorizeModel
    {
        public Client Client { get; set; }
        public EndUserAuthorizationRequest AuthorizationRequest { get; set; }
    }
}