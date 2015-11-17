using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using OAuth.AuthorizationServer.Core.Data.Extensions;

namespace OAuth.AuthorizationServer.Core.Data.Model
{
    /// <summary>
    ///     Represents a resource recognized by the authorization server
    /// </summary>
    public class Resource
    {
        // Encrypter to use when encyrpting authorization token for this resource
        private RSACryptoServiceProvider _publicTokenEncrypter;
        // Helper to correspond with how scopes are represented in DotNetOpenAuth
        private HashSet<string> _supportedScopes;

        /// <summary>
        ///     Constructor
        /// </summary>
        public Resource()
        {
            Scopes = new List<Scope>();
        }

      
        /// <summary>
        /// Internal id
        /// </summary>
        public int Id { get; set; }

      
        /// <summary>
        /// Display name of resource, not really used anywhere
        /// </summary>
        public string Identifier { get; set; }
      
        /// <summary>
        /// Token that identifies whether user has authenticated with the resource
        /// </summary>
        public string AuthenticationTokenName { get; set; }
       
        /// <summary>
        /// Where the authorization server should redirect the user to authenticate with the resource
        /// </summary>
        public string AuthenticationUrl { get; set; }
      
        /// <summary>
        /// Shared secret with resource login mechanism to verify token provided is really from there
        /// </summary>
        public string AuthenticationKey { get; set; }
    
        /// <summary>
        /// Encryption key for resource server to use in encrypting the authorization token
        /// Resource server has corresponding private key to decrypt
        /// </summary>
        public string PublicTokenEncryptionKey { get; set; }
      
        /// <summary>
        /// Scopes supported by this resource
        /// </summary>
        public virtual ICollection<Scope> Scopes { get; set; }

        /// <summary>
        /// Supported scopes
        /// </summary>
        public HashSet<string> SupportedScopes
        {
            get
            {
                if (_supportedScopes == null)
                {
                    lock (this)
                    {
                        _supportedScopes = Scopes.ToScopeIdentifierSet();
                    }
                }
                return _supportedScopes;
            }
        }

        /// <summary>
        /// public token encrypter
        /// </summary>
        public RSACryptoServiceProvider PublicTokenEncrypter
        {
            get
            {
                if (_publicTokenEncrypter == null)
                {
                    lock (this)
                    {
                        _publicTokenEncrypter = new RSACryptoServiceProvider();
                        var base64EncodedKey = PublicTokenEncryptionKey;
                        _publicTokenEncrypter.FromXmlString(
                            Encoding.UTF8.GetString(Convert.FromBase64String(base64EncodedKey)));
                    }
                }
                return _publicTokenEncrypter;
            }
        }
    }
}