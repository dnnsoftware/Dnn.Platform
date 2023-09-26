// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Modules.Admin.Modules
{
    using System;
    using System.IO;
    using System.Web.UI.WebControls;
    using System.Xml;

    using DotNetNuke.Abstractions;
    using DotNetNuke.Abstractions.Modules;
    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Framework;
    using DotNetNuke.Security;
    using DotNetNuke.Security.Permissions;
    using DotNetNuke.Services.Exceptions;
    using DotNetNuke.Services.FileSystem;
    using DotNetNuke.Services.Localization;
    using DotNetNuke.UI.Skins.Controls;

    using Microsoft.Extensions.DependencyInjection;

    /// <summary>The global import module action control.</summary>
    public partial class Import : PortalModuleBase
    {
        private readonly IBusinessControllerProvider businessControllerProvider;
        private readonly INavigationManager navigationManager;

        private int moduleId = -1;
        private ModuleInfo module;

        /// <summary>Initializes a new instance of the <see cref="Import"/> class.</summary>
        [Obsolete("Deprecated in DotNetNuke 10.0.0. Please use overload with IServiceProvider. Scheduled removal in v12.0.0.")]
        public Import()
            : this(null, null)
        {
        }

        /// <summary>Initializes a new instance of the <see cref="Import"/> class.</summary>
        /// <param name="businessControllerProvider">The business controller provider.</param>
        /// <param name="navigationManager">The navigation manager.</param>
        public Import(IBusinessControllerProvider businessControllerProvider, INavigationManager navigationManager)
        {
            this.businessControllerProvider = businessControllerProvider ?? this.DependencyProvider.GetRequiredService<IBusinessControllerProvider>();
            this.navigationManager = navigationManager ?? this.DependencyProvider.GetRequiredService<INavigationManager>();
        }

        private ModuleInfo Module
        {
            get
            {
                return this.module ?? (this.module = ModuleController.Instance.GetModule(this.moduleId, this.TabId, false));
            }
        }

        private string ReturnURL
        {
            get
            {
                return UrlUtils.ValidReturnUrl(this.Request.Params["ReturnURL"]) ?? this.navigationManager.NavigateURL();
            }
        }

        /// <inheritdoc/>
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            if (this.Request.QueryString["moduleid"] != null)
            {
                int.TryParse(this.Request.QueryString["moduleid"], out this.moduleId);
            }

            // Verify that the current user has access to edit this module
            if (!ModulePermissionController.HasModuleAccess(SecurityAccessLevel.Edit, "IMPORT", this.Module))
            {
                this.Response.Redirect(Globals.AccessDeniedURL(), true);
            }
        }

        /// <inheritdoc/>
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            this.cboFolders.SelectionChanged += this.OnFoldersIndexChanged;
            this.cboFiles.SelectedIndexChanged += this.OnFilesIndexChanged;
            this.cmdImport.Click += this.OnImportClick;

            try
            {
                if (!this.Page.IsPostBack)
                {
                    this.cmdCancel.NavigateUrl = this.ReturnURL;
                    this.cboFolders.UndefinedItem = new ListItem("<" + Localization.GetString("None_Specified") + ">", string.Empty);
                    this.cboFolders.Services.Parameters.Add("permission", "ADD");
                }
            }
            catch (Exception exc)
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }

        protected void OnFoldersIndexChanged(object sender, EventArgs e)
        {
            this.cboFiles.Items.Clear();
            this.cboFiles.InsertItem(0, "<" + Localization.GetString("None_Specified") + ">", "-");
            if (this.cboFolders.SelectedItem == null)
            {
                return;
            }

            if (this.Module == null)
            {
                return;
            }

            var folder = FolderManager.Instance.GetFolder(this.cboFolders.SelectedItemValueAsInt);
            if (folder == null)
            {
                return;
            }

            var files = Globals.GetFileList(this.PortalId, "export", false, folder.FolderPath);
            files.AddRange(Globals.GetFileList(this.PortalId, "xml", false, folder.FolderPath));
            foreach (FileItem file in files)
            {
                if (file.Text.IndexOf("content." + Globals.CleanName(this.Module.DesktopModule.ModuleName) + ".", System.StringComparison.Ordinal) != -1)
                {
                    this.cboFiles.AddItem(file.Text.Replace("content." + Globals.CleanName(this.Module.DesktopModule.ModuleName) + ".", string.Empty), file.Value);
                }

                // legacy support for files which used the FriendlyName
                if (Globals.CleanName(this.Module.DesktopModule.ModuleName) == Globals.CleanName(this.Module.DesktopModule.FriendlyName))
                {
                    continue;
                }

                if (file.Text.IndexOf("content." + Globals.CleanName(this.Module.DesktopModule.FriendlyName) + ".", System.StringComparison.Ordinal) != -1)
                {
                    this.cboFiles.AddItem(file.Text.Replace("content." + Globals.CleanName(this.Module.DesktopModule.FriendlyName) + ".", string.Empty), file.Value);
                }
            }
        }

        protected void OnFilesIndexChanged(object sender, EventArgs e)
        {
            if (this.cboFolders.SelectedItem == null)
            {
                return;
            }

            var folder = FolderManager.Instance.GetFolder(this.cboFolders.SelectedItemValueAsInt);
            if (folder == null)
            {
                return;
            }

            if (string.IsNullOrEmpty(this.cboFiles.SelectedValue) || this.cboFiles.SelectedValue == "-")
            {
                this.txtContent.Text = string.Empty;
                return;
            }

            try
            {
                var fileId = Convert.ToInt32(this.cboFiles.SelectedValue);
                var file = DotNetNuke.Services.FileSystem.FileManager.Instance.GetFile(fileId);
                using (var streamReader = new StreamReader(DotNetNuke.Services.FileSystem.FileManager.Instance.GetFileContent(file)))
                {
                    this.txtContent.Text = streamReader.ReadToEnd();
                }
            }
            catch (Exception)
            {
                this.txtContent.Text = string.Empty;
            }
        }

        protected void OnImportClick(object sender, EventArgs e)
        {
            try
            {
                if (this.Module != null)
                {
                    var strMessage = this.ImportModule();
                    if (string.IsNullOrEmpty(strMessage))
                    {
                        this.Response.Redirect(this.ReturnURL, true);
                    }
                    else
                    {
                        UI.Skins.Skin.AddModuleMessage(this, strMessage, ModuleMessage.ModuleMessageType.RedError);
                    }
                }
            }
            catch (Exception exc)
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }

        private string ImportModule()
        {
            if (this.Module == null)
            {
                return string.Empty;
            }

            if (string.IsNullOrEmpty(this.Module.DesktopModule.BusinessControllerClass) || !this.Module.DesktopModule.IsPortable)
            {
                return Localization.GetString("ImportNotSupported", this.LocalResourceFile);
            }

            try
            {
                var controller = this.businessControllerProvider.GetInstance<IPortable>(this.Module);
                if (controller is null)
                {
                    return Localization.GetString("ImportNotSupported", this.LocalResourceFile);
                }

                var xmlDoc = new XmlDocument { XmlResolver = null };
                try
                {
                    var content = XmlUtils.RemoveInvalidXmlCharacters(this.txtContent.Text);
                    xmlDoc.LoadXml(content);
                }
                catch
                {
                    return Localization.GetString("NotValidXml", this.LocalResourceFile);
                }

                var strType = xmlDoc.DocumentElement.GetAttribute("type");
                if (strType != Globals.CleanName(this.Module.DesktopModule.ModuleName) &&
                    strType != Globals.CleanName(this.Module.DesktopModule.FriendlyName))
                {
                    return Localization.GetString("NotCorrectType", this.LocalResourceFile);
                }

                var strVersion = xmlDoc.DocumentElement.GetAttribute("version");

                // DNN26810 if rootnode = "content", import only content(the old way)
                if (xmlDoc.DocumentElement.Name.ToLowerInvariant() == "content")
                {
                    controller.ImportModule(
                        this.moduleId,
                        xmlDoc.DocumentElement.InnerXml,
                        strVersion,
                        this.UserInfo.UserID);
                }

                // otherwise (="module") import the new way
                else
                {
                    ModuleController.DeserializeModule(
                        this.businessControllerProvider,
                        xmlDoc.DocumentElement,
                        this.Module,
                        this.PortalId,
                        this.TabId);
                }

                this.Response.Redirect(this.navigationManager.NavigateURL(), true);
                return string.Empty;
            }
            catch
            {
                return Localization.GetString("Error", this.LocalResourceFile);
            }
        }
    }
}
