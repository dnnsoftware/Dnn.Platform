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

using DotNetNuke.Common;
using DotNetNuke.Entities.Tabs;
using DotNetNuke.Services.Localization;
using DotNetNuke.Common.Utilities;
using System.Globalization;

#endregion

namespace DotNetNuke.UI.Skins.Controls
{
    /// -----------------------------------------------------------------------------
    /// <summary></summary>
    /// <returns></returns>
    /// <remarks></remarks>
    /// <history>
    /// 	[cniknet]	10/15/2004	Replaced public members with properties and removed
    ///                             brackets from property names
    /// </history>
    /// -----------------------------------------------------------------------------
    public partial class BreadCrumb : SkinObjectBase
    {
        private const string MyFileName = "Breadcrumb.ascx";
        public string Separator { get; set; }

        public string CssClass { get; set; }

        public string RootLevel { get; set; }

        public bool UseTitle { get; set; }

        public int ProfileUserId {
            get {
                var userId = Null.NullInteger;
                if (!string.IsNullOrEmpty(Request.Params["UserId"])) {
                    userId = Int32.Parse(Request.Params["UserId"]);
                }
                return userId;
            }
        }
        public int GroupId {
            get {
                var groupId = Null.NullInteger;
                if (!string.IsNullOrEmpty(Request.Params["GroupId"])) {
                    groupId = Int32.Parse(Request.Params["GroupId"]);
                }
                return groupId;
            }
        }
        private void InitializeComponent()
        {
        }

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            InitializeComponent();
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            //public attributes
            string strSeparator;
            if (!String.IsNullOrEmpty(Separator))
            {
                if (Separator.IndexOf("src=") != -1)
                {
                    Separator = Separator.Replace("src=\"", "src=\"" + PortalSettings.ActiveTab.SkinPath);
                }
                strSeparator = Separator;
            }
            else
            {
                strSeparator = "&nbsp;<img alt=\"*\" src=\"" + Globals.ApplicationPath + "/images/breadcrumb.gif\">&nbsp;";
            }
            string strCssClass;
            if (!String.IsNullOrEmpty(CssClass))
            {
                strCssClass = CssClass;
            }
            else
            {
                strCssClass = "SkinObject";
            }
            int intRootLevel;
            if (!String.IsNullOrEmpty(RootLevel))
            {
                intRootLevel = int.Parse(RootLevel);
            }
            else
            {
                intRootLevel = 1;
            }
            string strBreadCrumbs = "";

            if (intRootLevel == -1)
            {
                strBreadCrumbs += string.Format(Localization.GetString("Root", Localization.GetResourceFile(this, MyFileName)),
                                                Globals.GetPortalDomainName(PortalSettings.PortalAlias.HTTPAlias, Request, true),
                                                strCssClass);
                strBreadCrumbs += strSeparator;
                intRootLevel = 0;
            }
			
            //process bread crumbs
            int intTab;
            for (intTab = intRootLevel; intTab <= PortalSettings.ActiveTab.BreadCrumbs.Count - 1; intTab++)
            {
                if (intTab != intRootLevel)
                {
                    strBreadCrumbs += strSeparator;
                }
                var objTab = (TabInfo) PortalSettings.ActiveTab.BreadCrumbs[intTab];
                string strLabel = objTab.LocalizedTabName;
                if (UseTitle && !String.IsNullOrEmpty(objTab.Title))
                {
                    strLabel = objTab.Title;
                }
                var tabUrl = objTab.FullUrl;
                if (ProfileUserId > -1) {
                    tabUrl = Globals.NavigateURL(objTab.TabID, "", "UserId=" + ProfileUserId.ToString(CultureInfo.InvariantCulture));
                }

                if (GroupId > -1) {
                    tabUrl = Globals.NavigateURL(objTab.TabID, "", "GroupId=" + GroupId.ToString(CultureInfo.InvariantCulture));
                }
                if (objTab.DisableLink)
                {
                    strBreadCrumbs += "<span class=\"" + strCssClass + "\">" + strLabel + "</span>";
                }
                else
                {
                    strBreadCrumbs += "<a href=\"" + tabUrl + "\" class=\"" + strCssClass + "\">" + strLabel + "</a>";
                }
            }
            lblBreadCrumb.Text = strBreadCrumbs;
        }
    }
}