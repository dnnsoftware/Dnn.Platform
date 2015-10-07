using System;
using System.Configuration;
using System.Security.Cryptography;
using System.Text;
using DotNetNuke.Entities.Controllers;

namespace OAuth.AuthorizationServer.Core.Server
{
    
    /// <summary>
    /// Responsible for providing the authorization server key to use in signing the token
    /// </summary>
    public class AuthorizationServerSigningKeyManager
    {
        /// <summary>
        /// Get the RSA Signer 
        /// </summary>
        /// <returns></returns>
        public RSACryptoServiceProvider GetSigner()
        {
            var signer = new RSACryptoServiceProvider();
            var base64EncodedKey = OAUTHDataController.GetSettings().AuthorizationServerPrivateKey; 
            signer.FromXmlString(Encoding.UTF8.GetString(Convert.FromBase64String(base64EncodedKey)));
            return signer;            
        }
    }
}
