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
using System.Web;
using System.Web.UI.WebControls;
using System.Xml;

using DotNetNuke.Common;
using DotNetNuke.Common.Lists;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Profile;
using DotNetNuke.Framework;
using DotNetNuke.Instrumentation;
using DotNetNuke.Services.Exceptions;
using DotNetNuke.Services.Localization;
using DotNetNuke.UI.Skins.Controls;
using DotNetNuke.UI.WebControls;

#endregion

namespace DotNetNuke.Modules.Admin.Users
{
    /// -----------------------------------------------------------------------------
    /// <summary>
    /// The EditProfileDefinition PortalModuleBase is used to manage a Profile Property
    /// for a portal
    /// </summary>
    /// <remarks>
    /// </remarks>
    /// <history>
    /// 	[cnurse]	02/22/2006  Created
    /// </history>
    /// -----------------------------------------------------------------------------
    public partial class EditProfileDefinition : PortalModuleBase
    {
    	private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof (EditProfileDefinition));
		#region Private Members

        private string ResourceFile = "~/DesktopModules/Admin/Security/App_LocalResources/Profile.ascx";
        private string _Message = Null.NullString;
        private ProfilePropertyDefinition _PropertyDefinition;

		#endregion

		#region Protected Members

        protected bool IsAddMode
        {
            get
            {
                return (PropertyDefinitionID == Null.NullInteger);
            }
        }

