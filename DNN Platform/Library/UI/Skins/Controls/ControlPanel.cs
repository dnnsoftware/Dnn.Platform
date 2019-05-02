﻿#region Copyright
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
