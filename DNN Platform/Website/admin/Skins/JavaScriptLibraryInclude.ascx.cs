// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.UI.Skins.Controls
{
    using System;

    using DotNetNuke.Abstractions.Application;
    using DotNetNuke.Abstractions.Logging;
    using DotNetNuke.Common;
    using DotNetNuke.Framework.JavaScriptLibraries;

    using Microsoft.Extensions.DependencyInjection;

    /// <summary>A control which requests the inclusion of a JavaScript library on the page.</summary>
    public partial class JavaScriptLibraryInclude : SkinObjectBase
    {
        private readonly IJavaScriptLibraryHelper javaScript;

        /// <summary>Initializes a new instance of the <see cref="JavaScriptLibraryInclude"/> class.</summary>
        public JavaScriptLibraryInclude()
            : this(null)
        {
        }

        /// <summary>Initializes a new instance of the <see cref="JavaScriptLibraryInclude"/> class.</summary>
        /// <param name="javaScript">The JavaScript library helper.</param>
        public JavaScriptLibraryInclude(IJavaScriptLibraryHelper javaScript)
        {
            this.javaScript = javaScript ?? Globals.GetCurrentServiceProvider().GetRequiredService<IJavaScriptLibraryHelper>();
        }

        public string Name { get; set; }

        public Version Version { get; set; }

        public SpecificVersion? SpecificVersion { get; set; }

        /// <inheritdoc/>
        protected override void OnInit(EventArgs e)
        {
            if (this.Version == null)
            {
                this.javaScript.RequestRegistration(this.Name);
            }
            else if (this.SpecificVersion == null)
            {
                this.javaScript.RequestRegistration(this.Name, this.Version);
            }
            else
            {
                this.javaScript.RequestRegistration(this.Name, this.Version, this.SpecificVersion.Value);
            }
        }
    }
}
