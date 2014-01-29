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
using System.Collections.Generic;
using System.Linq;
using System.Web.UI.WebControls;

using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Content.Common;
using DotNetNuke.Entities.Content.Taxonomy;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Modules.Definitions;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Security.Permissions;
using DotNetNuke.Services.EventQueue;
using DotNetNuke.Services.Installer.Packages;
using DotNetNuke.Services.Localization;
using DotNetNuke.UI.Utilities;
using DotNetNuke.UI.WebControls;
using DotNetNuke.Web.UI.WebControls;

using DataCache = DotNetNuke.Common.Utilities.DataCache;
using Globals = DotNetNuke.Common.Globals;

#endregion

namespace DotNetNuke.Modules.Admin.Extensions
{
    /// -----------------------------------------------------------------------------
    /// <summary>
    /// The ModuleEditor ModuleUserControlBase is used to edit Module Definitions
    /// </summary>
    /// <history>
    /// 	[cnurse]	02/04/2008  created
    /// </history>
    /// -----------------------------------------------------------------------------
    public partial class ModuleEditor : PackageEditorBase
    {
		#region "Private Members"

        private DesktopModuleInfo _DesktopModule;
        private ModuleDefinitionInfo _ModuleDefinition;

		#endregion

		#region "Protected Properties"

        protected DesktopModuleInfo DesktopModule
        {
            get
            {
                return _DesktopModule ?? (_DesktopModule = DesktopModuleController.GetDesktopModuleByPackageID(PackageID));
            }
        }

        protected override string EditorID
        {
            get
            {
                return "ModuleEditor";
            }
        }

        protected bool IsAddDefinitionMode
        {
            get
            {
                bool _IsAddDefinitionMode = Null.NullBoolean;
                if (ViewState["IsAddDefinitionMode"] != null)
                {
                    _IsAddDefinitionMode = Convert.ToBoolean(ViewState["IsAddDefinitionMode"]);
                }
                return _IsAddDefinitionMode;
            }
            set
            {
                ViewState["IsAddDefinitionMode"] = value;
            }
        }

        protected int ModuleDefinitionID
        {
            get
            {
                int _ModuleDefinitionID = Null.NullInteger;
                if (ViewState["ModuleDefinitionID"] != null)
                {
                    _ModuleDefinitionID = Convert.ToInt32(ViewState["ModuleDefinitionID"]);
                }
                return _ModuleDefinitionID;
            }
            set
            {
                ViewState["ModuleDefinitionID"] = value;
            }
        }

        protected ModuleDefinitionInfo ModuleDefinition
        {
            get
            {
                return _ModuleDefinition ?? (_ModuleDefinition = ModuleDefinitionController.GetModuleDefinitionByID(ModuleDefinitionID));
            }
        }
		
		#endregion

		#region "Private Methods"


