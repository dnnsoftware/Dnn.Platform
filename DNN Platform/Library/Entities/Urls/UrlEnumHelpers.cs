// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Entities.Urls
{
    using System;

    using DotNetNuke.Internal.SourceGenerators;

    using NewBrowserTypes = DotNetNuke.Abstractions.Urls.BrowserTypes;
#pragma warning disable CS0618 // Type or member is obsolete
    using OldBrowserTypes = DotNetNuke.Entities.Urls.BrowserTypes;
#pragma warning restore CS0618 // Type or member is obsolete

    /// <summary>Helpers for <see cref="NewBrowserTypes"/>.</summary>
    public partial class UrlEnumHelpers
    {
        /// <summary>Converts a <paramref name="value"/> into a <see cref="OldBrowserTypes"/>.</summary>
        /// <param name="value">The string value.</param>
        /// <returns>The enum value.</returns>
        [DnnDeprecated(9, 7, 2, "Use DotNetNuke.Abstractions.Urls.BrowserTypes instead")]
        public static partial OldBrowserTypes FromString(string value)
            => ParseBrowserType(value).ToDeprecatedBrowserTypes();

        /// <summary>Converts a <paramref name="value"/> into a <see cref="OldBrowserTypes"/>.</summary>
        /// <param name="value">The string value.</param>
        /// <returns>The enum value.</returns>
        public static NewBrowserTypes ParseBrowserType(string value)
        {
            return string.Equals(value, "Mobile", StringComparison.OrdinalIgnoreCase)
                ? NewBrowserTypes.Mobile
                : NewBrowserTypes.Normal;
        }
    }
}
