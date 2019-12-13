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

        [Obsolete("There is no longer the concept of an Admin Page.  All pages are controlled by Permissions. Scheduled removal in v11.0.0.", true)]
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
