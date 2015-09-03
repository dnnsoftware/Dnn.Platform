using System;
using System.Collections.Generic;

namespace OAuth.AuthorizationServer.Core.Data.Model
{
    // Represents a user recognized by the authorization server
    public class OAUTHUser
    {
        public OAUTHUser()
        {
            this.Authorizations = new HashSet<Authorization>();
        }
    
        // User name, comes from resource authentication process
        public string Id { get; set; }

        public DateTime CreateDateUtc { get; set; }    

        public virtual ICollection<Authorization> Authorizations { get; set; }
    }
}
