// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Common.Utilities
{
    using System.Collections.Generic;
    using System.Linq;

    using DotNetNuke.Abstractions.Application;
    using DotNetNuke.Internal.SourceGenerators;
    using DotNetNuke.Security;

    using Microsoft.Extensions.DependencyInjection;

    /// <summary>Creates and compares validation codes used during file uploads.</summary>
    public sealed partial class ValidationUtils
    {
        /// <summary>Generates the validation code for the given <paramref name="parameters"/>.</summary>
        /// <param name="parameters">The parameters to use to generate the code.</param>
        /// <returns>The validation code.</returns>
        [DnnDeprecated(10, 0, 2, "Use overload taking IHostSettings")]
        public static partial string ComputeValidationCode(IList<object> parameters)
        {
            using var scope = Globals.GetOrCreateServiceScope();
            var hostSettings = scope.ServiceProvider.GetRequiredService<IHostSettings>();
            return ComputeValidationCode(hostSettings, parameters);
        }

        /// <summary>Generates the validation code for the given <paramref name="parameters"/>.</summary>
        /// <param name="hostSettings">The host settings.</param>
        /// <param name="parameters">The parameters to use to generate the code.</param>
        /// <returns>The validation code.</returns>
        public static string ComputeValidationCode(IHostSettings hostSettings, IList<object> parameters)
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

                return PortalSecurity.Instance.Encrypt(GetDecryptionKey(hostSettings), checkString);
            }

            return string.Empty;
        }

        /// <summary>The decryption key for the instance.</summary>
        /// <param name="hostSettings">The host settings.</param>
        /// <returns>The decryption key.</returns>
        internal static string GetDecryptionKey(IHostSettings hostSettings)
        {
            var machineKey = Config.GetDecryptionkey();
            var key = $"{machineKey ?? string.Empty}{hostSettings.Guid.Replace("-", string.Empty)}";
            return FIPSCompliant.EncryptAES(key, key, hostSettings.Guid);
        }

        /// <summary>Determines whether the <paramref name="validationCode"/> matches the given <paramref name="parameters"/>.</summary>
        /// <param name="hostSettings">The host settings.</param>
        /// <param name="parameters">The parameters.</param>
        /// <param name="validationCode">The code to validate.</param>
        /// <returns>Whether <paramref name="validationCode"/> matches the <paramref name="parameters"/>.</returns>
        internal static bool ValidationCodeMatched(IHostSettings hostSettings, IList<object> parameters, string validationCode)
        {
            return validationCode.Equals(ComputeValidationCode(hostSettings, parameters));
        }
    }
}