        private void BindDefinition()
        {
            if (IsAddDefinitionMode)
            {
                var definition = new ModuleDefinitionInfo {DesktopModuleID = DesktopModule.DesktopModuleID, ModuleDefID = Null.NullInteger};
                definitionsEditor.DataSource = definition;
                definitionsEditor.DataBind();

                cmdDeleteDefinition.Visible = false;
                cmdUpdateDefinition.Text = Localization.GetString("cmdCreateDefinition", LocalResourceFile);
                pnlDefinition.Visible = true;
                pnlControls.Visible = false;
            	definitionSelectRow.Visible = false;

                definitionName.Visible = true;
                definitionNameLiteral.Visible = false;
            }
            else
            {
                if (ModuleDefinition != null && ModuleDefinition.DesktopModuleID == DesktopModule.DesktopModuleID)
                {
                    definitionsEditor.DataSource = ModuleDefinition;
                    definitionsEditor.DataBind();

                    cmdDeleteDefinition.Visible = true;
                    cmdUpdateDefinition.Text = Localization.GetString("cmdUpdateDefinition", LocalResourceFile);

                    if (!Page.IsPostBack)
                    {
                        Localization.LocalizeDataGrid(ref grdControls, LocalResourceFile);
                    }
                    grdControls.DataSource = ModuleDefinition.ModuleControls.Values;
                    grdControls.DataBind();

                    pnlDefinition.Visible = true;
                    pnlControls.Visible = true;
                	definitionSelectRow.Visible = true;

                    definitionName.Visible = false;
                    definitionNameLiteral.Visible = true;

                    cmdAddControl.NavigateUrl = ModuleContext.EditUrl("ModuleControlID", "-1", "EditControl", "packageId=" + PackageID, "moduledefid=" + ModuleDefinition.ModuleDefID);
                }
                else
                {
                    pnlDefinition.Visible = false;
                }
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// This routine Binds the DesktopModule
        /// </summary>
        /// <history>
        /// 	[cnurse]	02/04/2008  created
        /// </history>
        /// -----------------------------------------------------------------------------
        private void BindDesktopModule(bool refreshDefinitions)
        {
            if (DesktopModule != null)
            {
                ITermController termController = Util.GetTermController();
                category.ListSource = termController.GetTermsByVocabulary("Module_Categories").OrderBy(t => t.Weight).ToList();

                Shareable.ListSource =
                    Enum.GetNames(typeof (ModuleSharing)).Select(
                        x => new {Name = x}).ToList();

                desktopModuleForm.DataSource = DesktopModule;
                desktopModuleForm.DataBind();

                IsPremiumm.Visible = !DesktopModule.IsAdmin;

                if (!Page.IsPostBack)
                {
                    if ((Request.QueryString["ModuleDefinitionID"] != null))
                    {
                        ModuleDefinitionID = Int32.Parse(Request.QueryString["ModuleDefinitionID"]);
                    }
                }
                if (!Page.IsPostBack || refreshDefinitions)
                {
                    cboDefinitions.DataSource = DesktopModule.ModuleDefinitions.Values;
                    cboDefinitions.DataBind();

                    if (ModuleDefinitionID == Null.NullInteger && cboDefinitions.Items.Count > 0)
                    {
                        ModuleDefinitionID = int.Parse(cboDefinitions.SelectedValue);
                    }
                    if (ModuleDefinitionID != Null.NullInteger)
                    {
						//Set the Combos selected value
                        //ListItem selectedDefinition = cboDefinitions.Items.FindByValue(ModuleDefinitionID.ToString());
                        //if (selectedDefinition != null)
                        //{
                        //    cboDefinitions.SelectedIndex = -1;
                        //    selectedDefinition.Selected = true;
                        //}

                        var selectedDefinition = cboDefinitions.FindItemByValue(ModuleDefinitionID.ToString());
                        if (selectedDefinition != null)
                        {
                            selectedDefinition.Selected = true;
                        }
                    }
                }
                if (!IsSuperTab)
                {
                    BindPermissions();
                }
                else
                {
                    pnlPermissions.Visible = false;
                }
                BindPortalDesktopModules();

                BindDefinition();

                lblDefinitionError.Visible = false;
            }
        }

        private void BindPermissions()
        {
            PortalDesktopModuleInfo portalModule = DesktopModuleController.GetPortalDesktopModule(ModuleContext.PortalSettings.PortalId, DesktopModule.DesktopModuleID);
            if (portalModule != null)
            {
                dgPermissions.PortalDesktopModuleID = portalModule.PortalDesktopModuleID;
                bool isVisible =
                    DesktopModulePermissionController.HasDesktopModulePermission(DesktopModulePermissionController.GetDesktopModulePermissions(portalModule.PortalDesktopModuleID), "DEPLOY") ||
                    ModuleContext.PortalSettings.UserInfo.IsInRole(ModuleContext.PortalSettings.AdministratorRoleName) || ModuleContext.PortalSettings.UserInfo.IsSuperUser;
                pnlPermissions.Visible = isVisible;
                if (!isVisible)
                {
                    lblHelp.Text = Localization.GetString("NoPermission", LocalResourceFile);
                }
            }
        }

        private void BindPortalDesktopModules()
        {
			if (!IsWizard && !DesktopModule.IsAdmin)
			{
				var objPortals = new PortalController();
				ArrayList arrPortals = objPortals.GetPortals();
				Dictionary<int, PortalDesktopModuleInfo> dicPortalDesktopModules = DesktopModuleController.GetPortalDesktopModulesByDesktopModuleID(DesktopModule.DesktopModuleID);
				foreach (PortalDesktopModuleInfo objPortalDesktopModule in dicPortalDesktopModules.Values)
				{
					foreach (PortalInfo objPortal in arrPortals)
					{
						if (objPortal.PortalID == objPortalDesktopModule.PortalID)
						{
							arrPortals.Remove(objPortal);
							break;
						}
					}
				}

				ctlPortals.AvailableDataSource = arrPortals;
				ctlPortals.SelectedDataSource = dicPortalDesktopModules.Values;
			}
        }

        private void UpdateModuleInterfaces(string BusinessControllerClass)
        {
			//this cannot be done directly at this time because 
            //the module may not be loaded into the app domain yet
            //So send an EventMessage that will process the update 
            //after the App recycles
            var oAppStartMessage = new EventMessage
                                       {
                                           Sender = ModuleContext.PortalSettings.UserInfo.Username,
                                           Priority = MessagePriority.High,
                                           ExpirationDate = DateTime.Now.AddYears(-1),
                                           SentDate = DateTime.Now,
                                           Body = "",
                                           ProcessorType = "DotNetNuke.Entities.Modules.EventMessageProcessor, DotNetNuke",
                                           ProcessorCommand = "UpdateSupportedFeatures"
                                       };
									   
            //Add custom Attributes for this message
            oAppStartMessage.Attributes.Add("BusinessControllerClass", BusinessControllerClass);
            oAppStartMessage.Attributes.Add("DesktopModuleId", DesktopModule.DesktopModuleID.ToString());

            //send it to occur on next App_Start Event
            EventQueueController.SendMessage(oAppStartMessage, "Application_Start");
			
			//force an app restart
            Config.Touch();
        }

		#endregion

		#region "Protected Methods"

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            cmdAddDefinition.Click += cmdAddDefinition_Click;
            cmdDeleteDefinition.Click += cmdDeleteDefinition_Click;
            cmdUpdate.Click += cmdUpdate_Click;
            cmdUpdateDefinition.Click += cmdUpdateDefinition_Click;
            grdControls.DeleteCommand += grdControls_DeleteCommand;
            grdControls.ItemDataBound += grdControls_ItemDataBound;

            cboDefinitions.SelectedIndexChanged += cboDefinitions_SelectedIndexChanged;


            lblHelp.Text = Localization.GetString(IsSuperTab ? "HostHelp" : "AdminHelp", LocalResourceFile);
            foreach (DataGridColumn column in grdControls.Columns)
            {
                if (ReferenceEquals(column.GetType(), typeof (ImageCommandColumn)))
                {
					//Manage Delete Confirm JS
                    var imageColumn = (ImageCommandColumn) column;
                    if (imageColumn.CommandName == "Delete")
                    {
                        imageColumn.OnClickJS = Localization.GetString("DeleteItem");
                    }
                }
            }
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            ctlPortals.LocalResourceFile = LocalResourceFile;
            if (!IsWizard)
            {
                ctlPortals.AddButtonClick += ctlPortals_AddButtonClick;
                ctlPortals.AddAllButtonClick += ctlPortals_AddAllButtonClick;
                ctlPortals.RemoveAllButtonClick += ctlPortals_RemoveAllButtonClick;
                ctlPortals.RemoveButtonClick += ctlPortals_RemoveButtonClick;
            }

            ClientAPI.AddButtonConfirm(cmdDeleteDefinition, Localization.GetString("DeleteItem"));
        }

        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);
            ctlPortals.LocalResourceFile = LocalResourceFile;
            helpPanel.Visible = !IsWizard;
            pnlDefinitions.Visible = (!IsWizard) && IsSuperTab;
            cmdUpdate.Visible = (!IsWizard) && (!IsSuperTab && pnlPermissions.Visible);
			PremiumModules.Visible = !IsWizard && !DesktopModule.IsAdmin;
        }

