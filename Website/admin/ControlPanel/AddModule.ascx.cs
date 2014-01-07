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
using System.Globalization;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI;

using DotNetNuke.Application;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Content.Taxonomy;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Modules.Definitions;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Tabs;
using DotNetNuke.Entities.Users;
using DotNetNuke.Framework;
using DotNetNuke.Instrumentation;
using DotNetNuke.Modules.Dashboard.Components.Portals;
using DotNetNuke.Security.Permissions;
using DotNetNuke.Services.Exceptions;
using DotNetNuke.Services.Localization;
using DotNetNuke.Services.Log.EventLog;
using DotNetNuke.Services.Personalization;
using DotNetNuke.UI.ControlPanels;
using DotNetNuke.UI.Utilities;
using DotNetNuke.Web.UI;
using DotNetNuke.Web.UI.WebControls;
using DotNetNuke.Entities.Content.Common;
using DotNetNuke.Web.UI.WebControls.Extensions;

using Globals = DotNetNuke.Common.Globals;
using PortalInfo = DotNetNuke.Entities.Portals.PortalInfo;
using Reflection = DotNetNuke.Framework.Reflection;

#endregion

namespace DotNetNuke.UI.ControlPanel
{
	using System.Web.UI.WebControls;

	public partial class AddModule : UserControlBase, IDnnRibbonBarTool
	{
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof (AddModule));
        private bool _enabled = true;

        /// <summary>
        /// Return the <see cref="PortalSettings"/> for the selected portal (from the Site list), unless
        /// the site list is not visible or there are no other sites in our site group, in which case
        /// it returns the PortalSettings for the current portal.
        /// </summary>
        private PortalSettings SelectedPortalSettings
        {
            get
            {
                var portalSettings = PortalSettings.Current;

                try
                {
                    if (SiteListPanel.Visible && SiteList.SelectedItem != null)
                    {
                        if (!string.IsNullOrEmpty(SiteList.SelectedItem.Value))
                        {
                            var selectedPortalId = int.Parse(SiteList.SelectedItem.Value);
                            if (PortalSettings.PortalId != selectedPortalId)
                            {
                                portalSettings = new PortalSettings(int.Parse(SiteList.SelectedItem.Value));
                            }
                        }
                    }
                }
                catch (Exception)
                {
                    portalSettings = PortalSettings.Current;
                }

                return portalSettings;
            }
        }


        #region Event Handlers

		protected void AddNewOrExisting_OnClick(Object sender, EventArgs e)
		{
			LoadAllLists();
		}

		protected void PaneLstSelectedIndexChanged(Object sender, EventArgs e)
		{
			LoadPositionList();
			LoadPaneModulesList();
		}

		protected void PageLstSelectedIndexChanged(Object sender, EventArgs e)
		{
			LoadModuleList();
		}

