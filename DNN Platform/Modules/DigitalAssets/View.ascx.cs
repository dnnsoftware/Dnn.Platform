#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2013
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
using DotNetNuke.Modules.DigitalAssets.Components.Controllers;
using DotNetNuke.Modules.DigitalAssets.Components.Controllers.Models;
using DotNetNuke.Modules.DigitalAssets.Services;
using DotNetNuke.Security;
using DotNetNuke.Security.Permissions;
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

        public View()
        {
            controller = new Factory().DigitalAssetsController;
        }

        private IExtensionPointFilter Filter
        {
            get
            {
                return new CompositeFilter()
                    .And(new FilterByHostMenu(IsHostMenu))
                    .And(new FilterByUnauthenticated(HttpContext.Current.Request.IsAuthenticated));
            }
        }

        #region Protected Properties
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
                return Globals.NavigateURL(TabId, "ControlKey", "mid=" + ModuleId, "popUp=true", "ReturnUrl=" + Server.UrlEncode(Globals.NavigateURL()));
            }
        }

        protected IEnumerable<string> DefaultFolderProviderValues
        {
            get
            {
                return new List<string>
                    {
                        FolderMappingController.Instance.GetFolderMapping(controller.CurrentPortalId, "Standard").FolderMappingID.ToString(CultureInfo.InvariantCulture),
                        FolderMappingController.Instance.GetFolderMapping(controller.CurrentPortalId, "Secure").FolderMappingID.ToString(CultureInfo.InvariantCulture),
                        FolderMappingController.Instance.GetFolderMapping(controller.CurrentPortalId, "Database").FolderMappingID.ToString(CultureInfo.InvariantCulture)
                    };
            }
        }

        protected string DefaultFolderTypeId
        {
            get
            {
                var defaultFolderTypeId = SettingsRepository.GetDefaultFolderTypeId(ModuleId);
                return defaultFolderTypeId.HasValue ? defaultFolderTypeId.ToString() : "";
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
            FolderTypeComboBox.DataSource = controller.GetFolderMappings();
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
            var folders = controller.GetFolders(folderId);
            foreach (var folder in folders)
            {
                var hasViewPermissions = HasViewPermissions(folder.Permissions);
                var newNode = new DnnTreeNode
                {
                    ExpandMode = folder.HasChildren && hasViewPermissions ? TreeNodeExpandMode.WebService : TreeNodeExpandMode.ClientSide,
                    Text = folder.FolderName,
                    ImageUrl = folder.IconUrl,
                    Value = folder.FolderID.ToString(CultureInfo.InvariantCulture),
                    Category = folder.FolderMappingID.ToString(CultureInfo.InvariantCulture),                    
                };

                // Setup attributes
                newNode.Attributes.Add("permissions", folder.Permissions.ToJson());
                foreach (var attribute in folder.Attributes)
                {
                    newNode.Attributes.Add(attribute.Key, attribute.Value.ToJson());
                }

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
            var rootNode = new DnnTreeNode
            {
                ExpandMode = HasViewPermissions(rootFolder.Permissions) ? TreeNodeExpandMode.WebService : TreeNodeExpandMode.ClientSide,
                Text = rootFolder.FolderName,
                ImageUrl = rootFolder.IconUrl,
                Value = rootFolder.FolderID.ToString(CultureInfo.InvariantCulture),
                Category = rootFolder.FolderMappingID.ToString(CultureInfo.InvariantCulture),
                Selected = true,
                Expanded = true
            };      

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
                rootNode.ExpandMode = TreeNodeExpandMode.ClientSide;
                rootNode.Selected = false;                    
            }

            // Setup attributes
            rootNode.Attributes.Add("permissions", GetPermissionsForRootFolder(rootFolder.Permissions).ToJson());
            foreach (var attribute in rootFolder.Attributes)
            {
                rootNode.Attributes.Add(attribute.Key, attribute.Value.ToJson());
            }

            FolderTreeView.Nodes.Add(rootNode);
            DestinationTreeView.Nodes.Add(rootNode.Clone());

            InitializeTreeViewContextMenu();
        }

        private void InitializeTreeViewContextMenu()
        {
            MainContextMenu.Items.AddRange(new[]
            {
                new DnnMenuItem
                    {
                        Text = Localization.GetString("CreateFolder", LocalResourceFile),
                        Value = "NewFolder",
                        CssClass = "permission_ADD",
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
                        Text = Localization.GetString("ViewFolderProperties", LocalResourceFile),
                        Value = "Properties",
                        CssClass = "permission_READ permission_BROWSE",
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
                        CssClass = "permission_READ permission_BROWSE",
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
                        CssClass = "permission_COPY",
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
                        Text = Localization.GetString("GetUrl", LocalResourceFile),
                        Value = "GetUrl",
                        CssClass = "permission_READ permission_BROWSE singleItem onlyFiles",
                        ImageUrl = IconController.IconURL("FileLink", "16x16", "Black")
                    }, 
                new DnnMenuItem
                    {
                        Text = Localization.GetString("UnzipFile", LocalResourceFile),
                        Value = "UnzipFile",
                        CssClass = "permission_READ permission_BROWSE singleItem onlyFiles",
                        ImageUrl = IconController.IconURL("Unzip", "16x16", "Gray")
                    },
                new DnnMenuItem
                    {
                        Text = Localization.GetString("ViewProperties", LocalResourceFile),
                        Value = "Properties",
                        CssClass = "permission_READ permission_BROWSE singleItem",
                        ImageUrl = IconController.IconURL("ViewProperties", "16x16", "CtxtMn")
                    },                        
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
                        CssClass = "permission_ADD",
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
                        CssClass = "permission_READ permission_BROWSE",
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

                if (IsPostBack) return;

                if (SettingsRepository.IsGroupMode(ModuleId))
                {
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
                }
                else
                {
                    var rootFolderId = SettingsRepository.GetRootFolderId(ModuleId);
                    this.RootFolderViewModel = rootFolderId.HasValue ? this.controller.GetFolder(rootFolderId.Value) : this.controller.GetRootFolder();
                }

                var stateCookie = Request.Cookies["damState-" + UserId];
                var state = HttpUtility.ParseQueryString(Uri.UnescapeDataString(stateCookie != null ? stateCookie.Value : ""));

                var initialPath = "";
                int folderId;
                if (int.TryParse(Request["folderId"] ?? state["folderId"], out folderId))
                {
                    var folder = FolderManager.Instance.GetFolder(folderId);
                    if (folder != null && folder.FolderPath.StartsWith(RootFolderViewModel.FolderPath))
                    {
                        initialPath = PathUtils.Instance.RemoveTrailingSlash(folder.FolderPath.Substring(RootFolderViewModel.FolderPath.Length));
                    }
                }

                PageSize = Request["pageSize"] ?? state["pageSize"] ?? "10";
                ActiveView = Request["view"] ?? state["view"] ?? "gridview";

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

                ServicesFramework.Instance.RequestAjaxScriptSupport();
                ServicesFramework.Instance.RequestAjaxAntiForgerySupport();
                jQuery.RequestDnnPluginsRegistration();
                jQuery.RegisterFileUpload(Page);

                ClientResourceManager.RegisterScript(Page, "~/js/dnn.modalpopup.js", FileOrder.Js.DnnModalPopup);
                ClientResourceManager.RegisterScript(Page, "~/DesktopModules/DigitalAssets/ClientScripts/dnn.DigitalAssets.FileUpload.js", FileOrder.Js.DefaultPriority);
                ClientResourceManager.RegisterScript(Page, "~/DesktopModules/DigitalAssets/ClientScripts/dnn.DigitalAssetsController.js", FileOrder.Js.DefaultPriority);

                int i = 1;
                foreach (var script in epm.GetScriptItemExtensionPoints("DigitalAssets"))
                {
                    ClientResourceManager.RegisterScript(Page, script.ScriptName, FileOrder.Js.DefaultPriority + i++);
                }

                ClientResourceManager.RegisterScript(Page, "~/DesktopModules/DigitalAssets/ClientScripts/dnn.DigitalAssets.js", FileOrder.Js.DefaultPriority + i);

                InitializeGrid();
                FolderTypeComboBox.ItemDataBound += OnItemDataBoundFolderTypeComboBox;
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
            if (e.Item is GridPagerItem)
            {
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
}