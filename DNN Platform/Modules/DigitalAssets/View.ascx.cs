#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2017
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

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;

using DotNetNuke.Common;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Icons;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Modules.Actions;
using DotNetNuke.ExtensionPoints;
using DotNetNuke.ExtensionPoints.Filters;
using DotNetNuke.Framework;
using DotNetNuke.Framework.JavaScriptLibraries;
using DotNetNuke.Modules.DigitalAssets.Components.Controllers;
using DotNetNuke.Modules.DigitalAssets.Components.Controllers.Models;
using DotNetNuke.Modules.DigitalAssets.Components.WebControls;
using DotNetNuke.Modules.DigitalAssets.Services;
using DotNetNuke.Security;
using DotNetNuke.Security.Permissions;
using DotNetNuke.Services.Assets;
using DotNetNuke.Services.Exceptions;
using DotNetNuke.Services.FileSystem;
using DotNetNuke.Services.Localization;
using DotNetNuke.UI.Skins;
using DotNetNuke.UI.Skins.Controls;
using DotNetNuke.Web.Client;
using DotNetNuke.Web.Client.ClientResourceManagement;
using DotNetNuke.Web.UI.WebControls;

using Telerik.Web.UI;

namespace DotNetNuke.Modules.DigitalAssets
{
    public partial class View : PortalModuleBase, IActionable
    {
        private static readonly DigitalAssetsSettingsRepository SettingsRepository = new DigitalAssetsSettingsRepository();

        private readonly IDigitalAssetsController controller;
        private readonly ExtensionPointManager epm = new ExtensionPointManager();
        private NameValueCollection damState;

        public View()
        {
            controller = new Factory().DigitalAssetsController;
        }

        private IExtensionPointFilter Filter
        {
            get
            {
                return new CompositeFilter()
                    .And(new FilterByHostMenu(IsHostPortal))
                    .And(new FilterByUnauthenticated(HttpContext.Current.Request.IsAuthenticated));
            }
        }

        private NameValueCollection DAMState
        {
            get
            {
                if (damState == null)
                {
                    var stateCookie = Request.Cookies["damState-" + UserId];
                    damState = HttpUtility.ParseQueryString(Uri.UnescapeDataString(stateCookie != null ? stateCookie.Value : ""));
                }

                return damState;
            }
        }

        #region Protected Properties

        protected int InitialTab
        {
            get
            {
                return controller.GetInitialTab(Request.Params, DAMState);
            }
        }

        protected bool IsHostPortal
        {
            get
            {
                return IsHostMenu || controller.GetCurrentPortalId(ModuleId) == Null.NullInteger;
            }
        }

        protected string InvalidCharacters
        {
            get
            {
                return GetNoControlCharsString(controller.GetInvalidChars());
            }
        }

        protected string InvalidCharactersErrorText
        {
            get
            {
                return controller.GetInvalidCharsErrorText();
            }
        }

        protected int MaxUploadSize
        {
            get
            {
                return (int)Config.GetMaxUploadSize();
            }
        }

        protected string NavigateUrl
        {
            get
            {
                var url = Globals.NavigateURL(TabId, "ControlKey", "mid=" + ModuleId, "ReturnUrl=" + Server.UrlEncode(Globals.NavigateURL()));

                //append popUp parameter
                var delimiter = url.Contains("?") ? "&" : "?";
                url = string.Format("{0}{1}popUp=true", url, delimiter);

                return url;
            }
        }

        protected IEnumerable<string> DefaultFolderProviderValues
        {            
            get
            {
                return this.controller.GetDefaultFolderProviderValues(this.ModuleId).Select(f => f.FolderMappingID.ToString(CultureInfo.InvariantCulture)).ToList();
            }
        }

        protected string DefaultFolderTypeId
        {
            get
            {
                var defaultFolderTypeId = controller.GetDefaultFolderTypeId(ModuleId);
                return defaultFolderTypeId.HasValue ? defaultFolderTypeId.ToString() : "";
            }
        }

        protected bool FilteredContent
        {
            get
            {
                return SettingsRepository.GetSubfolderFilter(ModuleId) != SubfolderFilter.IncludeSubfoldersFolderStructure;
            }
        }

        protected string PageSize { get; private set; }

