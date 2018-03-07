using Cantarus.Libraries.Encryption;
using DotNetNuke.ComponentModel.DataAnnotations;
using System;
using System.Security.Cryptography;
using System.Text;

namespace Cantarus.Modules.PolyDeploy.Components.DataAccess.Models
{
    [TableName("Cantarus_PolyDeploy_APIUsers")]
    [PrimaryKey("APIUserID")]
    internal class APIUser
    {
        private static RNGCryptoServiceProvider Rng = new RNGCryptoServiceProvider();

        private bool authenticated;
        private string apiKey;
        private string encryptionKey;

        public int APIUserId { get; set; }
        public string Name { get; set; }
        public byte[] APIKey_Sha { get; set; }
        public string EncryptionKey_Enc { get; set; }
        public string Salt { get; set; }

        [IgnoreColumn]
        public bool Authenticated
        {
            get
            {
                return authenticated;
            }
        }

        [IgnoreColumn]
        public string APIKey
        {
            get
            {
                return apiKey;
            }
        }

        [IgnoreColumn]
        public string EncryptionKey
        {
            get
            {
                return encryptionKey;
            }
        }

        /// <summary>
        /// Basic constructor, without it PetaPoco will throw an exception.
        /// </summary>
        public APIUser()
        {
            authenticated = false;
        }

        /// <summary>
        /// Constructor for creating a new APIUser.
        /// </summary>
        /// <param name="name"></param>
        public APIUser(string name) : this()
        {
            if(string.IsNullOrEmpty(name))
            {
                throw new Exception("Unable to create new APIUser without a name.");
            }

            Name = name;

            // Create keys and place them in the readable fields.
            apiKey = GenerateKey();
            encryptionKey = GenerateKey();

            // Generate salt.
            Salt = GenerateSalt(32);

            // Hash api key with salt.
            APIKey_Sha = GenerateHash(APIKey + Salt);

            // Encrypt encryption key with api key.
            EncryptionKey_Enc = Crypto.Encrypt(EncryptionKey, APIKey);
        }

        public bool Authenticate(string apiKey)
        {
            // Hash the passed api key with the salt.
            byte[] apiKeyHash = GenerateHash(apiKey + Salt);

            // Does it match the stored hash?
            if (!APIKey_Sha.Equals(apiKeyHash))
            {
                // No, authentication failure.
                return false;
            }

            // Store apiKey so we can use it.
            this.apiKey = apiKey;
            this.encryptionKey = Crypto.Decrypt(EncryptionKey_Enc, this.apiKey);

            // Authenticated.
            authenticated = true;

            return authenticated;
        }

        private static byte[] GenerateHash(string value)
        {
            SHA256 sha = new SHA256Managed();

            byte[] bytes = Encoding.UTF8.GetBytes(value);

            return sha.ComputeHash(bytes);
        }

        private static string GenerateSalt(int length)
        {
            // Create a new array of the size requested.
            byte[] salt = new byte[length];

            // Fill it with random bytes.
            Rng.GetBytes(salt);

            // Convert to a string and return.
            return BitConverter.ToString(salt);
        }

        private static string GenerateKey()
        {
            // Get new guid as string.
            string guidString = Guid.NewGuid().ToString();

            // Remove hyphens, uppercase and return.
            return guidString.Replace("-", null).ToUpper();
        }
    }
}
