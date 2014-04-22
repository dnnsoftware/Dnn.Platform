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

using System.Text.RegularExpressions;
using System.Web.Configuration;
using System.Web.Services.Description;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Tabs;
using DotNetNuke.Security.Roles;
using DotNetNuke.Services.Exceptions;
using DotNetNuke.Services.Localization;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;

using DotNetNuke.Entities.Users;
using System.Xml;
using System.IO;
using Telerik.Web.UI;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Services.FileSystem;
using DotNetNuke.Web.UI.WebControls;
using DotNetNuke.UI.WebControls;
using DotNetNuke.Common;

namespace DotNetNuke.Providers.RadEditorProvider
{

	public partial class ProviderConfig : Entities.Modules.PortalModuleBase, Entities.Modules.IActionable
	{

#region Private Members

		private const string htmlEditorNode = "/configuration/dotnetnuke/htmlEditor";
		private const string spellCheckRootNodeIIS6 = "/configuration/system.web";
		private const string spellCheckRootNodeIIS7 = "/configuration/system.webServer";
        private const string radEditorProviderName = "DotNetNuke.RadEditorProvider";

		private XmlDocument _dnnConfig;
		private List<ConfigInfo> _defaultconfig;

#endregion

		protected List<ConfigInfo> DefaultConfig
		{
			get
			{
				if (DotNetNuke.Common.Utilities.DataCache.GetCache("RAD_DEFAULT_CONFIG") != null)
				{
					_defaultconfig = (List<ConfigInfo>)(DotNetNuke.Common.Utilities.DataCache.GetCache("RAD_DEFAULT_CONFIG"));
				}

				if (_defaultconfig == null)
				{
					_defaultconfig = InitializeDefaultConfig();
					DotNetNuke.Common.Utilities.DataCache.SetCache("RAD_DEFAULT_CONFIG", _defaultconfig);
				}

				return _defaultconfig;

			}
		}

		protected XmlDocument DNNConfig
		{
			get
			{
				if (_dnnConfig == null)
				{
					UserInfo currentUser = UserController.Instance.GetCurrentUserInfo();
					if (currentUser != null && currentUser.IsSuperUser)
					{
						_dnnConfig = Config.Load();
					}
				}

				return _dnnConfig;
			}
		}

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            treeTools.NodeClick += treeTools_NodeClick;
            cmdUpdate.Click += OnUpdateClick;
            cmdCancel.Click += OnCancelClick;
            cmdCopy.Click += OnCopyClick;
            cmdDelete.Click += OnDeleteClick;
            cmdCreate.Click += cmdCreate_Click;
            chkPortal.CheckedChanged += chkPortal_CheckedChanged;
            btnEnable.Click += btnEnable_Click;
 
