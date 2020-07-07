// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Modules.Journal.Controls
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Web;
    using System.Web.UI;
    using System.Web.UI.WebControls;

    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Modules.Journal.Components;
    using DotNetNuke.Services.Journal;
    using DotNetNuke.Services.Localization;
    using DotNetNuke.Services.Tokens;

    [DefaultProperty("Text")]
    [ToolboxData("<{0}:JournalListControl runat=server></{0}:JournalListControl>")]
    public class JournalListControl : WebControl
    {
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public PortalSettings portalSettings
        {
            get
            {
                return PortalController.Instance.GetCurrentPortalSettings();
            }
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public UserInfo userInfo
        {
            get
            {
                return UserController.Instance.GetCurrentUserInfo();
            }
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
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
            if (this.Enabled)
            {
                if (this.CurrentIndex < 0)
                {
                    this.CurrentIndex = 0;
                }

                JournalParser jp = new JournalParser(this.portalSettings, this.ModuleId, this.ProfileId, this.SocialGroupId, this.userInfo) { JournalId = this.JournalId };
                output.Write(jp.GetList(this.CurrentIndex, this.PageSize));
            }
        }
    }
}
