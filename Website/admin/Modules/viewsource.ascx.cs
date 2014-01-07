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
using System.Web.UI.WebControls;

using DotNetNuke.Common;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Framework;
using DotNetNuke.Services.Exceptions;
using DotNetNuke.Services.Localization;
using DotNetNuke.UI.Skins.Controls;

#endregion

namespace DotNetNuke.Modules.Admin.Modules
{
    public partial class ViewSource : PortalModuleBase
    {

        #region Private Members

        protected bool CanEditSource
        {
            get
            {
                return Request.IsLocal;
            }
        }

        protected int ModuleControlId
        {
            get
            {
                var moduleControlId = Null.NullInteger;
                if ((Request.QueryString["ctlid"] != null))
                {
                    moduleControlId = Int32.Parse(Request.QueryString["ctlid"]);
                }
                return moduleControlId;
            }
        }

        private string ReturnURL
        {
            get
            {
                return UrlUtils.ValidReturnUrl(Request.Params["ReturnURL"]) ?? Globals.NavigateURL();
            }
        }

        #endregion

        #region Private Methods

        private void BindFiles(string controlSrc)
        {
            cboFile.Items.Clear();
            //cboFile.Items.Add(new ListItem(Localization.GetString("None_Specified"), "None"));
            //cboFile.Items.Add(new ListItem("User Control", "UserControl"));
            cboFile.AddItem(Localization.GetString("None_Specified"), "None");
            cboFile.AddItem("User Control", "UserControl");

            var srcPhysicalPath = Server.MapPath(controlSrc);
            if (File.Exists(srcPhysicalPath + ".vb") || File.Exists(srcPhysicalPath + ".cs"))
            {
                //cboFile.Items.Add(new ListItem("Code File", "CodeFile"));
                cboFile.AddItem("Code File", "CodeFile");
            }
            var fileName = Path.GetFileName(srcPhysicalPath);
            var folder = Path.GetDirectoryName(srcPhysicalPath);
            if (File.Exists(folder + "\\App_LocalResources\\" + fileName + ".resx"))
            {
                //cboFile.Items.Add(new ListItem("Resource File", "ResourceFile"));
                cboFile.AddItem("Resource File", "ResourceFile");
            }
        }

        private string GetSourceFileName(string controlSrc)
        {
            var srcPhysicalPath = Server.MapPath(controlSrc);
            var srcFile = Null.NullString;
            switch (cboFile.SelectedValue)
            {
                case "UserControl":
                    srcFile = srcPhysicalPath;
                    break;
                case "CodeFile":
                    if (File.Exists(srcPhysicalPath + ".vb"))
                    {
                        srcFile = srcPhysicalPath + ".vb";
                    }
                    else if (File.Exists(srcPhysicalPath + ".cs"))
                    {
                        srcFile = srcPhysicalPath + ".cs";
                    }
                    break;
                case "ResourceFile":
                    var fileName = Path.GetFileName(srcPhysicalPath);
                    var folder = Path.GetDirectoryName(srcPhysicalPath);
                    srcFile = folder + "\\App_LocalResources\\" + fileName + ".resx";
                    break;
            }
            return srcFile;
        }

        private void DisplayFile()
        {
            var objModuleControl = ModuleControlController.GetModuleControl(ModuleControlId);
            if (objModuleControl != null)
            {
                var srcVirtualPath = objModuleControl.ControlSrc;
                var srcFile = Null.NullString;
                var displaySource = cboFile.SelectedValue != "None";

                if (displaySource)
                {
                    srcFile = GetSourceFileName(srcVirtualPath);
                    lblSourceFile.Text = string.Format(Localization.GetString("SourceFile", LocalResourceFile), srcFile);

                    var objStreamReader = File.OpenText(srcFile);
                    txtSource.Text = objStreamReader.ReadToEnd();
                    objStreamReader.Close();
                }
                lblSourceFile.Visible = displaySource;
                trSource.Visible = displaySource;
            }
        }

        #endregion

        #region Event Handlers

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            cboFile.SelectedIndexChanged += OnFileIndexChanged;
            cmdUpdate.Click += OnUpdateClick;

            if (Page.IsPostBack == false)
            {
                cmdCancel.NavigateUrl = ReturnURL;

                var objModuleControl = ModuleControlController.GetModuleControl(ModuleControlId);
                if (objModuleControl != null)
                {
                    BindFiles(objModuleControl.ControlSrc);
                }
                if (Request.UrlReferrer != null)
                {
                    ViewState["UrlReferrer"] = Convert.ToString(Request.UrlReferrer);
                }
                else
                {
                    ViewState["UrlReferrer"] = "";
                }
            }
            cmdUpdate.Visible = CanEditSource;
            txtSource.Enabled = CanEditSource;
        }

        protected void OnFileIndexChanged(object sender, EventArgs e)
        {
            DisplayFile();
        }

        private void OnUpdateClick(object sender, EventArgs e)
        {
            try
            {
                if (cboFile.SelectedValue == "None")
                {
                    //No file type selected
                    UI.Skins.Skin.AddModuleMessage(this, Localization.GetString("NoFileTypeSelected", LocalResourceFile), ModuleMessage.ModuleMessageType.RedError);
                }
                else
                {
                    var objModuleControl = ModuleControlController.GetModuleControl(ModuleControlId);
                    if (objModuleControl != null)
                    {
                        var srcVirtualPath = objModuleControl.ControlSrc;
                        var srcPhysicalPath = GetSourceFileName(srcVirtualPath);
                        if (File.Exists(srcPhysicalPath))
                        {
                            File.SetAttributes(srcPhysicalPath, FileAttributes.Normal);
                            var objStream = File.CreateText(srcPhysicalPath);
                            objStream.WriteLine(txtSource.Text);
                            objStream.Close();
                        }
                    }
                    Response.Redirect(ReturnURL, true);
                }
            }
            catch (Exception exc) //Module failed to load
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }

        #endregion

    }
}