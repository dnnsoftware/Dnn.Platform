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

using Globals = DotNetNuke.Common.Globals;

#endregion

namespace DotNetNuke.Modules.Admin.ModuleDefinitions
{
    public partial class EditModuleControl : PortalModuleBase
    {
        private int ModuleControlId;
        private int ModuleDefId;
        private DesktopModuleInfo _DesktopModule;

        protected DesktopModuleInfo DesktopModule
        {
            get
            {
                return _DesktopModule ?? (_DesktopModule = DesktopModuleController.GetDesktopModuleByPackageID(PackageID));
            }
        }

        public int PackageID
        {
            get
            {
                int _PackageID = Null.NullInteger;
                if ((Request.QueryString["PackageID"] != null))
                {
                    _PackageID = Int32.Parse(Request.QueryString["PackageID"]);
                }
                return _PackageID;
            }
        }

        protected string ReturnURL
        {
            get
            {
                return EditUrl(TabId, "Edit", true, "PackageID=" + PackageID, "mid=" + ModuleId);
            }
        }

        private void AddFiles(string root, string filter)
        {
            string[] files = Directory.GetFiles(Request.MapPath(Globals.ApplicationPath + "/" + root), filter);
            foreach (string strFile in files)
            {
                string file = root.Replace('\\', '/') + "/" + Path.GetFileName(strFile);
                cboSource.AddItem(file, file.ToLower());
            }

        }

        private void BindControlList(string root, bool isRecursive)
        {
            if (Directory.Exists(Request.MapPath(Globals.ApplicationPath + "/" + root)))
            {
                string[] folders = Directory.GetDirectories(Request.MapPath(Globals.ApplicationPath + "/" + root));
                if (isRecursive)
                {
                    foreach (string strFolder in folders)
                    {
                        BindControlList(strFolder.Substring(Request.MapPath(Globals.ApplicationPath).Length + 1).Replace('\\', '/'), true);
                    }
                }

                AddFiles(root, "*.ascx");
                AddFiles(root, "*.cshtml");
                AddFiles(root, "*.vbhtml");
            }
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

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            cboSource.SelectedIndexChanged += cboSource_SelectedIndexChanged;
            cmdCancel.Click += cmdCancel_Click;
            cmdDelete.Click += cmdDelete_Click;
            cmdUpdate.Click += cmdUpdate_Click;

            try
            {
                ModuleDefId = (Request.QueryString["moduledefid"] != null) ? Int32.Parse(Request.QueryString["moduledefid"]) : Null.NullInteger;
                ModuleControlId = (Request.QueryString["modulecontrolid"] != null) ? Int32.Parse(Request.QueryString["modulecontrolid"]) : Null.NullInteger;
                if (Page.IsPostBack == false)
                {
                    lblModule.Text = DesktopModule.FriendlyName;
                    var objModuleDefinition = ModuleDefinitionController.GetModuleDefinitionByID(ModuleDefId);
                    if (objModuleDefinition != null)
                    {
                        lblDefinition.Text = objModuleDefinition.FriendlyName;
                    }
                    ClientAPI.AddButtonConfirm(cmdDelete, Localization.GetString("DeleteItem"));
                    var moduleControl = ModuleControlController.GetModuleControl(ModuleControlId);
                    BindControlList("DesktopModules", true);
                    BindControlList("Admin/Skins", false);
                    //cboSource.Items.Insert(0, new ListItem("<" + Localization.GetString("None_Specified") + ">", ""));
                    cboSource.InsertItem(0, "<" + Localization.GetString("None_Specified") + ">", "");
                    if (!Null.IsNull(ModuleControlId))
                    {
                        if (moduleControl != null)
                        {
                            txtKey.Text = moduleControl.ControlKey;
                            txtTitle.Text = moduleControl.ControlTitle;
                            if (cboSource.FindItemByValue(moduleControl.ControlSrc.ToLower()) != null)
                            {
                                cboSource.FindItemByValue(moduleControl.ControlSrc.ToLower()).Selected = true;
                                LoadIcons();
                            }
                            else
                            {
                                txtSource.Text = moduleControl.ControlSrc;
                            }
                            if (cboType.FindItemByValue(Convert.ToInt32(moduleControl.ControlType).ToString()) != null)
                            {
                                cboType.FindItemByValue(Convert.ToInt32(moduleControl.ControlType).ToString()).Selected = true;
                            }
                            if (!Null.IsNull(moduleControl.ViewOrder))
                            {
                                txtViewOrder.Text = moduleControl.ViewOrder.ToString();
                            }
                            if (cboIcon.FindItemByValue(moduleControl.IconFile.ToLower()) != null)
                            {
                                cboIcon.FindItemByValue(moduleControl.IconFile.ToLower()).Selected = true;
                            }
                            if (!Null.IsNull(moduleControl.HelpURL))
                            {
                                txtHelpURL.Text = moduleControl.HelpURL;
                            }
                            if (moduleControl.SupportsPartialRendering)
                            {
                                chkSupportsPartialRendering.Checked = true;
                            }
                            supportsModalPopUpsCheckBox.Checked = moduleControl.SupportsPopUps;
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

        private void cmdCancel_Click(object sender, EventArgs e)
        {
            try
            {
                Response.Redirect(ReturnURL, true);
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
                Response.Redirect(ReturnURL, true);
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
                        Response.Redirect(ReturnURL, true);
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
    }
}