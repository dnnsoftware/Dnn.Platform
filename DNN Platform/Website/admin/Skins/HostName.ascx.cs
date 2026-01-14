// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.UI.Skins.Controls
{
    using System;

    using DotNetNuke.Abstractions.Application;
    using DotNetNuke.Common;
    using DotNetNuke.Entities.Host;
    using DotNetNuke.Services.Exceptions;

    using Microsoft.Extensions.DependencyInjection;

    /// <summary>A skin/theme object which displays a link to the host site.</summary>
    public partial class HostName : SkinObjectBase
    {
        private readonly IHostSettings hostSettings;

        /// <summary>Initializes a new instance of the <see cref="HostName"/> class.</summary>
        public HostName()
            : this(null)
        {
        }

        /// <summary>Initializes a new instance of the <see cref="HostName"/> class.</summary>
        /// <param name="hostSettings">The host settings.</param>
        public HostName(IHostSettings hostSettings)
        {
            this.hostSettings = hostSettings ?? Globals.GetCurrentServiceProvider().GetRequiredService<IHostSettings>();
        }

        /// <summary>Gets or sets the CSS class to apply to the hyperlink.</summary>
        public string CssClass { get; set; }

        /// <inheritdoc/>
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            this.InitializeComponent();
        }

        /// <inheritdoc/>
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            try
            {
                if (!string.IsNullOrEmpty(this.CssClass))
                {
                    this.hypHostName.CssClass = this.CssClass;
                }

                this.hypHostName.Text = this.hostSettings.HostTitle;
                this.hypHostName.NavigateUrl = Globals.AddHTTP(this.hostSettings.HostUrl);
            }
            catch (Exception exc)
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }

        private void InitializeComponent()
        {
        }
    }
}
