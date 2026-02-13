// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.Modules.BulkInstall.Components.DataAccess.Models
{
    using System;

    using DotNetNuke.BulkInstall.Encryption;
    using DotNetNuke.ComponentModel.DataAnnotations;

    /// <summary>A database entity representing a user of the Bulk Install API.</summary>
    [TableName("Cantarus_PolyDeploy_APIUsers")]
    [PrimaryKey("APIUserID")]
    public sealed class APIUser : Obfuscated
    {
        /// <summary>Initializes a new instance of the <see cref="APIUser"/> class, required by PetaPoco.</summary>
        public APIUser()
        {
            this.APIKey = "********************************";
            this.EncryptionKey = "********************************";
            this.Prepared = false;
        }

        /// <summary>Initializes a new instance of the <see cref="APIUser"/> class.</summary>
        /// <param name="name">The label.</param>
        public APIUser(string name)
            : this(name, false)
        {
        }

        /// <summary>Initializes a new instance of the <see cref="APIUser"/> class.</summary>
        /// <param name="name">The label.</param>
        /// <param name="bypass">Whether the user can bypass the IP address allow list.</param>
        public APIUser(string name, bool bypass)
            : this()
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentNullException(nameof(name), "Unable to create new APIUser without a name.");
            }

            this.Name = name;
            this.BypassIPWhitelist = bypass;

            // Create keys and place them in the readable fields.
            this.APIKey = GenerateKey();
            this.EncryptionKey = GenerateKey();
            this.Prepared = true;

            // Generate salt.
            this.Salt = GenerateSalt();

            // Hash api key with salt.
            this.ApiKeySha = GenerateHash(this.APIKey, this.Salt);

            // Encrypt encryption key with api key.
            this.EncryptedEncryptionKey = Crypto.Encrypt(this.EncryptionKey, this.APIKey);
        }

        /// <summary>Gets or sets the integer ID of APIUser.</summary>
        public int APIUserId { get; set; }

        /// <summary>Gets or sets the name of this APIUser.</summary>
        public string Name { get; set; }

        /// <summary>Gets or sets the salted and hashed API key.</summary>
        [ColumnName("APIKey_Sha")]
        public string ApiKeySha { get; set; }

        /// <summary>
        /// Gets or sets the encrypted encryption key.
        ///
        /// Encrypted using the plain API key. Not suitable for hashing as we
        /// need to be able to read the value when the user accesses the API.
        /// </summary>
        [ColumnName("EncryptionKey_Enc")]
        public string EncryptedEncryptionKey { get; set; }

        /// <summary>Gets or sets the randomly generated and used when hashing the api key.</summary>
        public string Salt { get; set; }

        /// <summary>Gets or sets a value indicating whether this user bypasses IP whitelist checks.</summary>
        public bool BypassIPWhitelist { get; set; }

        /// <summary>
        /// Gets a value indicating whether this APIUser object is prepared for use. A
        /// prepared APIUser will have the APIKey and EncryptionKey properties
        /// set to their appropriate plain values for use.
        /// </summary>
        [IgnoreColumn]
        public bool Prepared { get; private set; }

        /// <summary>Gets the API key in plain text.</summary>
        [IgnoreColumn]
        public string APIKey { get; private set; }

        /// <summary>Gets the decrypted encryption key in plain text.</summary>
        [IgnoreColumn]
        public string EncryptionKey { get; private set; }

        /// <summary>Sets the API key and related properties. After a successful preparation, <see cref="Prepared"/> will be <see langword="true"/>.</summary>
        /// <param name="apiKey">The API key.</param>
        /// <returns><see langword="true"/> if the preparation was successful, otherwise <see langword="false"/>.</returns>
        public bool PrepareForUse(string apiKey)
        {
            // Hash the passed api key with the salt.
            string apiKeyHash = GenerateHash(apiKey, this.Salt);

            // Does it match the stored hash?
            if (!this.ApiKeySha.Equals(apiKeyHash, StringComparison.Ordinal))
            {
                // No, verification failure.
                return false;
            }

            // Store apiKey so we can use it.
            this.APIKey = apiKey;
            this.EncryptionKey = Crypto.Decrypt(this.EncryptedEncryptionKey, this.APIKey);

            // Prepared.
            this.Prepared = true;

            return this.Prepared;
        }

        private static string GenerateKey()
        {
            // Get new guid as string.
            string guidString = Guid.NewGuid().ToString();

            // Remove hyphens, uppercase and return.
            return guidString.Replace("-", null).ToUpperInvariant();
        }
    }
}
