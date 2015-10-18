

namespace OAuth.AuthorizationServer.Core.Data.Model
{
   
    /// <summary>
    /// Used internally by DotNetOpenAuth
    /// </summary>
    public partial class SymmetricCryptoKey
    {
       
        /// <summary>
        /// Get/Set Bucket
        /// </summary>
        public string Bucket { get; set; }
        /// <summary>
        /// Get/Set Handle
        /// </summary>
        public string Handle { get; set; }
        /// <summary>
        /// Get/Set ExpiresUtc
        /// </summary>
        public System.DateTime ExpiresUtc { get; set; }
        /// <summary>
        /// Get/Set Secret
        /// </summary>
        public byte[] Secret { get; set; }
    }
}
