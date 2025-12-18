// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.UI.Skins
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
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
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
        [SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields", Justification = "Breaking change")]
        protected DropDownList cboSkin;
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
        [SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields", Justification = "Breaking change")]
        protected CommandButton cmdPreview;
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
        [SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields", Justification = "Breaking change")]
        protected RadioButton optHost;
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
        [SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields", Justification = "Breaking change")]
        protected RadioButton optSite;

        private string defaultKey = "System";
        private string skinRoot;
        private string skinSrc;
        private string width = string.Empty;
        private string localResourceFile;
        private PortalInfo objPortal;

        public string DefaultKey
        {
            get => this.defaultKey;
            set => this.defaultKey = value;
        }

        public string Width
        {
            get => Convert.ToString(this.ViewState["SkinControlWidth"], CultureInfo.InvariantCulture);
            set => this.width = value;
        }

        public string SkinRoot
        {
            get => Convert.ToString(this.ViewState["SkinRoot"], CultureInfo.InvariantCulture);
            set => this.skinRoot = value;
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
                this.skinSrc = value;
            }
        }

        public string LocalResourceFile
        {
            get
            {
                string fileRoot;
                if (string.IsNullOrEmpty(this.localResourceFile))
                {
                    fileRoot = this.TemplateSourceDirectory + "/" + Localization.LocalResourceDirectory + "/SkinControl.ascx";
                }
                else
                {
                    fileRoot = this.localResourceFile;
                }

                return fileRoot;
            }

            set
            {
                this.localResourceFile = value;
            }
        }

        /// <summary>The Page_Load server event handler on this page is used to populate the role information for the page.</summary>
        /// <param name="e">The event arguments.</param>
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
                    this.objPortal = PortalController.Instance.GetPortal(int.Parse(this.Request.QueryString["pid"], CultureInfo.InvariantCulture));
                }
                else
                {
                    this.objPortal = PortalController.Instance.GetPortal(this.PortalSettings.PortalId);
                }

                if (!this.Page.IsPostBack)
                {
                    // save persistent values
                    this.ViewState["SkinControlWidth"] = this.width;
                    this.ViewState["SkinRoot"] = this.skinRoot;
                    this.ViewState["SkinSrc"] = this.skinSrc;

                    // set width of control
                    if (!string.IsNullOrEmpty(this.width))
                    {
                        this.cboSkin.Width = Unit.Parse(this.width, CultureInfo.InvariantCulture);
                    }

                    // set selected skin
                    if (!string.IsNullOrEmpty(this.skinSrc))
                    {
                        switch (this.skinSrc.Substring(0, 3))
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
                        string strRoot = this.objPortal.HomeDirectoryMapPath + this.SkinRoot;
                        if (Directory.Exists(strRoot) && Directory.GetDirectories(strRoot).Length > 0)
                        {
                            this.optHost.Checked = false;
                            this.optSite.Checked = true;
                        }
                    }

                    this.LoadSkins();
                }
            }
            catch (Exception exc)
            {
                // Module failed to load
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }

        [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1300:ElementMustBeginWithUpperCaseLetter", Justification = "Breaking Change")]

        // ReSharper disable once InconsistentNaming
        protected void optHost_CheckedChanged(object sender, EventArgs e)
        {
            this.LoadSkins();
        }

        [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1300:ElementMustBeginWithUpperCaseLetter", Justification = "Breaking Change")]

        // ReSharper disable once InconsistentNaming
        protected void optSite_CheckedChanged(object sender, EventArgs e)
        {
            this.LoadSkins();
        }

        [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1300:ElementMustBeginWithUpperCaseLetter", Justification = "Breaking Change")]

        // ReSharper disable once InconsistentNaming
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
                foreach (KeyValuePair<string, string> skin in SkinController.GetSkins(this.objPortal, this.SkinRoot, SkinScope.Host))
                {
                    this.cboSkin.Items.Add(new ListItem(skin.Key, skin.Value));
                }
            }

            if (this.optSite.Checked)
            {
                // load portal skins
                foreach (KeyValuePair<string, string> skin in SkinController.GetSkins(this.objPortal, this.SkinRoot, SkinScope.Site))
                {
                    this.cboSkin.Items.Add(new ListItem(skin.Key, skin.Value));
                }
            }

            this.cboSkin.Items.Insert(0, new ListItem("<" + Localization.GetString(this.DefaultKey, this.LocalResourceFile) + ">", string.Empty));

            // select current skin
            for (int intIndex = 0; intIndex < this.cboSkin.Items.Count; intIndex++)
            {
                if (this.cboSkin.Items[intIndex].Value.Equals(Convert.ToString(this.ViewState["SkinSrc"], CultureInfo.InvariantCulture), StringComparison.OrdinalIgnoreCase))
                {
                    this.cboSkin.Items[intIndex].Selected = true;
                    break;
                }
            }
        }
    }
}