        protected string ActiveView { get; private set; }

        #endregion

        #region Private Methods
        private static string GetNoControlCharsString(string text)
        {
            var result = new StringBuilder();
            foreach (char c in text.Where(c => !char.IsControl(c)))
            {
                result.Append(c);
            }

            return result.ToString();
        }

        private void InitializeFolderType()
        {
            FolderTypeComboBox.DataSource = controller.GetFolderMappings(ModuleId);
            FolderTypeComboBox.DataBind();
        }

        private void InitializeGrid()
        {
            Grid.MasterTableView.PagerStyle.PrevPageToolTip = LocalizeString("PagerPreviousPage.ToolTip");
            Grid.MasterTableView.PagerStyle.NextPageToolTip = LocalizeString("PagerNextPage.ToolTip");
            Grid.MasterTableView.PagerStyle.FirstPageToolTip = LocalizeString("PagerFirstPage.ToolTip");
            Grid.MasterTableView.PagerStyle.LastPageToolTip = LocalizeString("PagerLastPage.ToolTip");
            Grid.MasterTableView.PagerStyle.PageSizeLabelText = LocalizeString("PagerPageSize.Text");

            foreach (var columnExtension in epm.GetGridColumnExtensionPoints("DigitalAssets", "GridColumns", Filter))
            {
                var column = new DnnGridBoundColumn
                                    {
                                        HeaderText = columnExtension.HeaderText,
                                        DataField = columnExtension.DataField,
                                        UniqueName = columnExtension.UniqueName,
                                        ReadOnly = columnExtension.ReadOnly,
                                        Reorderable = columnExtension.Reorderable,
                                        SortExpression = columnExtension.SortExpression,
                                        HeaderTooltip = columnExtension.HeaderText
                                    };
                column.HeaderStyle.Width = columnExtension.HeaderStyleWidth;

                var index = Math.Min(columnExtension.ColumnAt, Grid.Columns.Count - 1);
                Grid.Columns.AddAt(index, column);
            }            
        }

        private void LoadSubfolders(DnnTreeNode node, int folderId, string nextFolderName, out DnnTreeNode nextNode, out int nextFolderId)
        {
            nextNode = null;
            nextFolderId = 0;
            var folders = controller.GetFolders(ModuleId, folderId);
            foreach (var folder in folders)
            {
                var hasViewPermissions = HasViewPermissions(folder.Permissions);
                var newNode = this.CreateNodeFromFolder(folder);
                SetupNodeAttributes(newNode, folder.Permissions, folder);

                node.Nodes.Add(newNode);

                if (hasViewPermissions && folder.FolderName.Equals(nextFolderName, StringComparison.InvariantCultureIgnoreCase))
                {
                    newNode.Expanded = true;
                    nextNode = newNode;
                    nextFolderId = folder.FolderID;                    
                }
            }
        }

        private void InitializeTreeViews(string initialPath)
        {
            var rootFolder = RootFolderViewModel;

            var rootNode = this.CreateNodeFromFolder(rootFolder);
            rootNode.Selected = true;
            rootNode.Expanded = true;

            var folderId = rootFolder.FolderID;
            var nextNode = rootNode;
            foreach (var folderName in initialPath.Split('/'))
            {
                LoadSubfolders(nextNode, folderId, folderName, out nextNode, out folderId);
                if (nextNode == null)
                {
                    // The requested folder does not exist or the user does not have permissions
                    break;
                }
            }

            if (nextNode != null)
            {
                nextNode.Expanded = false;
                nextNode.Selected = true;
                rootNode.Selected = false;
            }

            if (rootNode.Nodes.Count == 0)
            {
                this.SetExpandable(rootNode, false);                
            }
            
            SetupNodeAttributes(rootNode, GetPermissionsForRootFolder(rootFolder.Permissions), rootFolder);

			FolderTreeView.Nodes.Clear();
			DestinationTreeView.Nodes.Clear();

            FolderTreeView.Nodes.Add(rootNode);
            DestinationTreeView.Nodes.Add(rootNode.Clone());

            InitializeTreeViewContextMenu();
        }