		protected void PositionLstSelectedIndexChanged(Object sender, EventArgs e)
		{
			PaneModulesLst.Enabled = PositionLst.SelectedValue == "ABOVE" || PositionLst.SelectedValue == "BELOW";
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

            // Is there more than one site in this group?
            var multipleSites = GetCurrentPortalsGroup().Count() > 1;
            ClientAPI.RegisterClientVariable(Page, "moduleSharing", multipleSites.ToString().ToLowerInvariant(), true);

            ServicesFramework.Instance.RequestAjaxAntiForgerySupport();

            cmdAddModule.Click += CmdAddModuleClick;
			AddNewModule.CheckedChanged += AddNewOrExisting_OnClick;
			AddExistingModule.CheckedChanged += AddNewOrExisting_OnClick;
            SiteList.SelectedIndexChanged += SiteList_SelectedIndexChanged;
			CategoryList.SelectedIndexChanged += CategoryListSelectedIndexChanged;
			PageLst.SelectedIndexChanged += PageLstSelectedIndexChanged;
			PaneLst.SelectedIndexChanged += PaneLstSelectedIndexChanged;
			PositionLst.SelectedIndexChanged += PositionLstSelectedIndexChanged;

            try
			{
				if ((Visible))
				{
					cmdAddModule.Enabled = Enabled;
					AddExistingModule.Enabled = Enabled;
					AddNewModule.Enabled = Enabled;
					Title.Enabled = Enabled;
					PageLst.Enabled = Enabled;
					ModuleLst.Enabled = Enabled;
					VisibilityLst.Enabled = Enabled;
					PaneLst.Enabled = Enabled;
					PositionLst.Enabled = Enabled;
					PaneModulesLst.Enabled = Enabled;

					UserInfo objUser = UserController.GetCurrentUserInfo();
					if ((objUser != null))
					{
						if (objUser.IsSuperUser)
						{
							var objModules = new ModuleController();
							var objModule = objModules.GetModuleByDefinition(-1, "Extensions");
							if (objModule != null)
							{
								var strURL = Globals.NavigateURL(objModule.TabID, true);
								hlMoreExtensions.NavigateUrl = strURL + "#moreExtensions";
							}
							else
							{
								hlMoreExtensions.Enabled = false;
							}
							hlMoreExtensions.Text = GetString("hlMoreExtensions");
							hlMoreExtensions.Visible = true;
						}
					}
				}

				if ((!IsPostBack && Visible && Enabled))
				{
					LoadAllLists();
				}
			}
			catch (Exception exc)
			{
				Exceptions.ProcessModuleLoadException(this, exc);
			}
		}

	    private void CmdConfirmAddModuleClick(object sender, EventArgs e)
        {
            CmdAddModuleClick(sender, e);
        }

        void SiteList_SelectedIndexChanged(object sender, EventArgs e)
        {
            LoadModuleList();
            LoadPageList();
        }

	    private void CategoryListSelectedIndexChanged(object sender, EventArgs e)
		{
			LoadModuleList();
		}

		protected void CmdAddModuleClick(object sender, EventArgs e)
		{
			if (TabPermissionController.CanAddContentToPage() && CanAddModuleToPage())
			{
				int permissionType;
				try
				{
					permissionType = int.Parse(VisibilityLst.SelectedValue);
				}
				catch (Exception exc)
				{
					Logger.Error(exc);

					permissionType = 0;
				}

				int position = -1;
				switch (PositionLst.SelectedValue)
				{
					case "TOP":
						position = 0;
						break;
					case "ABOVE":
						if (!string.IsNullOrEmpty(PaneModulesLst.SelectedValue))
						{
							try
							{
								position = int.Parse(PaneModulesLst.SelectedValue) - 1;
							}
							catch (Exception exc)
							{
								Logger.Error(exc);

								position = -1;
							}
						}
						else
						{
							position = 0;
						}
						break;
					case "BELOW":
						if (!string.IsNullOrEmpty(PaneModulesLst.SelectedValue))
						{
							try
							{
								position = int.Parse(PaneModulesLst.SelectedValue) + 1;
							}
							catch (Exception exc)
							{
								Logger.Error(exc);

								position = -1;
							}
						}
						else
						{
							position = -1;
						}
						break;
					case "BOTTOM":
						position = -1;
						break;
				}

				int moduleLstID;
				try
				{
					moduleLstID = int.Parse(ModuleLst.SelectedValue);
				}
				catch (Exception exc)
				{
					Logger.Error(exc);

					moduleLstID = -1;
				}

				if ((moduleLstID > -1))
				{
					if ((AddExistingModule.Checked))
					{
						int pageID;
						try
						{
							pageID = int.Parse(PageLst.SelectedValue);
						}
						catch (Exception exc)
						{
							Logger.Error(exc);

							pageID = -1;
						}

						if ((pageID > -1))
						{
							DoAddExistingModule(moduleLstID, pageID, PaneLst.SelectedValue, position, "", chkCopyModule.Checked);
						}
					}
					else
					{
						DoAddNewModule(Title.Text, moduleLstID, PaneLst.SelectedValue, position, permissionType, "");
					}
				}

				//set view mode to edit after add module.
				if (PortalSettings.UserMode != PortalSettings.Mode.Edit)
				{
					Personalization.SetProfile("Usability", "UserMode" + PortalSettings.PortalId, "EDIT");
				}
				Response.Redirect(Request.RawUrl, true);
			}
		}

