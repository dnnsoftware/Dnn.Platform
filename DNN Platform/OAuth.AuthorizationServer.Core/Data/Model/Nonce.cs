

namespace OAuth.AuthorizationServer.Core.Data.Model
{
    // Used internally by DotNetOpenAuth
    public class Nonce
    {
       
        public string Context { get; set; }
       
        public string Code { get; set; }
       
        public System.DateTime Timestamp { get; set; }
    }
}
