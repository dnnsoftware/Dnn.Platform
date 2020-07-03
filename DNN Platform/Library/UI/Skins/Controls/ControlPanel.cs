// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.UI.Skins.Controls
{
    using System;
    using System.Web.UI.HtmlControls;

    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Host;
    using DotNetNuke.UI.ControlPanels;
    using DotNetNuke.Web.Client;
    using DotNetNuke.Web.Client.ClientResourceManagement;

    public class ControlPanel : SkinObjectBase
    {
        public bool IsDockable { get; set; }

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            if (this.Request.QueryString["dnnprintmode"] != "true" && !UrlUtils.InPopUp())
            {
                var objControlPanel = ControlUtilities.LoadControl<ControlPanelBase>(this, Host.ControlPanel);
                var objForm = (HtmlForm)this.Page.FindControl("Form");

                if (objControlPanel.IncludeInControlHierarchy)
                {
                    objControlPanel.IsDockable = this.IsDockable;
                    if (!Host.ControlPanel.EndsWith("controlbar.ascx", StringComparison.InvariantCultureIgnoreCase))
                    {
                        this.Controls.Add(objControlPanel);
                    }
                    else
                    {
                        if (objForm != null)
                        {
                            objForm.Controls.AddAt(0, objControlPanel);
                        }
                        else
                        {
                            this.Page.Controls.AddAt(0, objControlPanel);
                        }
                    }

                    // register admin.css
                    ClientResourceManager.RegisterAdminStylesheet(this.Page, Globals.HostPath + "admin.css");
                }
            }
        }
    }
}
