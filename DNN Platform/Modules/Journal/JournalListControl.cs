using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;
using DotNetNuke.Services.Journal;
using DotNetNuke.Services.Tokens;
using System.IO;
using System.Text.RegularExpressions;
using DotNetNuke.Services.Localization;
using DotNetNuke.Modules.Journal.Components;

namespace DotNetNuke.Modules.Journal.Controls {
    [DefaultProperty("Text")]
    [ToolboxData("<{0}:JournalListControl runat=server></{0}:JournalListControl>")]
    public class JournalListControl : WebControl {
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public PortalSettings portalSettings {
            get {
                return PortalController.Instance.GetCurrentPortalSettings();
            }
        }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public UserInfo userInfo {
            get {
                return UserController.Instance.GetCurrentUserInfo();
            }
        }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int JournalId
        {
            get
            {
                if (HttpContext.Current != null && !string.IsNullOrEmpty(HttpContext.Current.Request.QueryString["jid"]))
                {
                    return Convert.ToInt32(HttpContext.Current.Request.QueryString["jid"]);
                }

                return Null.NullInteger;
            }
        }

        public int ProfileId { get; set; }

        public int ModuleId { get; set; }

        public int SocialGroupId { get; set; }

        public int PageSize { get; set; }

        public int CurrentIndex { get; set; }

        protected override void Render(HtmlTextWriter output) 
        {
            if (Enabled) {
                if (CurrentIndex < 0) {
                    CurrentIndex = 0;
                }
                JournalParser jp = new JournalParser(portalSettings, ModuleId, ProfileId, SocialGroupId, userInfo){JournalId = JournalId};
                output.Write(jp.GetList(CurrentIndex, PageSize));
            }
            
        }
    }
}
