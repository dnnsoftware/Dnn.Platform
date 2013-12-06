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
using System.Text.RegularExpressions;
using System.Threading;
using System.Web.UI;

using DotNetNuke.Entities.Modules;
using DotNetNuke.ExtensionPoints;
using DotNetNuke.Framework;
using DotNetNuke.Modules.DigitalAssets.Components.Controllers;
using DotNetNuke.Modules.DigitalAssets.Components.Controllers.Models;
using DotNetNuke.Modules.DigitalAssets.Components.ExtensionPoint;
using DotNetNuke.Modules.DigitalAssets.Services;
using DotNetNuke.Security.Permissions;
using DotNetNuke.Services.Exceptions;
using DotNetNuke.Services.FileSystem;
using DotNetNuke.UI.Skins.Controls;
using DotNetNuke.Web.UI;

namespace DotNetNuke.Modules.DigitalAssets
{
    public partial class FolderProperties : PortalModuleBase
    {
        private readonly IDigitalAssetsController controller = (new Factory()).DigitalAssetsController;
        private FolderViewModel folderViewModel;
        private bool isRootFolder;
        private Control folderFieldsControl;

        protected IFolderInfo Folder { get; private set; }

        protected bool CanManageFolder
        {
            get
            {
                return UserInfo.IsSuperUser || FolderPermissionController.CanManageFolder((FolderInfo)Folder);
            }
        }

        protected bool HasFullControl
        {
            get
            {
                return UserInfo.IsSuperUser || FolderPermissionController.HasFolderPermission(Folder.FolderPermissions, "FULLCONTROL");
            }
        }

        protected string DialogTitle
        {
            get
            {
                return string.Format(LocalizeString("DialogTitle"), folderViewModel.FolderName);
            }
        }

