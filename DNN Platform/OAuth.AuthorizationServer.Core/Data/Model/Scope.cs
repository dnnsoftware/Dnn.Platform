using System.Collections.Generic;


namespace OAuth.AuthorizationServer.Core.Data.Model
{
    // Represents a scope recognized by the authorization server
    // Allows us to relate clients with scopes they are allowed to use, 
    // and to relate resources with scopes they support
    /// <summary>
    /// 
    /// </summary>
    public class Scope
    {
  
        
        /// <summary>
        /// This is the name of the scope that gets passed around in requests
        /// </summary>
        public string Identifier { get; set; }

     
        /// <summary>
        /// For display to user on authorization page to describe what this scope
        /// allows the client to do
        /// </summary>
        public string Description { get; set; } 

       
        /// <summary>
        /// Clients that can use this scope
        /// </summary>
        public virtual ICollection<Client> Clients { get; set; }

     
        /// <summary>
        /// Resources that support this scope
        /// </summary>
        public virtual ICollection<Resource> Resources { get; set; } 
    }
}
