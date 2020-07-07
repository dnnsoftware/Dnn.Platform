// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Modules.DigitalAssets
{
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

    public partial class FileProperties : PortalModuleBase
    {
        private readonly IDigitalAssetsController controller = new Factory().DigitalAssetsController;

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
                return this.fileItem.ItemName;
            }
        }

        protected bool CanManageFolder
        {
            get
            {
                return this.UserInfo.IsSuperUser || FolderPermissionController.CanManageFolder((FolderInfo)this.folder);
            }
        }

        protected string ActiveTab
        {
            get
            {
                var activeTab = this.Request.QueryString["activeTab"];
                return string.IsNullOrEmpty(activeTab) ? string.Empty : System.Text.RegularExpressions.Regex.Replace(activeTab, "[^\\w]", string.Empty);
            }
        }

        protected override void OnInit(EventArgs e)
        {
            try
            {
                base.OnInit(e);

                JavaScript.RequestRegistration(CommonJs.DnnPlugins);

                var fileId = Convert.ToInt32(this.Request.Params["FileId"]);
                this.file = FileManager.Instance.GetFile(fileId, true);
                this.fileItem = this.controller.GetFile(fileId);
                this.folder = FolderManager.Instance.GetFolder(this.file.FolderId);

                this.SaveButton.Click += this.OnSaveClick;
                this.CancelButton.Click += this.OnCancelClick;

                if (FolderPermissionController.CanViewFolder((FolderInfo)this.folder))
                {
                    var mef = new ExtensionPointManager();
                    var preViewPanelExtension = mef.GetUserControlExtensionPointFirstByPriority("DigitalAssets", "PreviewInfoPanelExtensionPoint");
                    this.previewPanelControl = this.Page.LoadControl(preViewPanelExtension.UserControlSrc);
                    this.PreviewPanelContainer.Controls.Add(this.previewPanelControl);

                    var fileFieldsExtension = mef.GetUserControlExtensionPointFirstByPriority("DigitalAssets", "FileFieldsControlExtensionPoint");
                    this.fileFieldsControl = this.Page.LoadControl(fileFieldsExtension.UserControlSrc);
                    this.fileFieldsControl.ID = this.fileFieldsControl.GetType().BaseType.Name;
                    this.FileFieldsContainer.Controls.Add(this.fileFieldsControl);

                    this.PrepareFilePreviewInfoControl();
                    this.PrepareFileFieldsControl();

                    // Tab Extension Point
                    var tabContentControlsInstances = new List<PropertiesTabContentControl>();
                    foreach (var extension in mef.GetEditPageTabExtensionPoints("DigitalAssets", "FilePropertiesTab"))
                    {
                        if (FolderPermissionController.HasFolderPermission(this.folder.FolderPermissions, extension.Permission))
                        {
                            var liElement = new HtmlGenericControl("li") { InnerHtml = "<a href=\"#" + extension.EditPageTabId + "\">" + extension.Text + "</a>", };
                            liElement.Attributes.Add("class", extension.CssClass);
                            liElement.Attributes.Add("id", extension.EditPageTabId + "_tab");
                            this.Tabs.Controls.Add(liElement);

                            var container = new PanelTabExtensionControl { PanelId = extension.EditPageTabId };
                            var control = (PortalModuleBase)this.Page.LoadControl(extension.UserControlSrc);
                            control.ID = Path.GetFileNameWithoutExtension(extension.UserControlSrc);
                            control.ModuleConfiguration = this.ModuleConfiguration;
                            var contentControl = control as PropertiesTabContentControl;
                            if (contentControl != null)
                            {
                                contentControl.OnItemUpdated += this.OnItemUpdated;
                                tabContentControlsInstances.Add(contentControl);
                            }

                            container.Controls.Add(control);
                            this.TabsPanel.Controls.Add(container);
                        }
                    }

                    this.tabContentControls = tabContentControlsInstances.ToList();
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
                if (!this.Page.IsPostBack)
                {
                    this.SetPropertiesAvailability(this.CanManageFolder);
                }

                if (!FolderPermissionController.CanViewFolder((FolderInfo)this.folder))
                {
                    this.SaveButton.Visible = false;
                    this.SetPropertiesVisibility(false);
                    UI.Skins.Skin.AddModuleMessage(this, this.LocalizeString("UserCannotReadFileError"), ModuleMessage.ModuleMessageType.RedError);
                }
                else
                {
                    this.SetFilePreviewInfo();
                    this.SaveButton.Visible = FolderPermissionController.CanViewFolder((FolderInfo)this.folder) && FolderPermissionController.CanManageFolder((FolderInfo)this.folder);
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
            this.SetFilePreviewInfo();
            foreach (var propertiesTabContentControl in this.tabContentControls)
            {
                propertiesTabContentControl.DataBindItem();
            }
        }

        private void OnSaveClick(object sender, EventArgs e)
        {
            if (!this.Page.IsValid)
            {
                return;
            }

            try
            {
                this.SaveFileProperties();
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

        private void OnCancelClick(object sender, EventArgs e)
        {
            this.Page.CloseClientDialog(false);
        }

        private void SaveFileProperties()
        {
            this.file = (IFileInfo)((FileFieldsControl)this.fileFieldsControl).SaveProperties();
        }

        private void SetPropertiesVisibility(bool visibility)
        {
            ((FileFieldsControl)this.fileFieldsControl).SetPropertiesVisibility(visibility);
        }

        private void SetPropertiesAvailability(bool availability)
        {
            ((FileFieldsControl)this.fileFieldsControl).SetPropertiesAvailability(availability);
        }

        private void SetFilePreviewInfo()
        {
            var previewPanelInstance = (PreviewPanelControl)this.previewPanelControl;
            previewPanelInstance.SetPreviewInfo(this.controller.GetFilePreviewInfo(this.file, this.fileItem));
        }

        private void PrepareFilePreviewInfoControl()
        {
            var previewPanelInstance = (PreviewPanelControl)this.previewPanelControl;
            previewPanelInstance.SetController(this.controller);
            previewPanelInstance.SetModuleConfiguration(this.ModuleConfiguration);
        }

        private void PrepareFileFieldsControl()
        {
            var fileFieldsIntance = (FileFieldsControl)this.fileFieldsControl;
            fileFieldsIntance.SetController(this.controller);
            fileFieldsIntance.SetItemViewModel(this.fileItem);
            fileFieldsIntance.SetFileInfo(this.file);
            fileFieldsIntance.SetModuleConfiguration(this.ModuleConfiguration);
        }
    }
}