        protected override void OnInit(EventArgs e)
        {
            try
            {
                base.OnInit(e);

                jQuery.RequestDnnPluginsRegistration();

                var folderId = Convert.ToInt32(Request.Params["FolderId"]);
                Folder = FolderManager.Instance.GetFolder(folderId);
                if (string.IsNullOrEmpty(Folder.FolderPath))
                {
                    folderViewModel = controller.GetRootFolder();
                    isRootFolder = true;
                }
                else
                {
                    folderViewModel = controller.GetFolder(folderId);
                }

                // Setup controls
                CancelButton.Click += OnCancelClick;
                SaveButton.Click += OnSaveClick;
                PrepareFolderPreviewInfo();
                cmdCopyPerm.Click += cmdCopyPerm_Click;

                var mef = new ExtensionPointManager();
                var folderFieldsExtension = mef.GetUserControlExtensionPointFirstByPriority("DigitalAssets", "FolderFieldsControlExtensionPoint");
                if (folderFieldsExtension != null)
                {
                    folderFieldsControl = Page.LoadControl(folderFieldsExtension.UserControlSrc);
                    folderFieldsControl.ID = folderFieldsControl.GetType().BaseType.Name;
                    FolderDynamicFieldsContainer.Controls.Add(folderFieldsControl);
                    var fieldsControl = folderFieldsControl as IFieldsControl;
                    if (fieldsControl != null)
                    {
                        fieldsControl.SetController(controller);
                        fieldsControl.SetItemViewModel(new ItemViewModel()
                        {
                            ItemID = folderViewModel.FolderID,
                            IsFolder = true,
                            PortalID = folderViewModel.PortalID,
                            ItemName = folderViewModel.FolderName
                        });
                    }
                }
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
                if (!Page.IsValid)
                {
                    return;
                }
                SaveFolderProperties();

                SavePermissions();
                Page.CloseClientDialog(true);
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
            if (!CanManageFolder)
            {
                throw new DotNetNukeException(LocalizeString("UserCannotEditFolderError"));
            }

            if (!isRootFolder)
            {
                controller.RenameFolder(folderViewModel.FolderID, FolderNameInput.Text);
            }
            var fieldsControl = folderFieldsControl as IFieldsControl;
            if (fieldsControl != null)
            {
                Folder = (IFolderInfo)fieldsControl.SaveProperties();
            }
        }

        private void SavePermissions()
        {
            if (!CanManageFolder)
            {
                throw new DotNetNukeException(LocalizeString("UserCannotChangePermissionsError"));
            }

            Folder = (FolderInfo)FolderManager.Instance.GetFolder(Folder.FolderID);
            Folder.FolderPermissions.Clear();
            Folder.FolderPermissions.AddRange(PermissionsGrid.Permissions);
            FolderPermissionController.SaveFolderPermissions(Folder);
        }

        private void OnCancelClick(object sender, EventArgs e)
        {
            Page.CloseClientDialog(false);
        }

        private void cmdCopyPerm_Click(object sender, EventArgs e)
        {
            try
            {
                FolderPermissionController.CopyPermissionsToSubfolders(Folder, PermissionsGrid.Permissions);
                UI.Skins.Skin.AddModuleMessage(this, LocalizeString("PermissionsCopied"), ModuleMessage.ModuleMessageType.GreenSuccess);
            }
            catch (Exception ex)
            {
                UI.Skins.Skin.AddModuleMessage(this, LocalizeString("PermissionCopyError"), ModuleMessage.ModuleMessageType.RedError);
                Exceptions.ProcessModuleLoadException(this, ex);
            }
        }

        protected override void OnLoad(EventArgs e)
        {
            try
            {
                if (!Page.IsPostBack)
                {
                    SetupPermissionGrid();
                    PrepareFolderProperties();
                    SetPropertiesAvailability(FolderPermissionController.CanManageFolder((FolderInfo)Folder));
                }

                if (!FolderPermissionController.CanViewFolder((FolderInfo)Folder))
                {
                    SaveButton.Visible = false;
                    SetPropertiesVisibility(false);
                    UI.Skins.Skin.AddModuleMessage(this, LocalizeString("UserCannotReadFolderError"), ModuleMessage.ModuleMessageType.RedError);
                }
                else
                {
                    SaveButton.Visible = FolderPermissionController.CanViewFolder((FolderInfo)Folder) && FolderPermissionController.CanManageFolder((FolderInfo)Folder);
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

        private void SetPropertiesAvailability(bool availability)
        {
            FolderNameInput.Enabled = (!isRootFolder) && availability;
            var fieldsControl = folderFieldsControl as IFieldsControl;
            if (fieldsControl != null)
            {
                fieldsControl.SetPropertiesAvailability(availability);
            }
        }

        private void SetPropertiesVisibility(bool visibility)
        {
            FolderNameInput.Visible = visibility;
            FolderTypeLiteral.Visible = visibility;
            FolderInfoPreviewPanel.Visible = visibility;
            var fieldsControl = folderFieldsControl as IFieldsControl;
            if (fieldsControl != null)
            {
                fieldsControl.SetPropertiesVisibility(visibility);
            }
        }

        private void PrepareFolderProperties()
        {
            FolderNameInput.Text = folderViewModel.FolderName;
            FolderTypeLiteral.Text = FolderMappingController.Instance.GetFolderMapping(folderViewModel.FolderMappingID).MappingName;

            FolderNameInvalidCharactersValidator.ValidationExpression = "^([^" + Regex.Escape(controller.GetInvalidChars()) + "]+)$";
            FolderNameInvalidCharactersValidator.ErrorMessage = controller.GetInvalidCharsErrorText();

            var fieldsControl = folderFieldsControl as IFieldsControl;
            if (fieldsControl != null)
            {
                fieldsControl.PrepareProperties();
            }
        }

        private void PrepareFolderPreviewInfo()
        {
            var folderPreviewPanel = (PreviewPanelControl)FolderInfoPreviewPanel;
            if (folderPreviewPanel != null)
            {
                folderPreviewPanel.SetPreviewInfo(controller.GetFolderPreviewInfo(Folder));
            }
        }

        private void SetupPermissionGrid()
        {
            PermissionsGrid.FolderPath = Folder.FolderPath;
            PermissionsGrid.Visible = HasFullControl;
        }
    }
}