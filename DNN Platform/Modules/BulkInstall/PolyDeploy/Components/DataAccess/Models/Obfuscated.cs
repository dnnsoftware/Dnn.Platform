using Cantarus.Libraries.Encryption;

namespace Cantarus.Modules.PolyDeploy.Components.DataAccess.Models
{
    public class Obfuscated
    {
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

            for (int i = 0; i < bytes.Length; i++)
            {
                salt = string.Format("{0}{1:X2}", salt, bytes[i]);
            }

            // Return upper case.
            return salt.ToUpper();
        }

        protected Obfuscated() { }
    }
}
