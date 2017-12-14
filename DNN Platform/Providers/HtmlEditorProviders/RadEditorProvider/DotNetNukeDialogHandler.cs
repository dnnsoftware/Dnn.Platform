#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2017
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

using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Web.UI;

using DotNetNuke.Services.Localization;

using System;

using DotNetNuke.Entities.Portals;
using System.Globalization;

namespace DotNetNuke.Providers.RadEditorProvider
{

	public class DotNetNukeDialogHandler : Telerik.Web.UI.DialogHandler
	{
	    private const string ResourceFile = "~/DesktopModules/Admin/RadEditorProvider/App_LocalResources/RadEditor.Dialogs.resx";
        private static readonly Regex LocalizeRegex = new Regex("\\[\\$LocalizeString\\(['\"](.+?)['\"]\\)\\]", RegexOptions.IgnoreCase | RegexOptions.Compiled);
		
        protected override void OnInit(EventArgs e)
		{
			base.OnInit(e);

			PortalSettings settings = PortalController.Instance.GetCurrentPortalSettings();
			CultureInfo pageLocale = Localization.GetPageLocale(settings);
			if (settings != null && pageLocale != null)
			{
				Localization.SetThreadCultures(pageLocale, settings);
			}
		}

        protected override void Render(HtmlTextWriter writer)
        {
            var  content = new StringBuilder();
            var stringWriter = new StringWriter(content);
            var htmlWriter = new HtmlTextWriter(stringWriter);
            base.Render(htmlWriter);

            var matches = LocalizeRegex.Matches(content.ToString());
            foreach (Match match in matches)
            {
                var key = match.Groups[1].Value;
                var localizedContent = GetLocalizeContent(key);

                content.Replace(match.Value, string.IsNullOrEmpty(localizedContent) ? key : localizedContent);
            }

	        content.Replace("[$protocol$]", Request.IsSecureConnection ? "https://" : "http://");

            writer.Write(content);
        }

        private string GetLocalizeContent(string key)
        {
            var content = Localization.GetString(string.Format("{0}_{1}", DialogName, key), ResourceFile);
            if(string.IsNullOrEmpty(content))
            {
                content = Localization.GetString(string.Format("Common_{0}", key), ResourceFile);
            }

            return content;
        }
	}

}