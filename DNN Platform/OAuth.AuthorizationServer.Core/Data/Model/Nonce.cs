

namespace OAuth.AuthorizationServer.Core.Data.Model
{

    /// <summary>
    /// Used internally by DotNetOpenAuth
    /// </summary>
    public class Nonce
    {
       
        /// <summary>
        /// gets/sets Context
        /// </summary>
        public string Context { get; set; }
       
        /// <summary>
        /// gets/sets Code
        /// </summary>
        public string Code { get; set; }
       
        /// <summary>
        /// gets/sets Timestamp
        /// </summary>
        public System.DateTime Timestamp { get; set; }
    }
}
