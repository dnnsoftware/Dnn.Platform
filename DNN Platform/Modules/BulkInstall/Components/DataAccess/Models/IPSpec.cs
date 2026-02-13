// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.Modules.BulkInstall.Components.DataAccess.Models
{
    using System;

    using DotNetNuke.ComponentModel.DataAnnotations;

    /// <summary>A database entity representing an allowed IP address.</summary>
    [TableName("Cantarus_PolyDeploy_IPSpecs")]
    [PrimaryKey("IPSpecID")]
    public class IPSpec : Obfuscated
    {
        /// <summary>Initializes a new instance of the <see cref="IPSpec"/> class, required by PetaPoco.</summary>
        public IPSpec()
        {
            this.Address = "********************************";
        }

        /// <summary>Initializes a new instance of the <see cref="IPSpec"/> class.</summary>
        /// <param name="name">The label.</param>
        /// <param name="address">The IP address.</param>
        public IPSpec(string name, string address)
            : this()
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentNullException(nameof(name), "Unable to create new IPSpec without a name.");
            }

            if (string.IsNullOrEmpty(address))
            {
                throw new ArgumentNullException(nameof(address), "Unable to create new IPSpec without an address.");
            }

            this.Name = name;
            this.Address = address;

            // Generate salt.
            this.Salt = GenerateSalt();

            // Hash address with salt.
            this.AddressSha = GenerateHash(address, this.Salt);
        }

        /// <summary>Gets or sets the integer ID of IPSpec.</summary>
        public int IPSpecId { get; set; }

        /// <summary>Gets or sets the name used to identify this address.</summary>
        public string Name { get; set; }

        /// <summary>Gets or sets the salted and hashed address.</summary>
        [ColumnName("Address_Sha")]
        public string AddressSha { get; set; }

        /// <summary>Gets or sets the randomly generated and used when hashing the address.</summary>
        public string Salt { get; set; }

        /// <summary>Gets the address in plain text.</summary>
        [IgnoreColumn]
        public string Address { get; }
    }
}
