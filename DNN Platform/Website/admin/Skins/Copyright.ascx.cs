// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
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
            if (!String.IsNullOrEmpty(CssClass))
            {
                lblCopyright.CssClass = CssClass;
            }
            if (!String.IsNullOrEmpty(PortalSettings.FooterText))
            {
                lblCopyright.Text = PortalSettings.FooterText.Replace("[year]", DateTime.Now.ToString("yyyy"));
            }
            else
            {
                lblCopyright.Text = string.Format(Localization.GetString("Copyright", Localization.GetResourceFile(this, MyFileName)), DateTime.Now.Year, PortalSettings.PortalName);
            }
        }
    }
}