	    #endregion

		#region Properties

		public override bool Visible
		{
			get
			{
				return base.Visible && TabPermissionController.CanAddContentToPage();
			}
			set
			{
				base.Visible = value;
			}
		}

		public bool Enabled
		{
			get
			{
				return _enabled && CanAddModuleToPage();
			}
			set
			{
				_enabled = value;
			}
		}

		public string ToolName
		{
			get
			{
				return "QuickAddModule";
			}
			set
			{
				throw new NotSupportedException("Set ToolName not supported");
			}
		}

        /// <summary>The currently-selected module.</summary>
	    protected DesktopModuleInfo SelectedModule
	    {
	        get
	        {
                if (AddExistingModule.Checked)
                {
                    var tabId = -1;
                    if (!string.IsNullOrEmpty(PageLst.SelectedValue))
                        tabId = int.Parse(PageLst.SelectedValue);

                    if (tabId < 0)
                        tabId = PortalSettings.Current.ActiveTab.TabID;

                    if (!string.IsNullOrEmpty(ModuleLst.SelectedValue))
                    {
                        var moduleId = int.Parse(ModuleLst.SelectedValue);
                        if (moduleId >= 0)
                        {
                            return new ModuleController().GetModule(moduleId, tabId).DesktopModule;
                        }
                    }
                }
                else
                {
                    var portalId = -1;

                    if (SiteListPanel.Visible) portalId = int.Parse(SiteList.SelectedValue);

                    if (portalId < 0) portalId = PortalSettings.Current.PortalId;

                    if (!string.IsNullOrEmpty(ModuleLst.SelectedValue))
                    {
                        var moduleId = int.Parse(ModuleLst.SelectedValue);
                        if (moduleId >= 0)
                        {
                            return DesktopModuleController.GetDesktopModule(moduleId, portalId);
                        }
                    }
                }

                return null;
	        }
	    } 

		#endregion

		#region Methods

        private static ModulePermissionInfo AddModulePermission(ModuleInfo objModule, PermissionInfo permission, int roleId, int userId, bool allowAccess)
        {
            var objModulePermission = new ModulePermissionInfo
            {
                ModuleID = objModule.ModuleID,
                PermissionID = permission.PermissionID,
                RoleID = roleId,
                UserID = userId,
                PermissionKey = permission.PermissionKey,
                AllowAccess = allowAccess
            };

            // add the permission to the collection
            if (!objModule.ModulePermissions.Contains(objModulePermission))
            {
                objModule.ModulePermissions.Add(objModulePermission);
            }

            return objModulePermission;
        }

