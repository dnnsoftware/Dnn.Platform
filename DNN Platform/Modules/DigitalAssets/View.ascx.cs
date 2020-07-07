// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Modules.DigitalAssets
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.Globalization;
    using System.Linq;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Web;

    using DotNetNuke.Abstractions;
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
    using Microsoft.Extensions.DependencyInjection;
    using Telerik.Web.UI;

    public partial class View : PortalModuleBase, IActionable
    {
        private static readonly DigitalAssetsSettingsRepository SettingsRepository = new DigitalAssetsSettingsRepository();

        private readonly IDigitalAssetsController controller;
        private readonly ExtensionPointManager epm = new ExtensionPointManager();

        private readonly INavigationManager _navigationManager;
        private NameValueCollection damState;

        public View()
        {
            this.controller = new Factory().DigitalAssetsController;
            this._navigationManager = this.DependencyProvider.GetRequiredService<INavigationManager>();
        }

        public ModuleActionCollection ModuleActions
        {
            get
            {
                var actions = new ModuleActionCollection();
                if (ModulePermissionController.CanManageModule(this.ModuleConfiguration))
                {
                    actions.Add(this.GetNextActionID(), Localization.GetString("ManageFolderTypes", this.LocalResourceFile), string.Empty, string.Empty, "../DesktopModules/DigitalAssets/Images/manageFolderTypes.png", this.EditUrl("FolderMappings"), false, SecurityAccessLevel.Edit, true, false);

                    foreach (var item in this.epm.GetMenuItemExtensionPoints("DigitalAssets", "ModuleActions", this.Filter))
                    {
                        actions.Add(this.GetNextActionID(), item.Text, string.Empty, string.Empty, item.Icon, this.EditUrl(item.Value), false, SecurityAccessLevel.Edit, true, false);
                    }
                }
                else
                {
                    actions = new ModuleActionCollection();
                }

                return actions;
            }
        }

        protected int InitialTab
        {
            get
            {
                return this.controller.GetInitialTab(this.Request.Params, this.DAMState);
            }
        }

        protected bool IsHostPortal
        {
            get
            {
                return this.IsHostMenu || this.controller.GetCurrentPortalId(this.ModuleId) == Null.NullInteger;
            }
        }

        protected string InvalidCharacters
        {
            get
            {
                return GetNoControlCharsString(this.controller.GetInvalidChars());
            }
        }

        protected string InvalidCharactersErrorText
        {
            get
            {
                return this.controller.GetInvalidCharsErrorText();
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
                var url = this._navigationManager.NavigateURL(this.TabId, "ControlKey", "mid=" + this.ModuleId, "ReturnUrl=" + this.Server.UrlEncode(this._navigationManager.NavigateURL()));

                // append popUp parameter
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
                var defaultFolderTypeId = this.controller.GetDefaultFolderTypeId(this.ModuleId);
                return defaultFolderTypeId.HasValue ? defaultFolderTypeId.ToString() : string.Empty;
            }
        }

        protected bool FilteredContent
        {
            get
            {
                return SettingsRepository.GetSubfolderFilter(this.ModuleId) != SubfolderFilter.IncludeSubfoldersFolderStructure;
            }
        }

        protected string PageSize { get; private set; }

        protected string ActiveView { get; private set; }

        protected FolderViewModel RootFolderViewModel { get; private set; }

        private IExtensionPointFilter Filter
        {
            get
            {
                return new CompositeFilter()
                    .And(new FilterByHostMenu(this.IsHostPortal))
                    .And(new FilterByUnauthenticated(HttpContext.Current.Request.IsAuthenticated));
            }
        }

        private NameValueCollection DAMState
        {
            get
            {
                if (this.damState == null)
                {
                    var stateCookie = this.Request.Cookies["damState-" + this.UserId];
                    this.damState = HttpUtility.ParseQueryString(Uri.UnescapeDataString(stateCookie != null ? stateCookie.Value : string.Empty));
                }

                return this.damState;
            }
        }

        protected override void OnLoad(EventArgs e)
        {
            try
            {
                base.OnLoad(e);

                // if (IsPostBack) return;
                switch (SettingsRepository.GetMode(this.ModuleId))
                {
                    case DigitalAssestsMode.Group:
                        int groupId;
                        if (string.IsNullOrEmpty(this.Request["groupId"]) || !int.TryParse(this.Request["groupId"], out groupId))
                        {
                            Skin.AddModuleMessage(this, Localization.GetString("InvalidGroup.Error", this.LocalResourceFile), ModuleMessage.ModuleMessageType.RedError);
                            return;
                        }

                        var groupFolder = this.controller.GetGroupFolder(groupId, this.PortalSettings);
                        if (groupFolder == null)
                        {
                            Skin.AddModuleMessage(this, Localization.GetString("InvalidGroup.Error", this.LocalResourceFile), ModuleMessage.ModuleMessageType.RedError);
                            return;
                        }

                        this.RootFolderViewModel = groupFolder;
                        break;

                    case DigitalAssestsMode.User:
                        if (this.PortalSettings.UserId == Null.NullInteger)
                        {
                            Skin.AddModuleMessage(this, Localization.GetString("InvalidUser.Error", this.LocalResourceFile), ModuleMessage.ModuleMessageType.RedError);
                            return;
                        }

                        this.RootFolderViewModel = this.controller.GetUserFolder(this.PortalSettings.UserInfo);
                        break;

                    default:
                        // handle upgrades where FilterCondition didn't exist
                        SettingsRepository.SetDefaultFilterCondition(this.ModuleId);
                        this.RootFolderViewModel = this.controller.GetRootFolder(this.ModuleId);
                        break;
                }

                var initialPath = string.Empty;
                int folderId;
                if (int.TryParse(this.Request["folderId"] ?? this.DAMState["folderId"], out folderId))
                {
                    var folder = FolderManager.Instance.GetFolder(folderId);
                    if (folder != null && folder.FolderPath.StartsWith(this.RootFolderViewModel.FolderPath))
                    {
                        initialPath = PathUtils.Instance.RemoveTrailingSlash(folder.FolderPath.Substring(this.RootFolderViewModel.FolderPath.Length));
                    }
                }

                this.PageSize = this.Request["pageSize"] ?? this.DAMState["pageSize"] ?? "10";
                this.ActiveView = this.Request["view"] ?? this.DAMState["view"] ?? "gridview";

                this.Page.DataBind();
                this.InitializeTreeViews(initialPath);
                this.InitializeSearchBox();
                this.InitializeFolderType();
                this.InitializeGridContextMenu();
                this.InitializeEmptySpaceContextMenu();

                this.FolderNameRegExValidator.ErrorMessage = this.controller.GetInvalidCharsErrorText();
                this.FolderNameRegExValidator.ValidationExpression = "^([^" + Regex.Escape(this.controller.GetInvalidChars()) + "]+)$";
            }
            catch (Exception exc) // Module failed to load
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }

        protected override void OnInit(EventArgs e)
        {
            try
            {
                base.OnInit(e);

                this.fileUpload.ModuleId = this.ModuleId;
                this.fileUpload.Options.Parameters.Add("isHostPortal", this.IsHostPortal ? "true" : "false");

                ServicesFramework.Instance.RequestAjaxScriptSupport();
                ServicesFramework.Instance.RequestAjaxAntiForgerySupport();
                JavaScript.RequestRegistration(CommonJs.DnnPlugins);

                // register the telerik core js manually
                var telerikCoreJs = this.Page.ClientScript.GetWebResourceUrl(typeof(RadGrid), "Telerik.Web.UI.Common.Core.js");
                ClientResourceManager.RegisterScript(this.Page, telerikCoreJs, FileOrder.Js.jQuery + 3);

                var popupFilePath = HttpContext.Current.IsDebuggingEnabled
                                   ? "~/js/Debug/dnn.modalpopup.js"
                                   : "~/js/dnn.modalpopup.js";
                ClientResourceManager.RegisterScript(this.Page, popupFilePath, FileOrder.Js.DnnModalPopup);
                ClientResourceManager.RegisterScript(this.Page, "~/DesktopModules/DigitalAssets/ClientScripts/dnn.DigitalAssetsController.js", FileOrder.Js.DefaultPriority);

                var i = 1;
                foreach (var script in this.epm.GetScriptItemExtensionPoints("DigitalAssets"))
                {
                    ClientResourceManager.RegisterScript(this.Page, script.ScriptName, FileOrder.Js.DefaultPriority + i++);
                }

                ClientResourceManager.RegisterScript(this.Page, "~/DesktopModules/DigitalAssets/ClientScripts/dnn.DigitalAssets.js", FileOrder.Js.DefaultPriority + i);

                this.InitializeGrid();
                this.FolderTypeComboBox.ItemDataBound += this.OnItemDataBoundFolderTypeComboBox;

                this.MainToolBar.ModuleContext = this.ModuleContext;
                this.SelectionToolBar.ModuleContext = this.ModuleContext;
            }
            catch (Exception exc) // Module failed to load
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }

        protected void GridOnItemCreated(object sender, GridItemEventArgs e)
        {
            if (!(e.Item is GridPagerItem))
            {
                return;
            }

            var items = new[]
            {
                new RadComboBoxItem { Text = "10", Value = "10" },
                new RadComboBoxItem { Text = "25", Value = "25" },
                new RadComboBoxItem { Text = "50", Value = "50" },
                new RadComboBoxItem { Text = "100", Value = "100" },
                new RadComboBoxItem
                    {
                        Text = Localization.GetString("All", this.LocalResourceFile),
                        Value = int.MaxValue.ToString(CultureInfo.InvariantCulture)
                    },
            };

            var dropDown = (RadComboBox)e.Item.FindControl("PageSizeComboBox");
            dropDown.Items.Clear();
            foreach (var item in items)
            {
                item.Attributes.Add("ownerTableViewId", e.Item.OwnerTableView.ClientID);
                dropDown.Items.Add(item);
            }
        }

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
            this.FolderTypeComboBox.DataSource = this.controller.GetFolderMappings(this.ModuleId);
            this.FolderTypeComboBox.DataBind();
        }

        private void InitializeGrid()
        {
            this.Grid.MasterTableView.PagerStyle.PrevPageToolTip = this.LocalizeString("PagerPreviousPage.ToolTip");
            this.Grid.MasterTableView.PagerStyle.NextPageToolTip = this.LocalizeString("PagerNextPage.ToolTip");
            this.Grid.MasterTableView.PagerStyle.FirstPageToolTip = this.LocalizeString("PagerFirstPage.ToolTip");
            this.Grid.MasterTableView.PagerStyle.LastPageToolTip = this.LocalizeString("PagerLastPage.ToolTip");
            this.Grid.MasterTableView.PagerStyle.PageSizeLabelText = this.LocalizeString("PagerPageSize.Text");

            foreach (var columnExtension in this.epm.GetGridColumnExtensionPoints("DigitalAssets", "GridColumns", this.Filter))
            {
                var column = new DnnGridBoundColumn
                {
                    HeaderText = columnExtension.HeaderText,
                    DataField = columnExtension.DataField,
                    UniqueName = columnExtension.UniqueName,
                    ReadOnly = columnExtension.ReadOnly,
                    Reorderable = columnExtension.Reorderable,
                    SortExpression = columnExtension.SortExpression,
                    HeaderTooltip = columnExtension.HeaderText,
                };
                column.HeaderStyle.Width = columnExtension.HeaderStyleWidth;

                var index = Math.Min(columnExtension.ColumnAt, this.Grid.Columns.Count - 1);
                this.Grid.Columns.AddAt(index, column);
            }
        }

        private void LoadSubfolders(DnnTreeNode node, int folderId, string nextFolderName, out DnnTreeNode nextNode, out int nextFolderId)
        {
            nextNode = null;
            nextFolderId = 0;
            var folders = this.controller.GetFolders(this.ModuleId, folderId);
            foreach (var folder in folders)
            {
                var hasViewPermissions = this.HasViewPermissions(folder.Permissions);
                var newNode = this.CreateNodeFromFolder(folder);
                this.SetupNodeAttributes(newNode, folder.Permissions, folder);

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
            var rootFolder = this.RootFolderViewModel;

            var rootNode = this.CreateNodeFromFolder(rootFolder);
            rootNode.Selected = true;
            rootNode.Expanded = true;

            var folderId = rootFolder.FolderID;
            var nextNode = rootNode;
            foreach (var folderName in initialPath.Split('/'))
            {
                this.LoadSubfolders(nextNode, folderId, folderName, out nextNode, out folderId);
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

            this.SetupNodeAttributes(rootNode, this.GetPermissionsForRootFolder(rootFolder.Permissions), rootFolder);

            this.FolderTreeView.Nodes.Clear();
            this.DestinationTreeView.Nodes.Clear();

            this.FolderTreeView.Nodes.Add(rootNode);
            this.DestinationTreeView.Nodes.Add(rootNode.Clone());

            this.InitializeTreeViewContextMenu();
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
            this.SetExpandable(node, folder.HasChildren && this.HasViewPermissions(folder.Permissions));
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
            this.MainContextMenu.Items.AddRange(new[]
            {
                new DnnMenuItem
                    {
                        Text = Localization.GetString("CreateFolder", this.LocalResourceFile),
                        Value = "NewFolder",
                        CssClass = "permission_ADD disabledIfFiltered",
                        ImageUrl = IconController.IconURL("FolderCreate", "16x16", "Gray"),
                    },
                new DnnMenuItem
                    {
                        Text = Localization.GetString("RefreshFolder", this.LocalResourceFile),
                        Value = "RefreshFolder",
                        CssClass = "permission_BROWSE permission_READ",
                        ImageUrl = IconController.IconURL("FolderRefreshSync", "16x16", "Gray"),
                    },
                new DnnMenuItem
                    {
                        Text = Localization.GetString("RenameFolder", this.LocalResourceFile),
                        Value = "RenameFolder",
                        CssClass = "permission_MANAGE",
                        ImageUrl = IconController.IconURL("FileRename", "16x16", "Black"),
                    },
                new DnnMenuItem
                    {
                        Text = Localization.GetString("Move", this.LocalResourceFile),
                        Value = "Move",
                        CssClass = "permission_COPY",
                        ImageUrl = IconController.IconURL("FileMove", "16x16", "Black"),
                    },
                new DnnMenuItem
                    {
                        Text = Localization.GetString("DeleteFolder", this.LocalResourceFile),
                        Value = "DeleteFolder",
                        CssClass = "permission_DELETE",
                        ImageUrl = IconController.IconURL("FileDelete", "16x16", "Black"),
                    },
                new DnnMenuItem
                    {
                        Text = Localization.GetString("UnlinkFolder", this.LocalResourceFile),
                        Value = "UnlinkFolder",
                        CssClass = "permission_DELETE",
                        ImageUrl = IconController.IconURL("UnLink", "16x16", "Black"),
                    },
                new DnnMenuItem
                    {
                        Text = Localization.GetString("ViewFolderProperties", this.LocalResourceFile),
                        Value = "Properties",
                        CssClass = "permission_READ",
                        ImageUrl = IconController.IconURL("ViewProperties", "16x16", "CtxtMn"),
                    },
            });

            // Dnn Menu Item Extension Point
            foreach (var menuItem in this.epm.GetMenuItemExtensionPoints("DigitalAssets", "TreeViewContextMenu", this.Filter))
            {
                this.MainContextMenu.Items.Add(new DnnMenuItem
                {
                    Text = menuItem.Text,
                    Value = menuItem.Value,
                    CssClass = menuItem.CssClass,
                    ImageUrl = menuItem.Icon,
                });
            }
        }

        private void InitializeSearchBox()
        {
            var extension = this.epm.GetUserControlExtensionPointFirstByPriority("DigitalAssets", "SearchBoxExtensionPoint");
            var searchControl = (PortalModuleBase)this.Page.LoadControl(extension.UserControlSrc);
            searchControl.ModuleConfiguration = this.ModuleConfiguration;

            searchControl.ID = searchControl.GetType().BaseType.Name;
            this.SearchBoxPanel.Controls.Add(searchControl);
        }

        private void InitializeGridContextMenu()
        {
            this.GridMenu.Items.AddRange(new[]
                {
                    new DnnMenuItem
                    {
                        Text = Localization.GetString("Download", this.LocalResourceFile),
                        Value = "Download",
                        CssClass = "permission_READ",
                        ImageUrl = IconController.IconURL("FileDownload", "16x16", "Black"),
                    },
                    new DnnMenuItem
                    {
                        Text = Localization.GetString("Rename", this.LocalResourceFile),
                        Value = "Rename",
                        CssClass = "permission_MANAGE singleItem",
                        ImageUrl = IconController.IconURL("FileRename", "16x16", "Black"),
                    },
                    new DnnMenuItem
                    {
                        Text = Localization.GetString("Copy", this.LocalResourceFile),
                        Value = "Copy",
                        CssClass = "permission_COPY onlyFiles",
                        ImageUrl = IconController.IconURL("FileCopy", "16x16", "Black"),
                    },
                    new DnnMenuItem
                    {
                        Text = Localization.GetString("Move", this.LocalResourceFile),
                        Value = "Move",
                        CssClass = "permission_COPY disabledIfFiltered",
                        ImageUrl = IconController.IconURL("FileMove", "16x16", "Black"),
                    },
                    new DnnMenuItem
                    {
                        Text = Localization.GetString("Delete", this.LocalResourceFile),
                        Value = "Delete",
                        CssClass = "permission_DELETE",
                        ImageUrl = IconController.IconURL("FileDelete", "16x16", "Black"),
                    },
                    new DnnMenuItem
                    {
                        Text = Localization.GetString("Unlink", this.LocalResourceFile),
                        Value = "Unlink",
                        CssClass = "permission_DELETE singleItem onlyFolders",
                        ImageUrl = IconController.IconURL("UnLink", "16x16", "Black"),
                    },
                    new DnnMenuItem
                    {
                        Text = Localization.GetString("UnzipFile", this.LocalResourceFile),
                        Value = "UnzipFile",
                        CssClass = "permission_MANAGE singleItem onlyFiles",
                        ImageUrl = IconController.IconURL("Unzip", "16x16", "Gray"),
                    },
                    new DnnMenuItem
                    {
                        Text = Localization.GetString("ViewProperties", this.LocalResourceFile),
                        Value = "Properties",
                        CssClass = "permission_READ singleItem",
                        ImageUrl = IconController.IconURL("ViewProperties", "16x16", "CtxtMn"),
                    },
                    new DnnMenuItem
                    {
                        Text = Localization.GetString("GetUrl", this.LocalResourceFile),
                        Value = "GetUrl",
                        CssClass = "permission_READ singleItem onlyFiles",
                        ImageUrl = IconController.IconURL("FileLink", "16x16", "Black")
                    },
                });

            // Dnn Menu Item Extension Point
            foreach (var menuItem in this.epm.GetMenuItemExtensionPoints("DigitalAssets", "GridContextMenu", this.Filter))
            {
                this.GridMenu.Items.Add(new DnnMenuItem
                {
                    Text = menuItem.Text,
                    Value = menuItem.Value,
                    CssClass = menuItem.CssClass,
                    ImageUrl = menuItem.Icon,
                });
            }
        }

        private void InitializeEmptySpaceContextMenu()
        {
            this.EmptySpaceMenu.Items.AddRange(new[]
            {
                new DnnMenuItem
                    {
                        Text = Localization.GetString("CreateFolder", this.LocalResourceFile),
                        Value = "NewFolder",
                        CssClass = "permission_ADD disabledIfFiltered",
                        ImageUrl = IconController.IconURL("FolderCreate", "16x16", "Gray"),
                    },
                new DnnMenuItem
                    {
                        Text = Localization.GetString("RefreshFolder", this.LocalResourceFile),
                        Value = "RefreshFolder",
                        CssClass = "permission_READ permission_BROWSE",
                        ImageUrl = IconController.IconURL("FolderRefreshSync", "16x16", "Gray"),
                    },
                new DnnMenuItem
                    {
                        Text = Localization.GetString("UploadFiles.Title", this.LocalResourceFile),
                        Value = "UploadFiles",
                        CssClass = "permission_ADD",
                        ImageUrl = IconController.IconURL("UploadFiles", "16x16", "Gray"),
                    },
                new DnnMenuItem
                    {
                        Text = Localization.GetString("ViewFolderProperties", this.LocalResourceFile),
                        Value = "Properties",
                        CssClass = "permission_READ",
                        ImageUrl = IconController.IconURL("ViewProperties", "16x16", "CtxtMn"),
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
    }
}