		#endregion

		#region "Public Methods"

        public override void Initialize()
        {
            desktopModuleForm.Visible = IsSuperTab;
            moduleSettingsHead.Visible = !IsWizard;
            BindDesktopModule(false);
        }

        public override void UpdatePackage()
        {
            bool bUpdateSupportedFeatures = Null.NullBoolean;
            PackageInfo _Package = PackageController.Instance.GetExtensionPackage(Null.NullInteger, p => p.PackageID == PackageID);

            //Update module settings
            if (desktopModuleForm.IsValid)
            {
                var desktopModule = desktopModuleForm.DataSource as DesktopModuleInfo;
                if (desktopModule != null && _Package != null)
                {
                    desktopModule.Shareable = (ModuleSharing) Enum.Parse(typeof (ModuleSharing), Shareable.ComboBox.SelectedValue.ToString());
                    desktopModule.FriendlyName = _Package.FriendlyName;
                    desktopModule.Version = Globals.FormatVersion(_Package.Version);
                    if (string.IsNullOrEmpty(desktopModule.BusinessControllerClass))
                    {
                        desktopModule.SupportedFeatures = 0;
                        //If there is no BusinessControllerClass, then there is no any implementation
                        
                    }
                    else
                    {
                        DesktopModuleController controller = new DesktopModuleController();
                        controller.UpdateModuleInterfaces(ref desktopModule);
                    }
                    DesktopModuleController.SaveDesktopModule(desktopModule, false, true);
                }

            }
        }

