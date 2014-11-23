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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;

using DotNetNuke.Application;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Framework;
using DotNetNuke.Instrumentation;
using DotNetNuke.Services.Exceptions;
using DotNetNuke.Services.Localization;
using DotNetNuke.UI.Skins.Controls;
using DotNetNuke.UI.Utilities;

using Globals = DotNetNuke.Common.Globals;


#endregion

namespace DotNetNuke.Modules.XmlMerge
{
    public partial class XmlMerge : PortalModuleBase
    {
    	private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof (XmlMerge));

	    protected string ConfirmText
	    {
		    get
		    {
				if (ddlConfig.SelectedValue.ToLowerInvariant() == "web.config")
				{
					return Localization.GetSafeJSString("SaveWarning", LocalResourceFile);
				}

				return Localization.GetSafeJSString("SaveConfirm", LocalResourceFile);
		    }
	    }

        #region Private Functions

        private static bool IsValidDocType(string contentType)
        {
            switch (contentType)
            {
                case "text/plain":
                case "application/xml":
                    return true;
                default:
                    return false;
            }
        }

        private bool IsValidXmlMergDocument(string mergeDocText)
        {
            if (string.IsNullOrEmpty(mergeDocText.Trim()))
            {
                return false;
            }
            //TODO: Add more checks here
            return true;
        }

        private void BindConfigList()
        {
            var files = Directory.GetFiles(Globals.ApplicationMapPath, "*.config");
            IEnumerable<string> fileList = (from file in files select Path.GetFileName(file));
            ddlConfig.DataSource = fileList;
            ddlConfig.DataBind();
            ddlConfig.InsertItem(0, Localization.GetString("SelectConfig", LocalResourceFile), "-1");
            ddlConfig.SelectedIndex = 0;
        }

        private void ValidateSuperUser()
        {
            //Verify that the current user has access to access this page
            if (!UserInfo.IsSuperUser)
            {
                Response.Redirect(Globals.NavigateURL("Access Denied"), true);
            }
        }

        private void LoadConfig(string configFile)
        {
            var configDoc = Config.Load(configFile);
            using (var txtWriter = new StringWriter())
            {
                using (var writer = new XmlTextWriter(txtWriter))
                {
                    writer.Formatting = Formatting.Indented;
                    configDoc.WriteTo(writer);
                }
                txtConfiguration.Text = txtWriter.ToString();
            }
        }

        #endregion

        #region Event Handlers

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            cmdExecute.Click += OnExecuteClick;
            cmdSave.Click += OnSaveClick;
            cmdUpload.Click += OnUploadClick;
            ddlConfig.SelectedIndexChanged += OnConfigFileIndexChanged;

            jQuery.RequestDnnPluginsRegistration();
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            ValidateSuperUser();

            if (!Page.IsPostBack)
            {
                BindConfigList();
            }
            else
            {
                if (ddlConfig.SelectedIndex != 0)
                {
                    //ddlConfig.Attributes.Add("onClick", "javascript: alert('" + Localization.GetString("LoadConfigWarning", LocalResourceFile) + "');");
                    txtConfiguration.Enabled = true;
                }
                else
                {
                    //ddlConfig.Attributes.Remove("onClick");
                    txtConfiguration.Text = string.Empty;
                    txtConfiguration.Enabled = true;
                }
            }
        }

        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);

            SetSaveButtonState();
        }

        protected void OnExecuteClick(object sender, EventArgs e)
        {
            ValidateSuperUser();
            if (IsValidXmlMergDocument(txtScript.Text))
            {
                try
                {
                    var doc = new XmlDocument();
                    doc.LoadXml(txtScript.Text);
                    Application.Application app = DotNetNukeContext.Current.Application;
                    var merge = new Services.Installer.XmlMerge(doc, Globals.FormatVersion(app.Version), app.Description);
                    merge.UpdateConfigs();

                    UI.Skins.Skin.AddModuleMessage(this, Localization.GetString("Success", LocalResourceFile), ModuleMessage.ModuleMessageType.GreenSuccess);
                }
                catch (Exception ex)
                {
                    UI.Skins.Skin.AddModuleMessage(this, Localization.GetString("ERROR_Merge", LocalResourceFile), ModuleMessage.ModuleMessageType.RedError);
                    Exceptions.LogException(ex);
                }
            }
        }

        protected void OnSaveClick(object sender, EventArgs e)
        {
            ValidateSuperUser();
            var configDoc = new XmlDocument();
            try
            {
                configDoc.LoadXml(txtConfiguration.Text);
                Config.Save(configDoc, ddlConfig.SelectedValue);
                LoadConfig(ddlConfig.SelectedValue);

                UI.Skins.Skin.AddModuleMessage(this, Localization.GetString("Success", LocalResourceFile), ModuleMessage.ModuleMessageType.GreenSuccess);
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                UI.Skins.Skin.AddModuleMessage(this, string.Format(Localization.GetString("ERROR_ConfigurationFormat", LocalResourceFile), ex.Message), ModuleMessage.ModuleMessageType.RedError);
                return;
            }
        }

        protected void OnUploadClick(object sender, EventArgs e)
        {
            ValidateSuperUser();
            if (IsValidDocType(uplScript.PostedFile.ContentType))
            {
                var scriptFile = new StreamReader(uplScript.PostedFile.InputStream);
                txtScript.Text = scriptFile.ReadToEnd();
            }
            else
            {
                UI.Skins.Skin.AddModuleMessage(this, string.Format(Localization.GetString("ERROR_ConfigurationFormat", LocalResourceFile),""), ModuleMessage.ModuleMessageType.RedError);
            }
        }

        protected void OnConfigFileIndexChanged(object sender, EventArgs e)
        {
            if (ddlConfig.SelectedIndex != 0)
            {
                LoadConfig(ddlConfig.SelectedValue.ToLowerInvariant());
            }
        }


        private void SetSaveButtonState()
        {

            if (ddlConfig.SelectedIndex <= 0)
            {
                cmdSave.Enabled = false;
                cmdExecute.Enabled = false;
            }
            else
            {
                cmdSave.Enabled = true;

                if (!String.IsNullOrEmpty(txtScript.Text.Trim()))
                {
                    cmdExecute.Enabled = true;
                }
                else
                {
                    cmdExecute.Enabled = false;
                }
            }

        }

        #endregion
    }
}