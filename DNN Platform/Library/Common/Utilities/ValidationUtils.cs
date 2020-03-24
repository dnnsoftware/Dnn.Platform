using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotNetNuke.Entities.Host;
using DotNetNuke.Security;

namespace DotNetNuke.Common.Utilities
{
    public sealed class ValidationUtils
    {
        internal static string GetDecryptionKey()
        {
            var machineKey = Config.GetDecryptionkey();
            var key = $"{machineKey ?? ""}{Host.GUID.Replace("-", string.Empty)}";
            return FIPSCompliant.EncryptAES(key, key, Host.GUID);
        }

        internal static string ComputeValidationCode(IList<string> extensions)
        {
            if (extensions != null)
            {
                var checkString = extensions.Select(i => i.ToLowerInvariant())
                    .OrderBy(i => i)
                    .Aggregate(string.Empty, (current, extension) => current.Append(extension, ", "));
                return PortalSecurity.Instance.Encrypt(GetDecryptionKey(), checkString);
            }

            return string.Empty;
        }

        internal static bool ValidationCodeMatched(string extensions, string validationCode)
        {
            var extensionList = new List<string>();
            if (!string.IsNullOrWhiteSpace(extensions))
            {
                extensionList = extensions.Split(',').Select(i => i.Trim()).ToList();
            }

            return validationCode.Equals(ComputeValidationCode(extensionList));
        }
    }
}
