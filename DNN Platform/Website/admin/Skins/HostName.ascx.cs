// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.UI.Skins.Controls
{
    using System;

    using DotNetNuke.Common;
    using DotNetNuke.Entities.Host;
    using DotNetNuke.Services.Exceptions;

    /// -----------------------------------------------------------------------------
    /// <summary></summary>
    /// <returns></returns>
    /// <remarks></remarks>
    /// -----------------------------------------------------------------------------
    public partial class HostName : SkinObjectBase
    {
        public string CssClass { get; set; }

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            this.InitializeComponent();
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            try
            {
                if (!string.IsNullOrEmpty(this.CssClass))
                {
                    this.hypHostName.CssClass = this.CssClass;
                }

                this.hypHostName.Text = Host.HostTitle;
                this.hypHostName.NavigateUrl = Globals.AddHTTP(Host.HostURL);
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
