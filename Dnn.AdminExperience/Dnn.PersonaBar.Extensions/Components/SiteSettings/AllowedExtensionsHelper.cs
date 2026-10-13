// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.PersonaBar.SiteSettings.Components
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using DotNetNuke.Abstractions.Security;
    using DotNetNuke.Common.Utilities;

    /// <summary>Helpers for the site level "Allowed File Extensions" setting.</summary>
    internal static class AllowedExtensionsHelper
    {
        /// <summary>The allowed extensions list is the host's default end user list.</summary>
        internal const int DefaultOption = 0;

        /// <summary>The allowed extensions list is a custom list.</summary>
        internal const int CustomOption = 1;

        /// <summary>The allowed extensions list only contains image extensions.</summary>
        internal const int OnlyImagesOption = 2;

        /// <summary>Determines whether two allow lists contain the same extensions, ignoring order, case and duplicates.</summary>
        /// <param name="first">The first list.</param>
        /// <param name="second">The second list.</param>
        /// <returns><see langword="true"/> if both lists allow exactly the same extensions.</returns>
        internal static bool AreEquivalent(IFileExtensionAllowList first, IFileExtensionAllowList second)
        {
            if (first == null || second == null)
            {
                return ReferenceEquals(first, second);
            }

            return new HashSet<string>(first.AllowedExtensions, StringComparer.OrdinalIgnoreCase)
                .SetEquals(second.AllowedExtensions);
        }

        /// <summary>Gets the image extensions that can actually be allowed on a site, i.e. the image types restricted by the host's permitted list.</summary>
        /// <param name="imageFileTypes">A comma separated list of image extensions (see <see cref="DotNetNuke.Common.Globals.ImageFileTypes"/>).</param>
        /// <param name="hostAllowList">The host's permitted extensions list.</param>
        /// <returns>The image extensions allow list.</returns>
        internal static IFileExtensionAllowList GetImageAllowList(string imageFileTypes, IFileExtensionAllowList hostAllowList)
        {
            return new FileExtensionWhitelist(imageFileTypes ?? string.Empty).RestrictBy(hostAllowList);
        }

        /// <summary>Determines which whitelist option matches the site's current allow list.</summary>
        /// <param name="siteAllowList">The site's current allow list.</param>
        /// <param name="hostDefaultAllowList">The host's default end user allow list.</param>
        /// <param name="imageAllowList">The image only allow list.</param>
        /// <returns><see cref="DefaultOption"/>, <see cref="CustomOption"/> or <see cref="OnlyImagesOption"/>.</returns>
        internal static int GetWhitelistOption(IFileExtensionAllowList siteAllowList, IFileExtensionAllowList hostDefaultAllowList, IFileExtensionAllowList imageAllowList)
        {
            if (AreEquivalent(siteAllowList, hostDefaultAllowList))
            {
                return DefaultOption;
            }

            if (imageAllowList != null && imageAllowList.AllowedExtensions.Any() && AreEquivalent(siteAllowList, imageAllowList))
            {
                return OnlyImagesOption;
            }

            return CustomOption;
        }

        /// <summary>Gets the requested extensions which are not part of the effective allow list (i.e. were removed because the host does not permit them).</summary>
        /// <param name="requested">The requested allow list.</param>
        /// <param name="effective">The allow list after being restricted by the host's permitted list.</param>
        /// <returns>The rejected extensions, without the leading dot, in the requested order.</returns>
        internal static IList<string> GetRejectedExtensions(IFileExtensionAllowList requested, IFileExtensionAllowList effective)
        {
            var allowed = new HashSet<string>(effective.AllowedExtensions, StringComparer.OrdinalIgnoreCase);
            return requested.AllowedExtensions
                .Where(ext => !allowed.Contains(ext))
                .Select(ext => ext.TrimStart('.'))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();
        }
    }
}
