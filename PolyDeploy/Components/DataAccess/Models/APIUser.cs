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
        private bool prepared;
        private string apiKey;
        private string encryptionKey;

        public int APIUserId { get; set; }
        public string Name { get; set; }
        public string APIKey_Sha { get; set; }
        public string EncryptionKey_Enc { get; set; }
        public string Salt { get; set; }

        [IgnoreColumn]
        public bool Prepared
        {
            get
            {
                return prepared;
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
            apiKey = "********************************";
            encryptionKey = "********************************";
            prepared = false;
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
            prepared = true;

            // Generate salt.
            Salt = GenerateSalt();

            // Hash api key with salt.
            APIKey_Sha = GenerateHash(apiKey, Salt);

            // Encrypt encryption key with api key.
            EncryptionKey_Enc = Crypto.Encrypt(encryptionKey, apiKey);
        }

        public bool PrepareForUse(string apiKey)
        {
            // Hash the passed api key with the salt.
            string apiKeyHash = GenerateHash(apiKey, Salt);

            // Does it match the stored hash?
            if (!APIKey_Sha.Equals(apiKeyHash))
            {
                // No, verification failure.
                return false;
            }

            // Store apiKey so we can use it.
            this.apiKey = apiKey;
            this.encryptionKey = Crypto.Decrypt(EncryptionKey_Enc, this.apiKey);

            // Prepared.
            prepared = true;

            return prepared;
        }

        internal static string GenerateHash(string value, string salt)
        {
            // Hash.
            string hash = CryptoUtilities.SHA256HashString(value + salt);

            // Return upper case.
            return hash.ToUpper();
        }

        internal static string GenerateSalt()
        {
            // Salt length of 16 bytes should be fine for now.
            int saltLength = 16;

            // Generate random bytes.
            byte[] bytes = CryptoUtilities.GenerateRandomBytes(saltLength);

            // Convert to string.
            string salt = "";

            for(int i = 0; i < bytes.Length; i++)
            {
                salt = string.Format("{0}{1:X2}", salt, bytes[i]);
            }

            // Return upper case.
            return salt.ToUpper();
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
