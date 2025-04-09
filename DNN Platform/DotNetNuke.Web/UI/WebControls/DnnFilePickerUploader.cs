// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.UI.WebControls
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.Linq;
    using System.Web.UI;
    using System.Web.UI.WebControls;

    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Framework;
    using DotNetNuke.Framework.JavaScriptLibraries;
    using DotNetNuke.Services.FileSystem;
    using DotNetNuke.Services.Localization;
    using DotNetNuke.UI.UserControls;
    using DotNetNuke.Web.Common;

    public class DnnFilePickerUploader : UserControl, IFilePickerUploader
    {
        // ReSharper disable InconsistentNaming
        [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1306:FieldNamesMustBeginWithLowerCaseLetter", Justification = "Breaking Change")]
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
        protected DnnFileDropDownList FilesComboBox;
        [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1306:FieldNamesMustBeginWithLowerCaseLetter", Justification = "Breaking Change")]
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
        protected DnnFolderDropDownList FoldersComboBox;
        [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1306:FieldNamesMustBeginWithLowerCaseLetter", Justification = "Breaking Change")]
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
        protected Label FoldersLabel;
        [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1306:FieldNamesMustBeginWithLowerCaseLetter", Justification = "Breaking Change")]
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
        protected DnnFileUpload FileUploadControl;

        // ReSharper restore InconsistentNaming
        private const string MyFileName = "filepickeruploader.ascx";
        private int? portalId = null;
        private string fileFilter;
        private string folderPath = string.Empty;
        private bool folderPathSet = false;

        public bool UsePersonalFolder { get; set; }

        /// <inheritdoc/>
        public string FilePath
        {
            get
            {
                this.EnsureChildControls();

                var path = string.Empty;
                if (this.FoldersComboBox.SelectedFolder != null && this.FilesComboBox.SelectedFile != null)
                {
                    path = this.FilesComboBox.SelectedFile.RelativePath;
                }

                return path;
            }

            set
            {
                this.EnsureChildControls();
                if (!string.IsNullOrEmpty(value))
                {
                    var file = FileManager.Instance.GetFile(this.PortalId, value);
                    if (file != null)
                    {
                        this.FoldersComboBox.SelectedFolder = FolderManager.Instance.GetFolder(file.FolderId);
                        this.FilesComboBox.SelectedFile = file;
                    }
                }
                else
                {
                    this.FoldersComboBox.SelectedFolder = null;
                    this.FilesComboBox.SelectedFile = null;

                    this.LoadFolders();
                }
            }
        }

        /// <inheritdoc/>
        public int FileID
        {
            get
            {
                this.EnsureChildControls();

                return this.FilesComboBox.SelectedFile != null ? this.FilesComboBox.SelectedFile.FileId : Null.NullInteger;
            }

            set
            {
                this.EnsureChildControls();
                var file = FileManager.Instance.GetFile(value);
                if (file != null)
                {
                    this.FoldersComboBox.SelectedFolder = FolderManager.Instance.GetFolder(file.FolderId);
                    this.FilesComboBox.SelectedFile = file;
                }
            }
        }

        public string FolderPath
        {
            get
            {
                return this.folderPathSet
                            ? this.folderPath
                            : this.FoldersComboBox.SelectedFolder != null
                                ? this.FoldersComboBox.SelectedFolder.FolderPath
                                : string.Empty;
            }

            set
            {
                this.folderPath = value;
                this.folderPathSet = true;
            }
        }

        /// <inheritdoc/>
        public string FileFilter
        {
            get
            {
                return this.fileFilter;
            }

            set
            {
                this.fileFilter = value;
                if (!string.IsNullOrEmpty(value))
                {
                    this.FileUploadControl.Options.Extensions = value.Split(',').ToList();
                }
                else
                {
                    this.FileUploadControl.Options.Extensions.RemoveAll(t => true);
                }
            }
        }

        public bool Required { get; set; }

        public UserInfo User { get; set; }

        public int PortalId
        {
            get
            {
                return !this.portalId.HasValue ? PortalSettings.Current.PortalId : this.portalId.Value;
            }

            set
            {
                this.portalId = value;
            }
        }

        public bool SupportHost
        {
            get { return this.FileUploadControl.SupportHost; }
            set { this.FileUploadControl.SupportHost = value; }
        }

        protected string FolderLabel
        {
            get
            {
                return Localization.GetString("Folder", Localization.GetResourceFile(this, MyFileName));
            }
        }

        protected string FileLabel
        {
            get
            {
                return Localization.GetString("File", Localization.GetResourceFile(this, MyFileName));
            }
        }

        protected string UploadFileLabel
        {
            get
            {
                return Localization.GetString("UploadFile", Localization.GetResourceFile(this, MyFileName));
            }
        }

        protected string DropFileLabel
        {
            get
            {
                return Localization.GetString("DropFile", Localization.GetResourceFile(this, MyFileName));
            }
        }

        /// <inheritdoc/>
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            this.FoldersComboBox.SelectItemDefaultText = (this.SupportHost && PortalSettings.Current.ActiveTab.IsSuperTab) ? DynamicSharedConstants.HostRootFolder : DynamicSharedConstants.RootFolder;
            this.FoldersComboBox.OnClientSelectionChanged.Add("dnn.dnnFileUpload.Folders_Changed");
            this.FoldersComboBox.Options.Services.Parameters.Add("permission", "READ,ADD");

            this.FilesComboBox.OnClientSelectionChanged.Add("dnn.dnnFileUpload.Files_Changed");
            this.FilesComboBox.SelectItemDefaultText = DynamicSharedConstants.Unspecified;
            this.FilesComboBox.IncludeNoneSpecificItem = true;
            this.FilesComboBox.Filter = this.FileFilter;

            if (UrlUtils.InPopUp())
            {
                this.FileUploadControl.Width = 630;
                this.FileUploadControl.Height = 400;
            }

            this.LoadFolders();
            JavaScript.RequestRegistration(CommonJs.jQueryFileUpload);
            JavaScript.RequestRegistration(CommonJs.DnnPlugins);
            ServicesFramework.Instance.RequestAjaxAntiForgerySupport();
        }

        /// <inheritdoc/>
        protected override void OnPreRender(EventArgs e)
        {
            if (this.FoldersComboBox.SelectedFolder != null && this.FoldersComboBox.SelectedFolder.FolderPath.StartsWith("Users/", StringComparison.InvariantCultureIgnoreCase))
            {
                var userFolder = FolderManager.Instance.GetUserFolder(this.User ?? UserController.Instance.GetCurrentUserInfo());
                if (this.FoldersComboBox.SelectedFolder.FolderID == userFolder.FolderID)
                {
                    this.FoldersComboBox.SelectedItem = new ListItem
                    {
                        Text = FolderManager.Instance.MyFolderName,
                        Value = userFolder.FolderID.ToString(CultureInfo.InvariantCulture),
                    };
                }
                else if (this.UsePersonalFolder)
                {
                    // if UserPersonalFolder is true, make sure the file is under the user folder.
                    this.FoldersComboBox.SelectedItem = new ListItem
                    {
                        Text = FolderManager.Instance.MyFolderName,
                        Value = userFolder.FolderID.ToString(CultureInfo.InvariantCulture),
                    };

                    this.FilesComboBox.SelectedFile = null;
                }
            }

            this.FoldersLabel.Text = FolderManager.Instance.MyFolderName;

            this.FileUploadControl.Options.FolderPicker.Disabled = this.UsePersonalFolder;
            if (this.FileUploadControl.Options.FolderPicker.Disabled && this.FoldersComboBox.SelectedFolder != null)
            {
                var selectedItem = new SerializableKeyValuePair<string, string>(
                    this.FoldersComboBox.SelectedItem.Value, this.FoldersComboBox.SelectedItem.Text);

                this.FileUploadControl.Options.FolderPicker.InitialState = new DnnDropDownListState
                {
                    SelectedItem = selectedItem,
                };
                this.FileUploadControl.Options.FolderPath = this.FoldersComboBox.SelectedFolder.FolderPath;
            }

            base.OnPreRender(e);
        }

        private void LoadFolders()
        {
            if (this.UsePersonalFolder)
            {
                var user = this.User ?? UserController.Instance.GetCurrentUserInfo();
                var userFolder = FolderManager.Instance.GetUserFolder(user);
                this.FoldersComboBox.SelectedFolder = userFolder;
            }
            else
            {
                // select folder
                string fileName;
                string folderPath;
                if (!string.IsNullOrEmpty(this.FilePath))
                {
                    fileName = this.FilePath.Substring(this.FilePath.LastIndexOf("/") + 1);
                    folderPath = string.IsNullOrEmpty(fileName) ? this.FilePath : this.FilePath.Replace(fileName, string.Empty);
                }
                else
                {
                    fileName = this.FilePath;
                    folderPath = this.FolderPath;
                }

                this.FoldersComboBox.SelectedFolder = FolderManager.Instance.GetFolder(this.PortalId, folderPath);

                if (!string.IsNullOrEmpty(fileName))
                {
                    this.FilesComboBox.SelectedFile = FileManager.Instance.GetFile(this.FoldersComboBox.SelectedFolder, fileName);
                }
            }

            this.FoldersComboBox.Enabled = !this.UsePersonalFolder;
            this.FoldersLabel.Visible = this.UsePersonalFolder;
        }
    }
}
