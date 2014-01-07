#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2014
// by DotNetNuke Corporation
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated 
// documentation files (the "Software"), to deal in the Software without restriction, including without limitation 
// the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and 
// to permit persons to whom the Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all copies or substantial portions 
// of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED 
// TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL 
// THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF 
// CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
// DEALINGS IN THE SOFTWARE.
#endregion
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