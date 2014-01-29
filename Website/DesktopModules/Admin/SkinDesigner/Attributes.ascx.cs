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
using System.Xml;

using DotNetNuke.Common;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Tabs;
using DotNetNuke.Instrumentation;
using DotNetNuke.Services.Exceptions;
using DotNetNuke.Services.Localization;
using DotNetNuke.UI.Skins;
using DotNetNuke.UI.Skins.Controls;

#endregion

namespace DotNetNuke.Modules.Admin.Skins
{
    public partial class Attributes : PortalModuleBase
    {
    	private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof (Attributes));

        #region Event Handlers

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            cboContainers.SelectedIndexChanged += OnContainerSelectedIndexChanged;
            cboFiles.SelectedIndexChanged += OnFilesSelectedIndexChanged;
            cboSettings.SelectedIndexChanged += OnSettingsSelectedIndexChanged;
            cboSkins.SelectedIndexChanged += OnSkinSelectedIndexChanged;
            cboTokens.SelectedIndexChanged += OnTokenSelectedIndexChanged;
            cmdUpdate.Click += OnUpdateClick;

            try
            {
                if (!Page.IsPostBack)
                {
                    LoadSkins();
                    LoadContainers();
                }
            }
            catch (Exception exc)
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }

        protected void OnSkinSelectedIndexChanged(object sender, EventArgs e)
        {
            ShowSkins();
        }

        protected void OnContainerSelectedIndexChanged(object sender, EventArgs e)
        {
            ShowContainers();
        }

        protected void OnFilesSelectedIndexChanged(object sender, EventArgs e)
        {
            LoadTokens();
        }

        protected void OnTokenSelectedIndexChanged(object sender, EventArgs e)
        {
            LoadSettings();
        }

        protected void OnSettingsSelectedIndexChanged(object sender, EventArgs e)
        {
            LoadValues();
        }

        protected void OnUpdateClick(object sender, EventArgs e)
        {
            try
            {
                Update();
            }
            catch (Exception exc)
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }

        #endregion

        #region Public members
        public void Update()
        {
            if (Page.IsValid)
            {
                UpdateSkin();
            }            
        }

        public bool HideUpdateButton
        {
            set
            {
                this.cmdUpdate.Visible = !value;
            }
        }
        #endregion

        #region Private Methods

        private void ShowSkins()
        {
            var strSkinPath = Globals.ApplicationMapPath.ToLower() + cboSkins.SelectedItem.Value;
            cboContainers.ClearSelection();

            if (cboSkins.SelectedIndex > 0)
            {
                LoadFiles(strSkinPath);
            }
        }

        private void ShowContainers()
        {
            var strContainerPath = Globals.ApplicationMapPath.ToLower() + cboContainers.SelectedItem.Value;
            cboSkins.ClearSelection();

            if (cboContainers.SelectedIndex > 0)
            {
                LoadFiles(strContainerPath);
            }
        }

        private void LoadSkins()
        {
            string strRoot;
            string[] arrFolders;
            string strName;
            string strSkin;

            cboSkins.Items.Clear();
            cboSkins.AddItem("<" + Localization.GetString("Not_Specified") + ">", "");

            //load host skins
            if (UserInfo.IsSuperUser)
            {
                strRoot = Request.MapPath(Globals.HostPath + SkinController.RootSkin);
                if (Directory.Exists(strRoot))
                {
                    arrFolders = Directory.GetDirectories(strRoot);
                    foreach (var strFolder in arrFolders)
                    {
                        strName = strFolder.Substring(strFolder.LastIndexOf("\\") + 1);
                        strSkin = strFolder.Replace(Globals.ApplicationMapPath, "");
                        if (strName != "_default")
                        {
                            cboSkins.AddItem(strName, strSkin.ToLower());
                        }
                    }
                }
            }
			
            //load portal skins
            strRoot = PortalSettings.HomeDirectoryMapPath + SkinController.RootSkin;
            if (Directory.Exists(strRoot))
            {
                arrFolders = Directory.GetDirectories(strRoot);
                foreach (var strFolder in arrFolders)
                {
                    strName = strFolder.Substring(strFolder.LastIndexOf("\\") + 1);
                    strSkin = strFolder.Replace(Globals.ApplicationMapPath, "");
                    cboSkins.AddItem(strName, strSkin.ToLower());
                }
            }
        }

        private void LoadContainers()
        {
            string strRoot;
            string[] arrFolders;
            string strName;
            string strSkin;

            cboContainers.Items.Clear();
            cboContainers.AddItem("<" + Localization.GetString("Not_Specified") + ">", "");

            //load host containers
            if (UserInfo.IsSuperUser)
            {
                strRoot = Request.MapPath(Globals.HostPath + SkinController.RootContainer);
                if (Directory.Exists(strRoot))
                {
                    arrFolders = Directory.GetDirectories(strRoot);
                    foreach (string strFolder in arrFolders)
                    {
                        strName = strFolder.Substring(strFolder.LastIndexOf("\\") + 1);
                        strSkin = strFolder.Replace(Globals.ApplicationMapPath, "");
                        if (strName != "_default")
                        {
                            cboContainers.AddItem(strName, strSkin.ToLower());
                        }
                    }
                }
            }
			
            //load portal containers
            strRoot = PortalSettings.HomeDirectoryMapPath + SkinController.RootContainer;
            if (Directory.Exists(strRoot))
            {
                arrFolders = Directory.GetDirectories(strRoot);
                foreach (var strFolder in arrFolders)
                {
                    strName = strFolder.Substring(strFolder.LastIndexOf("\\") + 1);
                    strSkin = strFolder.Replace(Globals.ApplicationMapPath, "");
                    cboContainers.AddItem(strName, strSkin.ToLower());
                }
            }
        }

        private void LoadFiles(string strFolderPath)
        {
            cboFiles.Items.Clear();
            if (Directory.Exists(strFolderPath))
            {
                var arrFiles = Directory.GetFiles(strFolderPath, "*.ascx");
                foreach (var strFile in arrFiles)
                {
                    cboFiles.AddItem(Path.GetFileNameWithoutExtension(strFile), strFile);
                }
            }
            cboFiles.InsertItem(0, "<" + Localization.GetString("Not_Specified") + ">", "");
        }

        private void LoadTokens()
        {
            cboTokens.DataSource = SkinControlController.GetSkinControls().Values;
            cboTokens.DataBind();

            cboTokens.InsertItem(0, "<" + Localization.GetString("Not_Specified") + ">", "");
        }

        private void LoadSettings()
        {
            cboSettings.Items.Clear();

            var strFile = Globals.ApplicationMapPath + "\\" + cboTokens.SelectedItem.Value.ToLower().Replace("/", "\\").Replace(".ascx", ".xml");
            if (File.Exists(strFile))
            {
                try
                {
                    var xmlDoc = new XmlDocument();
                    xmlDoc.Load(strFile);
                    foreach (XmlNode xmlSetting in xmlDoc.SelectNodes("//Settings/Setting"))
                    {
                        cboSettings.AddItem(xmlSetting.SelectSingleNode("Name").InnerText, xmlSetting.SelectSingleNode("Name").InnerText);
                    }
                }
                catch
                {
                    UI.Skins.Skin.AddModuleMessage(this, "Error Loading Settings File For Object", ModuleMessage.ModuleMessageType.RedError);
                }
            }
            else
            {
                UI.Skins.Skin.AddModuleMessage(this, "Object Selected Does Not Have Settings Defined", ModuleMessage.ModuleMessageType.YellowWarning);
            }
            cboSettings.InsertItem(0, "<" + Localization.GetString("Not_Specified") + ">", "");
        }

        private void LoadValues()
        {
            cboValue.Items.Clear();
            txtValue.Text = "";

            var strFile = Globals.ApplicationMapPath + "\\" + cboTokens.SelectedItem.Value.ToLower().Replace("/", "\\").Replace(".ascx", ".xml");
            if (File.Exists(strFile))
            {
                try
                {
                    var xmlDoc = new XmlDocument();
                    xmlDoc.Load(strFile);
                    foreach (XmlNode xmlSetting in xmlDoc.SelectNodes("//Settings/Setting"))
                    {
                        if (xmlSetting.SelectSingleNode("Name").InnerText == cboSettings.SelectedItem.Value)
                        {
                            string strValue = xmlSetting.SelectSingleNode("Value").InnerText;
                            switch (strValue)
                            {
                                case "":
                                    txtValue.Visible = true;
                                    cboValue.Visible = false;
                                    break;
                                case "[TABID]":
                                    var objTabs = new TabController();
                                    foreach (var objTab in objTabs.GetTabsByPortal(PortalId).Values)
                                    {
                                        cboValue.AddItem(objTab.TabName, objTab.TabID.ToString());
                                    }

                                    cboValue.InsertItem(0, "<" + Localization.GetString("Not_Specified") + ">", "");
                                    cboValue.Visible = true;
                                    txtValue.Visible = false;
                                    break;
                                default:
                                    var arrValues = (strValue + ",").Split(',');
                                    foreach (var value in arrValues)
                                    {
                                        if (!String.IsNullOrEmpty(value))
                                        {
                                            cboValue.AddItem(value, value);
                                        }
                                    }

                                    cboValue.InsertItem(0, "<" + Localization.GetString("Not_Specified") + ">", "");
                                    cboValue.Visible = true;
                                    txtValue.Visible = false;
                                    break;
                            }
                            lblHelp.Text = xmlSetting.SelectSingleNode("Help").InnerText;
                        }
                    }
                }
                catch
                {
                    UI.Skins.Skin.AddModuleMessage(this, "Error Loading Settings File For Object", ModuleMessage.ModuleMessageType.RedError);
                }
            }
            else
            {
                UI.Skins.Skin.AddModuleMessage(this, "Object Selected Does Not Have Settings Defined", ModuleMessage.ModuleMessageType.YellowWarning);
            }
        }

        private void UpdateSkin()
        {
            if (cboSettings.SelectedIndex > 0)
            {
                if ((cboValue.SelectedItem != null) || !String.IsNullOrEmpty(txtValue.Text))
                {
                    var objStreamReader = File.OpenText(cboFiles.SelectedItem.Value);
                    var strSkin = objStreamReader.ReadToEnd();
                    objStreamReader.Close();
                    var strTag = "<dnn:" + cboTokens.SelectedItem.Text + " runat=\"server\" id=\"dnn" + cboTokens.SelectedItem.Text + "\"";
                    var intOpenTag = strSkin.IndexOf(strTag);
                    if (intOpenTag != -1)
                    {
                        var intCloseTag = strSkin.IndexOf(" />", intOpenTag);
                        var strAttribute = cboSettings.SelectedItem.Value;
                        var intStartAttribute = strSkin.IndexOf(strAttribute, intOpenTag);
                        string strValue = cboValue.Visible ? cboValue.SelectedItem.Value : txtValue.Text;
                        if (intStartAttribute != -1 && intStartAttribute < intCloseTag)
                        {
							//remove attribute
                            var intEndAttribute = strSkin.IndexOf("\" ", intStartAttribute);
                            strSkin = strSkin.Substring(0, intStartAttribute) + strSkin.Substring(intEndAttribute + 2);
                        }
                        //add attribute
                        strSkin = strSkin.Insert(intOpenTag + strTag.Length, " " + strAttribute + "=\"" + strValue + "\"");
                        try
                        {
                            File.SetAttributes(cboFiles.SelectedItem.Value, FileAttributes.Normal);
                            var objStream = File.CreateText(cboFiles.SelectedItem.Value);
                            objStream.WriteLine(strSkin);
                            objStream.Close();

                            UpdateManifest();

                            UI.Skins.Skin.AddModuleMessage(this, "Skin Successfully Updated", ModuleMessage.ModuleMessageType.GreenSuccess);
                        }
                        catch
                        {
                            UI.Skins.Skin.AddModuleMessage(this, "Error Updating Skin File", ModuleMessage.ModuleMessageType.RedError);
                        }
                    }
                    else
                    {
                        UI.Skins.Skin.AddModuleMessage(this, "Selected File Does Not Contain Token", ModuleMessage.ModuleMessageType.YellowWarning);
                    }
                }
                else
                {
                    UI.Skins.Skin.AddModuleMessage(this, "You Must Specify A Value For The Setting", ModuleMessage.ModuleMessageType.YellowWarning);
                }
            }
            else
            {
                UI.Skins.Skin.AddModuleMessage(this, "You Must Select A Token Setting", ModuleMessage.ModuleMessageType.YellowWarning);
            }
        }

        private void UpdateManifest()
        {
            if (File.Exists(cboFiles.SelectedItem.Value.Replace(".ascx", ".htm")))
            {
                var strFile = cboFiles.SelectedItem.Value.Replace(".ascx", ".xml");
                if (File.Exists(strFile) == false)
                {
                    strFile = strFile.Replace(Path.GetFileName(strFile), "skin.xml");
                }
                XmlDocument xmlDoc = null;
                try
                {
                    xmlDoc = new XmlDocument();
                    xmlDoc.Load(strFile);
                }
                catch
                {
                    xmlDoc.InnerXml = "<Objects></Objects>";
                }
                var xmlToken = xmlDoc.DocumentElement.SelectSingleNode("descendant::Object[Token='[" + cboTokens.SelectedItem.Text + "]']");
                if (xmlToken == null)
                {
					//add token
                    string strToken = "<Token>[" + cboTokens.SelectedItem.Text + "]</Token><Settings></Settings>";
                    xmlToken = xmlDoc.CreateElement("Object");
                    xmlToken.InnerXml = strToken;
                    xmlDoc.SelectSingleNode("Objects").AppendChild(xmlToken);
                    xmlToken = xmlDoc.DocumentElement.SelectSingleNode("descendant::Object[Token='[" + cboTokens.SelectedItem.Text + "]']");
                }
                var strValue = cboValue.Visible ? cboValue.SelectedItem.Value : txtValue.Text;

                var blnUpdate = false;
                foreach (XmlNode xmlSetting in xmlToken.SelectNodes(".//Settings/Setting"))
                {
                    if (xmlSetting.SelectSingleNode("Name").InnerText == cboSettings.SelectedItem.Value)
                    {
                        xmlSetting.SelectSingleNode("Value").InnerText = strValue;
                        blnUpdate = true;
                    }
                }
                if (blnUpdate == false)
                {
                    var strSetting = "<Name>" + cboSettings.SelectedItem.Value + "</Name><Value>" + strValue + "</Value>";
                    XmlNode xmlSetting = xmlDoc.CreateElement("Setting");
                    xmlSetting.InnerXml = strSetting;
                    xmlToken.SelectSingleNode("Settings").AppendChild(xmlSetting);
                }
                try
                {
                    if (File.Exists(strFile))
                    {
                        File.SetAttributes(strFile, FileAttributes.Normal);
                    }
                    var objStream = File.CreateText(strFile);
                    var strXML = xmlDoc.InnerXml;
                    strXML = strXML.Replace("><", ">" + Environment.NewLine + "<");
                    objStream.WriteLine(strXML);
                    objStream.Close();
                }
				catch (Exception ex)
				{
					Logger.Error(ex);
				}
            }

        }

        #endregion

    }
}