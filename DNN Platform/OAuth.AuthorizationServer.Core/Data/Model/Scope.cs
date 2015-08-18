using System.Collections.Generic;


namespace OAuth.AuthorizationServer.Core.Data.Model
{
    // Represents a scope recognized by the authorization server
    // Allows us to relate clients with scopes they are allowed to use, 
    // and to relate resources with scopes they support
    public class Scope
    {
        // This is the name of the scope that gets passed around in requests
        
        public string Identifier { get; set; }

        // For display to user on authorization page to describe what this scope
        // allows the client to do
        public string Description { get; set; } 

        // Clients that can use this scope
        public virtual ICollection<Client> Clients { get; set; }

        // Resources that support this scope
        public virtual ICollection<Resource> Resources { get; set; } 
    }
}