        private void DoAddExistingModule(int moduleId, int tabId, string paneName, int position, string align, bool cloneModule)
        {
            var moduleCtrl = new ModuleController();
            ModuleInfo moduleInfo = moduleCtrl.GetModule(moduleId, tabId, false);

            int userID = -1;
            if (Request.IsAuthenticated)
            {
                UserInfo user = UserController.GetCurrentUserInfo();
                if (((user != null)))
                {
                    userID = user.UserID;
                }
            }

            if ((moduleInfo != null))
            {
                // Is this from a site other than our own? (i.e., is the user requesting "module sharing"?)
                var remote = moduleInfo.PortalID != PortalSettings.Current.PortalId;
                if (remote)
                {
                    switch (moduleInfo.DesktopModule.Shareable)
                    {
                        case ModuleSharing.Unsupported:
                            // Should never happen since the module should not be listed in the first place.
                            throw new ApplicationException(string.Format("Module '{0}' does not support Shareable and should not be listed in Add Existing Module from a different source site",
                                                                         moduleInfo.DesktopModule.FriendlyName));
                        case ModuleSharing.Supported:
                            break;
                        default:
                        case ModuleSharing.Unknown:
                            break;
                    }
                }

                // clone the module object ( to avoid creating an object reference to the data cache )
                ModuleInfo newModule = moduleInfo.Clone();

                newModule.UniqueId = Guid.NewGuid(); // Cloned Module requires a different uniqueID

                newModule.TabID = PortalSettings.Current.ActiveTab.TabID;
                newModule.ModuleOrder = position;
                newModule.PaneName = paneName;
                newModule.Alignment = align;

                if ((cloneModule))
                {
                    newModule.ModuleID = Null.NullInteger;
                    //reset the module id
                    newModule.ModuleID = moduleCtrl.AddModule(newModule);

                    if (!string.IsNullOrEmpty(newModule.DesktopModule.BusinessControllerClass))
                    {
                        object objObject = Reflection.CreateObject(newModule.DesktopModule.BusinessControllerClass, newModule.DesktopModule.BusinessControllerClass);
                        if (objObject is IPortable)
                        {
                            string content = Convert.ToString(((IPortable)objObject).ExportModule(moduleId));
                            if (!string.IsNullOrEmpty(content))
                            {
                                ((IPortable)objObject).ImportModule(newModule.ModuleID, content, newModule.DesktopModule.Version, userID);
                            }
                        }
                    }
                }
                else
                {
                    moduleCtrl.AddModule(newModule);
                }

                if (remote)
                {
                    //Ensure the Portal Admin has View rights
                    var permissionController = new PermissionController();
                    ArrayList arrSystemModuleViewPermissions = permissionController.GetPermissionByCodeAndKey("SYSTEM_MODULE_DEFINITION", "VIEW");
                    AddModulePermission(newModule,
                                    (PermissionInfo)arrSystemModuleViewPermissions[0],
                                    PortalSettings.Current.AdministratorRoleId,
                                    Null.NullInteger,
                                    true);

                    //Set PortalID correctly
                    newModule.OwnerPortalID = newModule.PortalID;
                    newModule.PortalID = PortalSettings.Current.PortalId;
                    ModulePermissionController.SaveModulePermissions(newModule);
                }

                //Add Event Log
                var objEventLog = new EventLogController();
                objEventLog.AddLog(newModule, PortalSettings.Current, userID, "", EventLogController.EventLogType.MODULE_CREATED);
            }
        }

