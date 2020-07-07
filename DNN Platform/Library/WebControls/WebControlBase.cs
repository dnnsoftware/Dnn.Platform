// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.UI.WebControls
{
    using System;
    using System.ComponentModel;
    using System.Web.UI;
    using System.Web.UI.WebControls;

    using DotNetNuke.Common;
    using DotNetNuke.Entities.Portals;

    public abstract class WebControlBase : WebControl
    {
        private string _styleSheetUrl = string.Empty;
        private string _theme = string.Empty;

        public string ResourcesFolderUrl
        {
            get
            {
                return Globals.ResolveUrl("~/Resources/");
            }
        }

        public bool IsHostMenu
        {
            get
            {
                return Globals.IsHostTab(this.PortalSettings.ActiveTab.TabID);
            }
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public PortalSettings PortalSettings
        {
            get
            {
                return PortalController.Instance.GetCurrentPortalSettings();
            }
        }

        public abstract string HtmlOutput { get; }

        [Obsolete("There is no longer the concept of an Admin Page.  All pages are controlled by Permissions. Scheduled removal in v11.0.0.", true)]
        public bool IsAdminMenu
        {
            get
            {
                bool _IsAdmin = false;
                return _IsAdmin;
            }
        }

        public string Theme
        {
            get
            {
                return this._theme;
            }

            set
            {
                this._theme = value;
            }
        }

        public string StyleSheetUrl
        {
            get
            {
                if (this._styleSheetUrl.StartsWith("~"))
                {
                    return Globals.ResolveUrl(this._styleSheetUrl);
                }
                else
                {
                    return this._styleSheetUrl;
                }
            }

            set
            {
                this._styleSheetUrl = value;
            }
        }

        protected override void RenderContents(HtmlTextWriter output)
        {
            output.Write(this.HtmlOutput);
        }
    }
}
