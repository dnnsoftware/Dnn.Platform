// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.UI.Skins.Controls
{
    using System;
    using System.IO;

    using DotNetNuke.Services.Localization;
    using DotNetNuke.Services.Tokens;

    public partial class Text : SkinObjectBase
    {
        public string ShowText { get; set; }

        public string CssClass { get; set; }

        public string ResourceKey { get; set; }

        public bool ReplaceTokens { get; set; }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            string strText = this.ShowText;

            // load resources
            if (!string.IsNullOrEmpty(this.ResourceKey))
            {
                // localization
                string strFile = Path.GetFileName(this.Server.MapPath(this.PortalSettings.ActiveTab.SkinSrc));
                strFile = this.PortalSettings.ActiveTab.SkinPath + Localization.LocalResourceDirectory + "/" + strFile;
                string strLocalization = Localization.GetString(this.ResourceKey, strFile);
                if (!string.IsNullOrEmpty(strLocalization))
                {
                    strText = strLocalization;
                }
            }

            // If no value is found then use the value set the the Text attribute
            if (string.IsNullOrEmpty(strText))
            {
                strText = this.ShowText;
            }

            // token replace
            if (this.ReplaceTokens)
            {
                var tr = new TokenReplace();
                tr.AccessingUser = this.PortalSettings.UserInfo;
                strText = tr.ReplaceEnvironmentTokens(strText);
            }

            this.lblText.Text = strText;
            if (!string.IsNullOrEmpty(this.CssClass))
            {
                this.lblText.CssClass = this.CssClass;
            }
        }
    }
}
