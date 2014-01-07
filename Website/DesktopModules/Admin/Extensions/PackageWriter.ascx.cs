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
using System.Text;
using System.Text.RegularExpressions;
using System.Web.UI.WebControls;
using System.Xml;
using System.Xml.XPath;

using DotNetNuke.Common;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Services.Exceptions;
using DotNetNuke.Services.Installer;
using DotNetNuke.Services.Installer.Packages;
using DotNetNuke.Services.Installer.Writers;
using DotNetNuke.Services.Localization;
using DotNetNuke.UI.Modules;
using DotNetNuke.UI.Skins.Controls;
using DotNetNuke.UI.WebControls;

#endregion

namespace DotNetNuke.Modules.Admin.Extensions
{
    /// -----------------------------------------------------------------------------
    /// Project	 : DotNetNuke
    /// Class	 : PackageWriter
    /// -----------------------------------------------------------------------------
    /// <summary>
    /// Supplies the functionality for creating Extension packages
    /// </summary>
    /// <history>
    ///     [cnurse]   01/31/2008    Created
    /// </history>
    /// -----------------------------------------------------------------------------
    public partial class PackageWriter : ModuleUserControlBase
    {
		#region "Members"

        private PackageInfo _Package;
        private PackageWriterBase _Writer;

		#endregion

		#region "Public Properties"

        public int PackageID
        {
            get
            {
                int _PageNo = 0;
                if (ViewState["PackageID"] != null)
                {
                    _PageNo = Convert.ToInt32(ViewState["PackageID"]);
                }
                return _PageNo;
            }
            set
            {
                ViewState["PackageID"] = value;
            }
        }

