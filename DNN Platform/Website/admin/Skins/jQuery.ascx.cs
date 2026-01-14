// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.UI.Skins.Controls
{
    using System;
    using System.Diagnostics.CodeAnalysis;

    using DotNetNuke.Common;
    using DotNetNuke.Framework.JavaScriptLibraries;

    using Microsoft.Extensions.DependencyInjection;

    /// <summary>A control which requests the inclusion of jQuery on the page.</summary>
    [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1300:ElementMustBeginWithUpperCaseLetter", Justification = "Breaking Change")]

    // ReSharper disable once InconsistentNaming
    public partial class jQuery : SkinObjectBase
    {
        private readonly IJavaScriptLibraryHelper javaScript;

        /// <summary>Initializes a new instance of the <see cref="jQuery"/> class.</summary>
        public jQuery()
            : this(null)
        {
        }

        /// <summary>Initializes a new instance of the <see cref="jQuery"/> class.</summary>
        /// <param name="javaScript">The JavaScript library helper.</param>
        public jQuery(IJavaScriptLibraryHelper javaScript)
        {
            this.javaScript = javaScript ?? Globals.GetCurrentServiceProvider().GetRequiredService<IJavaScriptLibraryHelper>();
        }

        public bool DnnjQueryPlugins { get; set; }

        [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1300:ElementMustBeginWithUpperCaseLetter", Justification = "Breaking Change")]

        // ReSharper disable once InconsistentNaming
        public bool jQueryHoverIntent { get; set; }

        [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1300:ElementMustBeginWithUpperCaseLetter", Justification = "Breaking Change")]

        // ReSharper disable once InconsistentNaming
        public bool jQueryUI { get; set; }

        /// <inheritdoc/>
        protected override void OnInit(EventArgs e)
        {
            this.javaScript.RequestRegistration(CommonJs.jQuery);
            this.javaScript.RequestRegistration(CommonJs.jQueryMigrate);

            if (this.jQueryUI)
            {
                this.javaScript.RequestRegistration(CommonJs.jQueryUI);
            }

            if (this.DnnjQueryPlugins)
            {
                this.javaScript.RequestRegistration(CommonJs.DnnPlugins);
            }

            if (this.jQueryHoverIntent)
            {
                this.javaScript.RequestRegistration(CommonJs.HoverIntent);
            }
        }
    }
}
