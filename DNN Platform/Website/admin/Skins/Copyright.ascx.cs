// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

#region Usings

using System;

using DotNetNuke.Services.Localization;

#endregion

namespace DotNetNuke.UI.Skins.Controls
{
    /// -----------------------------------------------------------------------------
    /// <summary></summary>
    /// <returns></returns>
    /// <remarks></remarks>
    /// -----------------------------------------------------------------------------
    public partial class Copyright : SkinObjectBase
    {
        private const string MyFileName = "Copyright.ascx";
        public string CssClass { get; set; }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            if (!String.IsNullOrEmpty(this.CssClass))
            {
                this.lblCopyright.CssClass = this.CssClass;
            }
            if (!String.IsNullOrEmpty(this.PortalSettings.FooterText))
            {
                this.lblCopyright.Text = this.PortalSettings.FooterText.Replace("[year]", DateTime.Now.ToString("yyyy"));
            }
            else
            {
                this.lblCopyright.Text = string.Format(Localization.GetString("Copyright", Localization.GetResourceFile(this, MyFileName)), DateTime.Now.Year, this.PortalSettings.PortalName);
            }
        }
    }
}
