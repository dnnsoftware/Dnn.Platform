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
using System.Activities.Expressions;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.UI.WebControls;

using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Modules.Definitions;
using DotNetNuke.Security;
using DotNetNuke.Services.Exceptions;
using DotNetNuke.Services.Localization;
using DotNetNuke.UI.Skins.Controls;
using DotNetNuke.UI.Utilities;
using DotNetNuke.Web.UI.WebControls;
using Globals = DotNetNuke.Common.Globals;

#endregion

namespace DotNetNuke.Modules.Admin.ModuleDefinitions
{
    public partial class EditModuleControl : PortalModuleBase
    {
        #region Private Fields

        
        private DesktopModuleInfo _desktopModule;
        private ModuleControlInfo _moduleControl;

        #endregion

        #region Properties

        protected DesktopModuleInfo DesktopModule
        {
            get
            {
                return _desktopModule ?? (_desktopModule = DesktopModuleController.GetDesktopModuleByPackageID(PackageId));
            }
        }

        private int ModuleControlId
        {
            get
            {
                return Request.QueryString["modulecontrolid"] != null ? Int32.Parse(Request.QueryString["modulecontrolid"]) : Null.NullInteger;
            }
        }
        private int ModuleDefId
        {
            get
            {
                return Request.QueryString["moduledefid"] != null ? Int32.Parse(Request.QueryString["moduledefid"]) : Null.NullInteger;
            }
        }

        public int PackageId
        {
            get
            {
                if (Request.QueryString["PackageID"] != null)
                {
                    return Int32.Parse(Request.QueryString["PackageID"]);
                }
                return Null.NullInteger;
            }
        }

        protected string ReturnUrl
        {
            get
            {
                return EditUrl(TabId, "Edit", true, "PackageID=" + PackageId, "mid=" + ModuleId);
            }
        }

        protected ModuleControlInfo ModuleControl
        {
            get
            {
                if (_moduleControl == null && ModuleControlId > Null.NullInteger)
                {
                    _moduleControl = ModuleControlController.GetModuleControl(ModuleControlId);
                }

                return _moduleControl;
            }
        }

        #endregion

        #region Private Methods

        private void AddFiles(string root, string filter)
        {
            string[] files = Directory.GetFiles(Request.MapPath(Globals.ApplicationPath + "/" + root), filter);
            foreach (string strFile in files)
            {
                string file = root.Replace('\\', '/') + "/" + Path.GetFileName(strFile);

                var item = new DnnComboBoxItem(file, file.ToLower());
                if (ModuleControl != null && item.Value.Equals(ModuleControl.ControlSrc.ToLower()))
                {
                    item.Selected = true;
                }
                cboSource.Items.Add(item);
            }

        }

        private void BindControlList()
        {
            cboSource.Items.Clear();
            cboSource.InsertItem(0, "<" + Localization.GetString("None_Specified") + ">", "");

            var root = cboSourceFolder.SelectedValue;
            if (Directory.Exists(Request.MapPath(Globals.ApplicationPath + "/" + root)))
            {
                AddFiles(root, "*.ascx");
                AddFiles(root, "*.cshtml");
                AddFiles(root, "*.vbhtml");
            }
        }

        private void BindSourceFolders()
        {
            IList<string> controlfolders = GetSubdirectories(Request.MapPath(Globals.ApplicationPath + "/DesktopModules"));
            controlfolders.Insert(0, Request.MapPath(Globals.ApplicationPath + "/Admin/Skins"));

            var currentControlFolder = ModuleControl != null ? Path.GetDirectoryName(ModuleControl.ControlSrc.ToLower()).Replace('\\', '/') : string.Empty;

            foreach (var folder in controlfolders)
            {
                  var moduleControls = Directory.EnumerateFiles(folder, "*.*", SearchOption.TopDirectoryOnly).Count(s => s.EndsWith(".ascx") || s.EndsWith(".cshtml")|| s.EndsWith(".vbhtml"));
                    if (moduleControls > 0)
                    {
                        var shortFolder =folder.Substring(Request.MapPath(Globals.ApplicationPath).Length + 1).Replace('\\', '/');

                        var item = new DnnComboBoxItem(shortFolder, shortFolder.ToLower());
                        if (item.Value.Equals(currentControlFolder))
                        {
                            item.Selected = true;
                        }
                        cboSourceFolder.Items.Add(item);
                    }                
            }
        }