        private static void DoAddNewModule(string title, int desktopModuleId, string paneName, int position, int permissionType, string align)
        {
            var objModules = new ModuleController();

            try
            {
                DesktopModuleInfo desktopModule;
                if (!DesktopModuleController.GetDesktopModules(PortalSettings.Current.PortalId).TryGetValue(desktopModuleId, out desktopModule))
                {
                    throw new ArgumentException("desktopModuleId");
                }
            }
            catch (Exception ex)
            {
                Exceptions.LogException(ex);
            }

            foreach (ModuleDefinitionInfo objModuleDefinition in
                ModuleDefinitionController.GetModuleDefinitionsByDesktopModuleID(desktopModuleId).Values)
            {
                var objModule = new ModuleInfo();
                objModule.Initialize(PortalSettings.Current.ActiveTab.PortalID);

                objModule.PortalID = PortalSettings.Current.ActiveTab.PortalID;
                objModule.TabID = PortalSettings.Current.ActiveTab.TabID;
                objModule.ModuleOrder = position;
                objModule.ModuleTitle = string.IsNullOrEmpty(title) ? objModuleDefinition.FriendlyName : title;
                objModule.PaneName = paneName;
                objModule.ModuleDefID = objModuleDefinition.ModuleDefID;
                if (objModuleDefinition.DefaultCacheTime > 0)
                {
                    objModule.CacheTime = objModuleDefinition.DefaultCacheTime;
                    if (PortalSettings.Current.DefaultModuleId > Null.NullInteger && PortalSettings.Current.DefaultTabId > Null.NullInteger)
                    {
                        ModuleInfo defaultModule = objModules.GetModule(PortalSettings.Current.DefaultModuleId, PortalSettings.Current.DefaultTabId, true);
                        if ((defaultModule != null))
                        {
                            objModule.CacheTime = defaultModule.CacheTime;
                        }
                    }
                }

                objModules.InitialModulePermission(objModule, objModule.TabID, permissionType);

                if (PortalSettings.Current.ContentLocalizationEnabled)
                {
                    Locale defaultLocale = LocaleController.Instance.GetDefaultLocale(PortalSettings.Current.PortalId);
                    //check whether original tab is exists, if true then set culture code to default language,
                    //otherwise set culture code to current.
                    if (new TabController().GetTabByCulture(objModule.TabID, PortalSettings.Current.PortalId, defaultLocale) != null)
                    {
                        objModule.CultureCode = defaultLocale.Code;
                    }
                    else
                    {
                        objModule.CultureCode = PortalSettings.Current.CultureCode;
                    }
                }
                else
                {
                    objModule.CultureCode = Null.NullString;
                }
                objModule.AllTabs = false;
                objModule.Alignment = align;

                objModules.AddModule(objModule);
            }
        }

        private IEnumerable<PortalInfo> GetCurrentPortalsGroup()
        {
            var groups = PortalGroupController.Instance.GetPortalGroups().ToArray();

            var result = (from @group in groups
                          select PortalGroupController.Instance.GetPortalsByGroup(@group.PortalGroupId)
                              into portals
                              where portals.Any(x => x.PortalID == PortalSettings.Current.PortalId)
                              select portals.ToArray()).FirstOrDefault();

            // Are we in a group of one?
            if (result == null || result.Length == 0)
            {
                var portalController = new PortalController();

                result = new[] { portalController.GetPortal(PortalSettings.Current.PortalId) };
            }

            return result;
        }

        private static bool GetIsPortable(ModuleController moduleCtrl, string moduleID, string tabID)
        {
            bool isPortable = false;
            int parsedModuleID;
            int parsedTabID;

            bool validModuleID = int.TryParse(moduleID, out parsedModuleID);
            bool validTabID = int.TryParse(tabID, out parsedTabID);

            if ((validModuleID && validTabID))
            {
                ModuleInfo moduleInfo = moduleCtrl.GetModule(parsedModuleID, parsedTabID);
                if (((moduleInfo != null)))
                {
                    DesktopModuleInfo moduleDesktopInfo = moduleInfo.DesktopModule;
                    if (((moduleDesktopInfo != null)))
                    {
                        isPortable = moduleDesktopInfo.IsPortable;
                    }
                }
            }

            return isPortable;
        }

        protected string GetString(string key)
        {
            return Localization.GetString(key, LocalResourceFile);
        }

        private void LoadAllLists()
		{
		    LoadSiteList();
			LoadCategoryList();
			LoadPageList();
			LoadModuleList();
			LoadVisibilityList();
			LoadPaneList();
			LoadPositionList();
			LoadPaneModulesList();
		}

        private void LoadCategoryList()
        {
            CategoryListPanel.Visible = !AddExistingModule.Checked;

            ITermController termController = Util.GetTermController();
            CategoryList.DataSource = termController.GetTermsByVocabulary("Module_Categories").OrderBy(t => t.Weight).Where(t => t.Name != "< None >").ToList();
            CategoryList.DataBind();
            //CategoryList.Items.Add(new ListItem(Localization.GetString("AllCategories", LocalResourceFile), "All"));
            CategoryList.AddItem(Localization.GetString("AllCategories", LocalResourceFile), "All");
            if (!IsPostBack)
            {
                CategoryList.Select("Common", false);
            }
        }