			Framework.AJAX.RegisterScriptManager();
			BindConfigForm();
		}

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            try
            {
				//If Not ViewState("EditorConfigPath") Is Nothing Then
				//    Dim path As String = CType(ViewState("EditorConfigPath"), String)
				//    BindSelectedConfig(path)
				//End If

				this.pnlSelectProvider.Visible = false;

				if (Request.IsAuthenticated)
				{
					if (UserInfo.IsSuperUser)
					{

						this.pnlSelectProvider.Visible = true;
						// No reason to show purpose of module, inconsistent w/ rest of UI (in other modules, parts of core)
						//DotNetNuke.UI.Skins.Skin.AddModuleMessage(Me, Localization.GetString("lblNote", LocalResourceFile), Skins.Controls.ModuleMessage.ModuleMessageType.YellowWarning)

						if (! IsPostBack)
						{
							BindEditorList();
						    BindRoles();
						}

						BindCurrentEditor();

						if (! Page.IsPostBack)
						{
							LoadConfiguration();
							LoadPages();
						}

					}
					else
					{
						UI.Skins.Skin.AddModuleMessage(this, Localization.GetString("lblHostOnly", LocalResourceFile), UI.Skins.Controls.ModuleMessage.ModuleMessageType.RedError);
					}
				}
				else
				{
					UI.Skins.Skin.AddModuleMessage(this, Localization.GetString("lblNotAuthorized", LocalResourceFile), UI.Skins.Controls.ModuleMessage.ModuleMessageType.RedError);
				}

			}
			catch (Exception exc)
			{
				Exceptions.ProcessModuleLoadException(this, exc);
			}
		}

		protected void treeTools_NodeClick(object sender, RadTreeNodeEventArgs e)
		{
			BindFile();
		}

		protected void OnUpdateClick(object sender, EventArgs e)
		{
			string orgConfigPath = this.treeTools.SelectedNode.Value;
			string orgToolsPath = orgConfigPath.ToLower().Replace("config", "tools");

			UpdateConfig(orgConfigPath);

			StreamWriter tw = File.CreateText(orgToolsPath);
			tw.Write(txtTools.Text);
			tw.Close();
			tw.Dispose();

			UI.Skins.Skin.AddModuleMessage(this, "The update was successful.", UI.Skins.Controls.ModuleMessage.ModuleMessageType.GreenSuccess);
			//BindFile()
		}

		protected void OnCancelClick(object sender, EventArgs e)
		{
			pnlEditor.Visible = false;
			pnlForm.Visible = false;
			//ulActions.Visible = False

			LoadConfiguration();
			LoadPages();

			UI.Skins.Skin.AddModuleMessage(this, "All unsaved changes have been discarded.", UI.Skins.Controls.ModuleMessage.ModuleMessageType.BlueInfo);
		}

		protected void OnCopyClick(object sender, EventArgs e)
		{
			pnlEditor.Visible = false;
			pnlForm.Visible = true;
			this.cmdCreate.Visible = true;
			rblMode.SelectedIndex = 0;

		    if (treeTools.SelectedNode != null)
		    {
		        var role = RoleController.Instance.GetRoleByName(PortalId, treeTools.SelectedNode.Text);
		        if (role != null)
		        {
		            rblMode.SelectedValue = role.RoleID.ToString();
		        }
		    }
		}

		protected void OnDeleteClick(object sender, System.EventArgs e)
		{
			string orgConfigPath = this.treeTools.SelectedNode.Value;
			string orgToolsPath = orgConfigPath.ToLower().Replace("config", "tools");

			if (! (orgConfigPath.ToLower().EndsWith("configfile.xml")))
			{
				System.IO.File.Delete(orgConfigPath);
				System.IO.File.Delete(orgToolsPath);
			}

			pnlEditor.Visible = false;
			pnlForm.Visible = false;
			LoadConfiguration();
			LoadPages();
		}

		private void cmdCreate_Click(object sender, System.EventArgs e)
		{
			string orgConfigPath = this.treeTools.SelectedNode.Value;
			string orgToolsPath = orgConfigPath.ToLower().Replace("config", "tools");

			string newConfigPath = Server.MapPath(this.TemplateSourceDirectory) + "\\ConfigFile\\ConfigFile";
			string newToolsPath = Server.MapPath(this.TemplateSourceDirectory) + "\\ToolsFile\\ToolsFile";

			if ( !string.IsNullOrEmpty(rblMode.SelectedValue) && rblMode.SelectedValue != Globals.glbRoleAllUsers)
			{
				newConfigPath += ".RoleId." + rblMode.SelectedValue;
				newToolsPath += ".RoleId." + rblMode.SelectedValue;
			}

			if (chkPortal.Checked)
			{
				newConfigPath += ".PortalId." + PortalSettings.PortalId.ToString();
				newToolsPath += ".PortalId." + PortalSettings.PortalId.ToString();
			}
			else
			{
				if (treePages.SelectedNode != null)
				{
					newConfigPath += ".TabId." + treePages.SelectedNode.Value;
					newToolsPath += ".TabId." + treePages.SelectedNode.Value;
				}
			}

			newConfigPath += ".xml";
			newToolsPath += ".xml";

			if (! (System.IO.File.Exists(newConfigPath)))
			{
				System.IO.File.Copy(orgConfigPath, newConfigPath, true);
			}

			if (! (System.IO.File.Exists(newToolsPath)))
			{
				System.IO.File.Copy(orgToolsPath, newToolsPath, true);
			}

			//reload tree    
			LoadConfiguration();

			//select new config
			this.treeTools.FindNodeByValue(newConfigPath).Selected = true;

			//re-bind new config
			BindFile();
		}

		protected void chkPortal_CheckedChanged(object sender, System.EventArgs e)
		{
			divTabs.Visible = (chkPortal.Checked == false);
		}

		protected void btnEnable_Click(object sender, EventArgs e)
		{
			switch (editorList.SelectedValue)
			{
				case radEditorProviderName:
					EnableRadEditor();
					break;
				default:
					EnableOtherEditor(editorList.SelectedValue);
					break;
			}

            MessagePanel.Visible = GetSelectedEditor() != radEditorProviderName;
		}