        private DnnTreeNode CreateNodeFromFolder(FolderViewModel folder)
        {
            var node = new DnnTreeNode
            {
                Text = folder.FolderName,
                ImageUrl = folder.IconUrl,
                Value = folder.FolderID.ToString(CultureInfo.InvariantCulture),
                Category = folder.FolderMappingID.ToString(CultureInfo.InvariantCulture),
            };
            this.SetExpandable(node, folder.HasChildren && HasViewPermissions(folder.Permissions));
            return node;
        }

        private void SetExpandable(DnnTreeNode node, bool expandable)
        {
            node.ExpandMode = expandable ? TreeNodeExpandMode.WebService : TreeNodeExpandMode.ClientSide;
        }

        private void SetupNodeAttributes(DnnTreeNode node, IEnumerable<PermissionViewModel> permissions, FolderViewModel folder)
        {
            node.Attributes.Add("permissions", permissions.ToJson());
            foreach (var attribute in folder.Attributes)
            {
                node.Attributes.Add(attribute.Key, attribute.Value.ToJson());
            }
        }

        private void InitializeTreeViewContextMenu()
        {
            MainContextMenu.Items.AddRange(new[]
            {
                new DnnMenuItem
                    {
                        Text = Localization.GetString("CreateFolder", LocalResourceFile),
                        Value = "NewFolder",
                        CssClass = "permission_ADD disabledIfFiltered",
                        ImageUrl = IconController.IconURL("FolderCreate", "16x16", "Gray")
                    },    
                new DnnMenuItem
                    {
                        Text = Localization.GetString("RefreshFolder", LocalResourceFile),
                        Value = "RefreshFolder",
                        CssClass = "permission_BROWSE permission_READ",
                        ImageUrl = IconController.IconURL("FolderRefreshSync", "16x16", "Gray")
                    },    
                new DnnMenuItem
                    {
                        Text = Localization.GetString("RenameFolder", LocalResourceFile),
                        Value = "RenameFolder",
                        CssClass = "permission_MANAGE",
                        ImageUrl = IconController.IconURL("FileRename", "16x16", "Black")
                    },  
                new DnnMenuItem
                    {
                        Text = Localization.GetString("Move", LocalResourceFile),
                        Value = "Move",
                        CssClass = "permission_COPY",
                        ImageUrl = IconController.IconURL("FileMove", "16x16", "Black")
                    },
                new DnnMenuItem
                    {
                        Text = Localization.GetString("DeleteFolder", LocalResourceFile),
                        Value = "DeleteFolder",
                        CssClass = "permission_DELETE",
                        ImageUrl = IconController.IconURL("FileDelete", "16x16", "Black")
                    },
                new DnnMenuItem
                    {
                        Text = Localization.GetString("UnlinkFolder", LocalResourceFile),
                        Value = "UnlinkFolder",
                        CssClass = "permission_DELETE",
                        ImageUrl = IconController.IconURL("UnLink", "16x16", "Black")
                    },
                new DnnMenuItem
                    {
                        Text = Localization.GetString("ViewFolderProperties", LocalResourceFile),
                        Value = "Properties",
                        CssClass = "permission_READ",
                        ImageUrl = IconController.IconURL("ViewProperties", "16x16", "CtxtMn")
                    },
            });
            
            // Dnn Menu Item Extension Point
            foreach (var menuItem in epm.GetMenuItemExtensionPoints("DigitalAssets", "TreeViewContextMenu", Filter))
            {
                MainContextMenu.Items.Add(new DnnMenuItem
                {
                    Text = menuItem.Text,
                    Value = menuItem.Value,
                    CssClass = menuItem.CssClass,
                    ImageUrl = menuItem.Icon
                });
            }
        }

        private void InitializeSearchBox()
        {
            var extension = epm.GetUserControlExtensionPointFirstByPriority("DigitalAssets", "SearchBoxExtensionPoint");
            var searchControl = (PortalModuleBase)Page.LoadControl(extension.UserControlSrc);
            searchControl.ModuleConfiguration = ModuleConfiguration;

            searchControl.ID = searchControl.GetType().BaseType.Name;
            SearchBoxPanel.Controls.Add(searchControl);
        }

