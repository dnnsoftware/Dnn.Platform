using System;
using System.Collections.Generic;

namespace OAuth.AuthorizationServer.Core.Data.Model
{
    
    /// <summary>
    /// Represents a user recognized by the authorization server
    /// </summary>
    public class OAUTHUser
    {
        /// <summary>
        /// default constructor
        /// </summary>
        public OAUTHUser()
        {
            this.Authorizations = new HashSet<Authorization>();
        }
    
       
        /// <summary>
        /// User name, comes from resource authentication process
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Get/Set CreateDateUtc
        /// </summary>
        public DateTime CreateDateUtc { get; set; }    

        /// <summary>
        /// Get/Set collection of Authorization
        /// </summary>
        public virtual ICollection<Authorization> Authorizations { get; set; }
    }
}