#region Private Methods

		private void EnableOtherEditor(string name)
		{
			XmlDocument xmlConfig = DNNConfig;

			if (xmlConfig != null && xmlConfig.DocumentElement != null)
			{

				XmlNode editorProviderNode = xmlConfig.DocumentElement.SelectSingleNode(htmlEditorNode);
				editorProviderNode.Attributes["defaultProvider"].Value = name;
				editorState.Text = name;

				XmlNode radNode = editorProviderNode.SelectSingleNode("providers/add[@name='" + radEditorProviderName + "']");
				if (radNode != null)
				{
					radNode.ParentNode.RemoveChild(radNode);
				}

				Config.Save(xmlConfig);

				BindCurrentEditor();

			}
		}

		private void EnableRadEditor()
		{
			XmlDocument xmlConfig = DNNConfig;

			if (xmlConfig != null && xmlConfig.DocumentElement != null)
			{
				XmlNode editorProviderNode = xmlConfig.DocumentElement.SelectSingleNode(htmlEditorNode);
				editorProviderNode.Attributes["defaultProvider"].Value = radEditorProviderName;
				//check if already added and if not, add definition
				XmlNode radNode = editorProviderNode.SelectSingleNode("providers/add[@name='" + radEditorProviderName + "']");
				if (radNode == null)
				{
					radNode = xmlConfig.CreateElement("add");
					XmlAttribute xmlAttr = xmlConfig.CreateAttribute("name");
					xmlAttr.Value = radEditorProviderName;
					radNode.Attributes.Append(xmlAttr);
					xmlAttr = xmlConfig.CreateAttribute("type");
					xmlAttr.Value = "DotNetNuke.Providers.RadEditorProvider.EditorProvider, DotNetNuke.RadEditorProvider";
					radNode.Attributes.Append(xmlAttr);
					xmlAttr = xmlConfig.CreateAttribute("providerPath");
					xmlAttr.Value = "~/DesktopModules/Admin/RadEditorProvider";
					radNode.Attributes.Append(xmlAttr);
					editorProviderNode.SelectSingleNode("providers").AppendChild(radNode);
				}

				Config.Save(xmlConfig);

				BindCurrentEditor();

			}
		}

		private void DisableRadEditor()
		{
			XmlDocument xmlConfig = DNNConfig;

			if (xmlConfig != null && xmlConfig.DocumentElement != null)
			{
				XmlNode editorProviderNode = xmlConfig.DocumentElement.SelectSingleNode(htmlEditorNode);
				//check if already added and if not, add definition
				XmlNode radNode = editorProviderNode.SelectSingleNode("providers/add[@name='" + radEditorProviderName + "']");
				if (radNode != null)
				{
					radNode.ParentNode.RemoveChild(radNode);
				}

				Config.Save(xmlConfig);

				BindCurrentEditor();
			}
		}

		private void BindCurrentEditor()
		{
			string editorType = GetSelectedEditor();
			editorState.Text = editorType;

			//Me.pnlTabContent.Visible = (editorType = radEditorProviderName)

			//If editorType = radEditorProviderName Then
			//    pnlTabContent.Visible = True
			//End If
		}

		private void BindEditorList()
		{
			editorList.DataSource = GetEditorsList();
			editorList.DataBind();

			string current = GetSelectedEditor();
			var item = editorList.FindItemByText(current);
			if (item != null)
			{
				editorList.SelectedIndex = -1;
				item.Selected = true;
			}

            MessagePanel.Visible = GetSelectedEditor() != radEditorProviderName;
		}

	    private void BindRoles()
	    {
            var roles = RoleController.Instance.GetRoles(PortalId, 
                                                            r => r.SecurityMode != SecurityMode.SocialGroup 
                                                                && r.Status == RoleStatus.Approved);

            roles.Insert(0, new RoleInfo { 
                                        RoleID = int.Parse(Globals.glbRoleAllUsers), 
                                        RoleName = Globals.glbRoleAllUsersName 
                                   });
            roles.Insert(1, new RoleInfo
            {
                RoleID = int.Parse(Globals.glbRoleSuperUser),
                RoleName = Globals.glbRoleSuperUserName
            });

            rblMode.DataSource = roles;
	        rblMode.DataTextField = "RoleName";
	        rblMode.DataValueField = "RoleId";
            rblMode.DataBind();
	    }

		private List<string> GetEditorsList()
		{
			List<string> editors = new List<string>();
			XmlDocument xmlConfig = DNNConfig;
			if (xmlConfig != null && xmlConfig.DocumentElement != null)
			{
				XmlNodeList editorNodes = xmlConfig.DocumentElement.SelectNodes(htmlEditorNode + "/providers/add");
				if (editorNodes != null)
				{
					int i = 0;
					while (i < editorNodes.Count)
					{
						XmlNode node = editorNodes[i];
						if (node.Attributes["name"] != null)
						{
							editors.Add(node.Attributes["name"].Value);
						}
						i = i + 1;
					}
				}
			}

			if (! (editors.Contains(radEditorProviderName)))
			{
				editors.Add(radEditorProviderName);
			}

			return editors;
		}

		private string GetSelectedEditor()
		{
			XmlDocument xmlConfig = DNNConfig;

			if (xmlConfig != null && xmlConfig.DocumentElement != null)
			{
				XmlNode editorProviderNode = xmlConfig.DocumentElement.SelectSingleNode(htmlEditorNode);
				return editorProviderNode.Attributes["defaultProvider"].Value;
			}

			return "";
		}

		private void UpdateConfig(string strPath)
		{
			XmlDocument xmlConfig = new XmlDocument();
			xmlConfig.Load(strPath);

			XmlNode rootNode = xmlConfig.DocumentElement.SelectSingleNode("/configuration");
			string setting = Null.NullString;
			List<ConfigInfo> currentConfig = DefaultConfig;
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
							CheckBoxList ctl = (CheckBoxList)(FindControlRecursive(plhConfig, "ctl_rc_" + objConfig.Key));

							if (ctl != null)
							{
								try
								{
									string strSetting = "";
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
							RadioButtonList ctl = (RadioButtonList)(FindControlRecursive(plhConfig, "ctl_rc_" + objConfig.Key));

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
							CheckBoxList ctl = (CheckBoxList)(FindControlRecursive(plhConfig, "ctl_rc_" + objConfig.Key));

							if (ctl != null)
							{
								try
								{
									string strSetting = "";
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
							DnnComboBox ctl = (DnnComboBox)(FindControlRecursive(plhConfig, "ctl_rc_" + objConfig.Key));

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
							DnnComboBox ctl = (DnnComboBox)(FindControlRecursive(plhConfig, "ctl_rc_" + objConfig.Key));

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
							TextBox ctl = (TextBox)(FindControlRecursive(plhConfig, "ctl_rc_" + objConfig.Key));

							if (ctl != null)
							{
								try
								{
									setting = ctl.Text.Replace(".00", "").Replace(".0", "");

                                    if(objConfig.Key.ToLowerInvariant().EndsWith("size"))
                                    {
                                        var allowSize = Convert.ToInt32(ctl.Text);
                                        if(allowSize > maxFileSize)
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
							var ctl = (DnnComboBox) (FindControlRecursive(plhConfig, "ctl_rc_" + objConfig.Key));

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
							CheckBox ctl = (CheckBox)(FindControlRecursive(plhConfig, "ctl_rc_" + objConfig.Key));

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
							TextBox ctl = (TextBox)(FindControlRecursive(plhConfig, "ctl_rc_" + objConfig.Key));

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

					//look for setting node
					XmlNode configNode = rootNode.SelectSingleNode("property[@name='" + objConfig.Key + "']");
					if (configNode != null)
					{
						//node found, remove it
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

					setting = "";

				}
			}

			xmlConfig.Save(strPath);

            //update web.config to allow the max file size in http runtime section.
		    var configAllowSize = Config.GetMaxUploadSize();
            if(maxFileSize > configAllowSize)
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
								string strSetting = "";
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

						key = "";
						setting = "";
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
										ctl.Text = Convert.ToInt32(objConfig.Value.Replace("px", "")).ToString();
									}
									catch
									{
									}
								}
								break;
							}
							case "linkstype":
							{
								var ctl = (DnnComboBox) (FindControlRecursive(plhConfig, "ctl_rc_" + objConfig.Key));

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
								TextBox ctl = (TextBox)(FindControlRecursive(plhConfig, "ctl_rc_" + objConfig.Key));

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

		private List<ConfigInfo> InitializeDefaultConfig()
		{
			string strPath = Server.MapPath(this.TemplateSourceDirectory + "/ConfigFile/configfile.xml.original.xml");

			List<ConfigInfo> config = new List<ConfigInfo>();

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
							string strSetting = "";
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
						config.Add(new ConfigInfo(key, "", true));
					}
					else
					{
						config.Add(new ConfigInfo(key, setting, false));
					}

					key = "";
					setting = "";
				}

			}

			return config;
		}

		private void BindConfigForm()
		{
			ArrayList foldersToShow = new ArrayList();

			ArrayList folders = new ArrayList();
			folders = DotNetNuke.Common.Utilities.FileSystemUtils.GetFoldersByUser(PortalSettings.PortalId, true, true, "READ");

			plhConfig.Controls.Clear();

			Panel pnlContent = new Panel();
			pnlContent.CssClass = "pcContent";

			HtmlGenericControl fsContent = null;
			int i = 0;

			foreach (ConfigInfo objConfig in DefaultConfig)
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
                    if(string.IsNullOrEmpty(localizedTitle))
                    {
                        localizedTitle = key;
                    }

                    pnlContent.Controls.Add(new LiteralControl("<h2 id='Panel-ProviderConfig-" + i.ToString() + "' class='dnnFormSectionHead'><a class='dnnSectionExpanded' href=\"\">" + localizedTitle + "</a></h2>"));
					fsContent = new HtmlGenericControl("fieldset");
				}
				else
				{
					Panel pnlRow = new Panel(); // a row starts here and ends at the right before next, where it is added to the fieldset)
					pnlRow.CssClass = "dnnFormItem";
					pnlRow.Controls.Add(BuildLabel(key));

					switch (key.ToLower())
					{
						case "stripformattingoptions":
						{
							CheckBoxList ctl = new CheckBoxList();
							ctl.ID = "ctl_rc_" + key;
							ctl.RepeatColumns = 2;
							ctl.CssClass = "dnnCBItem";

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
							RadioButtonList ctl = new RadioButtonList();
							ctl.ID = "ctl_rc_" + key;
							ctl.RepeatColumns = 2;
							ctl.CssClass = "dnnFormRadioButtons";

							foreach (string objEnum in Enum.GetNames(typeof(Telerik.Web.UI.EditorToolbarMode)))
							{
								ctl.Items.Add(new ListItem(objEnum, objEnum));
							}

							pnlRow.Controls.Add(ctl);
							break;
						}
						case "editmodes":
						{
							CheckBoxList ctl = new CheckBoxList();
							ctl.ID = "ctl_rc_" + key;
							ctl.RepeatColumns = 1;
							ctl.CssClass = "dnnCBItem";

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
							CheckBoxList ctl = new CheckBoxList();
							ctl.ID = "ctl_rc_" + key;
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
							DnnComboBox ctl = new DnnComboBox();
							ctl.ID = "ctl_rc_" + key;
							//ctl.Width = Unit.Pixel(253)
							ctl.Items.Clear();

							foreach (FolderInfo oFolder in folders)
							{
								if (! (oFolder.FolderPath.ToLower().StartsWith("cache")))
								{
									if (oFolder.FolderPath == "")
									{
										ctl.AddItem(Localization.GetString("PortalRoot", LocalResourceFile), "/");

										ctl.AddItem(Localization.GetString("UserFolder", LocalResourceFile), "[UserFolder]");
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
							DnnComboBox ctl = new DnnComboBox();
							ctl.ID = "ctl_rc_" + key;
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
							DnnComboBox ctl = new DnnComboBox();
							ctl.ID = "ctl_rc_" + key;
							ctl.AddItem(LocalizeString("LinksType_Normal"), "Normal");
							ctl.AddItem(LocalizeString("LinksType_UseTabName"), "UseTabName");
							ctl.AddItem(LocalizeString("LinksType_UseTabId"), "UseTabId");

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
							CheckBox ctl = new CheckBox();
							ctl.ID = "ctl_rc_" + key;
							ctl.CssClass = "dnnCBItem";

							pnlRow.Controls.Add(ctl);
							break;
						}
						case "borderwidth":
						case "height":
						case "width":
						case "toolswidth":
						{
						    TextBox ctl = new TextBox();
						    ctl.Text = "5";
						    ctl.CssClass = "SpinnerStepOne";
                            ctl.ID = "ctl_rc_" + key;
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
						    TextBox ctl = new TextBox();
						    ctl.Text = "1024";
						    ctl.CssClass = "SpinnerStep1024";
                            ctl.ID = "ctl_rc_" + key;
                            pnlRow.Controls.Add(ctl);
							break;
						}
						case "contentareamode":
						{
							DnnComboBox ctl = new DnnComboBox();
							ctl.ID = "ctl_rc_" + key;

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
					        var ctl = new DnnLanguageComboBox();
					        ctl.ID = "ctl_rc_" + key;
                            ctl.LanguagesListType = LanguagesListType.All;
					        ctl.IncludeNoneSpecified = true;
					        ctl.CssClass = "languageComboBox";
                            pnlRow.Controls.Add(ctl);
					        break;
					    }
					    default:
						{
							TextBox ctl = new TextBox();
							ctl.ID = "ctl_rc_" + key;
							ctl.Text = value;

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
		/// <returns></returns>
		/// <remarks></remarks>
		private PropertyLabelControl BuildLabel(string resourceKey)
		{
			var propLabel = new PropertyLabelControl();
			propLabel.ID = resourceKey + "_Label";
			propLabel.ShowHelp = true;
			propLabel.ResourceKey = resourceKey;

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
				BindSelectedConfig(configpath);
				ViewState["EditorConfigPath"] = configpath;

				cmdUpdate.Enabled = (! (configpath.ToLower().EndsWith("configfile.xml.original.xml")));
				cmdCreate.Enabled = true;
				cmdDelete.Enabled = (! (configpath.ToLower().EndsWith("configfile.xml.original.xml")) && ! (configpath.ToLower().EndsWith("configfile.xml")));

				if (File.Exists(toolspath))
				{
					StreamReader tr = new StreamReader(toolspath);
					this.txtTools.Text = tr.ReadToEnd();
					tr.Close();
					tr.Dispose();
				}
				else
				{
					//load default toolsfile
					string orgPath = Server.MapPath(this.TemplateSourceDirectory + "/ToolsFile/ToolsFile.xml.Original.xml");
					if (File.Exists(orgPath))
					{
						File.Copy(orgPath, toolspath);
					}

					if (File.Exists(toolspath))
					{
						StreamReader tr = new StreamReader(toolspath);
						this.txtTools.Text = tr.ReadToEnd();
						tr.Close();
						tr.Dispose();
					}
					else
					{
						this.txtTools.Text = "Could not load tools file...";
					}
				}
			}
		}

		protected Control FindControlRecursive(Control objRoot, string id)
		{
			if (objRoot.ID == id)
			{
				return objRoot;
			}
			foreach (Control c in objRoot.Controls)
			{
				Control t = FindControlRecursive(c, id);
				if (t != null)
				{
					return t;
				}
			}
			return null;
		}

		private void LoadPages()
		{
			treePages.Nodes.Clear();

            var tabs = TabController.Instance.GetTabsByPortal(PortalSettings.PortalId);
			foreach (var oTab in tabs.Values)
			{
				if (oTab.Level == 0)
				{
					var node = new RadTreeNode {Text = oTab.TabName, Value = oTab.TabID.ToString()};
				    treePages.Nodes.Add(node);
					AddChildren(ref node);
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
					RadTreeNode node = new RadTreeNode();
					node.Text = objTab.TabName;
					node.Value = objTab.TabID.ToString();
					treenode.Nodes.Add(node);
					AddChildren(ref node);
				}
			}
		}

		private void LoadConfiguration()
		{
			this.treeTools.Nodes.Clear();

			pnlEditor.Visible = false;
			pnlForm.Visible = false;

			var rootnode = new RadTreeNode("Default Configuration");
			rootnode.Expanded = true;

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

					string nodename = file.Substring(file.LastIndexOf("\\") + 1).Replace(".xml", "").ToLowerInvariant();
					if (nodename.StartsWith("configfile") && file.EndsWith(".xml"))
					{

						string nodeTitle = "Everyone";

						string strTargetGroup = nodename.Replace("configfile.", "");
						string strTargetTab = "";

						if (strTargetGroup.Length > 0)
						{
						    var roleMatch = Regex.Match(strTargetGroup, "^RoleId\\.([-\\d]+)", RegexOptions.IgnoreCase);
							if (roleMatch.Success)
							{
							    var roleId = roleMatch.Groups[1].Value;
                                rblMode.SelectedValue = roleId;
							    strTargetTab = strTargetGroup.Replace(roleMatch.Value + ".", string.Empty);
							    var role = RoleController.Instance.GetRoleById(PortalId, Convert.ToInt32(roleId));
							    if (role != null)
							    {
							        nodeTitle = role.RoleName;
							    }
							    else
							    {
							        blnAddNode = false; //do not show the node if the role is not in current portal, or the role is not valid any more(such as deleted).
							    }
							}
						}

						if (strTargetTab.Length > 0)
						{
							if (SimulateIsNumeric.IsNumeric(strTargetTab.ToLower().Replace("tabid.", "")))
							{
								try
								{
                                    TabInfo t = TabController.Instance.GetTab(Convert.ToInt32(strTargetTab.ToLower().Replace("tabid.", "")), PortalSettings.PortalId, false);
									if (t != null)
									{
										if (t.PortalID != PortalSettings.PortalId)
										{
											//fix for codeplex issue #187
											blnAddNode = false;
										}
										nodeTitle += " (Page \"" + t.TabName + "\" only)";
									}
								}
								catch
								{
								}
							}
							if (SimulateIsNumeric.IsNumeric(strTargetTab.ToLower().Replace("portalid.", "")))
							{
								try
								{
                                    PortalInfo p = PortalController.Instance.GetPortal(Convert.ToInt32(strTargetTab.ToLower().Replace("portalid.", "")));
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
		/// <history>
		/// </history>
		/// -----------------------------------------------------------------------------
		public Entities.Modules.Actions.ModuleActionCollection ModuleActions
		{
			get
			{
				Entities.Modules.Actions.ModuleActionCollection Actions = new Entities.Modules.Actions.ModuleActionCollection();
				//Actions.Add(GetNextActionID, Localization.GetString(Entities.Modules.Actions.ModuleActionType.AddContent, LocalResourceFile), Entities.Modules.Actions.ModuleActionType.AddContent, "", "", EditUrl(), False, Security.SecurityAccessLevel.Edit, True, False)
				return Actions;
			}
		}

#endregion

	}

}