        private void InitializeGridContextMenu()
        {
            GridMenu.Items.AddRange(new[]
                {
                    new DnnMenuItem
                    {
                        Text = Localization.GetString("Download", LocalResourceFile),
                        Value = "Download",
                        CssClass = "permission_READ",
                        ImageUrl = IconController.IconURL("FileDownload", "16x16", "Black")
                    },    
                new DnnMenuItem
                    {
                        Text = Localization.GetString("Rename", LocalResourceFile),
                        Value = "Rename",
                        CssClass = "permission_MANAGE singleItem",
                        ImageUrl = IconController.IconURL("FileRename", "16x16", "Black")
                    },    
                new DnnMenuItem
                    {
                        Text = Localization.GetString("Copy", LocalResourceFile),
                        Value = "Copy",
                        CssClass = "permission_COPY onlyFiles",
                        ImageUrl = IconController.IconURL("FileCopy", "16x16", "Black")
                    },  
                new DnnMenuItem
                    {
                        Text = Localization.GetString("Move", LocalResourceFile),
                        Value = "Move",
                        CssClass = "permission_COPY disabledIfFiltered",
                        ImageUrl = IconController.IconURL("FileMove", "16x16", "Black")
                    }, 
                new DnnMenuItem
                    {
                        Text = Localization.GetString("Delete", LocalResourceFile),
                        Value = "Delete",
                        CssClass = "permission_DELETE",
                        ImageUrl = IconController.IconURL("FileDelete", "16x16", "Black")
                    }, 
                new DnnMenuItem
                    {
                        Text = Localization.GetString("Unlink", LocalResourceFile),
                        Value = "Unlink",
                        CssClass = "permission_DELETE singleItem onlyFolders",
                        ImageUrl = IconController.IconURL("UnLink", "16x16", "Black")
                    }, 
                new DnnMenuItem
                    {
                        Text = Localization.GetString("UnzipFile", LocalResourceFile),
                        Value = "UnzipFile",
                        CssClass = "permission_MANAGE singleItem onlyFiles",
                        ImageUrl = IconController.IconURL("Unzip", "16x16", "Gray")
                    },
                new DnnMenuItem
                    {
                        Text = Localization.GetString("ViewProperties", LocalResourceFile),
                        Value = "Properties",
                        CssClass = "permission_READ singleItem",
                        ImageUrl = IconController.IconURL("ViewProperties", "16x16", "CtxtMn")
                    }, 
                new DnnMenuItem
                    {
                        Text = Localization.GetString("GetUrl", LocalResourceFile),
                        Value = "GetUrl",
                        CssClass = "permission_READ singleItem onlyFiles",
                        ImageUrl = IconController.IconURL("FileLink", "16x16", "Black")
                    }
                });

            // Dnn Menu Item Extension Point
            foreach (var menuItem in epm.GetMenuItemExtensionPoints("DigitalAssets", "GridContextMenu", Filter))
            {
                GridMenu.Items.Add(new DnnMenuItem
                                       {
                                           Text = menuItem.Text,
                                           Value = menuItem.Value,
                                           CssClass = menuItem.CssClass,
                                           ImageUrl = menuItem.Icon
                                       });
            }
        }

        private void InitializeEmptySpaceContextMenu()
        {
            EmptySpaceMenu.Items.AddRange(new[]
            {
                new DnnMenuItem
                    {
                        Text = Localization.GetString("CreateFolder", LocalResourceFile),
                        Value = "NewFolder",
                        CssClass = "permission_ADD disabledIfFiltered",
                        ImageUrl = IconController.IconURL("FolderCreate", "16x16", "Gray")
                    },    
                new DnnMenuItem
                    {
                        Text = Localization.GetString("RefreshFolder", LocalResourceFile),
                        Value = "RefreshFolder",
                        CssClass = "permission_READ permission_BROWSE",
                        ImageUrl = IconController.IconURL("FolderRefreshSync", "16x16", "Gray")
                    },    
                new DnnMenuItem
                    {
                        Text = Localization.GetString("UploadFiles.Title", LocalResourceFile),
                        Value = "UploadFiles",
                        CssClass = "permission_ADD",
                        ImageUrl = IconController.IconURL("UploadFiles", "16x16", "Gray")
                    },  
                new DnnMenuItem
                    {
                        Text = Localization.GetString("ViewFolderProperties", LocalResourceFile),
                        Value = "Properties",
                        CssClass = "permission_READ",
                        ImageUrl = IconController.IconURL("ViewProperties", "16x16", "CtxtMn")
                    },
            });
        }

