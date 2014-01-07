#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2014
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
using System.Collections;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Xml;

using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Modules.Actions;
using DotNetNuke.Instrumentation;
using DotNetNuke.Security;
using DotNetNuke.Services.Exceptions;
using DotNetNuke.Services.Localization;
using DotNetNuke.Services.Personalization;
using DotNetNuke.UI.Skins.Controls;
using DotNetNuke.UI.Utilities;

using Telerik.Web.UI;

using DataCache = DotNetNuke.Common.Utilities.DataCache;
using DNNControls = DotNetNuke.UI.WebControls;
using Globals = DotNetNuke.Common.Globals;

#endregion

namespace DotNetNuke.Modules.Admin.Languages
{
    /// -----------------------------------------------------------------------------
    /// <summary>
    ///   Manages translations for Resource files
    /// </summary>
    /// <remarks>
    /// </remarks>
    /// <history>
    ///   [vmasanas]	10/04/2004  Created
    ///   [vmasanas]	25/03/2006	Modified to support new host resources and incremental saving
    /// </history>
    /// -----------------------------------------------------------------------------
    public partial class LanguageEditor : PortalModuleBase, IActionable
    {
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(LanguageEditor));
        #region Private Enums

        /// -----------------------------------------------------------------------------
        /// <summary>
        ///   Identifies images in TreeView
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <history>
        ///   [vmasanas]	07/10/2004	Created
        /// </history>
        /// -----------------------------------------------------------------------------
        private enum eImageType
        {
            Folder = 0,
            Page = 1
        }

        #endregion

        #region Protected Properties

        protected string Locale
        {
            get
            {
                string _Locale = Null.NullString;

                if (!string.IsNullOrEmpty(Request.QueryString["locale"]))
                {
                    _Locale = Request.QueryString["locale"];
                }

                return _Locale;
            }
        }

        protected int PageSize
        {
            get
            {
                int _PageSize = 1000;
                if (Settings["PageSize"] != null)
                {
                    _PageSize = Convert.ToInt32(Settings["PageSize"]);

                    //Make sure Page Size is not invalid
                    if (_PageSize < 1)
                    {
                        _PageSize = 1000;
                    }
                }
                return _PageSize;
            }
        }

        protected bool UsePaging
        {
            get
            {
                bool _UsePaging = false;
                if (Settings["UsePaging"] != null)
                {
                    _UsePaging = Convert.ToBoolean(Settings["UsePaging"]);
                }
                return _UsePaging;
            }
        }

        #endregion

        #region Private Methods

        /// -----------------------------------------------------------------------------
        /// <summary>
        ///   Saves / Gets the selected resource file being edited in viewstate
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// </remarks>
        /// <history>
        ///   [vmasanas]	07/10/2004	Created
        /// </history>
        /// -----------------------------------------------------------------------------
        private string SelectedResourceFile
        {
            get
            {
                return ViewState["SelectedResourceFile"].ToString();
            }
            set
            {
                ViewState["SelectedResourceFile"] = value;
            }
        }