        public PackageInfo Package
        {
            get
            {
                if (_Package == null && PackageID > Null.NullInteger)
                {
                    _Package = PackageController.Instance.GetExtensionPackage(Null.NullInteger, p => p.PackageID == PackageID);
                }
                return _Package;
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets the Return Url
        /// </summary>
        /// <history>
        ///     [cnurse]   07/31/2007    Created
        /// </history>
        /// -----------------------------------------------------------------------------
        public string ReturnURL
        {
            get
            {
                int TabID = ModuleContext.PortalSettings.HomeTabId;
                if (Request.Params["rtab"] != null)
                {
                    TabID = int.Parse(Request.Params["rtab"]);
                }
                return Globals.NavigateURL(TabID);
            }
        }

		#endregion

		#region "Private Methods"

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// This routine checks the Access Security
        /// </summary>
        /// <history>
        ///     [cnurse]   07/26/2007    Created
        /// </history>
        /// -----------------------------------------------------------------------------
        private void CheckSecurity()
        {
            if (!ModuleContext.PortalSettings.UserInfo.IsSuperUser)
            {
                Response.Redirect(Globals.NavigateURL("Access Denied"), true);
            }
        }

        private void CreateManifest()
        {
            foreach (string fileName in Regex.Split(txtFiles.Text, Environment.NewLine))
            {
                string name = fileName.Trim();
                if (!string.IsNullOrEmpty(name))
                {
                    var file = new InstallFile(name);
                    _Writer.AddFile(file);
                }
            }
            foreach (string fileName in Regex.Split(txtAssemblies.Text, Environment.NewLine))
            {
                string name = fileName.Trim();
                if (!string.IsNullOrEmpty(name))
                {
                    var file = new InstallFile(name);
                    _Writer.AddFile(file);
                }
            }
            txtManifest.Text = _Writer.WriteManifest(false);
        }

        private void CreatePackage()
        {
            CheckSecurity();
            string manifestName = txtManifestName.Text;
            if (string.IsNullOrEmpty(manifestName))
            {
                manifestName = txtArchiveName.Text.ToLower().Replace("zip", "dnn");
            }
            if (chkPackage.Checked)
            {
                //Use the installer to parse the manifest and load the files that need to be packaged
                var installer = new Installer(Package, Request.MapPath("."));
                foreach (InstallFile file in installer.InstallerInfo.Files.Values)
                {
                    _Writer.AddFile(file);
                }
                string basePath;
                switch (Package.PackageType)
                {
                    case "Auth_System":
                        basePath = Globals.InstallMapPath + ("AuthSystem");
                        break;
                    case "Container":
                        basePath = Globals.InstallMapPath + ("Container");
                        break;
                    case "CoreLanguagePack":
                    case "ExtensionLanguagePack":
                        basePath = Globals.InstallMapPath + ("Language");
                        break;
                    case "Module":
                        basePath = Globals.InstallMapPath + ("Module");
                        break;
                    case "Provider":
                        basePath = Globals.InstallMapPath + ("Provider");
                        break;
                    case "Skin":
                        basePath = Globals.InstallMapPath + ("Skin");
                        break;
                    default:
                        basePath = Globals.HostMapPath;
                        break;
                }
                if (!manifestName.EndsWith(".dnn"))
                {
                    manifestName += ".dnn";
                }
                if (!txtArchiveName.Text.EndsWith(".zip"))
                {
                    txtArchiveName.Text += ".zip";
                }
                _Writer.CreatePackage(Path.Combine(basePath, txtArchiveName.Text), manifestName, Package.Manifest, true);
                UI.Skins.Skin.AddModuleMessage(this,
                                               string.Format(Localization.GetString("Success", LocalResourceFile),
                                                             ModuleContext.PortalSettings.PortalAlias.HTTPAlias + basePath.Replace(Globals.ApplicationMapPath, "").Replace("\\", "/")),
                                               ModuleMessage.ModuleMessageType.GreenSuccess);
            }
            else if (chkManifest.Checked)
            {
                _Writer.WriteManifest(manifestName, Package.Manifest);
            }
            phInstallLogs.Controls.Add(_Writer.Log.GetLogsTable());
        }

        private void GetAssemblies(bool refreshList)
        {
            GetFiles(string.IsNullOrEmpty(txtFiles.Text));
            if (refreshList)
            {
                txtAssemblies.Text = Null.NullString;
                foreach (InstallFile file in _Writer.Assemblies.Values)
                {
                    txtAssemblies.Text += file.FullName + Environment.NewLine;
                }
            }
        }

        private void GetFiles(bool refreshList)
        {
            _Writer.GetFiles(chkIncludeSource.Checked);
            if (refreshList)
            {
                txtFiles.Text = Null.NullString;
				
                //Display App Code files
                foreach (InstallFile file in _Writer.AppCodeFiles.Values)
                {
                    txtFiles.Text += "[app_code]" + file.FullName + Environment.NewLine;
                }
				
                //Display Script files
                foreach (InstallFile file in _Writer.Scripts.Values)
                {
                    txtFiles.Text += file.FullName + Environment.NewLine;
                }
				
                //Display regular files
                foreach (InstallFile file in _Writer.Files.Values)
                {
                    txtFiles.Text += file.FullName + Environment.NewLine;
                }
            }
        }

		#endregion

		#region "Protected Methods"

        protected string GetText(string type)
        {
            string text = Null.NullString;
            if (type == "Title")
            {
                text = Localization.GetString(wizPackage.ActiveStep.Title + ".Title", LocalResourceFile);
            }
            else if (type == "Help")
            {
                text = Localization.GetString(wizPackage.ActiveStep.Title + ".Help", LocalResourceFile);
            }
            return text;
        }

		#endregion

		#region "Event Handlers"

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Page_Init runs when the control is initialised.
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <history>
        ///     [cnurse]   01/31/2008    Created
        /// </history>
        /// -----------------------------------------------------------------------------
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            if ((Request.QueryString["packageid"] != null))
            {
                PackageID = Int32.Parse(Request.QueryString["packageid"]);
            }
            else
            {
                PackageID = Null.NullInteger;
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// The Page_Load runs when the page loads
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <remarks>
        /// </remarks>
        /// <history>
        ///     [cnurse]   01/31/2008    Created
        /// </history>
        /// -----------------------------------------------------------------------------
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            chkUseManifest.CheckedChanged += chkUseManifest_CheckedChanged;
            cmdGetFiles.Click += cmdGetFiles_Click;
            wizPackage.ActiveStepChanged += wizPackage_ActiveStepChanged;
            wizPackage.CancelButtonClick += wizPackage_CancelButtonClick;
            wizPackage.FinishButtonClick += wizPackage_FinishButtonClick;
            wizPackage.NextButtonClick += wizPackage_NextButtonClick;

            try
            {
                CheckSecurity();

                ctlPackage.EditMode = PropertyEditorMode.View;            
                
                switch (Package.PackageType)
                {
                    case "CoreLanguagePack":
                        Package.IconFile = "N\\A";
                        break;
                    default:
                        Package.IconFile = Util.ParsePackageIconFileName(Package);
                        break;
                }

                ctlPackage.DataSource = Package;
                ctlPackage.DataBind();

                _Writer = PackageWriterFactory.GetWriter(Package);

                if (Page.IsPostBack)
                {
                    _Writer.BasePath = txtBasePath.Text;
                }
                else
                {
                    txtBasePath.Text = _Writer.BasePath;

                    //Load Manifests
                    if (!string.IsNullOrEmpty(Package.Manifest))
                    {
                        cboManifests.Items.Add(new ListItem("Database version", ""));
                    }
                    string filePath = Server.MapPath(_Writer.BasePath);
                    if (!string.IsNullOrEmpty(filePath))
                    {
                        if (Directory.Exists(filePath))
                        {
                            foreach (string file in Directory.GetFiles(filePath, "*.dnn"))
                            {
                                string fileName = file.Replace(filePath + "\\", "");
                                cboManifests.Items.Add(new ListItem(fileName, fileName));
                            }
                            foreach (string file in Directory.GetFiles(filePath, "*.dnn.resources"))
                            {
                                string fileName = file.Replace(filePath + "\\", "");
                                cboManifests.Items.Add(new ListItem(fileName, fileName));
                            }
                        }
                    }
                    if (cboManifests.Items.Count > 0)
                    {
                        trUseManifest.Visible = true;
                    }
                }
            }
            catch (Exception exc) //Module failed to load
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }

        protected void chkUseManifest_CheckedChanged(object sender, EventArgs e)
        {
            trManifestList.Visible = chkUseManifest.Checked;
        }

        protected void cmdGetFiles_Click(object sender, EventArgs e)
        {
            GetFiles(true);
        }

        protected void wizPackage_ActiveStepChanged(object sender, EventArgs e)
        {
            switch (wizPackage.ActiveStepIndex)
            {
                case 1: //Display the files
                    if (chkUseManifest.Checked)
                    {
                        wizPackage.ActiveStepIndex = 3;
                    }
                    GetFiles(string.IsNullOrEmpty(txtFiles.Text));
                    includeSourceRow.Visible = _Writer.HasProjectFile || _Writer.AppCodeFiles.Count > 0;
                    break;
                case 2: //Display the assemblies
                    if (_Writer.IncludeAssemblies)
                    {
                        GetAssemblies(string.IsNullOrEmpty(txtAssemblies.Text));
                    }
                    else
                    {
                        wizPackage.ActiveStepIndex = 3;
                    }
                    break;
                case 3: //Display the manfest
                    if (chkUseManifest.Checked)
                    {
                        if (string.IsNullOrEmpty(cboManifests.SelectedValue))
                        {
							//Use Database
                            var sb = new StringBuilder();
                            var settings = new XmlWriterSettings();
                            settings.ConformanceLevel = ConformanceLevel.Fragment;
                            settings.OmitXmlDeclaration = true;
                            settings.Indent = true;

                            _Writer.WriteManifest(XmlWriter.Create(sb, settings), Package.Manifest);

                            txtManifest.Text = sb.ToString();
                        }
                        else
                        {
                            string filename = Path.Combine(Server.MapPath(_Writer.BasePath), cboManifests.SelectedValue);
                            StreamReader objStreamReader = File.OpenText(filename);
                            txtManifest.Text = objStreamReader.ReadToEnd();
                        }
                    }
                    else
                    {
                        CreateManifest();
                    }
                    if (!chkReviewManifest.Checked)
                    {
                        wizPackage.ActiveStepIndex = 4;
                    }
                    break;
                case 4:
                    txtManifestName.Text = Package.Owner + "_" + Package.Name;
                    if (chkUseManifest.Checked)
                    {
                        txtArchiveName.Text = Package.Owner + "_" + Package.Name + "_" + Globals.FormatVersion(Package.Version) + "_Install.zip";
                        chkManifest.Checked = true;
                        trManifest1.Visible = false;
                        trManifest2.Visible = false;
                    }
                    else
                    {
                        if (chkIncludeSource.Checked)
                        {
                            txtArchiveName.Text = Package.Owner + "_" + Package.Name + "_" + Globals.FormatVersion(Package.Version) + "_Source.zip";
                        }
                        else
                        {
                            txtArchiveName.Text = Package.Owner + "_" + Package.Name + "_" + Globals.FormatVersion(Package.Version) + "_Install.zip";
                        }
                    }
                    if (!txtManifestName.Text.ToLower().EndsWith(".dnn"))
                    {
                        txtManifestName.Text = txtManifestName.Text + ".dnn";
                    }
                    wizPackage.DisplayCancelButton = false;
                    break;
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// wizPackage_CancelButtonClick runs when the Cancel Button on the Wizard is clicked.
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <history>
        ///     [cnurse]   02/01/2008    Created
        /// </history>
        /// -----------------------------------------------------------------------------
        protected void wizPackage_CancelButtonClick(object sender, EventArgs e)
        {
            try
            {
				//Redirect to Definitions page
                Response.Redirect(ReturnURL, true);
            }
            catch (Exception exc) //Module failed to load
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// wizPackage_FinishButtonClick runs when the Finish Button on the Wizard is clicked.
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <history>
        ///     [cnurse]   02/01/2008    Created
        /// </history>
        /// -----------------------------------------------------------------------------
        protected void wizPackage_FinishButtonClick(object sender, WizardNavigationEventArgs e)
        {
            try
            {
				//Redirect to Definitions page
                Response.Redirect(ReturnURL, true);
            }
            catch (Exception exc) //Module failed to load
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// wizPackage_NextButtonClick runs when the next Button is clicked.  It provides
        ///	a mechanism for cancelling the page change if certain conditions aren't met.
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <history>
        ///     [cnurse]   01/31/2008    Created
        /// </history>
        /// -----------------------------------------------------------------------------
        protected void wizPackage_NextButtonClick(object sender, WizardNavigationEventArgs e)
        {
            switch (e.CurrentStepIndex)
            {
                case 3: //Save the Manifest
                    var doc = new XPathDocument(new StringReader(txtManifest.Text));
                    XPathNavigator nav = doc.CreateNavigator();
                    XPathNavigator packageNav = nav.SelectSingleNode("dotnetnuke/packages");
                    Package.Manifest = packageNav.InnerXml;
                    var pkgIconFile = Util.ParsePackageIconFileName(Package);
                    Package.IconFile = (pkgIconFile.Trim().Length > 0) ? Util.ParsePackageIconFile(Package) : null;
                    PackageController.Instance.SaveExtensionPackage(Package);
                    break;
                case 4:
                    if (chkManifest.Checked && (!string.IsNullOrEmpty(txtManifestName.Text)) && (!txtManifestName.Text.ToLower().EndsWith(".dnn")))
                    {
                        lblMessage.Text = Localization.GetString("InvalidManifestExtension", LocalResourceFile);
                        lblMessage.Parent.Visible = true;
                        e.Cancel = true;
                    }
                    else if (chkPackage.Checked && string.IsNullOrEmpty(txtArchiveName.Text))
                    {
                        lblMessage.Text = Localization.GetString("NoFileName", LocalResourceFile);
                        lblMessage.Parent.Visible = true;
                        e.Cancel = true;
                    }
                    else if (chkPackage.Checked && !txtArchiveName.Text.ToLower().EndsWith(".zip"))
                    {
                        lblMessage.Text = Localization.GetString("InvalidPackageName", LocalResourceFile);
                        lblMessage.Parent.Visible = true;
                        e.Cancel = true;
                    }
                    else
                    {
						//Create the Package
                        CreatePackage();
                    }
                    break;
            }
        }
		
		#endregion
    }
}