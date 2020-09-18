// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.UI.Skins
{
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

    public class SkinControl : UserControlBase
    {
        protected DropDownList cboSkin;
        protected CommandButton cmdPreview;
        protected RadioButton optHost;
        protected RadioButton optSite;
        private string _DefaultKey = "System";
        private string _SkinRoot;
        private string _SkinSrc;
        private string _Width = string.Empty;
        private string _localResourceFile;
        private PortalInfo _objPortal;

        public string DefaultKey
        {
            get
            {
                return this._DefaultKey;
            }

            set
            {
                this._DefaultKey = value;
            }
        }

        public string Width
        {
            get
            {
                return Convert.ToString(this.ViewState["SkinControlWidth"]);
            }

            set
            {
                this._Width = value;
            }
        }

        public string SkinRoot
        {
            get
            {
                return Convert.ToString(this.ViewState["SkinRoot"]);
            }

            set
            {
                this._SkinRoot = value;
            }
        }

        public string SkinSrc
        {
            get
            {
                if (this.cboSkin.SelectedItem != null)
                {
                    return this.cboSkin.SelectedItem.Value;
                }
                else
                {
                    return string.Empty;
                }
            }

            set
            {
                this._SkinSrc = value;
            }
        }

        public string LocalResourceFile
        {
            get
            {
                string fileRoot;
                if (string.IsNullOrEmpty(this._localResourceFile))
                {
                    fileRoot = this.TemplateSourceDirectory + "/" + Localization.LocalResourceDirectory + "/SkinControl.ascx";
                }
                else
                {
                    fileRoot = this._localResourceFile;
                }

                return fileRoot;
            }

            set
            {
                this._localResourceFile = value;
            }
        }

        /// <summary>
        /// The Page_Load server event handler on this page is used
        /// to populate the role information for the page.
        /// </summary>
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            this.optHost.CheckedChanged += this.optHost_CheckedChanged;
            this.optSite.CheckedChanged += this.optSite_CheckedChanged;
            this.cmdPreview.Click += this.cmdPreview_Click;
            try
            {
                if (this.Request.QueryString["pid"] != null && (Globals.IsHostTab(this.PortalSettings.ActiveTab.TabID) || UserController.Instance.GetCurrentUserInfo().IsSuperUser))
                {
                    this._objPortal = PortalController.Instance.GetPortal(int.Parse(this.Request.QueryString["pid"]));
                }
                else
                {
                    this._objPortal = PortalController.Instance.GetPortal(this.PortalSettings.PortalId);
                }

                if (!this.Page.IsPostBack)
                {
                    // save persistent values
                    this.ViewState["SkinControlWidth"] = this._Width;
                    this.ViewState["SkinRoot"] = this._SkinRoot;
                    this.ViewState["SkinSrc"] = this._SkinSrc;

                    // set width of control
                    if (!string.IsNullOrEmpty(this._Width))
                    {
                        this.cboSkin.Width = Unit.Parse(this._Width);
                    }

                    // set selected skin
                    if (!string.IsNullOrEmpty(this._SkinSrc))
                    {
                        switch (this._SkinSrc.Substring(0, 3))
                        {
                            case "[L]":
                                this.optHost.Checked = false;
                                this.optSite.Checked = true;
                                break;
                            case "[G]":
                                this.optSite.Checked = false;
                                this.optHost.Checked = true;
                                break;
                        }
                    }
                    else
                    {
                        // no skin selected, initialized to site skin if any exists
                        string strRoot = this._objPortal.HomeDirectoryMapPath + this.SkinRoot;
                        if (Directory.Exists(strRoot) && Directory.GetDirectories(strRoot).Length > 0)
                        {
                            this.optHost.Checked = false;
                            this.optSite.Checked = true;
                        }
                    }

                    this.LoadSkins();
                }
            }
            catch (Exception exc) // Module failed to load
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }

        protected void optHost_CheckedChanged(object sender, EventArgs e)
        {
            this.LoadSkins();
        }

        protected void optSite_CheckedChanged(object sender, EventArgs e)
        {
            this.LoadSkins();
        }

        protected void cmdPreview_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(this.SkinSrc))
            {
                string strType = this.SkinRoot.Substring(0, this.SkinRoot.Length - 1);

                string strURL = Globals.ApplicationURL() + "&" + strType + "Src=" + Globals.QueryStringEncode(this.SkinSrc.Replace(".ascx", string.Empty));

                if (this.SkinRoot == SkinController.RootContainer)
                {
                    if (this.Request.QueryString["ModuleId"] != null)
                    {
                        strURL += "&ModuleId=" + this.Request.QueryString["ModuleId"];
                    }
                }

                this.Response.Redirect(strURL, true);
            }
        }

        private void LoadSkins()
        {
            this.cboSkin.Items.Clear();

            if (this.optHost.Checked)
            {
                // load host skins
                foreach (KeyValuePair<string, string> Skin in SkinController.GetSkins(this._objPortal, this.SkinRoot, SkinScope.Host))
                {
                    this.cboSkin.Items.Add(new ListItem(Skin.Key, Skin.Value));
                }
            }

            if (this.optSite.Checked)
            {
                // load portal skins
                foreach (KeyValuePair<string, string> Skin in SkinController.GetSkins(this._objPortal, this.SkinRoot, SkinScope.Site))
                {
                    this.cboSkin.Items.Add(new ListItem(Skin.Key, Skin.Value));
                }
            }

            this.cboSkin.Items.Insert(0, new ListItem("<" + Localization.GetString(this.DefaultKey, this.LocalResourceFile) + ">", string.Empty));

            // select current skin
            for (int intIndex = 0; intIndex < this.cboSkin.Items.Count; intIndex++)
            {
                if (this.cboSkin.Items[intIndex].Value.Equals(Convert.ToString(this.ViewState["SkinSrc"]), StringComparison.InvariantCultureIgnoreCase))
                {
                    this.cboSkin.Items[intIndex].Selected = true;
                    break;
                }
            }
        }
    }
}
