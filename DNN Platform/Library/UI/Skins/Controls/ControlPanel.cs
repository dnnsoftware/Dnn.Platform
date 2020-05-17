﻿// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
using System;
using System.Web.UI.HtmlControls;

using DotNetNuke.Common;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Host;
using DotNetNuke.UI.ControlPanels;
using DotNetNuke.Web.Client;
using DotNetNuke.Web.Client.ClientResourceManagement;

namespace DotNetNuke.UI.Skins.Controls
{
	public class ControlPanel : SkinObjectBase
	{
		public bool IsDockable { get; set; }

		protected override void  OnInit(EventArgs e)
		{
			base.OnInit(e);

            if (Request.QueryString["dnnprintmode"] != "true" && !UrlUtils.InPopUp())
			{
				var objControlPanel = ControlUtilities.LoadControl<ControlPanelBase>(this, Host.ControlPanel);
                var objForm = (HtmlForm)Page.FindControl("Form");

                if(objControlPanel.IncludeInControlHierarchy)
                {
                    objControlPanel.IsDockable = IsDockable;
                    if (!Host.ControlPanel.EndsWith("controlbar.ascx", StringComparison.InvariantCultureIgnoreCase))
                        Controls.Add(objControlPanel);
                    else
                    {
                        if (objForm != null)
                        {
                            objForm.Controls.AddAt(0, objControlPanel);
                        }
                        else
                        {
                            Page.Controls.AddAt(0, objControlPanel);
                        }
                    }

                    //register admin.css
                    ClientResourceManager.RegisterAdminStylesheet(Page, Globals.HostPath + "admin.css");
                }
			}
		}
	}
}
