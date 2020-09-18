// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Modules.DigitalAssets
{
    using System;
    using System.Text.RegularExpressions;
    using System.Threading;
    using System.Web.UI;

    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Modules;
    using DotNetNuke.ExtensionPoints;
    using DotNetNuke.Framework.JavaScriptLibraries;
    using DotNetNuke.Modules.DigitalAssets.Components.Controllers;
    using DotNetNuke.Modules.DigitalAssets.Components.Controllers.Models;
    using DotNetNuke.Modules.DigitalAssets.Components.ExtensionPoint;
    using DotNetNuke.Modules.DigitalAssets.Services;
    using DotNetNuke.Security.Permissions;
    using DotNetNuke.Services.Exceptions;
    using DotNetNuke.Services.FileSystem;
    using DotNetNuke.UI.Skins.Controls;
    using DotNetNuke.Web.UI;

    public partial class FolderProperties : PortalModuleBase
    {
        private static readonly DigitalAssetsSettingsRepository SettingsRepository = new DigitalAssetsSettingsRepository();

        private readonly IDigitalAssetsController controller = new Factory().DigitalAssetsController;
        private FolderViewModel folderViewModel;
        private bool isRootFolder;
        private Control folderFieldsControl;

        protected bool CanManageFolder
        {
            get
            {
                return this.UserInfo.IsSuperUser || FolderPermissionController.CanManageFolder((FolderInfo)this.Folder);
            }
        }

        protected string DialogTitle
        {
            get
            {
                return string.Format(this.LocalizeString("DialogTitle"), this.folderViewModel.FolderName);
            }
        }

        protected bool IsHostPortal
        {
            get
            {
                return this.IsHostMenu || this.controller.GetCurrentPortalId(this.ModuleId) == Null.NullInteger;
            }
        }

        protected IFolderInfo Folder { get; private set; }

        protected bool HasFullControl { get; private set; }

        protected override void OnInit(EventArgs e)
        {
            try
            {
                base.OnInit(e);

                JavaScript.RequestRegistration(CommonJs.DnnPlugins);

                var folderId = Convert.ToInt32(this.Request.Params["FolderId"]);
                this.Folder = FolderManager.Instance.GetFolder(folderId);
                this.HasFullControl = this.UserInfo.IsSuperUser || FolderPermissionController.HasFolderPermission(this.Folder.FolderPermissions, "FULLCONTROL");

                FolderViewModel rootFolder;
                switch (SettingsRepository.GetMode(this.ModuleId))
                {
                    case DigitalAssestsMode.Group:
                        var groupId = Convert.ToInt32(this.Request.Params["GroupId"]);
                        rootFolder = this.controller.GetGroupFolder(groupId, this.PortalSettings);
                        if (rootFolder == null)
                        {
                            throw new Exception("Invalid group folder");
                        }

                        break;

                    case DigitalAssestsMode.User:
                        rootFolder = this.controller.GetUserFolder(this.PortalSettings.UserInfo);
                        break;

                    default:
                        rootFolder = this.controller.GetRootFolder(this.ModuleId);
                        break;
                }

                this.isRootFolder = rootFolder.FolderID == folderId;
                this.folderViewModel = this.isRootFolder ? rootFolder : this.controller.GetFolder(folderId);

                // Setup controls
                this.CancelButton.Click += this.OnCancelClick;
                this.SaveButton.Click += this.OnSaveClick;
                this.PrepareFolderPreviewInfo();
                this.cmdCopyPerm.Click += this.cmdCopyPerm_Click;

                var mef = new ExtensionPointManager();
                var folderFieldsExtension = mef.GetUserControlExtensionPointFirstByPriority("DigitalAssets", "FolderFieldsControlExtensionPoint");
                if (folderFieldsExtension != null)
                {
                    this.folderFieldsControl = this.Page.LoadControl(folderFieldsExtension.UserControlSrc);
                    this.folderFieldsControl.ID = this.folderFieldsControl.GetType().BaseType.Name;
                    this.FolderDynamicFieldsContainer.Controls.Add(this.folderFieldsControl);
                    var fieldsControl = this.folderFieldsControl as IFieldsControl;
                    if (fieldsControl != null)
                    {
                        fieldsControl.SetController(this.controller);
                        fieldsControl.SetItemViewModel(new ItemViewModel
                        {
                            ItemID = this.folderViewModel.FolderID,
                            IsFolder = true,
                            PortalID = this.folderViewModel.PortalID,
                            ItemName = this.folderViewModel.FolderName,
                        });
                    }
                }
            }
            catch (Exception exc)
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }

        protected override void OnLoad(EventArgs e)
        {
            try
            {
                if (!this.Page.IsPostBack)
                {
                    this.SetupPermissionGrid();
                    this.PrepareFolderProperties();
                    this.SetPropertiesAvailability(FolderPermissionController.CanManageFolder((FolderInfo)this.Folder));
                }

                if (!FolderPermissionController.CanViewFolder((FolderInfo)this.Folder))
                {
                    this.SaveButton.Visible = false;
                    this.SetPropertiesVisibility(false);
                    UI.Skins.Skin.AddModuleMessage(this, this.LocalizeString("UserCannotReadFolderError"), ModuleMessage.ModuleMessageType.RedError);
                }
                else
                {
                    this.SaveButton.Visible = FolderPermissionController.CanViewFolder((FolderInfo)this.Folder) && FolderPermissionController.CanManageFolder((FolderInfo)this.Folder);
                }
            }
            catch (DotNetNukeException dnnex)
            {
                UI.Skins.Skin.AddModuleMessage(this, dnnex.Message, ModuleMessage.ModuleMessageType.RedError);
            }
            catch (Exception exc)
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }

        private void OnSaveClick(object sender, EventArgs e)
        {
            try
            {
                if (!this.Page.IsValid)
                {
                    return;
                }

                this.SaveFolderProperties();

                this.SavePermissions();
                this.Page.CloseClientDialog(true);
            }
            catch (ThreadAbortException)
            {
            }
            catch (DotNetNukeException dnnex)
            {
                UI.Skins.Skin.AddModuleMessage(this, dnnex.Message, ModuleMessage.ModuleMessageType.RedError);
            }
            catch (Exception ex)
            {
                Exceptions.LogException(ex);
                UI.Skins.Skin.AddModuleMessage(this, ex.Message, ModuleMessage.ModuleMessageType.RedError);
            }
        }

        private void SaveFolderProperties()
        {
            if (!this.CanManageFolder)
            {
                throw new DotNetNukeException(this.LocalizeString("UserCannotEditFolderError"));
            }

            if (!this.isRootFolder)
            {
                this.controller.RenameFolder(this.folderViewModel.FolderID, this.FolderNameInput.Text);
            }

            var fieldsControl = this.folderFieldsControl as IFieldsControl;
            if (fieldsControl != null)
            {
                this.Folder = (IFolderInfo)fieldsControl.SaveProperties();
            }
        }

        private void SavePermissions()
        {
            if (!this.CanManageFolder)
            {
                throw new DotNetNukeException(this.LocalizeString("UserCannotChangePermissionsError"));
            }

            this.Folder = FolderManager.Instance.GetFolder(this.Folder.FolderID);
            this.Folder.FolderPermissions.Clear();
            this.Folder.FolderPermissions.AddRange(this.PermissionsGrid.Permissions);
            FolderPermissionController.SaveFolderPermissions(this.Folder);
        }

        private void OnCancelClick(object sender, EventArgs e)
        {
            this.Page.CloseClientDialog(false);
        }

        private void cmdCopyPerm_Click(object sender, EventArgs e)
        {
            try
            {
                FolderPermissionController.CopyPermissionsToSubfolders(this.Folder, this.PermissionsGrid.Permissions);
                UI.Skins.Skin.AddModuleMessage(this, this.LocalizeString("PermissionsCopied"), ModuleMessage.ModuleMessageType.GreenSuccess);
            }
            catch (Exception ex)
            {
                UI.Skins.Skin.AddModuleMessage(this, this.LocalizeString("PermissionCopyError"), ModuleMessage.ModuleMessageType.RedError);
                Exceptions.ProcessModuleLoadException(this, ex);
            }
        }

        private void SetPropertiesAvailability(bool availability)
        {
            this.FolderNameInput.Enabled = (!this.isRootFolder) && availability;
            var fieldsControl = this.folderFieldsControl as IFieldsControl;
            if (fieldsControl != null)
            {
                fieldsControl.SetPropertiesAvailability(availability);
            }
        }

        private void SetPropertiesVisibility(bool visibility)
        {
            this.FolderNameInput.Visible = visibility;
            this.FolderTypeLiteral.Visible = visibility;
            this.FolderInfoPreviewPanel.Visible = visibility;
            var fieldsControl = this.folderFieldsControl as IFieldsControl;
            if (fieldsControl != null)
            {
                fieldsControl.SetPropertiesVisibility(visibility);
            }
        }

        private void PrepareFolderProperties()
        {
            this.FolderNameInput.Text = this.folderViewModel.FolderName;
            this.FolderTypeLiteral.Text = FolderMappingController.Instance.GetFolderMapping(this.folderViewModel.FolderMappingID).MappingName;

            this.FolderNameInvalidCharactersValidator.ValidationExpression = "^([^" + Regex.Escape(this.controller.GetInvalidChars()) + "]+)$";
            this.FolderNameInvalidCharactersValidator.ErrorMessage = this.controller.GetInvalidCharsErrorText();

            var fieldsControl = this.folderFieldsControl as IFieldsControl;
            if (fieldsControl != null)
            {
                fieldsControl.PrepareProperties();
            }
        }

        private void PrepareFolderPreviewInfo()
        {
            var folderPreviewPanel = (PreviewPanelControl)this.FolderInfoPreviewPanel;
            if (folderPreviewPanel != null)
            {
                folderPreviewPanel.SetPreviewInfo(this.controller.GetFolderPreviewInfo(this.Folder));
            }
        }

        private void SetupPermissionGrid()
        {
            this.PermissionsGrid.FolderPath = this.Folder.FolderPath;
            this.PermissionsGrid.Visible = this.HasFullControl && !this.IsHostPortal;
        }
    }
}
