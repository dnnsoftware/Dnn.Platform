// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Abstractions.ClientResources
{
    /// <summary>
    /// Contains constants for the supported Client Resource Providers within the framework's registration system.
    /// These values determine where a resource gets injected in the page.
    /// </summary>
    public class ClientResourceProviders
    {
        /// <summary>
        /// In the page header.
        /// </summary>
        public const string DnnPageHeaderProvider = "DnnPageHeaderProvider";

        /// <summary>
        /// In the page body at the top of the main form (when using webforms).
        /// </summary>
        public const string DnnBodyProvider = "DnnBodyProvider";

        /// <summary>
        /// In the page body at the bottom of the main form (when using webforms).
        /// </summary>
        public const string DnnFormBottomProvider = "DnnFormBottomProvider";

        /// <summary>
        /// In the page header.
        /// </summary>
        public const string Default = "DnnPageHeaderProvider";

        /// <summary>
        /// The default css provider.
        /// </summary>
        public const string DefaultCssProvider = "DnnPageHeaderProvider";

        /// <summary>
        /// The default javascript provider.
        /// </summary>
        public const string DefaultJsProvider = "DnnBodyProvider";
    }
}
