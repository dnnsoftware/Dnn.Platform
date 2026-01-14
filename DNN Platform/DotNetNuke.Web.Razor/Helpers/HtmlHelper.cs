// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Web.Razor.Helpers
{
    using System;
    using System.Web;

    using DotNetNuke.Internal.SourceGenerators;
    using DotNetNuke.Services.Localization;
    using DotNetNuke.UI.Modules;

    /// <summary>A razor helper for HTML generation.</summary>
    [DnnDeprecated(9, 3, 2, "Use Razor Pages instead")]
    public partial class HtmlHelper
    {
        private readonly string resourceFile;
        private ModuleInstanceContext context;

        public HtmlHelper(ModuleInstanceContext context, string resourcefile)
        {
            this.context = context;
            this.resourceFile = resourcefile;
        }

        /// <summary>Gets localized text.</summary>
        /// <param name="key">The resource key.</param>
        /// <returns>The localized text.</returns>
        [DnnDeprecated(9, 3, 2, "Use Razor Pages instead")]
        public partial object GetLocalizedString(string key)
        {
            return Localization.GetString(key, this.resourceFile);
        }

        /// <summary>Gets localized text.</summary>
        /// <param name="key">The resource key.</param>
        /// <param name="culture">The culture code.</param>
        /// <returns>The localized text.</returns>
        [DnnDeprecated(9, 3, 2, "Use Razor Pages instead")]
        public partial object GetLocalizedString(string key, string culture)
        {
            return Localization.GetString(key, this.resourceFile, culture);
        }

        /// <summary>Renders HTML text without encoding.</summary>
        /// <param name="text">The HTML text.</param>
        /// <returns>An HTML string which does not encode <paramref name="text"/>.</returns>
        [DnnDeprecated(9, 3, 2, "Use Razor Pages instead")]
        public partial HtmlString Raw(string text)
        {
            return new HtmlString(text);
        }
    }
}
