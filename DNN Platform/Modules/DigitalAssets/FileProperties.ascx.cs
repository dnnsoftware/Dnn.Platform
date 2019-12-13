﻿// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Web.UI;
using System.Web.UI.HtmlControls;

using DotNetNuke.Entities.Modules;
using DotNetNuke.ExtensionPoints;
using DotNetNuke.Framework;
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

namespace DotNetNuke.Modules.DigitalAssets
{
    public partial class FileProperties : PortalModuleBase
    {
        private readonly IDigitalAssetsController controller = (new Factory()).DigitalAssetsController;

        private IFileInfo file;
        private IFolderInfo folder;
        private ItemViewModel fileItem;
        private Control previewPanelControl;
        private Control fileFieldsControl;
        private IEnumerable<PropertiesTabContentControl> tabContentControls;

        protected string DialogTitle
        {
            get
            {
                return fileItem.ItemName;
            }
        }

        protected bool CanManageFolder
        {
            get
            {
                return UserInfo.IsSuperUser || FolderPermissionController.CanManageFolder((FolderInfo)folder);
            }
        }

        protected string ActiveTab
        {
            get
            {
                var activeTab = Request.QueryString["activeTab"];
                return string.IsNullOrEmpty(activeTab) ? "" : System.Text.RegularExpressions.Regex.Replace(activeTab, "[^\\w]", "");
            }
        }


        protected override void OnInit(EventArgs e)
        {
            try
            {
                base.OnInit(e);

                JavaScript.RequestRegistration(CommonJs.DnnPlugins);

                var fileId = Convert.ToInt32(Request.Params["FileId"]);
                file = FileManager.Instance.GetFile(fileId, true);
                fileItem = controller.GetFile(fileId);
                folder = FolderManager.Instance.GetFolder(file.FolderId);

                SaveButton.Click += OnSaveClick;
                CancelButton.Click += OnCancelClick;

                if (FolderPermissionController.CanViewFolder((FolderInfo)folder))
                {
                    var mef = new ExtensionPointManager();
                    var preViewPanelExtension = mef.GetUserControlExtensionPointFirstByPriority("DigitalAssets", "PreviewInfoPanelExtensionPoint");
                    previewPanelControl = Page.LoadControl(preViewPanelExtension.UserControlSrc);
                    PreviewPanelContainer.Controls.Add(previewPanelControl);

                    var fileFieldsExtension = mef.GetUserControlExtensionPointFirstByPriority("DigitalAssets", "FileFieldsControlExtensionPoint");
                    fileFieldsControl = Page.LoadControl(fileFieldsExtension.UserControlSrc);
                    fileFieldsControl.ID = fileFieldsControl.GetType().BaseType.Name;
                    FileFieldsContainer.Controls.Add(fileFieldsControl);

                    PrepareFilePreviewInfoControl();
                    PrepareFileFieldsControl();

                    // Tab Extension Point
                    var tabContentControlsInstances = new List<PropertiesTabContentControl>();
                    foreach (var extension in mef.GetEditPageTabExtensionPoints("DigitalAssets", "FilePropertiesTab"))
                    {
                        if (FolderPermissionController.HasFolderPermission(folder.FolderPermissions, extension.Permission))
                        {
                            var liElement = new HtmlGenericControl("li") { InnerHtml = "<a href=\"#" + extension.EditPageTabId + "\">" + extension.Text + "</a>", };
                            liElement.Attributes.Add("class", extension.CssClass);
                            liElement.Attributes.Add("id", extension.EditPageTabId + "_tab");
                            Tabs.Controls.Add(liElement);

                            var container = new PanelTabExtensionControl { PanelId = extension.EditPageTabId };
                            var control = (PortalModuleBase)Page.LoadControl(extension.UserControlSrc);
                            control.ID = Path.GetFileNameWithoutExtension(extension.UserControlSrc);
                            control.ModuleConfiguration = ModuleConfiguration;
                            var contentControl = control as PropertiesTabContentControl;
                            if (contentControl != null)
                            {
                                contentControl.OnItemUpdated += OnItemUpdated;
                                tabContentControlsInstances.Add(contentControl);
                            }
                            container.Controls.Add(control);
                            TabsPanel.Controls.Add(container);
                        }
                    }
                    tabContentControls = tabContentControlsInstances.ToList();
                }
            }
            catch (Exception ex)
            {
                Exceptions.ProcessModuleLoadException(this, ex);
            }
        }

        protected override void OnLoad(EventArgs e)
        {
            try
            {
                if (!Page.IsPostBack)
                {
                    SetPropertiesAvailability(CanManageFolder);
                }

                if (!FolderPermissionController.CanViewFolder((FolderInfo)folder))
                {
                    SaveButton.Visible = false;
                    SetPropertiesVisibility(false);
                    UI.Skins.Skin.AddModuleMessage(this, LocalizeString("UserCannotReadFileError"), ModuleMessage.ModuleMessageType.RedError);
                }
                else
                {
                    SetFilePreviewInfo();
                    SaveButton.Visible = FolderPermissionController.CanViewFolder((FolderInfo)folder) && FolderPermissionController.CanManageFolder((FolderInfo)folder);
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

        private void OnItemUpdated()
        {
            SetFilePreviewInfo();
            foreach (var propertiesTabContentControl in tabContentControls)
            {
                propertiesTabContentControl.DataBindItem();
            }
        }

        private void OnSaveClick(object sender, EventArgs e)
        {
            if (!Page.IsValid)
            {
                return;
            }

            try
            {
                SaveFileProperties();
                Page.CloseClientDialog(true);
            }
            catch (ThreadAbortException) { }
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

        private void OnCancelClick(object sender, EventArgs e)
        {
            Page.CloseClientDialog(false);
        }

        private void SaveFileProperties()
        {
            file = (IFileInfo)((FileFieldsControl)fileFieldsControl).SaveProperties();
        }

        private void SetPropertiesVisibility(bool visibility)
        {
            ((FileFieldsControl)fileFieldsControl).SetPropertiesVisibility(visibility);
        }

        private void SetPropertiesAvailability(bool availability)
        {
            ((FileFieldsControl)fileFieldsControl).SetPropertiesAvailability(availability);
        }

        private void SetFilePreviewInfo()
        {
            var previewPanelInstance = (PreviewPanelControl)previewPanelControl;
            previewPanelInstance.SetPreviewInfo(controller.GetFilePreviewInfo(file, fileItem));
        }

        private void PrepareFilePreviewInfoControl()
        {
            var previewPanelInstance = (PreviewPanelControl)previewPanelControl;
            previewPanelInstance.SetController(controller);
            previewPanelInstance.SetModuleConfiguration(ModuleConfiguration);
        }

        private void PrepareFileFieldsControl()
        {
            var fileFieldsIntance = (FileFieldsControl)fileFieldsControl;
            fileFieldsIntance.SetController(controller);
            fileFieldsIntance.SetItemViewModel(fileItem);
            fileFieldsIntance.SetFileInfo(file);
            fileFieldsIntance.SetModuleConfiguration(ModuleConfiguration);
        }
    }
}
