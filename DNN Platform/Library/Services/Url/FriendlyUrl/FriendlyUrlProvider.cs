// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.Url.FriendlyUrl
{
    using DotNetNuke.Abstractions.Portals;
    using DotNetNuke.ComponentModel;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Tabs;
    using DotNetNuke.Internal.SourceGenerators;

    /// <summary>The base provider for friendly URLs.</summary>
    public abstract partial class FriendlyUrlProvider
    {
        /// <summary>Gets the <see cref="FriendlyUrlProvider"/> implementation.</summary>
        /// <returns>A <see cref="FriendlyUrlProvider"/> instance.</returns>
        public static FriendlyUrlProvider Instance()
        {
            return ComponentFactory.GetComponent<FriendlyUrlProvider>();
        }

        /// <summary>Generate a friendly URL.</summary>
        /// <param name="tab">The page.</param>
        /// <param name="path">The path.</param>
        /// <returns>The friendly URL.</returns>
        public abstract string FriendlyUrl(TabInfo tab, string path);

        /// <summary>Generate a friendly URL.</summary>
        /// <param name="tab">The page.</param>
        /// <param name="path">The path.</param>
        /// <param name="pageName">The page name.</param>
        /// <returns>The friendly URL.</returns>
        public abstract string FriendlyUrl(TabInfo tab, string path, string pageName);

        /// <summary>Generate a friendly URL.</summary>
        /// <param name="tab">The page.</param>
        /// <param name="path">The path.</param>
        /// <param name="pageName">The page name.</param>
        /// <param name="settings">The portal settings.</param>
        /// <returns>The friendly URL.</returns>
        [DnnDeprecated(9, 4, 3, "Use the IPortalSettings overload")]
        public virtual partial string FriendlyUrl(TabInfo tab, string path, string pageName, PortalSettings settings)
        {
            return this.FriendlyUrl(tab, path, pageName, (IPortalSettings)settings);
        }

        /// <summary>Generate a friendly URL.</summary>
        /// <param name="tab">The page.</param>
        /// <param name="path">The path.</param>
        /// <param name="pageName">The page name.</param>
        /// <param name="settings">The portal settings.</param>
        /// <returns>The friendly URL.</returns>
        public abstract string FriendlyUrl(TabInfo tab, string path, string pageName, IPortalSettings settings);

        /// <summary>Generate a friendly URL.</summary>
        /// <param name="tab">The page.</param>
        /// <param name="path">The path.</param>
        /// <param name="pageName">The page name.</param>
        /// <param name="portalAlias">The portal alias.</param>
        /// <returns>The friendly URL.</returns>
        public abstract string FriendlyUrl(TabInfo tab, string path, string pageName, string portalAlias);
    }
}
