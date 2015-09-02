using System;

namespace OAuth.AuthorizationServer.Core.Data.Model
{
    // Represents an authorization for a client to access a particular resource, optionally by a particular user
    public class Authorization
    {
        public int AuthorizationId { get; set; }
        public string ClientId { get; set; }
        public string UserId { get; set; }
        public int ResourceId { get; set; }

        // This is a space-delimited list of scopes, for simplicity here
        public string Scope { get; set; }

        public DateTime CreatedOnUtc { get; set; }
        public DateTime? ExpirationDateUtc { get; set; }
    
        public virtual Client Client { get; set; }
        public virtual OAUTHUser User { get; set; }
    }
}
