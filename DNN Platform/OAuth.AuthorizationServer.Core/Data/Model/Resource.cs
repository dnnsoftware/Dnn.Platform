using System;
using System.Collections.Generic;

using System.Security.Cryptography;
using System.Text;
using OAuth.AuthorizationServer.Core.Data.Extensions;

namespace OAuth.AuthorizationServer.Core.Data.Model
{
    // Represents a resource recognized by the authorization server
    public class Resource
    {
        public Resource()
        {
            Scopes = new List<Scope>();
        }

        // Internal id
        public int Id { get; set; }
        
        // Display name of resource, not really used anywhere
        public string Identifier { get; set; }

        // Token that identifies whether user has authenticated with the resource
        public string AuthenticationTokenName { get; set; }

        // Where the authorization server should redirect the user to authenticate with the resource
        public string AuthenticationUrl { get; set; }

        // Shared secret with resource login mechanism to verify token provided is really from there
        public string AuthenticationKey { get; set; }

        // Encryption key for resource server to use in encrypting the authorization token
        // Resource server has corresponding private key to decrypt
        public string PublicTokenEncryptionKey { get; set; }

        // Scopes supported by this resource
        public virtual ICollection<Scope> Scopes { get; set; }

        // Helper to correspond with how scopes are represented in DotNetOpenAuth
        private HashSet<string> _supportedScopes;
     
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

        // Encrypter to use when encyrpting authorization token for this resource
        private RSACryptoServiceProvider _publicTokenEncrypter;
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
                        _publicTokenEncrypter.FromXmlString(Encoding.UTF8.GetString(Convert.FromBase64String(base64EncodedKey)));
                    }
                }
                return _publicTokenEncrypter;
            }
        }
    }
}
