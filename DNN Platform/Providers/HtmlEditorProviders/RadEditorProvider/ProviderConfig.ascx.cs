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

using DotNetNuke.Web.Client.ClientResourceManagement;

namespace DotNetNuke.Providers.RadEditorProvider
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Text.RegularExpressions;
    using System.Web.UI;
    using System.Web.UI.HtmlControls;
    using System.Web.UI.WebControls;
    using System.Xml;

    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Tabs;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Security.Roles;
    using DotNetNuke.Services.Exceptions;
    using DotNetNuke.Services.FileSystem;
    using DotNetNuke.Services.Localization;
    using DotNetNuke.UI.WebControls;
    using DotNetNuke.Web.UI.WebControls;

    using Telerik.Web.UI;

    public partial class ProviderConfig : Entities.Modules.PortalModuleBase, Entities.Modules.IActionable
    {
        private static Regex RoleMatchRegex = new Regex("^RoleId\\.([-\\d]+)", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        #region Private Members


        private const string spellCheckRootNodeIIS6 = "/configuration/system.web";

        private const string spellCheckRootNodeIIS7 = "/configuration/system.webServer";


        private XmlDocument _dnnConfig;

        private List<ConfigInfo> _defaultconfig;

        #endregion

        protected List<ConfigInfo> DefaultConfig
        {
            get
            {
                if (DataCache.GetCache("RAD_DEFAULT_CONFIG") != null)
                {
                    this._defaultconfig = (List<ConfigInfo>)DataCache.GetCache("RAD_DEFAULT_CONFIG");
                }

                if (this._defaultconfig == null)
                {
                    this._defaultconfig = this.InitializeDefaultConfig();
                    DataCache.SetCache("RAD_DEFAULT_CONFIG", this._defaultconfig);
                }

                return this._defaultconfig;
            }
        }

        /// <summary>Gets the DNN configuration.</summary>
        /// <value>The DNN configuration.</value>
        protected XmlDocument DNNConfig
        {
            get
            {
                if (this._dnnConfig == null)
                {
                    UserInfo currentUser = UserController.Instance.GetCurrentUserInfo();
                    if (currentUser != null && currentUser.IsSuperUser)
                    {
                        this._dnnConfig = Config.Load();
                    }
                }

                return this._dnnConfig;
            }
        }

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

			ClientResourceManager.EnableAsyncPostBackHandler();

            this.treeTools.NodeClick += this.treeTools_NodeClick;
            this.cmdUpdate.Click += this.OnUpdateClick;
            this.cmdCancel.Click += this.OnCancelClick;
            this.cmdCopy.Click += this.OnCopyClick;
            this.cmdDelete.Click += this.OnDeleteClick;
            this.cmdCreate.Click += this.cmdCreate_Click;
            this.chkPortal.CheckedChanged += this.chkPortal_CheckedChanged;

            Framework.AJAX.RegisterScriptManager();
            this.BindConfigForm();
        }

        /// <summary>Raises the <see cref="E:System.Web.UI.Control.Load" /> event.</summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            try
            {
                if (Request.IsAuthenticated)
                {
                    if (UserInfo.IsSuperUser)
                    {
                        // No reason to show purpose of module, inconsistent w/ rest of UI (in other modules, parts of core)
                        // DotNetNuke.UI.Skins.Skin.AddModuleMessage(Me, Localization.GetString("lblNote", LocalResourceFile), Skins.Controls.ModuleMessage.ModuleMessageType.YellowWarning)

                        if (!this.IsPostBack)
                        {
                            this.BindRoles();
                        }

                        if (!Page.IsPostBack)
                        {
                            this.LoadConfiguration();
                            this.LoadPages();
                        }
                    }
                    else
                    {
                        UI.Skins.Skin.AddModuleMessage(
                            this,
                            Localization.GetString("lblHostOnly", this.LocalResourceFile),
                            UI.Skins.Controls.ModuleMessage.ModuleMessageType.RedError);
                    }
                }
                else
                {
                    UI.Skins.Skin.AddModuleMessage(
                        this,
                        Localization.GetString("lblNotAuthorized", this.LocalResourceFile),
                        UI.Skins.Controls.ModuleMessage.ModuleMessageType.RedError);
                }
            }
            catch (Exception exc)
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }

        /// <summary>Handles the NodeClick event of the treeTools control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RadTreeNodeEventArgs"/> instance containing the event data.</param>
        protected void treeTools_NodeClick(object sender, RadTreeNodeEventArgs e)
        {
            this.BindFile();
        }

        /// <summary>Called when [update click].</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void OnUpdateClick(object sender, EventArgs e)
        {
            string orgConfigPath = this.treeTools.SelectedNode.Value;
            string orgToolsPath = orgConfigPath.ToLower().Replace("config", "tools");

            this.UpdateConfig(orgConfigPath);

            StreamWriter tw = File.CreateText(orgToolsPath);
            tw.Write(this.txtTools.Text);
            tw.Close();
            tw.Dispose();

            UI.Skins.Skin.AddModuleMessage(this, "The update was successful.", UI.Skins.Controls.ModuleMessage.ModuleMessageType.GreenSuccess);
        }

        /// <summary>Called when [cancel click].</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void OnCancelClick(object sender, EventArgs e)
        {
            this.pnlEditor.Visible = false;
            this.pnlForm.Visible = false;
            this.LoadConfiguration();
            this.LoadPages();

            UI.Skins.Skin.AddModuleMessage(
                this,
                "All unsaved changes have been discarded.",
                UI.Skins.Controls.ModuleMessage.ModuleMessageType.BlueInfo);
        }

        /// <summary>Called when copy is clicked.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void OnCopyClick(object sender, EventArgs e)
        {
            this.pnlEditor.Visible = false;
            this.pnlForm.Visible = true;
            this.cmdCreate.Visible = true;
            this.rblMode.SelectedIndex = 0;

            if (this.treeTools.SelectedNode != null)
            {
                var role = RoleController.Instance.GetRoleByName(PortalId, this.treeTools.SelectedNode.Text);
                if (role != null)
                {
                    this.rblMode.SelectedValue = role.RoleID.ToString(CultureInfo.InvariantCulture);
                }
            }
        }

        /// <summary>Called when delete is clicked.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        protected void OnDeleteClick(object sender, System.EventArgs e)
        {
            string orgConfigPath = this.treeTools.SelectedNode.Value;
            string orgToolsPath = orgConfigPath.ToLower().Replace("config", "tools");

            if (!orgConfigPath.ToLower().EndsWith("configfile.xml"))
            {
                System.IO.File.Delete(orgConfigPath);
                System.IO.File.Delete(orgToolsPath);
            }

            this.pnlEditor.Visible = false;
            this.pnlForm.Visible = false;
            this.LoadConfiguration();
            this.LoadPages();
        }

        /// <summary>Handles the Click event of the cmdCreate control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void cmdCreate_Click(object sender, EventArgs e)
        {
            string orgConfigPath = this.treeTools.SelectedNode.Value;
            string orgToolsPath = orgConfigPath.ToLower().Replace("config", "tools");

            string newConfigPath = Server.MapPath(this.TemplateSourceDirectory) + "\\ConfigFile\\ConfigFile";
            string newToolsPath = Server.MapPath(this.TemplateSourceDirectory) + "\\ToolsFile\\ToolsFile";

            if (!string.IsNullOrEmpty(this.rblMode.SelectedValue) && this.rblMode.SelectedValue != Globals.glbRoleAllUsers)
            {
                newConfigPath += ".RoleId." + this.rblMode.SelectedValue;
                newToolsPath += ".RoleId." + this.rblMode.SelectedValue;
            }

            if (this.chkPortal.Checked)
            {
                newConfigPath += ".PortalId." + this.PortalSettings.PortalId;
                newToolsPath += ".PortalId." + this.PortalSettings.PortalId;
            }
            else
            {
                if (this.treePages.SelectedNode != null)
                {
                    newConfigPath += ".TabId." + this.treePages.SelectedNode.Value;
                    newToolsPath += ".TabId." + this.treePages.SelectedNode.Value;
                }
            }

            newConfigPath += ".xml";
            newToolsPath += ".xml";

            if (!File.Exists(newConfigPath))
            {
                File.Copy(orgConfigPath, newConfigPath, true);
            }

            if (!File.Exists(newToolsPath))
            {
                File.Copy(orgToolsPath, newToolsPath, true);
            }

            // reload tree    
            this.LoadConfiguration();

            // select new config
            this.treeTools.FindNodeByValue(newConfigPath).Selected = true;

            // re-bind new config
            this.BindFile();
        }

        /// <summary>Handles the CheckedChanged event of the chkPortal control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        protected void chkPortal_CheckedChanged(object sender, System.EventArgs e)
        {
            divTabs.Visible = (chkPortal.Checked == false);
        }

        #region Private Methods

        private void BindRoles()
        {
            var roles = RoleController.Instance.GetRoles(PortalId, r => r.SecurityMode != SecurityMode.SocialGroup && r.Status == RoleStatus.Approved);

            roles.Insert(0, new RoleInfo { RoleID = int.Parse(Globals.glbRoleAllUsers), RoleName = Globals.glbRoleAllUsersName });
            roles.Insert(1, new RoleInfo { RoleID = int.Parse(Globals.glbRoleSuperUser), RoleName = Globals.glbRoleSuperUserName });

            this.rblMode.DataSource = roles;
            this.rblMode.DataTextField = "RoleName";
            this.rblMode.DataValueField = "RoleId";
            this.rblMode.DataBind();
        }

        private void UpdateConfig(string strPath)
        {
            var xmlConfig = new XmlDocument();
            xmlConfig.Load(strPath);

            XmlNode rootNode = xmlConfig.DocumentElement.SelectSingleNode("/configuration");
            string setting = Null.NullString;
            List<ConfigInfo> currentConfig = this.DefaultConfig;
            var maxFileSize = 0;

            foreach (ConfigInfo objConfig in currentConfig)
            {
                if (objConfig.IsSeparator == false)
                {
                    switch (objConfig.Key.ToLower())
                    {
                        case "stripformattingoptions":
                        case "contentfilters":
                            {
                                var ctl = (CheckBoxList)(FindControlRecursive(plhConfig, "ctl_rc_" + objConfig.Key));

                                if (ctl != null)
                                {
                                    try
                                    {
                                        string strSetting = string.Empty;
                                        bool blnAllSelected = true;
                                        foreach (ListItem item in ctl.Items)
                                        {
                                            if (item.Selected)
                                            {
                                                strSetting += item.Value + ",";
                                            }
                                            else
                                            {
                                                blnAllSelected = false;
                                            }
                                        }

                                        if (blnAllSelected)
                                        {
                                            if (objConfig.Key.ToLower() == "stripformattingoptions")
                                            {
                                                strSetting = "All";
                                            }
                                            else
                                            {
                                                strSetting = "DefaultFilters";
                                            }
                                        }
                                        else
                                        {
                                            if (strSetting.EndsWith(","))
                                            {
                                                strSetting = strSetting.Substring(0, strSetting.Length - 1);
                                            }
                                            if (string.IsNullOrEmpty(strSetting))
                                            {
                                                strSetting = "None";
                                            }
                                        }

                                        setting = strSetting;
                                    }
                                    catch
                                    {
                                    }
                                }

                                break;
                            }

                        case "toolbarmode":
                            {
                                var ctl = (RadioButtonList)(FindControlRecursive(plhConfig, "ctl_rc_" + objConfig.Key));

                                if (ctl != null)
                                {
                                    try
                                    {
                                        setting = ctl.SelectedValue;
                                    }
                                    catch
                                    {
                                    }
                                }

                                break;
                            }
                        case "editmodes":
                            {
                                var ctl = (CheckBoxList)(FindControlRecursive(plhConfig, "ctl_rc_" + objConfig.Key));

                                if (ctl != null)
                                {
                                    try
                                    {
                                        string strSetting = string.Empty;
                                        bool blnAllSelected = true;
                                        foreach (ListItem item in ctl.Items)
                                        {
                                            if (item.Selected)
                                            {
                                                strSetting += item.Value + ",";
                                            }
                                            else
                                            {
                                                blnAllSelected = false;
                                            }
                                        }
                                        if (blnAllSelected)
                                        {
                                            strSetting = "All";
                                        }
                                        else
                                        {
                                            if (strSetting.EndsWith(","))
                                            {
                                                strSetting = strSetting.Substring(0, strSetting.Length - 1);
                                            }
                                            if (string.IsNullOrEmpty(strSetting))
                                            {
                                                strSetting = "All";
                                            }
                                        }

                                        setting = strSetting;
                                    }
                                    catch
                                    {
                                    }
                                }

                                break;
                            }

                        case "imagespath":
                        case "mediapath":
                        case "documentspath":
                        case "flashpath":
                        case "silverlightpath":
                        case "templatepath":
                            {
                                var ctl = (DnnComboBox)(FindControlRecursive(plhConfig, "ctl_rc_" + objConfig.Key));

                                if (ctl != null)
                                {
                                    try
                                    {
                                        setting = ctl.SelectedValue;
                                    }
                                    catch
                                    {
                                    }
                                }

                                break;
                            }

                        case "skin":
                        case "contentareamode":
                            {
                                var ctl = (DnnComboBox)(FindControlRecursive(plhConfig, "ctl_rc_" + objConfig.Key));

                                if (ctl != null)
                                {
                                    try
                                    {
                                        setting = ctl.SelectedValue;
                                    }
                                    catch
                                    {
                                    }
                                }

                                break;
                            }

                        case "borderwidth":
                        case "maxflashsize":
                        case "height":
                        case "maxsilverlightsize":
                        case "maxtemplatesize":
                        case "maximagesize":
                        case "width":
                        case "maxdocumentsize":
                        case "maxmediasize":
                        case "toolswidth":
                            {
                                var ctl = (TextBox)(FindControlRecursive(plhConfig, "ctl_rc_" + objConfig.Key));

                                if (ctl != null)
                                {
                                    try
                                    {
                                        setting = ctl.Text.Replace(".00", string.Empty).Replace(".0", string.Empty);

                                        if (objConfig.Key.ToLowerInvariant().EndsWith("size"))
                                        {
                                            var allowSize = Convert.ToInt32(ctl.Text);
                                            if (allowSize > maxFileSize)
                                            {
                                                maxFileSize = allowSize;
                                            }
                                        }
                                    }
                                    catch
                                    {
                                    }
                                }
                                break;
                            }

                        case "linkstype":
                            {
                                var ctl = (DnnComboBox)(FindControlRecursive(plhConfig, "ctl_rc_" + objConfig.Key));

                                if (ctl != null)
                                {
                                    try
                                    {
                                        setting = ctl.SelectedValue;
                                    }
                                    catch
                                    {
                                    }
                                }
                            }

                            break;
                        case "enableresize":
                        case "allowscripts":
                        case "showportallinks":
                        case "autoresizeheight":
                        case "linksuserelativeurls":
                        case "newlinebr":
                            {
                                var ctl = (CheckBox)(FindControlRecursive(plhConfig, "ctl_rc_" + objConfig.Key));

                                if (ctl != null)
                                {
                                    try
                                    {
                                        setting = ctl.Checked.ToString();
                                    }
                                    catch
                                    {
                                    }
                                }

                                break;
                            }

                        case "language":
                            {
                                var ctl = (DnnLanguageComboBox)(FindControlRecursive(plhConfig, "ctl_rc_" + objConfig.Key));

                                if (ctl != null)
                                {
                                    try
                                    {
                                        setting = ctl.SelectedValue;
                                    }
                                    catch
                                    {
                                    }
                                }

                                break;
                            }
                        default:
                            {
                                var ctl = (TextBox)(FindControlRecursive(plhConfig, "ctl_rc_" + objConfig.Key));

                                if (ctl != null)
                                {
                                    try
                                    {
                                        setting = ctl.Text;
                                    }
                                    catch
                                    {
                                    }
                                }

                                break;
                            }
                    }

                    // look for setting node
                    XmlNode configNode = rootNode.SelectSingleNode("property[@name='" + objConfig.Key + "']");
                    if (configNode != null)
                    {
                        // node found, remove it
                        rootNode.RemoveChild(configNode);
                    }

                    configNode = xmlConfig.CreateElement("property");
                    XmlAttribute xmlAttr = xmlConfig.CreateAttribute("name");
                    xmlAttr.Value = objConfig.Key;
                    configNode.Attributes.Append(xmlAttr);

                    XmlNode settingnode = null;
                    if (setting.Contains(";"))
                    {
                        string[] newsettings = setting.Split(char.Parse(";"));
                        foreach (string value in newsettings)
                        {
                            settingnode = xmlConfig.CreateElement("item");
                            settingnode.InnerText = value;
                            configNode.AppendChild(settingnode);
                        }
                    }
                    else
                    {
                        settingnode = xmlConfig.CreateTextNode(setting);
                        configNode.AppendChild(settingnode);
                    }

                    rootNode.AppendChild(configNode);

                    setting = string.Empty;
                }
            }

            xmlConfig.Save(strPath);

            //update web.config to allow the max file size in http runtime section.
            var configAllowSize = Config.GetMaxUploadSize();
            if (maxFileSize > configAllowSize)
            {
                var configNav = Config.Load();
                var httpNode = configNav.SelectSingleNode("configuration//system.web//httpRuntime");

                XmlUtils.UpdateAttribute(httpNode, "maxRequestLength", (maxFileSize / 1024).ToString());

                Config.Save(configNav);
            }
        }

        private void BindSelectedConfig(string strPath)
        {
            string strCompare = treeTools.SelectedNode.Value.ToLower();
            string strValue = strPath.ToLower();

            if (strValue == strCompare)
            {
                List<ConfigInfo> currentconfig = new List<ConfigInfo>();

                XmlDocument xmlConfig = new XmlDocument();
                xmlConfig.Load(strPath);

                XmlNode rootNode = xmlConfig.DocumentElement.SelectSingleNode("/configuration");
                if (rootNode != null)
                {
                    string key = Null.NullString;
                    string setting = Null.NullString;

                    foreach (XmlNode childnode in rootNode.ChildNodes)
                    {
                        key = childnode.Attributes["name"].Value;

                        if (childnode.HasChildNodes)
                        {
                            if (childnode.ChildNodes.Count == 1)
                            {
                                if (childnode.ChildNodes[0].NodeType == XmlNodeType.Text)
                                {
                                    setting = childnode.InnerText;
                                }
                                else if (childnode.ChildNodes[0].NodeType == XmlNodeType.Element)
                                {
                                    setting = childnode.ChildNodes[0].InnerText;
                                }
                            }
                            else
                            {
                                string strSetting = string.Empty;
                                foreach (XmlNode itemnode in childnode.ChildNodes)
                                {
                                    strSetting += itemnode.InnerText + ";";
                                }
                                setting = strSetting;
                            }
                        }

                        if (setting.EndsWith(";"))
                        {
                            setting = setting.Substring(0, setting.Length - 1);
                        }

                        currentconfig.Add(new ConfigInfo(key, setting, false));

                        key = string.Empty;
                        setting = string.Empty;
                    }

                    foreach (ConfigInfo objConfig in currentconfig)
                    {
                        switch (objConfig.Key.ToLower())
                        {
                            case "stripformattingoptions":
                            case "contentfilters":
                                {
                                    CheckBoxList ctl = (CheckBoxList)(FindControlRecursive(plhConfig, "ctl_rc_" + objConfig.Key));

                                    if (ctl != null)
                                    {
                                        try
                                        {
                                            ctl.ClearSelection();
                                            if (objConfig.Value.Contains(","))
                                            {
                                                foreach (string strSetting in objConfig.Value.Split(char.Parse(",")))
                                                {
                                                    foreach (ListItem item in ctl.Items)
                                                    {
                                                        if (item.Value.ToLower() == strSetting.ToLower())
                                                        {
                                                            item.Selected = true;
                                                            break;
                                                        }
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                if (objConfig.Value.ToLower() == "all" || objConfig.Value.ToLower() == "defaultfilters")
                                                {
                                                    foreach (ListItem item in ctl.Items)
                                                    {
                                                        item.Selected = true;
                                                    }
                                                }
                                                else if (objConfig.Value.ToLower() == "none")
                                                {
                                                    foreach (ListItem item in ctl.Items)
                                                    {
                                                        item.Selected = false;
                                                    }
                                                }
                                                else
                                                {
                                                    foreach (ListItem item in ctl.Items)
                                                    {
                                                        if (item.Value.ToLower() == objConfig.Value.ToLower())
                                                        {
                                                            item.Selected = true;
                                                            break;
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                        catch
                                        {
                                        }
                                    }

                                    break;
                                }
                            case "toolbarmode":
                                {
                                    RadioButtonList ctl = (RadioButtonList)(FindControlRecursive(plhConfig, "ctl_rc_" + objConfig.Key));

                                    if (ctl != null)
                                    {
                                        try
                                        {
                                            ctl.SelectedValue = objConfig.Value;
                                        }
                                        catch
                                        {
                                        }
                                    }
                                    break;
                                }
                            case "editmodes":
                                {
                                    CheckBoxList ctl = (CheckBoxList)(FindControlRecursive(plhConfig, "ctl_rc_" + objConfig.Key));

                                    if (ctl != null)
                                    {
                                        try
                                        {
                                            ctl.ClearSelection();
                                            if (objConfig.Value.Contains(","))
                                            {
                                                foreach (string strSetting in objConfig.Value.Split(char.Parse(",")))
                                                {
                                                    foreach (ListItem item in ctl.Items)
                                                    {
                                                        if (item.Value.ToLower() == strSetting.ToLower())
                                                        {
                                                            item.Selected = true;
                                                            break;
                                                        }
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                if (objConfig.Value.ToLower() == "all")
                                                {
                                                    foreach (ListItem item in ctl.Items)
                                                    {
                                                        item.Selected = true;
                                                    }
                                                }
                                                else
                                                {
                                                    foreach (ListItem item in ctl.Items)
                                                    {
                                                        if (item.Value.ToLower() == objConfig.Value.ToLower())
                                                        {
                                                            item.Selected = true;
                                                            break;
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                        catch
                                        {
                                        }
                                    }
                                    break;
                                }
                            case "imagespath":
                            case "mediapath":
                            case "documentspath":
                            case "flashpath":
                            case "silverlightpath":
                            case "templatepath":
                                {
                                    DnnComboBox ctl = (DnnComboBox)(FindControlRecursive(plhConfig, "ctl_rc_" + objConfig.Key));

                                    if (ctl != null)
                                    {
                                        try
                                        {
                                            ctl.SelectedValue = objConfig.Value;
                                        }
                                        catch
                                        {
                                        }
                                    }
                                    break;
                                }
                            case "skin":
                            case "contentareamode":
                                {
                                    DnnComboBox ctl = (DnnComboBox)(FindControlRecursive(plhConfig, "ctl_rc_" + objConfig.Key));

                                    if (ctl != null)
                                    {
                                        try
                                        {
                                            ctl.SelectedValue = objConfig.Value;
                                        }
                                        catch
                                        {
                                        }
                                    }
                                    break;
                                }
                            case "borderwidth":
                            case "maxflashsize":
                            case "height":
                            case "maxsilverlightsize":
                            case "maxtemplatesize":
                            case "maximagesize":
                            case "width":
                            case "maxdocumentsize":
                            case "maxmediasize":
                            case "toolswidth":
                                {
                                    TextBox ctl = (TextBox)(FindControlRecursive(plhConfig, "ctl_rc_" + objConfig.Key));

                                    if (ctl != null)
                                    {
                                        try
                                        {
                                            ctl.Text = Convert.ToInt32(objConfig.Value.Replace("px", string.Empty)).ToString();
                                        }
                                        catch
                                        {
                                        }
                                    }
                                    break;
                                }
                            case "linkstype":
                                {
                                    var ctl = (DnnComboBox)(FindControlRecursive(plhConfig, "ctl_rc_" + objConfig.Key));

                                    if (ctl != null)
                                    {
                                        try
                                        {
                                            ctl.SelectedValue = objConfig.Value;
                                        }
                                        catch
                                        {
                                        }
                                    }
                                }
                                break;
                            case "enableresize":
                            case "allowscripts":
                            case "showportallinks":
                            case "autoresizeheight":
                            case "linksuserelativeurls":
                            case "newlinebr":
                                {
                                    CheckBox ctl = (CheckBox)(FindControlRecursive(plhConfig, "ctl_rc_" + objConfig.Key));

                                    if (ctl != null)
                                    {
                                        try
                                        {
                                            ctl.Checked = bool.Parse(objConfig.Value);
                                        }
                                        catch
                                        {
                                        }
                                    }
                                    break;
                                }
                            case "language":
                                {
                                    var ctl = (DnnLanguageComboBox)(FindControlRecursive(plhConfig, "ctl_rc_" + objConfig.Key));

                                    if (ctl != null)
                                    {
                                        try
                                        {
                                            ctl.BindData(true);
                                            ctl.SetLanguage(objConfig.Value);
                                        }
                                        catch 
                                        {
                                        }
                                    }
                                    break;
                                }

                            default:
                                {
                                    var ctl = (TextBox)(FindControlRecursive(plhConfig, "ctl_rc_" + objConfig.Key));

                                    if (ctl != null)
                                    {
                                        try
                                        {
                                            ctl.Text = objConfig.Value;
                                        }
                                        catch
                                        {
                                        }
                                    }

                                    break;
                                }
                        }
                    }
                }
            }
        }

        /// <summary>Initializes the default configuration.</summary>
        /// <returns></returns>
        private List<ConfigInfo> InitializeDefaultConfig()
        {
            string strPath = Server.MapPath(this.TemplateSourceDirectory + "/ConfigFile/configfile.xml.original.xml");

            var config = new List<ConfigInfo>();

            var xmlConfig = new XmlDocument();
            xmlConfig.Load(strPath);

            XmlNode rootNode = xmlConfig.DocumentElement.SelectSingleNode("/configuration");
            if (rootNode != null)
            {
                string setting = Null.NullString;

                foreach (XmlNode childnode in rootNode.ChildNodes)
                {
                    string key = childnode.Attributes["name"].Value;

                    if (childnode.HasChildNodes)
                    {
                        if (childnode.ChildNodes.Count == 1)
                        {
                            if (childnode.ChildNodes[0].NodeType == XmlNodeType.Text)
                            {
                                setting = childnode.InnerText;
                            }
                            else if (childnode.ChildNodes[0].NodeType == XmlNodeType.Element)
                            {
                                setting = childnode.ChildNodes[0].InnerText;
                            }
                        }
                        else
                        {
                            string strSetting = string.Empty;
                            foreach (XmlNode itemnode in childnode.ChildNodes)
                            {
                                strSetting += itemnode.InnerText + ";";
                            }

                            setting = strSetting;
                        }
                    }

                    if (setting.EndsWith(";"))
                    {
                        setting = setting.Substring(0, setting.Length - 1);
                    }

                    if (childnode.Attributes["IsSeparator"] != null)
                    {
                        config.Add(new ConfigInfo(key, string.Empty, true));
                    }
                    else
                    {
                        config.Add(new ConfigInfo(key, setting, false));
                    }

                    setting = string.Empty;
                }
            }

            return config;
        }

        private void BindConfigForm()
        {
            var folders = FileSystemUtils.GetFoldersByUser(PortalSettings.PortalId, true, true, "READ");
            this.plhConfig.Controls.Clear();
            var pnlContent = new Panel { CssClass = "pcContent" };

            HtmlGenericControl fsContent = null;
            int i = 0;

            foreach (ConfigInfo objConfig in this.DefaultConfig)
            {
                string key = objConfig.Key;
                string value = objConfig.Value;

                if (objConfig.IsSeparator)
                {
                    if (i > 0)
                    {
                        // it's currently a separator, so if its not the first item it needs to close the previous 'feildset'
                        pnlContent.Controls.Add(fsContent);
                    }

                    i += 1;

                    var localizedTitle = Localization.GetString(key + ".Title", LocalResourceFile);
                    if (string.IsNullOrEmpty(localizedTitle))
                    {
                        localizedTitle = key;
                    }

                    pnlContent.Controls.Add(
                        new LiteralControl(
                            "<h2 id='Panel-ProviderConfig-" + i + "' class='dnnFormSectionHead'><a class='dnnSectionExpanded' href=\"\">"
                            + localizedTitle + "</a></h2>"));
                    fsContent = new HtmlGenericControl("fieldset");
                }
                else
                {
                    var pnlRow = new Panel { CssClass = "dnnFormItem" }; // a row starts here and ends at the right before next, where it is added to the fieldset)
	                var editControlId = "ctl_rc_" + key;
					pnlRow.Controls.Add(this.BuildLabel(key, editControlId));

                    switch (key.ToLower())
                    {
                        case "stripformattingoptions":
                            {
								var ctl = new CheckBoxList { ID = editControlId, RepeatColumns = 2, CssClass = "dnnCBItem" };

                                foreach (string objEnum in Enum.GetNames(typeof(Telerik.Web.UI.EditorStripFormattingOptions)))
                                {
                                    if (objEnum != "All" && objEnum != "None")
                                    {
                                        ctl.Items.Add(new ListItem(objEnum, objEnum));
                                    }
                                }

                                pnlRow.Controls.Add(ctl);
                                break;
                            }

                        case "toolbarmode":
                            {
								var ctl = new RadioButtonList { ID = editControlId, RepeatColumns = 2, CssClass = "dnnFormRadioButtons" };

                                foreach (string objEnum in Enum.GetNames(typeof(Telerik.Web.UI.EditorToolbarMode)))
                                {
                                    ctl.Items.Add(new ListItem(objEnum, objEnum));
                                }

                                pnlRow.Controls.Add(ctl);
                                break;
                            }

                        case "editmodes":
                            {
								var ctl = new CheckBoxList { ID = editControlId, RepeatColumns = 1, CssClass = "dnnCBItem" };

                                foreach (string objEnum in Enum.GetNames(typeof(Telerik.Web.UI.EditModes)))
                                {
                                    if (objEnum != "All")
                                    {
                                        ctl.Items.Add(new ListItem(objEnum, objEnum));
                                    }
                                }

                                pnlRow.Controls.Add(ctl);
                                break;
                            }
                        case "contentfilters":
                            {
                                var ctl = new CheckBoxList();
								ctl.ID = editControlId;
                                ctl.RepeatColumns = 2;
                                ctl.CssClass = "dnnCBItem";

                                foreach (string objEnum in Enum.GetNames(typeof(Telerik.Web.UI.EditorFilters)))
                                {
                                    if (objEnum != "None" && objEnum != "DefaultFilters")
                                    {
                                        ctl.Items.Add(new ListItem(objEnum, objEnum));
                                    }
                                }

                                pnlRow.Controls.Add(ctl);
                                break;
                            }
                        case "imagespath":
                        case "mediapath":
                        case "documentspath":
                        case "flashpath":
                        case "silverlightpath":
                        case "templatepath":
                            {
								var ctl = new DnnComboBox { ID = editControlId };
                                // ctl.Width = Unit.Pixel(253)
                                ctl.Items.Clear();

                                foreach (FolderInfo oFolder in folders)
                                {
                                    if (!oFolder.FolderPath.ToLower().StartsWith("cache"))
                                    {
                                        if (oFolder.FolderPath == string.Empty)
                                        {
                                            ctl.AddItem(Localization.GetString("PortalRoot", this.LocalResourceFile), "/");

                                            ctl.AddItem(Localization.GetString("UserFolder", this.LocalResourceFile), "[UserFolder]");
                                        }
                                        else
                                        {
                                            ctl.AddItem(oFolder.FolderPath, oFolder.FolderPath);
                                        }
                                    }
                                }

                                pnlRow.Controls.Add(ctl);
                                break;
                            }

                        case "skin":
                            {
								var ctl = new DnnComboBox { ID = editControlId };
                                ctl.AddItem("Default", "Default");
                                ctl.AddItem("Black", "Black");
                                ctl.AddItem("Sunset", "Sunset");
                                ctl.AddItem("Hay", "Hay");
                                ctl.AddItem("Forest", "Forest");
                                ctl.AddItem("Vista", "Vista");

                                pnlRow.Controls.Add(ctl);
                                break;
                            }

                        case "linkstype":
                            {
								var ctl = new DnnComboBox { ID = editControlId };
                                ctl.AddItem(this.LocalizeString("LinksType_Normal"), "Normal");
                                ctl.AddItem(this.LocalizeString("LinksType_UseTabName"), "UseTabName");
                                ctl.AddItem(this.LocalizeString("LinksType_UseTabId"), "UseTabId");

                                pnlRow.Controls.Add(ctl);
                            }

                            break;
                        case "enableresize":
                        case "allowscripts":
                        case "showportallinks":
                        case "autoresizeheight":
                        case "linksuserelativeurls":
                        case "newlinebr":
                            {
								var ctl = new CheckBox { ID = editControlId, CssClass = "dnnCBItem" };

                                pnlRow.Controls.Add(ctl);
                                break;
                            }
                        case "borderwidth":
                        case "height":
                        case "width":
                        case "toolswidth":
                            {
								var ctl = new TextBox { Text = @"5", CssClass = "SpinnerStepOne", ID = editControlId };
                                pnlRow.Controls.Add(ctl);
                                break;
                            }

                        case "maxflashsize":
                        case "maxsilverlightsize":
                        case "maxtemplatesize":
                        case "maximagesize":
                        case "maxdocumentsize":
                        case "maxmediasize":
                            {
								var ctl = new TextBox { Text = @"1024", CssClass = "SpinnerStep1024", ID = editControlId };
                                pnlRow.Controls.Add(ctl);
                                break;
                            }

                        case "contentareamode":
                            {
								var ctl = new DnnComboBox { ID = editControlId };

                                foreach (string name in Enum.GetNames(typeof(EditorContentAreaMode)))
                                {
                                    if (name != "All")
                                    {
                                        ctl.AddItem(name, name);
                                    }
                                }

                                pnlRow.Controls.Add(ctl);
                                break;
                            }

                        case "language":
                            {
                                var ctl = new DnnLanguageComboBox
                                              {
												  ID = editControlId,
                                                  LanguagesListType = LanguagesListType.All,
                                                  IncludeNoneSpecified = true,
                                                  CssClass = "languageComboBox"
                                              };
                                pnlRow.Controls.Add(ctl);
                                break;
                            }

                        default:
                            {
								var ctl = new TextBox { ID = editControlId, Text = value };
                                pnlRow.Controls.Add(ctl);
                                break;
                            }
                    }

                    fsContent.Controls.Add(pnlRow);
                }
            }

            pnlContent.Controls.Add(fsContent);

            plhConfig.Controls.Add(pnlContent);
        }

        /// <summary>
        /// This method will build a dnn property label (Same as used in the user profile edit area) that can be added to a control.
        /// </summary>
        /// <param name="resourceKey"></param>
		/// <param name="associatedControlId"></param>
        /// <returns></returns>
        /// <remarks></remarks>
		private PropertyLabelControl BuildLabel(string resourceKey, string associatedControlId)
        {
			var propLabel = new PropertyLabelControl { ID = resourceKey + "_Label", ShowHelp = true, ResourceKey = resourceKey, AssociatedControlId = associatedControlId };

            return propLabel;
        }

        private void BindFile()
        {
            //ulActions.Visible = True
            this.pnlEditor.Visible = true;
            this.pnlForm.Visible = false;

            string configpath = this.treeTools.SelectedNode.Value;
            string toolspath = configpath.ToLower().Replace("config", "tools");

            try
            {
                this.treePages.FindNodeByValue(configpath).ExpandParentNodes();
                this.treePages.FindNodeByValue(configpath).Selected = true;
            }
            catch 
            {
            }

            if (File.Exists(configpath))
            {
                this.BindSelectedConfig(configpath);
                this.ViewState["EditorConfigPath"] = configpath;

                this.cmdUpdate.Enabled = (!(configpath.ToLower().EndsWith("configfile.xml.original.xml")));
                this.cmdCreate.Enabled = true;
                this.cmdDelete.Enabled = (!(configpath.ToLower().EndsWith("configfile.xml.original.xml"))
                                     && ! (configpath.ToLower().EndsWith("configfile.xml")));

                if (File.Exists(toolspath))
                {
                    using (var tr = new StreamReader(toolspath))
                    {
                        this.txtTools.Text = tr.ReadToEnd();
                        tr.Close();
                    }
                }
                else
                {
                    // load default toolsfile
                    string orgPath = Server.MapPath(this.TemplateSourceDirectory + "/ToolsFile/ToolsFile.xml.Original.xml");
                    if (File.Exists(orgPath))
                    {
                        File.Copy(orgPath, toolspath);
                    }

                    if (File.Exists(toolspath))
                    {
                        using (var tr = new StreamReader(toolspath))
                        {
                            this.txtTools.Text = tr.ReadToEnd();
                            tr.Close();
                        }
                    }
                    else
                    {
                        this.txtTools.Text = @"Could not load tools file...";
                    }
                }
            }
        }

        /// <summary>Finds the control recursive.</summary>
        /// <param name="objRoot">The object root.</param>
        /// <param name="id">The identifier.</param>
        /// <returns></returns>
        protected Control FindControlRecursive(Control objRoot, string id)
        {
            if (objRoot.ID == id)
            {
                return objRoot;
            }

            foreach (Control c in objRoot.Controls)
            {
                Control t = this.FindControlRecursive(c, id);
                if (t != null)
                {
                    return t;
                }
            }

            return null;
        }

        private void LoadPages()
        {
            this.treePages.Nodes.Clear();

            var tabs = TabController.Instance.GetTabsByPortal(PortalSettings.PortalId);
            foreach (var oTab in tabs.Values)
            {
                if (oTab.Level == 0)
                {
                    var node = new RadTreeNode { Text = oTab.TabName, Value = oTab.TabID.ToString() };
                    this.treePages.Nodes.Add(node);
                    this.AddChildren(ref node);
                }
            }
        }

        private void AddChildren(ref RadTreeNode treenode)
        {
            var tabs = TabController.Instance.GetTabsByPortal(PortalSettings.PortalId);
            foreach (var objTab in tabs.Values)
            {
                if (objTab.ParentId == int.Parse(treenode.Value))
                {
                    var node = new RadTreeNode { Text = objTab.TabName, Value = objTab.TabID.ToString(CultureInfo.InvariantCulture) };
                    treenode.Nodes.Add(node);
                    this.AddChildren(ref node);
                }
            }
        }

        /// <summary>Loads the configuration.</summary>
        private void LoadConfiguration()
        {
            this.treeTools.Nodes.Clear();

            this.pnlEditor.Visible = false;
            this.pnlForm.Visible = false;

            var rootnode = new RadTreeNode("Default Configuration") { Expanded = true };

            EditorProvider.EnsureDefaultConfigFileExists();
            EditorProvider.EnsurecDefaultToolsFileExists();

            foreach (string file in Directory.GetFiles(Server.MapPath(this.TemplateSourceDirectory + "/ConfigFile")))
            {
                if (file.ToLower().EndsWith("configfile.xml.original.xml"))
                {
                    rootnode.Value = file;
                }
                else
                {
                    //fix for codeplex issue #187
                    bool blnAddNode = true;

                    string nodename = file.Substring(file.LastIndexOf("\\") + 1).Replace(".xml", string.Empty).ToLowerInvariant();
                    if (nodename.StartsWith("configfile") && file.EndsWith(".xml"))
                    {
                        string nodeTitle = "Everyone";

                        string strTargetGroup = nodename.Replace("configfile.", string.Empty);
                        string strTargetTab = string.Empty;

                        if (strTargetGroup.Length > 0)
                        {
                            var roleMatch = RoleMatchRegex.Match(strTargetGroup);
                            if (roleMatch.Success)
                            {
                                var roleId = roleMatch.Groups[1].Value;
                                strTargetTab = strTargetGroup.Replace(roleMatch.Value + ".", string.Empty);
                                var role = RoleController.Instance.GetRoleById(PortalId, Convert.ToInt32(roleId));
                                if (role != null)
                                {
                                    //DNN-6840 - assign selected value only if role exists on the current portal
                                    rblMode.SelectedValue = roleId;
                                    nodeTitle = role.RoleName;
                                }
                                else
                                {
                                    blnAddNode = false;
                                    //do not show the node if the role is not in current portal, or the role is not valid any more(such as deleted).
                                }
                            }
                        }

                        if (strTargetTab.Length > 0)
                        {
                            if (SimulateIsNumeric.IsNumeric(strTargetTab.ToLower().Replace("tabid.", string.Empty)))
                            {
                                try
                                {
                                    TabInfo t = TabController.Instance.GetTab(
                                        Convert.ToInt32(strTargetTab.ToLower().Replace("tabid.", string.Empty)),
                                        PortalSettings.PortalId,
                                        false);
                                    if (t != null)
                                    {
                                        if (t.PortalID != PortalSettings.PortalId)
                                        {
                                            // fix for codeplex issue #187
                                            blnAddNode = false;
                                        }
                                        nodeTitle += " (Page \"" + t.TabName + "\" only)";
                                    }
                                }
                                catch
                                {
                                }
                            }

                            if (SimulateIsNumeric.IsNumeric(strTargetTab.ToLower().Replace("portalid.", string.Empty)))
                            {
                                try
                                {
                                    PortalInfo p =
                                        PortalController.Instance.GetPortal(Convert.ToInt32(strTargetTab.ToLower().Replace("portalid.", string.Empty)));
                                    if (p != null)
                                    {
                                        if (p.PortalID != PortalSettings.PortalId)
                                        {
                                            //fix for codeplex issue #187
                                            blnAddNode = false;
                                        }
                                        nodeTitle += " (Current Portal only)";
                                    }
                                }
                                catch
                                {
                                }
                            }
                        }

                        if (blnAddNode)
                        {
                            rootnode.Nodes.Add(new RadTreeNode(nodeTitle, file));
                        }
                    }
                }
            }

            this.treeTools.Nodes.Add(rootnode);
        }

        #endregion

        #region Optional Interfaces

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Registers the module actions required for interfacing with the portal framework
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        /// <remarks></remarks>
        /// -----------------------------------------------------------------------------
        public Entities.Modules.Actions.ModuleActionCollection ModuleActions
        {
            get
            {
                var actions = new Entities.Modules.Actions.ModuleActionCollection();
                return actions;
            }
        }

        #endregion
    }
}