        private void LoadModuleList()
        {
            if (AddExistingModule.Checked)
            {
                //Get list of modules for the selected tab
                if (!string.IsNullOrEmpty(PageLst.SelectedValue))
                {
                    var moduleCtrl = new ModuleController();
                    var tabId = int.Parse(PageLst.SelectedValue);
                    if (tabId >= 0)
                    {
                        ModuleLst.BindTabModulesByTabID(tabId);
                    }
                    if ((ModuleLst.ItemCount > 0))
                    {
                        chkCopyModule.Visible = true;
                        SetCopyModuleMessage(GetIsPortable(moduleCtrl, ModuleLst.SelectedValue, PageLst.SelectedValue));
                    }
                }
            }
            else
            {
                ModuleLst.Filter = CategoryList.SelectedValue == "All"
                                        ? (Func<KeyValuePair<string, PortalDesktopModuleInfo>, bool>)(kvp => true)
                                         : (Func<KeyValuePair<string, PortalDesktopModuleInfo>, bool>)(kvp => kvp.Value.DesktopModule.Category == CategoryList.SelectedValue);
                ModuleLst.BindAllPortalDesktopModules();
            }

            ModuleLst.Enabled = ModuleLst.ItemCount > 0;
        }

        private void LoadPageList()
		{
			PageListPanel.Visible = AddExistingModule.Checked;
			TitlePanel.Enabled = !AddExistingModule.Checked;
			chkCopyModule.Visible = AddExistingModule.Checked;

			if ((AddExistingModule.Checked))
			{
				chkCopyModule.Text = Localization.GetString("CopyModuleDefault.Text", LocalResourceFile);
			}

	        var portalSettings = SelectedPortalSettings;

	        PageLst.Items.Clear();

            if (PageListPanel.Visible)
            {
                PageLst.DataValueField = "TabID";
				PageLst.DataTextField = "IndentedTabName";
                if(PortalSettings.PortalId == SelectedPortalSettings.PortalId)
                {
                    PageLst.DataSource = TabController.GetPortalTabs(portalSettings.PortalId, portalSettings.ActiveTab.TabID, true, string.Empty, true, false, false, false, true);
                }
                else
                {
                    PageLst.DataSource = TabController.GetPortalTabs(portalSettings.PortalId, Null.NullInteger, true, string.Empty, true, false, false, false, true);
                }
				PageLst.DataBind();
			}
		}

        private void LoadPaneList()
        {
            PaneLst.Items.Clear();
            PaneLst.DataSource = PortalSettings.Current.ActiveTab.Panes;
            PaneLst.DataBind();
            if ((PortalSettings.Current.ActiveTab.Panes.Contains(Globals.glbDefaultPane)))
            {
                PaneLst.SelectedValue = Globals.glbDefaultPane;
            }
        }

        private void LoadPaneModulesList()
        {
            var items = new Dictionary<string, string> { { string.Empty, string.Empty } };

            foreach (ModuleInfo m in PortalSettings.Current.ActiveTab.Modules)
            {
                //if user is allowed to view module and module is not deleted
                if (ModulePermissionController.CanViewModule(m) && !m.IsDeleted)
                {
                    //modules which are displayed on all tabs should not be displayed on the Admin or Super tabs
                    if (!m.AllTabs || !PortalSettings.Current.ActiveTab.IsSuperTab)
                    {
                        if (m.PaneName == PaneLst.SelectedValue)
                        {
                            int moduleOrder = m.ModuleOrder;

                            while (items.ContainsKey(moduleOrder.ToString()) || moduleOrder == 0)
                            {
                                moduleOrder++;
                            }
                            items.Add(moduleOrder.ToString(), m.ModuleTitle);
                        }
                    }
                }
            }

            PaneModulesLst.Enabled = true;
            PaneModulesLst.Items.Clear();
            PaneModulesLst.DataValueField = "key";
            PaneModulesLst.DataTextField = "value";
            PaneModulesLst.DataSource = items;
            PaneModulesLst.DataBind();

            if ((PaneModulesLst.Items.Count <= 1))
            {
                var listItem = PositionLst.FindItemByValue("ABOVE");
                if (((listItem != null)))
                {
                    PositionLst.Items.Remove(listItem);
                }
                listItem = PositionLst.FindItemByValue("BELOW");
                if (((listItem != null)))
                {
                    PositionLst.Items.Remove(listItem);
                }
                PaneModulesLst.Enabled = false;
            }

            if ((PositionLst.SelectedValue == "TOP" || PositionLst.SelectedValue == "BOTTOM"))
            {
                PaneModulesLst.Enabled = false;
            }
        }

