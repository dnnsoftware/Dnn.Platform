// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
#region Usings

using System;
using System.IO;

using DotNetNuke.Services.Localization;
using DotNetNuke.Services.Tokens;

#endregion

namespace DotNetNuke.UI.Skins.Controls
{
    public partial class Text : SkinObjectBase
    {
        public string ShowText { get; set; }

        public string CssClass { get; set; }

        public string ResourceKey { get; set; }

        public bool ReplaceTokens { get; set; }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            string strText = ShowText;
			
			//load resources
            if (!String.IsNullOrEmpty(ResourceKey))
            {
				//localization
                string strFile = Path.GetFileName(Server.MapPath(PortalSettings.ActiveTab.SkinSrc));
                strFile = PortalSettings.ActiveTab.SkinPath + Localization.LocalResourceDirectory + "/" + strFile;
                string strLocalization = Localization.GetString(ResourceKey, strFile);
                if (!String.IsNullOrEmpty(strLocalization))
                {
                    strText = strLocalization;
                }
            }
			
            //If no value is found then use the value set the the Text attribute
            if (string.IsNullOrEmpty(strText))
            {
                strText = ShowText;
            }
			
			//token replace
            if (ReplaceTokens)
            {
                var tr = new TokenReplace();
                tr.AccessingUser = PortalSettings.UserInfo;
                strText = tr.ReplaceEnvironmentTokens(strText);
            }
            lblText.Text = strText;
            if (!String.IsNullOrEmpty(CssClass))
            {
                lblText.CssClass = CssClass;
            }
        }
    }
}
