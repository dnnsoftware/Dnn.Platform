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
#region Usings

using System;
using System.Collections.Generic;
using System.IO;
using System.Web.UI.WebControls;

using DotNetNuke.Common;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;
using DotNetNuke.Framework;
using DotNetNuke.Services.Exceptions;
using DotNetNuke.Services.Localization;
using DotNetNuke.UI.WebControls;

#endregion

namespace DotNetNuke.UI.Skins
{
    public class SkinControl : UserControlBase
    {
		#region "Private Members"	
		
        private string _DefaultKey = "System";
        private string _SkinRoot;
        private string _SkinSrc;
        private string _Width = "";
        private string _localResourceFile;
        private PortalInfo _objPortal;
        protected DropDownList cboSkin;
        protected CommandButton cmdPreview;
        protected RadioButton optHost;
        protected RadioButton optSite;
		
		#endregion

		#region "Public Properties"

        public string DefaultKey
        {
            get
            {
                return _DefaultKey;
            }
            set
            {
                _DefaultKey = value;
            }
        }

        public string Width
        {
            get
            {
                return Convert.ToString(ViewState["SkinControlWidth"]);
            }
            set
            {
                _Width = value;
            }
        }

        public string SkinRoot
        {
            get
            {
                return Convert.ToString(ViewState["SkinRoot"]);
            }
            set
            {
                _SkinRoot = value;
            }
        }

        public string SkinSrc
        {
            get
            {
                if (cboSkin.SelectedItem != null)
                {
                    return cboSkin.SelectedItem.Value;
                }
                else
                {
                    return "";
                }
            }
            set
            {
                _SkinSrc = value;
            }
        }

        public string LocalResourceFile
        {
            get
            {
                string fileRoot;
                if (String.IsNullOrEmpty(_localResourceFile))
                {
                    fileRoot = TemplateSourceDirectory + "/" + Localization.LocalResourceDirectory + "/SkinControl.ascx";
                }
                else
                {
                    fileRoot = _localResourceFile;
                }
                return fileRoot;
            }
            set
            {
                _localResourceFile = value;
            }
        }
		
		#endregion

		#region "Private Methods"

        private void LoadSkins()
        {
            cboSkin.Items.Clear();

            if (optHost.Checked)
            {
                // load host skins
                foreach (KeyValuePair<string, string> Skin in SkinController.GetSkins(_objPortal, SkinRoot, SkinScope.Host))
                {
                    cboSkin.Items.Add(new ListItem(Skin.Key, Skin.Value));
                }
            }

            if (optSite.Checked)
            {
                // load portal skins
                foreach (KeyValuePair<string, string> Skin in SkinController.GetSkins(_objPortal, SkinRoot, SkinScope.Site))
                {
                    cboSkin.Items.Add(new ListItem(Skin.Key, Skin.Value));
                }
            }

            cboSkin.Items.Insert(0, new ListItem("<" + Localization.GetString(DefaultKey, LocalResourceFile) + ">", ""));


            // select current skin
            for (int intIndex = 0; intIndex < cboSkin.Items.Count; intIndex++)
            {
                if (cboSkin.Items[intIndex].Value.ToLower() == Convert.ToString(ViewState["SkinSrc"]).ToLower())
                {
                    cboSkin.Items[intIndex].Selected = true;
                    break;
                }
            }
        }
		
		#endregion

		#region "Event Handlers"

        /// <summary>
		/// The Page_Load server event handler on this page is used
        /// to populate the role information for the page
		/// </summary>
		protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            #region Bind Handles

            optHost.CheckedChanged += optHost_CheckedChanged;
            optSite.CheckedChanged += optSite_CheckedChanged;
            cmdPreview.Click += cmdPreview_Click;

            #endregion

            try
            {
				if (Request.QueryString["pid"] != null && (Globals.IsHostTab(PortalSettings.ActiveTab.TabID) || UserController.Instance.GetCurrentUserInfo().IsSuperUser))
                {
                    _objPortal = PortalController.Instance.GetPortal(Int32.Parse(Request.QueryString["pid"]));
                }
                else
                {
                    _objPortal = PortalController.Instance.GetPortal(PortalSettings.PortalId);
                }
                if (!Page.IsPostBack)
                {
					//save persistent values
                    ViewState["SkinControlWidth"] = _Width;
                    ViewState["SkinRoot"] = _SkinRoot;
                    ViewState["SkinSrc"] = _SkinSrc;
					
					//set width of control
                    if (!String.IsNullOrEmpty(_Width))
                    {
                        cboSkin.Width = Unit.Parse(_Width);
                    }
					
					//set selected skin
                    if (!String.IsNullOrEmpty(_SkinSrc))
                    {
                        switch (_SkinSrc.Substring(0, 3))
                        {
                            case "[L]":
                                optHost.Checked = false;
                                optSite.Checked = true;
                                break;
                            case "[G]":
                                optSite.Checked = false;
                                optHost.Checked = true;
                                break;
                        }
                    }
                    else
                    {
						//no skin selected, initialized to site skin if any exists
                        string strRoot = _objPortal.HomeDirectoryMapPath + SkinRoot;
                        if (Directory.Exists(strRoot) && Directory.GetDirectories(strRoot).Length > 0)
                        {
                            optHost.Checked = false;
                            optSite.Checked = true;
                        }
                    }
                    LoadSkins();
                }
            }
            catch (Exception exc) //Module failed to load
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }

        protected void optHost_CheckedChanged(object sender, EventArgs e)
        {
            LoadSkins();
        }

        protected void optSite_CheckedChanged(object sender, EventArgs e)
        {
            LoadSkins();
        }

        protected void cmdPreview_Click(object sender, EventArgs e)
        {
            if (!String.IsNullOrEmpty(SkinSrc))
            {
                string strType = SkinRoot.Substring(0, SkinRoot.Length - 1);

                string strURL = Globals.ApplicationURL() + "&" + strType + "Src=" + Globals.QueryStringEncode(SkinSrc.Replace(".ascx", ""));

                if (SkinRoot == SkinController.RootContainer)
                {
                    if (Request.QueryString["ModuleId"] != null)
                    {
                        strURL += "&ModuleId=" + Request.QueryString["ModuleId"];
                    }
                }
                Response.Redirect(strURL, true);
            }
        }
		
		#endregion
    }
}