        private void LoadPositionList()
        {
            var items = new Dictionary<string, string>
							{
								{"TOP", GetString("Top")},
								{"ABOVE", GetString("Above")},
								{"BELOW", GetString("Below")},
								{"BOTTOM", GetString("Bottom")}
							};

            PositionLst.Items.Clear();
            PositionLst.DataValueField = "key";
            PositionLst.DataTextField = "value";
            PositionLst.DataSource = items;
            PositionLst.DataBind();
            PositionLst.SelectedValue = "BOTTOM";
        }

        private void LoadSiteList()
        {
            // Is there more than one site in this group?
            var multipleSites = GetCurrentPortalsGroup().Count() > 1;

            SiteListPanel.Visible = multipleSites && AddExistingModule.Checked;

            if (SiteListPanel.Visible)
            {
                // Get a list of portals in this SiteGroup.
                var controller = new PortalController();

                var portals = controller.GetPortals().Cast<PortalInfo>().ToArray();

                SiteList.DataSource = portals.Select(
                    x => new {Value = x.PortalID, Name = x.PortalName, GroupID = x.PortalGroupID}).ToList();
                SiteList.DataTextField = "Name";
                SiteList.DataValueField = "Value";
                SiteList.DataBind();
            }
        }

		private void LoadVisibilityList()
		{
			VisibilityLst.Enabled = !AddExistingModule.Checked;
			if ((VisibilityLst.Enabled))
			{
				var items = new Dictionary<string, string> {{"0", GetString("PermissionView")}, {"1", GetString("PermissionEdit")}};

				VisibilityLst.Items.Clear();
				VisibilityLst.DataValueField = "key";
				VisibilityLst.DataTextField = "value";
				VisibilityLst.DataSource = items;
				VisibilityLst.DataBind();
			}
		}

        private string LocalResourceFile
        {
            get
            {
                return string.Format("{0}/{1}/{2}.ascx.resx", TemplateSourceDirectory, Localization.LocalResourceDirectory, GetType().BaseType.Name);
            }
        }

        private void SetCopyModuleMessage(bool isPortable)
        {
            if ((isPortable))
            {
                chkCopyModule.Text = Localization.GetString("CopyModuleWcontent", LocalResourceFile);
                chkCopyModule.ToolTip = Localization.GetString("CopyModuleWcontent.ToolTip", LocalResourceFile);
            }
            else
            {
                chkCopyModule.Text = Localization.GetString("CopyModuleWOcontent", LocalResourceFile);
                chkCopyModule.ToolTip = Localization.GetString("CopyModuleWOcontent.ToolTip", LocalResourceFile);
            }
        }

		#endregion


        public bool CanAddModuleToPage()
        {
            if (HttpContext.Current == null)
            {
                return false;
            }
            //If we are not in an edit page
            return (string.IsNullOrEmpty(HttpContext.Current.Request.QueryString["mid"])) && (string.IsNullOrEmpty(HttpContext.Current.Request.QueryString["ctl"]));
        }

    }
}