        private bool HasViewPermissions(IEnumerable<PermissionViewModel> permissions)
        {
            return permissions.Where(permission => permission.Key == "BROWSE" || permission.Key == "READ").Any(permission => permission.Value);
        }

        private IEnumerable<PermissionViewModel> GetPermissionsForRootFolder(IEnumerable<PermissionViewModel> rootPermissions)
        {
            var result = new List<PermissionViewModel>();

            var deniedPermissionsForRoot = new[] { "DELETE", "MANAGE", "COPY" };

            foreach (var permission in rootPermissions)
            {
                result.Add(deniedPermissionsForRoot.Contains(permission.Key)
                               ? new PermissionViewModel { Key = permission.Key, Value = false }
                               : permission);
            }

            return result;
        }

        private void OnItemDataBoundFolderTypeComboBox(object sender, RadComboBoxItemEventArgs e)
        {
            var dataSource = (FolderMappingViewModel)e.Item.DataItem;
            e.Item.Attributes["SupportsMappedPaths"] = FolderProvider.GetProviderList()[dataSource.FolderTypeName].SupportsMappedPaths.ToString().ToLowerInvariant();
        }
        #endregion

        protected FolderViewModel RootFolderViewModel { get; private set; }

        protected override void OnLoad(EventArgs e)
        {
            try
            {
                base.OnLoad(e);

                //if (IsPostBack) return;

                switch (SettingsRepository.GetMode(ModuleId))
                {
                    case DigitalAssestsMode.Group:
                        int groupId;
                        if (string.IsNullOrEmpty(Request["groupId"]) || !int.TryParse(Request["groupId"], out groupId))
                        {
                            Skin.AddModuleMessage(this, Localization.GetString("InvalidGroup.Error", LocalResourceFile), ModuleMessage.ModuleMessageType.RedError);
                            return;
                        }

                        var groupFolder = controller.GetGroupFolder(groupId, PortalSettings);
                        if (groupFolder == null)
                        {
                            Skin.AddModuleMessage(this, Localization.GetString("InvalidGroup.Error", LocalResourceFile), ModuleMessage.ModuleMessageType.RedError);
                            return;
                        }

                        this.RootFolderViewModel = groupFolder;
                        break;

                    case DigitalAssestsMode.User:
                        if (PortalSettings.UserId == Null.NullInteger)
                        {
                            Skin.AddModuleMessage(this, Localization.GetString("InvalidUser.Error", LocalResourceFile), ModuleMessage.ModuleMessageType.RedError);
                            return;
                        }
                  
                        this.RootFolderViewModel = this.controller.GetUserFolder(this.PortalSettings.UserInfo);     
                        break;

                    default:
                        //handle upgrades where FilterCondition didn't exist
                        SettingsRepository.SetDefaultFilterCondition(ModuleId);
                        this.RootFolderViewModel = this.controller.GetRootFolder(ModuleId);
                        break;
                }
                
                var initialPath = "";
                int folderId;
                if (int.TryParse(Request["folderId"] ?? DAMState["folderId"], out folderId))
                {
                    var folder = FolderManager.Instance.GetFolder(folderId);
                    if (folder != null && folder.FolderPath.StartsWith(RootFolderViewModel.FolderPath))
                    {
                        initialPath = PathUtils.Instance.RemoveTrailingSlash(folder.FolderPath.Substring(RootFolderViewModel.FolderPath.Length));
                    }
                }

                PageSize = Request["pageSize"] ?? DAMState["pageSize"] ?? "10";
                ActiveView = Request["view"] ?? DAMState["view"] ?? "gridview";

                Page.DataBind();
                InitializeTreeViews(initialPath);
                InitializeSearchBox();
                InitializeFolderType();
                InitializeGridContextMenu();
                InitializeEmptySpaceContextMenu();

                FolderNameRegExValidator.ErrorMessage = controller.GetInvalidCharsErrorText();
                FolderNameRegExValidator.ValidationExpression = "^([^" + Regex.Escape(controller.GetInvalidChars()) + "]+)$";
            }
            catch (Exception exc) //Module failed to load
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }

