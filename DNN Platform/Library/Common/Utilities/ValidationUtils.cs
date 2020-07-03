// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Common.Utilities
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    using DotNetNuke.Entities.Host;
    using DotNetNuke.Security;

    public sealed class ValidationUtils
    {
        internal static string GetDecryptionKey()
        {
            var machineKey = Config.GetDecryptionkey();
            var key = $"{machineKey ?? string.Empty}{Host.GUID.Replace("-", string.Empty)}";
            return FIPSCompliant.EncryptAES(key, key, Host.GUID);
        }

        internal static string ComputeValidationCode(IList<object> parameters)
        {
            if (parameters != null && parameters.Any())
            {
                var checkString = string.Join("_", parameters.Select(p =>
                {
                    if (p is IList<string> list)
                    {
                        return list.Select(i => i.ToLowerInvariant())
                            .OrderBy(i => i)
                            .Aggregate(string.Empty, (current, extension) => current.Append(extension, ", "));
                    }

                    return p.ToString();
                }));

                return PortalSecurity.Instance.Encrypt(GetDecryptionKey(), checkString);
            }

            return string.Empty;
        }

        internal static bool ValidationCodeMatched(IList<object> parameters, string validationCode)
        {
            return validationCode.Equals(ComputeValidationCode(parameters));
        }
    }
}
