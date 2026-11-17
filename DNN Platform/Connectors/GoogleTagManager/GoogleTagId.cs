// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DNN.Connectors.GoogleTagManager
{
    using System;
    using System.Text.RegularExpressions;

    /// <summary>Helpers to classify the IDs entered in the Google Tag Manager connector.</summary>
    /// <remarks>
    /// Google no longer supports loading non-GTM IDs through the <c>gtm.js</c> snippet,
    /// see <see href="https://support.google.com/tagmanager/answer/17231523"/>.
    /// </remarks>
    internal static class GoogleTagId
    {
        private static readonly Regex ContainerIdRegex = new Regex(@"^GTM-[A-Z0-9]+$", RegexOptions.Compiled | RegexOptions.CultureInvariant);
        private static readonly Regex GoogleTagIdRegex = new Regex(@"^(G|AW|DC|GT)-[A-Z0-9]+$", RegexOptions.Compiled | RegexOptions.CultureInvariant);

        /// <summary>Normalizes an ID entered by the user (trimmed and upper case).</summary>
        /// <param name="id">The raw ID.</param>
        /// <returns>The normalized ID, or an empty string.</returns>
        public static string Normalize(string id)
        {
            return (id ?? string.Empty).Trim().ToUpperInvariant();
        }

        /// <summary>Gets the type of the given ID.</summary>
        /// <param name="id">The ID to classify.</param>
        /// <returns>The <see cref="GoogleTagIdType"/> of the ID.</returns>
        public static GoogleTagIdType GetIdType(string id)
        {
            var normalizedId = Normalize(id);
            if (ContainerIdRegex.IsMatch(normalizedId))
            {
                return GoogleTagIdType.Container;
            }

            if (GoogleTagIdRegex.IsMatch(normalizedId))
            {
                return GoogleTagIdType.GoogleTag;
            }

            return GoogleTagIdType.Invalid;
        }
    }
}
