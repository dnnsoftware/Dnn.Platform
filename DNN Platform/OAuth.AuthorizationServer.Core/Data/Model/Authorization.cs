using System;

namespace OAuth.AuthorizationServer.Core.Data.Model
{
    
    /// <summary>
    /// Represents an authorization for a client to access a particular resource, optionally by a particular user
    /// </summary>
    public class Authorization
    {
        /// <summary>
        /// gets/sets AuthorizationId
        /// </summary>
        public int AuthorizationId { get; set; }
        /// <summary>
        /// gets/sets ClientId
        /// </summary>
        public string ClientId { get; set; }
        /// <summary>
        /// gets/sets UserId
        /// </summary>
        public string UserId { get; set; }
        /// <summary>
        /// gets/sets ResourceId
        /// </summary>
        public int ResourceId { get; set; }

     
        /// <summary>
        /// gets/sets a space-delimited list of scopes, for simplicity here
        /// </summary>
        public string Scope { get; set; }
        /// <summary>
        /// gets/sets CreatedOnUtc
        /// </summary>
        public DateTime CreatedOnUtc { get; set; }
        /// <summary>
        /// gets/sets ExpirationDateUtc
        /// </summary>
        public DateTime? ExpirationDateUtc { get; set; }
        /// <summary>
        /// gets/sets Client
        /// </summary>
        public virtual Client Client { get; set; }
        /// <summary>
        /// gets/sets OAUTHUser
        /// </summary>
        public virtual OAUTHUser User { get; set; }
    }
}