        protected bool IsList
        {
            get
            {
                bool _IsList = false;
                var objListController = new ListController();
                ListEntryInfo dataType = objListController.GetListEntryInfo("DataType", PropertyDefinition.DataType);

                if ((dataType != null) && (dataType.Value == "List"))
                {
                    _IsList = true;
                }
                return _IsList;
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets whether we are dealing with SuperUsers
        /// </summary>
        /// <history>
        /// 	[cnurse]	05/11/2006  Created
        /// </history>
        /// -----------------------------------------------------------------------------
        protected bool IsSuperUser
        {
            get
            {
            	return Globals.IsHostTab(PortalSettings.ActiveTab.TabID);
            }
        }

        protected ProfilePropertyDefinition PropertyDefinition
        {
            get
            {
                if (_PropertyDefinition == null)
                {
                    if (IsAddMode)
                    {
						//Create New Property Definition
                        _PropertyDefinition = new ProfilePropertyDefinition();
                        _PropertyDefinition.PortalId = UsersPortalId;
                    }
                    else
                    {
						//Get Property Definition from Data Store
                        _PropertyDefinition = ProfileController.GetPropertyDefinition(PropertyDefinitionID, UsersPortalId);
                    }
                }
                return _PropertyDefinition;
            }
        }

        protected int PropertyDefinitionID
        {
            get
            {
                int _DefinitionID = Null.NullInteger;
                if (ViewState["PropertyDefinitionID"] != null)
                {
                    _DefinitionID = Int32.Parse(ViewState["PropertyDefinitionID"].ToString());
                }
                return _DefinitionID;
            }
            set
            {
                ViewState["PropertyDefinitionID"] = value;
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets the Portal Id whose Users we are managing
        /// </summary>
        /// <history>
        /// 	[cnurse]	05/11/2006  Created
        /// </history>
        /// -----------------------------------------------------------------------------
        protected int UsersPortalId
        {
            get
            {
                int intPortalId = PortalId;
                if (IsSuperUser)
                {
                    intPortalId = Null.NullInteger;
                }
                return intPortalId;
            }
        }

		#endregion

		#region Private Methods

        private void UpdateResourceFileNode(XmlDocument xmlDoc, string key, string text)
        {
            XmlNode node;
            XmlNode nodeData;
            XmlAttribute attr;
            node = xmlDoc.SelectSingleNode("//root/data[@name='" + key + "']/value");
            if (node == null)
            {
				//missing entry
                nodeData = xmlDoc.CreateElement("data");
                attr = xmlDoc.CreateAttribute("name");
                attr.Value = key;
                nodeData.Attributes.Append(attr);
                xmlDoc.SelectSingleNode("//root").AppendChild(nodeData);
                node = nodeData.AppendChild(xmlDoc.CreateElement("value"));
            }
            node.InnerXml = Server.HtmlEncode(text);
        }

        private void BindLanguages()
        {
            txtPropertyName.Text = Localization.GetString("ProfileProperties_" + PropertyDefinition.PropertyName, ResourceFile, cboLocales.SelectedValue);
            txtPropertyHelp.Text = Localization.GetString("ProfileProperties_" + PropertyDefinition.PropertyName + ".Help", ResourceFile, cboLocales.SelectedValue);
            txtPropertyRequired.Text = Localization.GetString("ProfileProperties_" + PropertyDefinition.PropertyName + ".Required", ResourceFile, cboLocales.SelectedValue);
            txtPropertyValidation.Text = Localization.GetString("ProfileProperties_" + PropertyDefinition.PropertyName + ".Validation", ResourceFile, cboLocales.SelectedValue);
            txtCategoryName.Text = Localization.GetString("ProfileProperties_" + PropertyDefinition.PropertyCategory + ".Header", ResourceFile, cboLocales.SelectedValue);
        }

        private void BindList()
        {
            if (IsList)
            {
                lstEntries.Mode = "ListEntries";
                lstEntries.SelectedKey = PropertyDefinition.PropertyName;
                lstEntries.ListPortalID = PortalController.GetEffectivePortalId(UsersPortalId);
                lstEntries.ShowDelete = false;
                lstEntries.DataBind();
            }
        }

        private string GetResourceFile(string type, string language)
        {
            string resourcefilename = ResourceFile;
            if (language != Localization.SystemLocale)
            {
                resourcefilename = resourcefilename + "." + language;
            }
            if (type == "Portal")
            {
                resourcefilename = resourcefilename + "." + "Portal-" + PortalId;
            }
            else if (type == "Host")
            {
                resourcefilename = resourcefilename + "." + "Host";
            }
            return HttpContext.Current.Server.MapPath(resourcefilename + ".resx");
        }

        private bool ValidateProperty(ProfilePropertyDefinition definition)
        {
            bool isValid = true;

            var objListController = new ListController();
            string strDataType = objListController.GetListEntryInfo("DataType", definition.DataType).Value;

            switch (strDataType)
            {
                case "Text":
                    if (definition.Required && definition.Length == 0)
                    {
                        _Message = "RequiredTextBox";
                        isValid = Null.NullBoolean;
                    }
                    break;
            }
            return isValid;
        }

        private void SaveLocalizedKeys()
        {
            var portalResources = new XmlDocument();
            var defaultResources = new XmlDocument();
            XmlNode parent;
            string filename;
            try
            {
                defaultResources.Load(GetResourceFile("", Localization.SystemLocale));
                if (IsHostMenu)
                {
                    filename = GetResourceFile("Host", cboLocales.SelectedValue);
                }
                else
                {
                    filename = GetResourceFile("Portal", cboLocales.SelectedValue);
                }
                if (File.Exists(filename))
                {
                    portalResources.Load(filename);
                }
                else
                {
                    portalResources.Load(GetResourceFile("", Localization.SystemLocale));
                }
                UpdateResourceFileNode(portalResources, "ProfileProperties_" + PropertyDefinition.PropertyName + ".Text", txtPropertyName.Text);
                UpdateResourceFileNode(portalResources, "ProfileProperties_" + PropertyDefinition.PropertyName + ".Help", txtPropertyHelp.Text);
                UpdateResourceFileNode(portalResources, "ProfileProperties_" + PropertyDefinition.PropertyName + ".Required", txtPropertyRequired.Text);
                UpdateResourceFileNode(portalResources, "ProfileProperties_" + PropertyDefinition.PropertyName + ".Validation", txtPropertyValidation.Text);
                UpdateResourceFileNode(portalResources, "ProfileProperties_" + PropertyDefinition.PropertyCategory + ".Header", txtCategoryName.Text);

                //remove unmodified keys
                foreach (XmlNode node in portalResources.SelectNodes("//root/data"))
                {
                    XmlNode defaultNode = defaultResources.SelectSingleNode("//root/data[@name='" + node.Attributes["name"].Value + "']");
                    if (defaultNode != null && defaultNode.InnerXml == node.InnerXml)
                    {
                        parent = node.ParentNode;
                        parent.RemoveChild(node);
                    }
                }

                //remove duplicate keys
                foreach (XmlNode node in portalResources.SelectNodes("//root/data"))
                {
                    if (portalResources.SelectNodes("//root/data[@name='" + node.Attributes["name"].Value + "']").Count > 1)
                    {
                        parent = node.ParentNode;
                        parent.RemoveChild(node);
                    }
                }
                if (portalResources.SelectNodes("//root/data").Count > 0)
                {
                    //there's something to save
                    portalResources.Save(filename);
                }
                else
                {
                    //nothing to be saved, if file exists delete
                    if (File.Exists(filename))
                    {
                        File.Delete(filename);
                    }
                }
            }
            catch (Exception exc) //Module failed to load
            {
                Logger.Error(exc);
                UI.Skins.Skin.AddModuleMessage(this, Localization.GetString("Save.ErrorMessage", LocalResourceFile), ModuleMessage.ModuleMessageType.YellowWarning);
            }
        }

		#endregion

		#region Public Methods

        public string GetText(string type)
        {
            string text = Null.NullString;
            if (IsAddMode && Wizard.ActiveStepIndex == 0)
            {
                if (type == "Title")
                {
                    text = Localization.GetString(Wizard.ActiveStep.Title + "_Add.Title", LocalResourceFile);
                }
                else if (type == "Help")
                {
                    text = Localization.GetString(Wizard.ActiveStep.Title + "_Add.Help", LocalResourceFile);
                }
            }
            else
            {
                if (type == "Title")
                {
                    text = Localization.GetString(Wizard.ActiveStep.Title + ".Title", LocalResourceFile);
                }
                else if (type == "Help")
                {
                    text = Localization.GetString(Wizard.ActiveStep.Title + ".Help", LocalResourceFile);
                }
            }
            return text;
        }

		#endregion

		#region Event Handlers

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Page_Init runs when the control is initialised
        /// </summary>
        /// <history>
        /// 	[cnurse]	02/22/2006  Created
        /// </history>
        /// -----------------------------------------------------------------------------
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            //Set the List Entries Control Properties
            lstEntries.ID = "ListEntries";

            //Get Property Definition Id from Querystring
            if (PropertyDefinitionID == Null.NullInteger)
            {
                if ((Request.QueryString["PropertyDefinitionId"] != null))
                {
                    PropertyDefinitionID = Int32.Parse(Request.QueryString["PropertyDefinitionId"]);
                }
            }
            if (IsAddMode)
            {
                ModuleConfiguration.ModuleTitle = Localization.GetString("AddProperty", LocalResourceFile);
            }
            else
            {
                ModuleConfiguration.ModuleTitle = Localization.GetString("EditProperty", LocalResourceFile);
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Page_Load runs when the control is loaded
        /// </summary>
        /// <history>
        /// 	[cnurse]	02/22/2006  Created
        /// </history>
        /// -----------------------------------------------------------------------------
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            cboLocales.SelectedIndexChanged += cboLocales_SelectedIndexChanged;
            Wizard.ActiveStepChanged += Wizard_ActiveStepChanged;
            Wizard.CancelButtonClick += Wizard_CancelButtonClick;
            Wizard.FinishButtonClick += Wizard_FinishButtonClick;
            Wizard.NextButtonClick += Wizard_NextButtonClick;

            try
            {
                if (!Page.IsPostBack)
                {
                    //Localization.LoadCultureDropDownList(cboLocales, CultureDropDownTypes.NativeName, ((PageBase) Page).PageCulture.Name);
                    IEnumerable<ListItem> cultureList = Localization.LoadCultureInListItems(CultureDropDownTypes.NativeName, ((PageBase)Page).PageCulture.Name, "", false);
                    //If the drop down list already has items, clear the list
                    if (cboLocales.Items.Count > 0)
                    {
                        cboLocales.Items.Clear();
                    }

                    foreach (var listItem in cultureList)
                    {
                        cboLocales.AddItem(listItem.Text, listItem.Value);
                    }

                    var selectedItem = cboLocales.FindItemByValue(((PageBase)Page).PageCulture.Name);
                    if (selectedItem != null)
                    {
                        selectedItem.Selected = true;
                    }
                    

                    if (cboLocales.SelectedItem != null)
                    {
                        lblLocales.Text = cboLocales.SelectedItem.Text;
                    }

                    cboLocales.Visible = cboLocales.Items.Count != 1;
                    lblLocales.Visible = cboLocales.Items.Count == 1;
                }
				
                //Bind Property Definition to Data Store
                Properties.LocalResourceFile = LocalResourceFile;
                Properties.DataSource = PropertyDefinition;
                Properties.DataBind();

                foreach (FieldEditorControl editor in Properties.Fields)
                {
                    if (editor.DataField == "Required")
                    {
                        editor.Visible = UsersPortalId != Null.NullInteger;
                    }
                }
            }
            catch (Exception exc) //Module failed to load
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }

        protected void cboLocales_SelectedIndexChanged(object sender, EventArgs e)
        {
            BindLanguages();
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Wizard_ActiveStepChanged runs when the Wizard page has been changed
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <history>
        /// 	[cnurse]	01/30/2007	created
        /// </history>
        /// -----------------------------------------------------------------------------
        protected void Wizard_ActiveStepChanged(object sender, EventArgs e)
        {
            switch (Wizard.ActiveStepIndex)
            {
                case 1: //Lists
                    if (!IsList)
                    {
                        Wizard.ActiveStepIndex = 2;
                    }
                    else
                    {
                        BindList();
                    }
                    break;
                case 2:
                    BindLanguages();
                    Wizard.DisplayCancelButton = false;
                    break;
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Wizard_CancelButtonClick runs when the Cancel Button on the Wizard is clicked.
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <history>
        /// 	[cnurse]	01/30/2007	created
        /// </history>
        /// -----------------------------------------------------------------------------
        protected void Wizard_CancelButtonClick(object sender, EventArgs e)
        {
            try
            {
				//Redirect to Definitions page
                Response.Redirect(Globals.NavigateURL(TabId), true);
            }
            catch (Exception exc) //Module failed to load
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Wizard_FinishButtonClick runs when the Finish Button on the Wizard is clicked.
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <history>
        /// 	[cnurse]	01/30/2007	created
        /// </history>
        /// -----------------------------------------------------------------------------
        protected void Wizard_FinishButtonClick(object sender, WizardNavigationEventArgs e)
        {
            try
            {
                if(!Page.IsValid)
                {
                    return;
                }

                SaveLocalizedKeys();
				//Redirect to Definitions page
                Response.Redirect(Globals.NavigateURL(TabId), true);
            }
            catch (Exception exc) //Module failed to load
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Wizard_NextButtonClickruns when the next Button is clicked.  It provides
        ///	a mechanism for cancelling the page change if certain conditions aren't met.
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <history>
        /// 	[cnurse]	01/30/2007	created
        /// </history>
        /// -----------------------------------------------------------------------------
        protected void Wizard_NextButtonClick(object sender, WizardNavigationEventArgs e)
        {
            if(!Page.IsValid)
            {
                return;
            }

            switch (e.CurrentStepIndex)
            {
                case 0: //Property Details
                    try
                    {
						//Check if Property Editor has been updated by user
                        if (Properties.IsDirty && Properties.IsValid)
                        {
							//Declare Definition and "retrieve" it from the Property Editor
                            ProfilePropertyDefinition propertyDefinition;
                            propertyDefinition = (ProfilePropertyDefinition) Properties.DataSource;
                            if (UsersPortalId == Null.NullInteger)
                            {
                                propertyDefinition.Required = false;
                            }
                            if (ValidateProperty(propertyDefinition))
                            {
                                if (PropertyDefinitionID == Null.NullInteger)
                                {
									//Add the Property Definition
                                    PropertyDefinitionID = ProfileController.AddPropertyDefinition(propertyDefinition);
                                    if (PropertyDefinitionID < Null.NullInteger)
                                    {
                                        UI.Skins.Skin.AddModuleMessage(this, Localization.GetString("DuplicateName", LocalResourceFile), ModuleMessage.ModuleMessageType.RedError);
                                        e.Cancel = true;
                                    }
                                }
                                else
                                {
									//Update the Property Definition
                                    ProfileController.UpdatePropertyDefinition(propertyDefinition);
                                }
                            }
                            else
                            {
                                UI.Skins.Skin.AddModuleMessage(this, Localization.GetString(_Message, LocalResourceFile), ModuleMessage.ModuleMessageType.RedError);
                                e.Cancel = true;
                            }
                        }
                    }
                    catch (Exception exc) //Module failed to load
                    {
                        Exceptions.ProcessModuleLoadException(this, exc);
                    }
                    break;
            }
        }
		
		#endregion
    }
}