        protected override void OnInit(EventArgs e)
        {
            try
            {
                base.OnInit(e);

                fileUpload.ModuleId = ModuleId;
                fileUpload.Options.Parameters.Add("isHostPortal", IsHostPortal ? "true" : "false");

                ServicesFramework.Instance.RequestAjaxScriptSupport();
                ServicesFramework.Instance.RequestAjaxAntiForgerySupport();
                JavaScript.RequestRegistration(CommonJs.DnnPlugins);

                var popupFilePath = HttpContext.Current.IsDebuggingEnabled
                                   ? "~/js/Debug/dnn.modalpopup.js"
                                   : "~/js/dnn.modalpopup.js";
                ClientResourceManager.RegisterScript(Page, popupFilePath, FileOrder.Js.DnnModalPopup);
                ClientResourceManager.RegisterScript(Page, "~/DesktopModules/DigitalAssets/ClientScripts/dnn.DigitalAssetsController.js", FileOrder.Js.DefaultPriority);

                var i = 1;
                foreach (var script in epm.GetScriptItemExtensionPoints("DigitalAssets"))
                {
                    ClientResourceManager.RegisterScript(Page, script.ScriptName, FileOrder.Js.DefaultPriority + i++);
                }

                ClientResourceManager.RegisterScript(Page, "~/DesktopModules/DigitalAssets/ClientScripts/dnn.DigitalAssets.js", FileOrder.Js.DefaultPriority + i);
                ClientResourceManager.RegisterScript(Page, "~/DesktopModules/DigitalAssets/ClientScripts/dnn.DigitalAssets.WebControls.js", FileOrder.Js.DefaultPriority + i);

                ClientResourceManager.RegisterStyleSheet(Page, "~/DesktopModules/DigitalAssets/Css/ComboBox.Default.css");
                ClientResourceManager.RegisterStyleSheet(Page, "~/DesktopModules/DigitalAssets/Css/Grid.Default.css");

                InitializeGrid();
                FolderTypeComboBox.ItemDataBound += OnItemDataBoundFolderTypeComboBox;

                MainToolBar.ModuleContext = ModuleContext;
                SelectionToolBar.ModuleContext = ModuleContext;
            }
            catch (Exception exc) //Module failed to load
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }

        public ModuleActionCollection ModuleActions
        {
            get
            {
                var actions = new ModuleActionCollection();
                if (ModulePermissionController.CanManageModule(ModuleConfiguration))
                {
                    actions.Add(GetNextActionID(), Localization.GetString("ManageFolderTypes", LocalResourceFile), "", "", "../DesktopModules/DigitalAssets/Images/manageFolderTypes.png", EditUrl("FolderMappings"), false, SecurityAccessLevel.Edit, true, false);

                    foreach (var item in epm.GetMenuItemExtensionPoints("DigitalAssets", "ModuleActions", Filter))
                    {
                        actions.Add(GetNextActionID(), item.Text, "", "", item.Icon, EditUrl(item.Value), false, SecurityAccessLevel.Edit, true, false);
                    }
                }
                else
                {
                    actions = new ModuleActionCollection();
                }

                return actions;
            }
        }

        protected void GridOnItemCreated(object sender, GridItemEventArgs e)
        {
            if (!(e.Item is GridPagerItem)) return;

            var items = new[]
            {
                new RadComboBoxItem { Text = "10", Value = "10" },
                new RadComboBoxItem { Text = "25", Value = "25" },
                new RadComboBoxItem { Text = "50", Value = "50" },
                new RadComboBoxItem { Text = "100", Value = "100" },
                new RadComboBoxItem 
                    { 
                        Text = Localization.GetString("All", LocalResourceFile), 
                        Value = int.MaxValue.ToString(CultureInfo.InvariantCulture) 
                    }
            };

            var dropDown = (RadComboBox)e.Item.FindControl("PageSizeComboBox");
            dropDown.Items.Clear();
            foreach (var item in items)
            {
                item.Attributes.Add("ownerTableViewId", e.Item.OwnerTableView.ClientID);
                dropDown.Items.Add(item);
            }
        }
    }
}