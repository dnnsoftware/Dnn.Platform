using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OAuth.AuthorizationServer.Core.Data.Model
{
    /// <summary>
    /// Get OAuth settings 
    /// </summary>
    public partial class Settings
    {
        /// <summary>
        /// Get/Set AuthorizationServerPrivateKey
        /// </summary>
        public string AuthorizationServerPrivateKey { get; set; }
        /// <summary>
        /// Get/Set ResourceServerDecryptionKey
        /// </summary>
        public string ResourceServerDecryptionKey { get; set; }
        /// <summary>
        /// Get/Set AuthorizationServerVerificationKey
        /// </summary>
        public string AuthorizationServerVerificationKey { get; set; }
    }
}
