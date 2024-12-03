// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Services.GeneratedImage
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using DotNetNuke.Abstractions.Portals;
    using DotNetNuke.Entities.Portals;

    /// <summary>
    /// Validates urls that could be used in the Image Handler.
    /// </summary>
    internal class UriValidator
    {
        private readonly IPortalAliasController portalAliasController;

        /// <summary>
        /// Initializes a new instance of the <see cref="UriValidator"/> class.
        /// </summary>
        /// <param name="portalAliasController">Provides services related to portal aliases.</param>
        public UriValidator(IPortalAliasController portalAliasController)
        {
            this.portalAliasController = portalAliasController;
        }

        /// <summary>
        /// Checks if a URI belongs to hosted sites.
        /// </summary>
        /// <param name="uri">The URI to validate.</param>
        /// <returns>A value indicating whether the provided Uri belongs to the a valid site.</returns>
        internal bool UriBelongsToSite(Uri uri)
        {
            IEnumerable<string> hostAliases =
                this.portalAliasController
                    .GetPortalAliases().Values.Cast<IPortalAliasInfo>()
                    .Select(alias => alias.HttpAlias);

            // Extract the host and normalize the path from the incoming URI
            string uriHost = uri.DnsSafeHost; // Just the host (e.g., "mysite.com")
            string uriPath = uri.LocalPath.TrimEnd('/'); // Path (e.g., "/siteB")

            // Split the alias into host and optional path (e.g., "mysite.com/siteB")
            foreach (var alias in hostAliases)
            {
                var aliasParts = alias.Split(new[] { '/' }, 2, StringSplitOptions.None); // Split on the first '/' to separate host and path
                string aliasHost = aliasParts[0]; // Host part of the alias (e.g., "mysite.com")
                string aliasPath = aliasParts.Length > 1 ? "/" + aliasParts[1].TrimEnd('/') : string.Empty; // Path part, if any

                // Ensure exact host match and validate the path
                if (string.Equals(uriHost, aliasHost, StringComparison.OrdinalIgnoreCase) &&
                    uriPath.StartsWith(aliasPath, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            // No matching alias found
            return false;
        }
    }
}