        private IList<string> GetSubdirectories(string path)
        {
            return (from subdirectory in Directory.GetDirectories(path, "*", SearchOption.AllDirectories)
                   select subdirectory).ToList();
        }

        private void LoadIcons()
        {
            string root;
            cboIcon.Items.Clear();
            cboIcon.AddItem("<" + Localization.GetString("Not_Specified") + ">", "");
            if (!String.IsNullOrEmpty(cboSource.SelectedItem.Value))
            {
                root = cboSource.SelectedItem.Value;
                root = Request.MapPath(Globals.ApplicationPath + "/" + root.Substring(0, root.LastIndexOf("/")));
                if (Directory.Exists(root))
                {
                    string[] files = Directory.GetFiles(root);
                    foreach (string file in files)
                    {
                        string extension = Path.GetExtension(file);
                        if (extension != null)
                        {
                            extension = extension.Replace(".", "");
                        }
                        if ((Globals.glbImageFileTypes + ",").IndexOf(extension + ",") != -1)
                        {
                            string path = Path.GetFileName(file);
                            if (path != null)
                            {
                                cboIcon.AddItem(Path.GetFileName(file), path.ToLower());
                            }
                        }
                    }
                }
            }
        }

        #endregion

        #region Event Handlers

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            cboSource.SelectedIndexChanged += cboSource_SelectedIndexChanged;
            cboSourceFolder.SelectedIndexChanged += cboSourceFolder_SelectedIndexChanged;
            cmdCancel.Click += cmdCancel_Click;
            cmdDelete.Click += cmdDelete_Click;
            cmdUpdate.Click += cmdUpdate_Click;

            try
            {
                if (!Page.IsPostBack)
                {
                    lblModule.Text = DesktopModule.FriendlyName;
                    var objModuleDefinition = ModuleDefinitionController.GetModuleDefinitionByID(ModuleDefId);
                    if (objModuleDefinition != null)
                    {
                        lblDefinition.Text = objModuleDefinition.FriendlyName;
                    }
                    ClientAPI.AddButtonConfirm(cmdDelete, Localization.GetString("DeleteItem"));
                    
                    
                    BindSourceFolders();
                    BindControlList();
                    
                    if (!Null.IsNull(ModuleControlId))
                    {
                        if (ModuleControl != null)
                        {
                            txtKey.Text = ModuleControl.ControlKey;
                            txtTitle.Text = ModuleControl.ControlTitle;

                            if (!string.IsNullOrEmpty(cboSource.SelectedValue))
                            {
                                LoadIcons();
                            }
                            else
                            {
                                txtSource.Text = ModuleControl.ControlSrc;
                            }

                            if (cboType.FindItemByValue(Convert.ToInt32(ModuleControl.ControlType).ToString()) != null)
                            {
                                cboType.FindItemByValue(Convert.ToInt32(ModuleControl.ControlType).ToString()).Selected = true;
                            }
                            if (!Null.IsNull(ModuleControl.ViewOrder))
                            {
                                txtViewOrder.Text = ModuleControl.ViewOrder.ToString();
                            }
                            if (cboIcon.FindItemByValue(ModuleControl.IconFile.ToLower()) != null)
                            {
                                cboIcon.FindItemByValue(ModuleControl.IconFile.ToLower()).Selected = true;
                            }
                            if (!Null.IsNull(ModuleControl.HelpURL))
                            {
                                txtHelpURL.Text = ModuleControl.HelpURL;
                            }
                            if (ModuleControl.SupportsPartialRendering)
                            {
                                chkSupportsPartialRendering.Checked = true;
                            }
                            supportsModalPopUpsCheckBox.Checked = ModuleControl.SupportsPopUps;
                        }
                    }
                    else
                    {
                        if (cboType.Enabled)
                        {
                            cboType.FindItemByValue("0").Selected = true;
                        }
                        else
                        {
                            cboType.FindItemByValue("-2").Selected = true;
                        }
                    }
                }
            }
            catch (Exception exc)
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }

        private void cboSource_SelectedIndexChanged(object sender, EventArgs e)
        {
            LoadIcons();
        }

        private void cboSourceFolder_SelectedIndexChanged(object sender, EventArgs e)
        {
            BindControlList();
        }

        private void cmdCancel_Click(object sender, EventArgs e)
        {
            try
            {
                Response.Redirect(ReturnUrl, true);
            }
            catch (Exception exc)
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }

        private void cmdDelete_Click(object sender, EventArgs e)
        {
            try
            {
                if (!Null.IsNull(ModuleControlId))
                {
                    ModuleControlController.DeleteModuleControl(ModuleControlId);
                }
                Response.Redirect(ReturnUrl, true);
            }
            catch (Exception exc)
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }

        private void cmdUpdate_Click(object sender, EventArgs e)
        {
            try
            {
                if (Page.IsValid)
                {
                    if (cboSource.SelectedIndex != 0 || !String.IsNullOrEmpty(txtSource.Text))
                    {
                        //check whether have a same control key in the module definition
                        var controlKey = !String.IsNullOrEmpty(txtKey.Text) ? txtKey.Text : Null.NullString;
                        var moduleControls = ModuleControlController.GetModuleControlsByModuleDefinitionID(ModuleDefId).Values;
                        var keyExists = moduleControls.Any(c => c.ControlKey.Equals(controlKey, StringComparison.InvariantCultureIgnoreCase)
                                                            && c.ModuleControlID != ModuleControlId);
                        if(keyExists)
                        {
                            UI.Skins.Skin.AddModuleMessage(this, Localization.GetString("DuplicateKey.ErrorMessage", LocalResourceFile), ModuleMessage.ModuleMessageType.RedError);
                            return;
                        }
                        var moduleControl = new ModuleControlInfo
                                                {
                                                    ModuleControlID = ModuleControlId,
                                                    ModuleDefID = ModuleDefId,
                                                    ControlKey = controlKey,
                                                    ControlTitle = !String.IsNullOrEmpty(txtTitle.Text) ? txtTitle.Text : Null.NullString,
                                                    ControlSrc = !String.IsNullOrEmpty(txtSource.Text) ? txtSource.Text : cboSource.SelectedItem.Text,
                                                    ControlType = (SecurityAccessLevel) Enum.Parse(typeof (SecurityAccessLevel), cboType.SelectedItem.Value),
                                                    ViewOrder = !String.IsNullOrEmpty(txtViewOrder.Text) ? int.Parse(txtViewOrder.Text) : Null.NullInteger,
                                                    IconFile = cboIcon.SelectedIndex > 0 ? cboIcon.SelectedItem.Text : Null.NullString,
                                                    HelpURL = !String.IsNullOrEmpty(txtHelpURL.Text) ? txtHelpURL.Text : Null.NullString,
                                                    SupportsPartialRendering = chkSupportsPartialRendering.Checked,
                                                    SupportsPopUps = supportsModalPopUpsCheckBox.Checked
                                                };
                        try
                        {
                            ModuleControlController.SaveModuleControl(moduleControl, true);
                        }
                        catch
                        {
                            UI.Skins.Skin.AddModuleMessage(this, Localization.GetString("AddControl.ErrorMessage", LocalResourceFile), ModuleMessage.ModuleMessageType.RedError);
                            return;
                        }
                        Response.Redirect(ReturnUrl, true);
                    }
                    else
                    {
                        UI.Skins.Skin.AddModuleMessage(this, Localization.GetString("MissingSource.ErrorMessage", LocalResourceFile), ModuleMessage.ModuleMessageType.RedError);
                    }
                }
            }
            catch (Exception exc)
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }

        #endregion
    }
}