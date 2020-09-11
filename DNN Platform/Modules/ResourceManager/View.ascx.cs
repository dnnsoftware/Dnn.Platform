// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace Dnn.Modules.ResourceManager
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Dnn.Modules.ResourceManager.Components;
    using Dnn.Modules.ResourceManager.Exceptions;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Framework.JavaScriptLibraries;
    using DotNetNuke.Services.FileSystem;
    using DotNetNuke.Web.Client.ClientResourceManagement;

    using DnnExceptions = DotNetNuke.Services.Exceptions.Exceptions;

    /// <summary>
    /// Provides the module view capabilities.
    /// </summary>
    public partial class View : PortalModuleBase
    {
        private readonly string bundleJsPath;
        private int? gid;
        private int? folderId;
        private string extensionWhitelist;
        private string validationCode;

        /// <summary>
        /// Initializes a new instance of the <see cref="View"/> class.
        /// </summary>
        public View()
        {
            this.bundleJsPath = Constants.ModulePath + "Scripts/resourceManager-bundle.js";
        }

        /// <summary>
        /// Gets the group id.
        /// </summary>
        public int GroupId
        {
            get
            {
                if (this.gid.HasValue)
                {
                    return this.gid.Value;
                }

                if (!int.TryParse(this.Request.QueryString["groupid"], out var id))
                {
                    id = Null.NullInteger;
                }

                this.gid = id;
                return this.gid.Value;
            }
        }

        /// <summary>
        /// Gets the list of allowed file extensions.
        /// </summary>
        public string ExtensionWhitelist
        {
            get
            {
                if (string.IsNullOrEmpty(this.extensionWhitelist))
                {
                    this.extensionWhitelist = FileManager.Instance.WhiteList.ToStorageString();
                }

                return this.extensionWhitelist;
            }
        }

        /// <summary>
        /// Gets the file upload validation code.
        /// </summary>
        public string ValidationCode
        {
            get
            {
                if (string.IsNullOrEmpty(this.validationCode))
                {
                    var parameters = new List<object>() { this.ExtensionWhitelist.Split(',').Select(i => i.Trim()).ToList() };
                    parameters.Add(this.PortalSettings.UserInfo.UserID);
                    if (!this.PortalSettings.UserInfo.IsSuperUser)
                    {
                        parameters.Add(this.PortalSettings.PortalId);
                    }

                    this.validationCode = ValidationUtils.ComputeValidationCode(parameters);
                }

                return this.validationCode;
            }
        }

        /// <summary>
        /// Gets the culture code used for localization.
        /// </summary>
        protected string ResxCulture => LocalizationController.Instance.CultureName;

        /// <summary>
        /// Gets the timestamp of the localization resource file.
        /// </summary>
        protected string ResxTimeStamp =>
            LocalizationController.Instance.GetResxTimeStamp(Constants.ViewResourceFileName, Constants.ResourceManagerLocalization).ToString();

        /// <summary>
        /// Gets the id of the home folder.
        /// </summary>
        protected int HomeFolderId => new SettingsManager(this.ModuleId, this.GroupId).HomeFolderId;

        /// <summary>
        /// Gets a value indicating whether the user is logged in.
        /// </summary>
        protected bool UserLogged => this.UserId > 0;

        /// <summary>
        /// Gets a value indicating whether the currently logged in user is an administrator.
        /// </summary>
        protected bool IsAdmin => this.UserInfo.IsAdmin;

        /// <summary>
        /// Gets how many items to show per page.
        /// </summary>
        protected int FolderPanelNumItems => Constants.ItemsPerPage;

        /// <summary>
        /// Gets the width of each item in the resource grid.
        /// </summary>
        protected int ItemWidth => Constants.ItemWidth;

        /// <summary>
        /// Gets the maximum allowed upload size.
        /// </summary>
        protected long MaxUploadSize => Config.GetMaxUploadSize();

        /// <summary>
        /// Gets a json serialized object defining the possible sort fields.
        /// </summary>
        protected string SortingFields => Json.Serialize(Constants.SortingFields);

        /// <summary>
        /// Gets the default sort field key.
        /// </summary>
        protected string DefaultSortingField => Constants.DefaultSortingField;

        /// <summary>
        /// Gets a human readable representation of the maximum upload size.
        /// </summary>
        protected string MaxUploadSizeHumanReadable
            => string.Format(new FileSizeFormatProvider(), "{0:fs}", this.MaxUploadSize);

        /// <summary>
        /// Gets the id of the currently open folder.
        /// </summary>
        protected int OpenFolderId => this.FolderId.GetValueOrDefault();

        /// <summary>
        /// Gets the path of the currently open folder.
        /// </summary>
        protected string OpenFolderPath
        {
            get
            {
                if (!this.FolderId.HasValue)
                {
                    return string.Empty;
                }

                var folder = FolderManager.Instance.GetFolder(this.FolderId.Value);
                var folderPathElements = folder.FolderPath.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
                return Json.Serialize(folderPathElements);
            }
        }

        /// <summary>
        /// Gets the id of the url path requested folder id.
        /// </summary>
        private int? FolderId
        {
            get
            {
                if (!this.folderId.HasValue)
                {
                    if (int.TryParse(this.Request.QueryString["folderId"], out var id))
                    {
                        this.folderId = id;
                    }
                }

                return this.folderId;
            }
        }

        /// <summary>
        /// Handles the page load event.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">The event arguments.</param>
        protected void Page_Load(object sender, EventArgs e)
        {
            try
            {
                JavaScript.RequestRegistration(CommonJs.DnnPlugins);
                ClientResourceManager.RegisterScript(this.Page, Constants.ModulePath + "Scripts/dnn.Localization.js");
                ClientResourceManager.RegisterScript(this.Page, this.bundleJsPath);
            }
            catch (ModeValidationException exc)
            {
                DnnExceptions.ProcessModuleLoadException(this.LocalizeString(exc.Message), this, exc);
            }
            catch (Exception exc)
            {
                // Module failed to load
                DnnExceptions.ProcessModuleLoadException(exc.Message, this, exc);
            }
        }
    }
}
