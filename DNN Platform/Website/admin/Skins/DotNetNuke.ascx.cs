// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.UI.Skins.Controls
{
    using System;

    using DotNetNuke.Abstractions.Application;
    using DotNetNuke.Application;
    using DotNetNuke.Common;

    using Microsoft.Extensions.DependencyInjection;

    /// <summary>A skin/theme object which displays a link to the DNN website.</summary>
    public partial class DotNetNukeControl : SkinObjectBase
    {
        private readonly IHostSettingsService hostSettingsService;

        /// <summary>Initializes a new instance of the <see cref="DotNetNukeControl"/> class.</summary>
        public DotNetNukeControl()
            : this(null)
        {
        }

        /// <summary>Initializes a new instance of the <see cref="DotNetNukeControl"/> class.</summary>
        /// <param name="hostSettingsService">The host settings service.</param>
        public DotNetNukeControl(IHostSettingsService hostSettingsService)
        {
            this.hostSettingsService = hostSettingsService ?? Globals.GetCurrentServiceProvider().GetRequiredService<IHostSettingsService>();
        }

        public string CssClass { get; set; }

        /// <inheritdoc/>
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            if (!string.IsNullOrEmpty(this.CssClass))
            {
                this.hypDotNetNuke.CssClass = this.CssClass;
            }

            // get Product Name and Legal Copyright from constants (Medium Trust)
            this.hypDotNetNuke.Text = DotNetNukeContext.Current.Application.LegalCopyright;
            this.hypDotNetNuke.NavigateUrl = DotNetNukeContext.Current.Application.Url;

            // show copyright credits?
            this.Visible = this.hostSettingsService.GetBoolean("Copyright", true);
        }
    }
}