        private static string GetResourceKeyXPath(string resourceKeyName)
        {
            return "//root/data[@name=" + XmlUtils.XPathLiteral(resourceKeyName) + "]";
        }
        private XmlNode AddResourceKey(XmlDocument resourceDoc, string resourceKey)
        {
            XmlNode nodeData = null;
            XmlAttribute attr = null;

            // missing entry
            nodeData = resourceDoc.CreateElement("data");
            attr = resourceDoc.CreateAttribute("name");
            attr.Value = resourceKey;
            nodeData.Attributes.Append(attr);
            resourceDoc.SelectSingleNode("//root").AppendChild(nodeData);

            return nodeData.AppendChild(resourceDoc.CreateElement("value"));
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        ///   Loads Resource information into the datagrid
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <history>
        ///   [vmasanas]	04/10/2004	Created
        ///   [vmasanas}  25/03/2006  Modified to support new features
        /// </history>
        /// -----------------------------------------------------------------------------
        private void BindGrid(bool reBind)
        {
            Hashtable EditTable = null;
            Hashtable DefaultTable = null;

            EditTable = LoadFile(rbMode.SelectedValue, "Edit");
            DefaultTable = LoadFile(rbMode.SelectedValue, "Default");

            lblResourceFile.Text = Path.GetFileName(ResourceFile(Locale, rbMode.SelectedValue).Replace(Globals.ApplicationMapPath, ""));
            lblFolder.Text = ResourceFile(Locale, rbMode.SelectedValue).Replace(Globals.ApplicationMapPath, "").Replace("\\" + lblResourceFile.Text, "");

            // check edit table
            // if empty, just use default
            if (EditTable.Count == 0)
            {
                EditTable = DefaultTable;
            }
            else
            {
                //remove obsolete keys
                var ToBeDeleted = new ArrayList();
                foreach (string key in EditTable.Keys)
                {
                    if (!DefaultTable.Contains(key))
                    {
                        ToBeDeleted.Add(key);
                    }
                }
                if (ToBeDeleted.Count > 0)
                {
                    UI.Skins.Skin.AddModuleMessage(this, Localization.GetString("Obsolete", LocalResourceFile), ModuleMessage.ModuleMessageType.YellowWarning);
                    foreach (string key in ToBeDeleted)
                    {
                        EditTable.Remove(key);
                    }
                }

                //add missing keys
                foreach (string key in DefaultTable.Keys)
                {
                    if (!EditTable.Contains(key))
                    {
                        EditTable.Add(key, DefaultTable[key]);
                    }
                    else
                    {
                        // Update default value
                        var p = (Pair)EditTable[key];
                        p.Second = ((Pair)DefaultTable[key]).First;
                        EditTable[key] = p;
                    }
                }
            }

            var s = new SortedList(EditTable);

            resourcesGrid.DataSource = s;
            if (reBind)
            {
                resourcesGrid.DataBind();
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        ///   Initializes ResourceFile treeView
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <history>
        ///   [vmasanas]	25/03/2006	Created
        /// </history>
        /// -----------------------------------------------------------------------------
        private void LoadRootNodes()
        {
            var node = new RadTreeNode();
            node.Text = LocalizeString("LocalResources");
            node.Value = "Local Resources";
            node.ExpandMode = TreeNodeExpandMode.ServerSideCallBack;
            resourceFiles.Nodes.Add(node);

            node = new RadTreeNode();
            node.Text = LocalizeString("GlobalResources");
            node.Value = "Global Resources";
            node.ExpandMode = TreeNodeExpandMode.ServerSideCallBack;
            resourceFiles.Nodes.Add(node);

            node = new RadTreeNode();
            node.Text = LocalizeString("SiteTemplates");
            node.Value = "Site Templates";
            node.ExpandMode = TreeNodeExpandMode.ServerSideCallBack;
            resourceFiles.Nodes.Add(node);

        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        ///   Loads resources from file
        /// </summary>
        /// <param name = "mode">Active editor mode</param>
        /// <param name = "type">Resource being loaded (edit or default)</param>
        /// <returns></returns>
        /// <remarks>
        ///   Depending on the editor mode, resources will be overrided using default DNN schema.
        ///   "Edit" resources will only load selected file.
        ///   When loading "Default" resources (to be used on the editor as helpers) fallback resource
        ///   chain will be used in order for the editor to be able to correctly see what 
        ///   is the current default value for the any key. This process depends on the current active
        ///   editor mode:
        ///   - System: when editing system base resources on en-US needs to be loaded
        ///   - Host: base en-US, and base locale especific resource
        ///   - Portal: base en-US, host override for en-US, base locale especific resource, and host override 
        ///   for locale
        /// </remarks>
        /// <history>
        ///   [vmasanas]	25/03/2006	Created
        /// </history>
        /// -----------------------------------------------------------------------------
        private Hashtable LoadFile(string mode, string type)
        {
            string file = null;
            var ht = new Hashtable();

            switch (type)
            {
                case "Edit":
                    // Only load resources from the file being edited
                    file = ResourceFile(Locale, mode);
                    ht = LoadResource(ht, file);
                    break;
                case "Default":
                    // Load system default
                    file = ResourceFile(Localization.SystemLocale, "System");
                    ht = LoadResource(ht, file);

                    switch (mode)
                    {
                        case "Host":
                            // Load base file for selected locale
                            file = ResourceFile(Locale, "System");
                            ht = LoadResource(ht, file);
                            break;
                        case "Portal":
                            //Load host override for default locale
                            file = ResourceFile(Localization.SystemLocale, "Host");
                            ht = LoadResource(ht, file);

                            if (Locale != Localization.SystemLocale)
                            {
                                // Load base file for locale
                                file = ResourceFile(Locale, "System");
                                ht = LoadResource(ht, file);

                                //Load host override for selected locale
                                file = ResourceFile(Locale, "Host");
                                ht = LoadResource(ht, file);
                            }
                            break;
                    }

                    break;
            }

            return ht;
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        ///   Loads resources from file into the HastTable
        /// </summary>
        /// <param name = "ht">Current resources HashTable</param>
        /// <param name = "filepath">Resources file</param>
        /// <returns>Base table updated with new resources </returns>
        /// <remarks>
        ///   Returned hashtable uses resourcekey as key.
        ///   Value contains a Pair object where:
        ///   First=>value to be edited
        ///   Second=>default value
        /// </remarks>
        /// <history>
        ///   [vmasanas]	25/03/2006	Created
        /// </history>
        /// -----------------------------------------------------------------------------
        private Hashtable LoadResource(Hashtable ht, string filepath)
        {
            var d = new XmlDocument();
            bool xmlLoaded = false;
            try
            {
                d.Load(filepath);
                xmlLoaded = true;
                //exc As Exception
            }
            catch
            {
                xmlLoaded = false;
            }
            if (xmlLoaded)
            {
                XmlNode n = null;
                foreach (XmlNode n_loopVariable in d.SelectNodes("root/data"))
                {
                    n = n_loopVariable;
                    if (n.NodeType != XmlNodeType.Comment)
                    {
                        string val = n.SelectSingleNode("value").InnerXml;
                        if (ht[n.Attributes["name"].Value] == null)
                        {
                            ht.Add(n.Attributes["name"].Value, new Pair(val, val));
                        }
                        else
                        {
                            ht[n.Attributes["name"].Value] = new Pair(val, val);
                        }
                    }
                }
            }
            return ht;
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        ///   Returns the resource file name for a given resource and language
        /// </summary>
        /// <param name = "mode">Identifies the resource being searched (System, Host, Portal)</param>
        /// <returns>Localized File Name</returns>
        /// <remarks>
        /// </remarks>
        /// <history>
        ///   [vmasanas]	04/10/2004	Created
        ///   [vmasanas]	25/03/2006	Modified to support new host resources and incremental saving
        /// </history>
        /// -----------------------------------------------------------------------------
        private string ResourceFile(string language, string mode)
        {
            return Localization.GetResourceFileName(SelectedResourceFile, language, mode, PortalId);
        }

        #endregion

        #region Protected Methods

        /// -----------------------------------------------------------------------------
        /// <summary>
        ///   Configures the initial visibility status of the default label
        /// </summary>
        /// <param name = "p"></param>
        /// <returns></returns>
        /// <remarks>
        /// </remarks>
        /// <history>
        ///   [Vicenç]	26/03/2006	Created
        /// </history>
        /// -----------------------------------------------------------------------------
        protected bool ExpandDefault(Pair p)
        {
            return p.Second.ToString().Length < 150;
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        ///   Builds the url for the lang. html editor control
        /// </summary>
        /// <param name = "name"></param>
        /// <returns></returns>
        /// <remarks>
        /// </remarks>
        /// <history>
        ///   [vmasanas]	07/10/2004	Created
        /// </history>
        /// -----------------------------------------------------------------------------
        protected string OpenFullEditor(string name)
        {
            string file = null;
            file = SelectedResourceFile.Replace(Server.MapPath(Globals.ApplicationPath + "/"), "");
            return EditUrl("Name",
                           name,
                           "EditResourceKey",
                           "Locale=" + Locale,
                           "ResourceFile=" + Globals.QueryStringEncode(file.Replace("\\", "/")),
                           "Mode=" + rbMode.SelectedValue,
                           "Highlight=" + chkHighlight.Checked.ToString().ToLower());
        }

        #endregion

        #region Event Handlers

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            chkHighlight.CheckedChanged += chkHighlight_CheckedChanged;
            cmdCancel.Click += cmdCancel_Click;
            cmdDelete.Click += cmdDelete_Click;
            cmdUpdate.Click += cmdUpdate_Click;
            rbMode.SelectedIndexChanged += rbMode_SelectedIndexChanged;
            resourceFiles.NodeClick += resourceFiles_NodeClick;
            resourceFiles.NodeExpand += resourceFiles_NodeExpand;
            resourcesGrid.ItemDataBound += resourcesGrid_ItemDataBound;
            resourcesGrid.NeedDataSource += resourcesGrid_NeedDataSource;

            resourcesGrid.AllowPaging = UsePaging;
            resourcesGrid.PageSize = PageSize;
            resourcesGrid.ScreenRowNumber = PageSize;
            resourcesGrid.MasterTableView.NoMasterRecordsText = Localization.GetString("NoRecords", LocalResourceFile);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        ///   Loads suported locales and shows default values
        /// </summary>
        /// <param name = "sender"></param>
        /// <param name = "e"></param>
        /// <remarks>
        /// </remarks>
        /// <history>
        ///   [vmasanas]	04/10/2004	Created
        ///   [vmasanas]	25/03/2006	Modified to support new host resources and incremental saving
        /// </history>
        /// -----------------------------------------------------------------------------
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            try
            {
                if (!Page.IsPostBack)
                {
                    ClientAPI.AddButtonConfirm(cmdDelete, Localization.GetString("DeleteItem"));

                    // init tree
                    LoadRootNodes();

                    Locale language = LocaleController.Instance.GetLocale(Locale);
                    languageLabel.Language = language.Code;

                    if (UserInfo.IsSuperUser)
                    {
                        string mode = Request.QueryString["mode"];
                        if (!string.IsNullOrEmpty(mode) && (rbMode.Items.FindByValue(mode) != null))
                        {
                            rbMode.SelectedValue = mode;
                        }
                        else
                        {
                            rbMode.SelectedValue = "Host";
                        }
                    }
                    else
                    {
                        rbMode.SelectedValue = "Portal";
                        rowMode.Visible = false;
                    }

                    string PersonalHighlight = Convert.ToString(Personalization.GetProfile("LanguageEditor", "HighLight" + PortalId));
                    string highlight = Request.QueryString["highlight"];
                    if (!string.IsNullOrEmpty(highlight) && highlight.ToLower() == "true")
                    {
                        chkHighlight.Checked = true;
                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(PersonalHighlight))
                        {
                            chkHighlight.Checked = Convert.ToBoolean(PersonalHighlight);
                        }
                    }

                    if (!string.IsNullOrEmpty(Request.QueryString["resourcefile"]))
                    {
                        SelectedResourceFile = Server.MapPath("~/" + Globals.QueryStringDecode(Request.QueryString["resourcefile"]));
                    }
                    else
                    {
                        SelectedResourceFile = Server.MapPath(Localization.GlobalResourceFile);
                    }

                    if (!string.IsNullOrEmpty(Request.QueryString["message"]))
                    {
                        UI.Skins.Skin.AddModuleMessage(this, Localization.GetString(Request.QueryString["message"], LocalResourceFile), ModuleMessage.ModuleMessageType.GreenSuccess);
                    }

                    BindGrid(!IsPostBack);
                }
                //Module failed to load
            }
            catch (Exception exc)
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        ///   Rebinds the grid
        /// </summary>
        /// <param name = "sender"></param>
        /// <param name = "e"></param>
        /// <remarks>
        /// </remarks>
        /// <history>
        ///   [vmasanas]	25/03/2006	Created
        /// </history>
        /// -----------------------------------------------------------------------------
        private void chkHighlight_CheckedChanged(Object sender, EventArgs e)
        {
            try
            {
                Personalization.SetProfile("LanguageEditor", "HighLight" + PortalSettings.PortalId, chkHighlight.Checked.ToString());
                BindGrid(true);
                //Module failed to load
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
                UI.Skins.Skin.AddModuleMessage(this, Localization.GetString("Save.ErrorMessage", LocalResourceFile), ModuleMessage.ModuleMessageType.YellowWarning);
            }
        }

        protected void cmdCancel_Click(object sender, EventArgs e)
        {
            Response.Redirect(Globals.NavigateURL());
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        ///   Deletes the localized file for a given locale
        /// </summary>
        /// <param name = "sender"></param>
        /// <param name = "e"></param>
        /// <remarks>
        ///   System Default file cannot be deleted
        /// </remarks>
        /// <history>
        ///   [vmasanas]	04/10/2004	Created
        ///   [vmasanas]	25/03/2006	Modified to support new host resources and incremental saving
        /// </history>
        /// -----------------------------------------------------------------------------
        private void cmdDelete_Click(Object sender, EventArgs e)
        {
            try
            {
                if (Locale == Localization.SystemLocale && rbMode.SelectedValue == "System")
                {
                    UI.Skins.Skin.AddModuleMessage(this, Localization.GetString("Delete.ErrorMessage", LocalResourceFile), ModuleMessage.ModuleMessageType.YellowWarning);
                }
                else
                {
                    try
                    {
                        if (File.Exists(ResourceFile(Locale, rbMode.SelectedValue)))
                        {
                            File.SetAttributes(ResourceFile(Locale, rbMode.SelectedValue), FileAttributes.Normal);
                            File.Delete(ResourceFile(Locale, rbMode.SelectedValue));
                            UI.Skins.Skin.AddModuleMessage(this,
                                                           string.Format(Localization.GetString("Deleted", LocalResourceFile), ResourceFile(Locale, rbMode.SelectedValue)),
                                                           ModuleMessage.ModuleMessageType.GreenSuccess);

                            BindGrid(true);

                            //Clear the resource file lookup dictionary as we have removed a file
                            DataCache.RemoveCache(DataCache.ResourceFileLookupDictionaryCacheKey);
                        }
                    }
                    catch
                    {
                        UI.Skins.Skin.AddModuleMessage(this, Localization.GetString("Save.ErrorMessage", LocalResourceFile), ModuleMessage.ModuleMessageType.YellowWarning);
                    }
                }
                //Module failed to load
            }
            catch (Exception exc)
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        ///   Updates all values from the datagrid
        /// </summary>
        /// <param name = "sender"></param>
        /// <param name = "e"></param>
        /// <remarks>
        /// </remarks>
        /// <history>
        ///   [vmasanas]	04/10/2004	Created
        ///   [vmasanas]	25/03/2006	Modified to support new host resources and incremental saving
        ///   [sleupold]  23/04/2010  Fixed missing parameters for navigateURL
        /// </history>
        /// -----------------------------------------------------------------------------
        private void cmdUpdate_Click(Object sender, EventArgs e)
        {
            XmlNode node = null;
            XmlNode parent = null;
            var resDoc = new XmlDocument();
            var defDoc = new XmlDocument();
            string filename = null;

            try
            {
                filename = ResourceFile(Locale, rbMode.SelectedValue);
                if (!File.Exists(filename))
                {
                    // load system default
                    resDoc.Load(ResourceFile(Localization.SystemLocale, "System"));
                }
                else
                {
                    resDoc.Load(filename);
                }
                defDoc.Load(ResourceFile(Localization.SystemLocale, "System"));

                // only items different from default will be saved
                foreach (GridDataItem di in resourcesGrid.Items)
                {
                    if ((di.ItemType == GridItemType.Item || di.ItemType == GridItemType.AlternatingItem))
                    {
                        var resourceKey = (Label)di.FindControl("resourceKey");
                        var txtValue = (TextBox)di.FindControl("txtValue");
                        var txtDefault = (TextBox)di.FindControl("txtDefault");

                        node = resDoc.SelectSingleNode(GetResourceKeyXPath(resourceKey.Text) + "/value");

                        switch (rbMode.SelectedValue)
                        {
                            case "System":
                                // this will save all items
                                if (node == null)
                                {
                                    node = AddResourceKey(resDoc, resourceKey.Text);
                                }
                                node.InnerXml = Server.HtmlEncode(txtValue.Text);

                                break;
                            case "Host":
                            case "Portal":
                                // only items different from default will be saved

                                if (txtValue.Text != txtDefault.Text)
                                {
                                    if (node == null)
                                    {
                                        node = AddResourceKey(resDoc, resourceKey.Text);
                                    }
                                    node.InnerXml = Server.HtmlEncode(txtValue.Text);
                                }
                                else if ((node != null))
                                {
                                    // remove item = default
                                    resDoc.SelectSingleNode("//root").RemoveChild(node.ParentNode);
                                }
                                break;
                        }
                    }
                }

                // remove obsolete keys
                foreach (XmlNode node_loopVariable in resDoc.SelectNodes("//root/data"))
                {
                    node = node_loopVariable;
                    if (defDoc.SelectSingleNode(GetResourceKeyXPath(node.Attributes["name"].Value)) == null)
                    {
                        parent = node.ParentNode;
                        parent.RemoveChild(node);
                    }
                }
                // remove duplicate keys
                foreach (XmlNode node_loopVariable in resDoc.SelectNodes("//root/data"))
                {
                    node = node_loopVariable;
                    if (resDoc.SelectNodes(GetResourceKeyXPath(node.Attributes["name"].Value)).Count > 1)
                    {
                        parent = node.ParentNode;
                        parent.RemoveChild(node);
                    }
                }

                switch (rbMode.SelectedValue)
                {
                    case "System":
                        resDoc.Save(filename);
                        break;
                    case "Host":
                    case "Portal":
                        if (resDoc.SelectNodes("//root/data").Count > 0)
                        {
                            // there's something to save
                            resDoc.Save(filename);
                        }
                        else
                        {
                            // nothing to be saved, if file exists delete
                            if (File.Exists(filename))
                            {
                                File.Delete(filename);
                            }
                        }
                        break;
                }
                string selectedFile = SelectedResourceFile.Replace(Server.MapPath(Globals.ApplicationPath + "/"), "");
                BindGrid(true);
            }
            catch (Exception exc)
            {
                Logger.Error(exc);

                UI.Skins.Skin.AddModuleMessage(this, Localization.GetString("Save.ErrorMessage", LocalResourceFile), ModuleMessage.ModuleMessageType.YellowWarning);
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        ///   Rebinds the grid
        /// </summary>
        /// <param name = "sender"></param>
        /// <param name = "e"></param>
        /// <remarks>
        /// </remarks>
        /// <history>
        ///   [vmasanas]	25/03/2006	Created
        ///   [erikvb]    24/02/2010  added personalization
        /// </history>
        /// -----------------------------------------------------------------------------
        private void rbMode_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                Personalization.SetProfile("LanguageEditor", "Mode" + PortalSettings.PortalId, rbMode.SelectedValue);
                BindGrid(true);
            }
            catch
            {
                UI.Skins.Skin.AddModuleMessage(this, Localization.GetString("Save.ErrorMessage", LocalResourceFile), ModuleMessage.ModuleMessageType.YellowWarning);
            }
        }

        protected void resourceFiles_NodeClick(object sender, RadTreeNodeEventArgs e)
        {
            try
            {
                if (e.Node.Nodes.Count == 0)
                {
                    SelectedResourceFile = e.Node.Value;
                    try
                    {
                        BindGrid(true);
                    }
                    catch
                    {
                        UI.Skins.Skin.AddModuleMessage(this, Localization.GetString("Save.ErrorMessage", LocalResourceFile), ModuleMessage.ModuleMessageType.YellowWarning);
                    }
                }
                //Module failed to load
            }
            catch (Exception exc)
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }

        protected void resourceFiles_NodeExpand(object sender, RadTreeNodeEventArgs e)
        {
            RadTreeNode node = default(RadTreeNode);
            switch (e.Node.Value)
            {
                case "Local Resources":
                    node = new RadTreeNode();
                    node.Text = "Admin";
                    node.Value = Server.MapPath("~/Admin");
                    node.ExpandMode = TreeNodeExpandMode.ServerSideCallBack;
                    e.Node.Nodes.Add(node);
                    node = new RadTreeNode();
                    node.Text = "Controls";
                    node.Value = Server.MapPath("~/Controls");
                    node.ExpandMode = TreeNodeExpandMode.ServerSideCallBack;
                    e.Node.Nodes.Add(node);
                    node = new RadTreeNode();
                    node.Text = "DesktopModules";
                    node.Value = Server.MapPath("~/DesktopModules");
                    node.ExpandMode = TreeNodeExpandMode.ServerSideCallBack;
                    e.Node.Nodes.Add(node);
                    node = new RadTreeNode();
                    node.Text = "Install";
                    node.Value = Server.MapPath("~/Install");
                    node.ExpandMode = TreeNodeExpandMode.ServerSideCallBack;
                    e.Node.Nodes.Add(node);
                    node = new RadTreeNode();
                    node.Text = "Providers";
                    node.Value = Server.MapPath("~/Providers");
                    node.ExpandMode = TreeNodeExpandMode.ServerSideCallBack;
                    e.Node.Nodes.Add(node);

                    if (HasLocalResources(Path.Combine(Globals.HostMapPath, "Skins")))
                    {
                        node = new RadTreeNode();
                        node.Text = LocalizeString("HostSkins");
                        node.Value = Path.Combine(Globals.HostMapPath, "Skins");
                        node.ExpandMode = TreeNodeExpandMode.ServerSideCallBack;
                        e.Node.Nodes.Add(node);
                    }

                    string portalSkinFolder = Path.Combine(PortalSettings.HomeDirectoryMapPath, "Skins");
                    if (Directory.Exists(portalSkinFolder) && (PortalSettings.ActiveTab.ParentId == PortalSettings.AdminTabId))
                    {
                        node = new RadTreeNode();
                        node.Text = LocalizeString("PortalSkins");
                        node.Value = Path.Combine(PortalSettings.HomeDirectoryMapPath, "Skins");
                        node.ExpandMode = TreeNodeExpandMode.ServerSideCallBack;
                        e.Node.Nodes.Add(node);
                    }
                    break;
                case "Global Resources":
                    node = new RadTreeNode();
                    node.Text = LocalizeString("Exceptions");
                    node.Value = Server.MapPath("~/App_GlobalResources/Exceptions");
                    e.Node.Nodes.Add(node);
                    node = new RadTreeNode();
                    node.Text = Path.GetFileNameWithoutExtension(Localization.GlobalResourceFile);
                    node.Value = Server.MapPath(Localization.GlobalResourceFile);
                    e.Node.Nodes.Add(node);
                    node = new RadTreeNode();
                    node.Text = Path.GetFileNameWithoutExtension(Localization.SharedResourceFile);
                    node.Value = Server.MapPath(Localization.SharedResourceFile);
                    e.Node.Nodes.Add(node);
                    node = new RadTreeNode();
                    node.Text = LocalizeString("Template");
                    node.Value = Server.MapPath("~/App_GlobalResources/Template");
                    e.Node.Nodes.Add(node);
                    node = new RadTreeNode();
                    node.Text = LocalizeString("WebControls");
                    node.Value = Server.MapPath("~/App_GlobalResources/WebControls");
                    e.Node.Nodes.Add(node);
                    break;
                case "Site Templates":
                    GetResxFiles(Server.MapPath("~/Portals/_default"), e);
                    break;
                default:
                    GetResxDirectories(e.Node.Value, e);
                    GetResxFiles(e.Node.Value, e);
                    break;
            }

            e.Node.Expanded = true;
        }

        private void GetResxDirectories(string path, RadTreeNodeEventArgs e)
        {
            foreach (string folder in Directory.GetDirectories(path))
            {
                var folderInfo = new DirectoryInfo(folder);
                var node = new RadTreeNode { Value = folderInfo.FullName, Text = folderInfo.Name, ExpandMode = TreeNodeExpandMode.ServerSideCallBack };

                if (HasLocalResources(folderInfo.FullName))
                {
                    e.Node.Nodes.Add(node);
                }
            }
        }

        private bool HasLocalResources(string path)
        {
            var folderInfo = new DirectoryInfo(path);

            if (path.ToLowerInvariant().EndsWith(Localization.LocalResourceDirectory))
            {
                return true;
            }

            bool hasResources = false;
            foreach (string folder in Directory.GetDirectories(path))
            {
                if ((File.GetAttributes(folder) & FileAttributes.ReparsePoint) != FileAttributes.ReparsePoint)
                {
                    folderInfo = new DirectoryInfo(folder);
                    hasResources = hasResources || HasLocalResources(folderInfo.FullName);
                }
            }
            return hasResources || folderInfo.GetFiles("*.resx").Length > 0;

        }

        private void GetResxFiles(string path, RadTreeNodeEventArgs e)
        {
            foreach (string file in Directory.GetFiles(path, "*.resx"))
            {
                var fileInfo = new FileInfo(file);
                var match = Regex.Match(fileInfo.Name, @"\.(\w\w\-\w\w)\.resx");

                if (match.Success && match.Groups[1].Value.ToLowerInvariant() != "en-us")
                {
                    continue;
                }
                var node = new RadTreeNode { Value = fileInfo.FullName, Text = fileInfo.Name.Replace(".resx", "") };
                e.Node.Nodes.Add(node);
            }
        }

        protected void resourcesGrid_ItemDataBound(object sender, GridItemEventArgs e)
        {
            try
            {
                if (e.Item.ItemType == GridItemType.AlternatingItem || e.Item.ItemType == GridItemType.Item)
                {
                    HyperLink c = null;
                    c = (HyperLink)e.Item.FindControl("lnkEdit");
                    if ((c != null))
                    {
                        ClientAPI.AddButtonConfirm(c, Localization.GetString("SaveWarning", LocalResourceFile));
                    }

                    var p = (Pair)((DictionaryEntry)e.Item.DataItem).Value;

                    var t = (TextBox)e.Item.FindControl("txtValue");
                    var d = (TextBox)e.Item.FindControl("txtDefault");

                    if (p.First.ToString() == p.Second.ToString() && chkHighlight.Checked && !string.IsNullOrEmpty(p.Second.ToString()))
                    {
                        t.CssClass = "Pending";
                    }
                    int length = p.First.ToString().Length;
                    if (p.Second.ToString().Length > length)
                    {
                        length = p.Second.ToString().Length;
                    }
                    if (length > 30)
                    {
                        int height = 18 * (length / 30);
                        if (height > 108)
                        {
                            height = 108;
                        }
                        t.Height = new Unit(height);
                        t.TextMode = TextBoxMode.MultiLine;
                        d.Height = new Unit(height);
                        d.TextMode = TextBoxMode.MultiLine;
                        d.CssClass += " dnnTextArea";
                    }
                    t.Text = Server.HtmlDecode(p.First.ToString());
                    d.Text = Server.HtmlDecode(p.Second.ToString());
                }
                //Module failed to load
            }
            catch (Exception exc)
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }

        protected void resourcesGrid_NeedDataSource(object source, GridNeedDataSourceEventArgs e)
        {
            BindGrid(false);
        }

        #endregion

        #region Optional Interfaces

        public ModuleActionCollection ModuleActions
        {
            get
            {
                return new ModuleActionCollection();
            }
        }

        #endregion
    }
}