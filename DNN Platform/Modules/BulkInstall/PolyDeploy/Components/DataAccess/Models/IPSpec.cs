using DotNetNuke.ComponentModel.DataAnnotations;
using System;

namespace Cantarus.Modules.PolyDeploy.Components.DataAccess.Models
{
    [TableName("Cantarus_PolyDeploy_IPSpecs")]
    [PrimaryKey("IPSpecID")]
    public class IPSpec : Obfuscated
    {
        /// <summary>
        /// Integer ID of IPSpec.
        /// </summary>
        public int IPSpecId { get; set; }

        /// <summary>
        /// Name used to identify this address.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Salted and hashed address.
        /// </summary>
        public string Address_Sha { get; set; }

        /// <summary>
        /// Randomly generated and used when hashing the address.
        /// </summary>
        public string Salt { get; set; }

        /// <summary>
        /// Address in plain text.
        /// </summary>
        [IgnoreColumn]
        public string Address { get; }

        /// <summary>
        /// Basic constructor, without it PetaPoco will throw an exception.
        /// </summary>
        public IPSpec ()
        {
            Address = "********************************";
        }

        /// <summary>
        /// Constructor for creating a new IPSpec.
        /// </summary>
        /// <param name="address"></param>
        public IPSpec (string name, string address) : this()
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new Exception("Unable to create new IPSpec without a name.");
            }

            if (string.IsNullOrEmpty(address))
            {
                throw new Exception("Unable to create new IPSpec without an address.");
            }

            Name = name;
            Address = address;

            // Generate salt.
            Salt = GenerateSalt();

            // Hash address with salt.
            Address_Sha = GenerateHash(address, Salt);
        }
    }
}