		#endregion

		#region "Event Handlers"

        protected void cboDefinitions_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!IsAddDefinitionMode)
            {
                ModuleDefinitionID = int.Parse(cboDefinitions.SelectedValue);
                //Force Module Definition to refresh
                _ModuleDefinition = null;

				//empty definition form's content
				definitionsEditor.Items.ForEach(i =>
				                                	{
				                                		i.Value = null;
				                                	});
                BindDefinition();
            }
        }

        protected void cmdAddDefinition_Click(object sender, EventArgs e)
        {
            IsAddDefinitionMode = true;
            ModuleDefinitionID = Null.NullInteger;
            _ModuleDefinition = null;

			////empty definition form's content
        	definitionsEditor.Items.ForEach(i =>
        	                                	{
        	                                		i.Value = null;
        	                                	});
			
            BindDefinition();
        }

        protected void cmdDeleteDefinition_Click(object sender, EventArgs e)
        {
            var objModuleDefinitions = new ModuleDefinitionController();
            objModuleDefinitions.DeleteModuleDefinition(ModuleDefinitionID);

            //Force Definitions list to refresh by rebinding DesktopModule
            ModuleDefinitionID = Null.NullInteger;
            _ModuleDefinition = null;
            _DesktopModule = null;

			//empty definition form's content
			definitionsEditor.Items.ForEach(i =>
			                                	{
			                                		i.Value = null;
			                                	});
            BindDesktopModule(true);
        }

        private void cmdUpdate_Click(object sender, EventArgs e)
        {
            if (ModuleContext.PortalSettings.ActiveTab.IsSuperTab)
            {
                UpdatePackage();
            }
            else
            {
				//Update DesktopModule Permissions
                DesktopModulePermissionCollection objCurrentPermissions = DesktopModulePermissionController.GetDesktopModulePermissions(dgPermissions.PortalDesktopModuleID);
                if (!objCurrentPermissions.CompareTo(dgPermissions.Permissions))
                {
                    DesktopModulePermissionController.DeleteDesktopModulePermissionsByPortalDesktopModuleID(dgPermissions.PortalDesktopModuleID);
                    foreach (DesktopModulePermissionInfo objPermission in dgPermissions.Permissions)
                    {
                        DesktopModulePermissionController.AddDesktopModulePermission(objPermission);
                    }
                }
                DataCache.RemoveCache(string.Format(DataCache.PortalDesktopModuleCacheKey, ModuleContext.PortalId));

                dgPermissions.ResetPermissions();
            }
        }

        protected void cmdUpdateDefinition_Click(object sender, EventArgs e)
        {
            if (definitionsEditor.IsValid)
            {
                var definition = definitionsEditor.DataSource as ModuleDefinitionInfo;
                if (definition != null)
                {
                    if ((IsAddDefinitionMode && ModuleDefinitionController.GetModuleDefinitionByFriendlyName(definition.FriendlyName) == null) || (!IsAddDefinitionMode))
                    {
                        ModuleDefinitionID = ModuleDefinitionController.SaveModuleDefinition(definition, false, true);

                        //Force Definitions list to refresh by rebinding DesktopModule
                        IsAddDefinitionMode = false;
                        _DesktopModule = null;
                        BindDesktopModule(true);
                    }
                    else
                    {
						//The FriendlyName is being used
                        lblDefinitionError.Visible = true;
                    }
                }
            }
        }

        protected void ctlPortals_AddAllButtonClick(object sender, EventArgs e)
        {
			//Add all Portals
            var objPortals = new PortalController();
            foreach (PortalInfo objPortal in objPortals.GetPortals())
            {
                DesktopModuleController.AddDesktopModuleToPortal(objPortal.PortalID, DesktopModule.DesktopModuleID, true, false);
            }
            DataCache.ClearHostCache(true);

            BindDesktopModule(false);
        }

        protected void ctlPortals_AddButtonClick(object sender, DualListBoxEventArgs e)
        {
            if (e.Items != null)
            {
                foreach (string portal in e.Items)
                {
                    DesktopModuleController.AddDesktopModuleToPortal(int.Parse(portal), DesktopModule.DesktopModuleID, true, false);
                }
            }
            DataCache.ClearHostCache(true);

            BindDesktopModule(false);
        }

        protected void ctlPortals_RemoveAllButtonClick(object sender, EventArgs e)
        {
			//Add all Portals
            var objPortals = new PortalController();
            foreach (PortalInfo objPortal in objPortals.GetPortals())
            {
                DesktopModuleController.RemoveDesktopModuleFromPortal(objPortal.PortalID, DesktopModule.DesktopModuleID, false);
            }
            DataCache.ClearHostCache(true);

            BindDesktopModule(false);
        }

        protected void ctlPortals_RemoveButtonClick(object sender, DualListBoxEventArgs e)
        {
            if (e.Items != null)
            {
                foreach (string portal in e.Items)
                {
                    DesktopModuleController.RemoveDesktopModuleFromPortal(int.Parse(portal), DesktopModule.DesktopModuleID, false);
                }
            }
            DataCache.ClearHostCache(true);

            BindDesktopModule(false);
        }

        protected void grdControls_DeleteCommand(object source, DataGridCommandEventArgs e)
        {
            int controlID = Int32.Parse(e.CommandArgument.ToString());
            ModuleControlController.DeleteModuleControl(controlID);

            //Force Module Definition to refresh
            _ModuleDefinition = null;
            BindDefinition();
        }

        protected void grdControls_ItemDataBound(object sender, DataGridItemEventArgs e)
        {
            DataGridItem item = e.Item;
            if (item.ItemType == ListItemType.Item || item.ItemType == ListItemType.AlternatingItem || item.ItemType == ListItemType.SelectedItem)
            {
                var editHyperlink = item.Controls[3].Controls[0] as HyperLink;
                if (editHyperlink != null)
                {
                    editHyperlink.NavigateUrl = ModuleContext.EditUrl("ModuleControlID",
                                                                      editHyperlink.NavigateUrl,
                                                                      "EditControl",
                                                                      "packageId=" + PackageID,
                                                                      "moduledefid=" + ModuleDefinition.ModuleDefID);
                }
            }
        }
		
		#endregion
    }
}