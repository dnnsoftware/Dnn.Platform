#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2018
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
using System.ComponentModel;
using System.Web.UI;
using System.Web.UI.WebControls;

using DotNetNuke.Common;
using DotNetNuke.Entities.Portals;

#endregion

namespace DotNetNuke.UI.WebControls
{
    public abstract class WebControlBase : WebControl
    {
		#region "Private Members"

        private string _styleSheetUrl = "";
        private string _theme = "";
		
		#endregion

		#region "Public Properties"

        public string Theme
        {
            get
            {
                return _theme;
            }
            set
            {
                _theme = value;
            }
        }

        public string ResourcesFolderUrl
        {
            get
            {
                return Globals.ResolveUrl("~/Resources/");
            }
        }

        public string StyleSheetUrl
        {
            get
            {
                if ((_styleSheetUrl.StartsWith("~")))
                {
                    return Globals.ResolveUrl(_styleSheetUrl);
                }
                else
                {
                    return _styleSheetUrl;
                }
            }
            set
            {
                _styleSheetUrl = value;
            }
        }

        public bool IsHostMenu
        {
            get
            {
            	return Globals.IsHostTab(PortalSettings.ActiveTab.TabID);
            }
        }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public PortalSettings PortalSettings
        {
            get
            {
                return PortalController.Instance.GetCurrentPortalSettings();
            }
        }
		
		#endregion

        public abstract string HtmlOutput { get; }

        [Obsolete("There is no longer the concept of an Admin Page.  All pages are controlled by Permissions", true)]
        public bool IsAdminMenu
        {
            get
            {
                bool _IsAdmin = false;
                return _IsAdmin;
            }
        }

        protected override void RenderContents(HtmlTextWriter output)
        {
            output.Write(HtmlOutput);
        }
    }
}