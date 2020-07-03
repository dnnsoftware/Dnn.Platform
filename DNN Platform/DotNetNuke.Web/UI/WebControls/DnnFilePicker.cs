// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Web.UI.WebControls
{
    using System;
    using System.IO;
    using System.Web.UI.HtmlControls;
    using System.Web.UI.WebControls;

    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Host;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Framework;
    using DotNetNuke.Instrumentation;
    using DotNetNuke.Security.Permissions;
    using DotNetNuke.Services.FileSystem;
    using DotNetNuke.Services.Localization;

    using FileInfo = DotNetNuke.Services.FileSystem.FileInfo;

    /// <summary>
    ///   The FilePicker Class provides a File Picker Control for DotNetNuke.
    /// </summary>
    public class DnnFilePicker : CompositeControl, ILocalizable
    {
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(DnnFilePicker));

        private Panel _pnlContainer;
        private Panel _pnlLeftDiv;
        private Panel _pnlFolder;

        private Label _lblFolder;
        private DropDownList _cboFolders;
        private Panel _pnlFile;
        private Label _lblFile;
        private DropDownList _cboFiles;
        private Panel _pnlUpload;
        private HtmlInputFile _txtFile;
        private Panel _pnlButtons;
        private LinkButton _cmdCancel;
        private LinkButton _cmdSave;
        private LinkButton _cmdUpload;
        private Panel _pnlMessage;
        private Label _lblMessage;
        private Panel _pnlRightDiv;
        private Image _imgPreview;
        private bool _localize = true;
        private int _maxHeight = 100;
        private int _maxWidth = 135;

        /// <summary>
        ///   Represents a possible mode for the File Control.
        /// </summary>
        protected enum FileControlMode
        {
            /// <summary>
            ///   The File Control is in its Normal mode
            /// </summary>
            Normal,

            /// <summary>
            ///   The File Control is in the Upload File mode
            /// </summary>
            UpLoadFile,

            /// <summary>
            ///   The File Control is in the Preview mode
            /// </summary>
            Preview,
        }

        public int MaxHeight
        {
            get
            {
                return this._maxHeight;
            }

            set
            {
                this._maxHeight = value;
            }
        }

        public int MaxWidth
        {
            get
            {
                return this._maxWidth;
            }

            set
            {
                this._maxWidth = value;
            }
        }

        /// <summary>
        ///   Gets or sets the class to be used for the Labels.
        /// </summary>
        /// <remarks>
        ///   Defaults to 'CommandButton'.
        /// </remarks>
        /// <value>A String.</value>
        public string CommandCssClass
        {
            get
            {
                var cssClass = Convert.ToString(this.ViewState["CommandCssClass"]);
                return string.IsNullOrEmpty(cssClass) ? "dnnSecondaryAction" : cssClass;
            }

            set
            {
                this.ViewState["CommandCssClass"] = value;
            }
        }

        /// <summary>
        ///   Gets or sets the file Filter to use.
        /// </summary>
        /// <remarks>
        ///   Defaults to ''.
        /// </remarks>
        /// <value>a comma seperated list of file extenstions no wildcards or periods e.g. "jpg,png,gif".</value>
        public string FileFilter
        {
            get
            {
                return this.ViewState["FileFilter"] != null ? (string)this.ViewState["FileFilter"] : string.Empty;
            }

            set
            {
                this.ViewState["FileFilter"] = value;
            }
        }

        /// <summary>
        ///   Gets or sets the FileID for the control.
        /// </summary>
        /// <value>An Integer.</value>
        public int FileID
        {
            get
            {
                this.EnsureChildControls();
                if (this.ViewState["FileID"] == null)
                {
                    // Get FileId from the file combo
                    var fileId = Null.NullInteger;
                    if (this._cboFiles.SelectedItem != null)
                    {
                        fileId = int.Parse(this._cboFiles.SelectedItem.Value);
                    }

                    this.ViewState["FileID"] = fileId;
                }

                return Convert.ToInt32(this.ViewState["FileID"]);
            }

            set
            {
                this.EnsureChildControls();
                this.ViewState["FileID"] = value;
                if (string.IsNullOrEmpty(this.FilePath))
                {
                    var fileInfo = FileManager.Instance.GetFile(value);
                    if (fileInfo != null)
                    {
                        this.SetFilePath(fileInfo.Folder + fileInfo.FileName);
                    }
                }
            }
        }

        /// <summary>
        ///   Gets or sets the FilePath for the control.
        /// </summary>
        /// <value>A String.</value>
        public string FilePath
        {
            get
            {
                return Convert.ToString(this.ViewState["FilePath"]);
            }

            set
            {
                this.ViewState["FilePath"] = value;
            }
        }

        /// <summary>
        ///   Gets or sets a value indicating whether gets or sets whether to Include Personal Folder.
        /// </summary>
        /// <remarks>
        ///   Defaults to false.
        /// </remarks>
        /// <value>A Boolean.</value>
        public bool UsePersonalFolder
        {
            get
            {
                return this.ViewState["UsePersonalFolder"] != null && Convert.ToBoolean(this.ViewState["UsePersonalFolder"]);
            }

            set
            {
                this.ViewState["UsePersonalFolder"] = value;
            }
        }

        /// <summary>
        ///   Gets or sets the class to be used for the Labels.
        /// </summary>
        /// <value>A String.</value>
        public string LabelCssClass
        {
            get
            {
                var cssClass = Convert.ToString(this.ViewState["LabelCssClass"]);
                return string.IsNullOrEmpty(cssClass) ? string.Empty : cssClass;
            }

            set
            {
                this.ViewState["LabelCssClass"] = value;
            }
        }

        public string Permissions
        {
            get
            {
                var permissions = Convert.ToString(this.ViewState["Permissions"]);
                return string.IsNullOrEmpty(permissions) ? "BROWSE,ADD" : permissions;
            }

            set
            {
                this.ViewState["Permissions"] = value;
            }
        }

        /// <summary>
        ///   Gets or sets a value indicating whether gets or sets whether the combos have a "Not Specified" option.
        /// </summary>
        /// <remarks>
        ///   Defaults to True (ie no "Not Specified").
        /// </remarks>
        /// <value>A Boolean.</value>
        public bool Required
        {
            get
            {
                return this.ViewState["Required"] != null && Convert.ToBoolean(this.ViewState["Required"]);
            }

            set
            {
                this.ViewState["Required"] = value;
            }
        }

        public bool ShowFolders
        {
            get
            {
                return this.ViewState["ShowFolders"] == null || Convert.ToBoolean(this.ViewState["ShowFolders"]);
            }

            set
            {
                this.ViewState["ShowFolders"] = value;
            }
        }

        /// <summary>
        ///   Gets or sets a value indicating whether gets or sets whether to Show the Upload Button.
        /// </summary>
        /// <remarks>
        ///   Defaults to True.
        /// </remarks>
        /// <value>A Boolean.</value>
        public bool ShowUpLoad
        {
            get
            {
                return this.ViewState["ShowUpLoad"] == null || Convert.ToBoolean(this.ViewState["ShowUpLoad"]);
            }

            set
            {
                this.ViewState["ShowUpLoad"] = value;
            }
        }

        public UserInfo User { get; set; }

        public bool Localize
        {
            get
            {
                return this._localize;
            }

            set
            {
                this._localize = value;
            }
        }

        public string LocalResourceFile { get; set; }

        /// <summary>
        ///   Gets a value indicating whether gets whether the control is on a Host or Portal Tab.
        /// </summary>
        /// <value>A Boolean.</value>
        protected bool IsHost
        {
            get
            {
                var isHost = Globals.IsHostTab(this.PortalSettings.ActiveTab.TabID);

                // if not host tab but current edit user is a host user, then return true
                if (!isHost && this.User != null && this.User.IsSuperUser)
                {
                    isHost = true;
                }

                return isHost;
            }
        }

        /// <summary>
        ///   Gets the root folder for the control.
        /// </summary>
        /// <value>A String.</value>
        protected string ParentFolder
        {
            get
            {
                return this.IsHost ? Globals.HostMapPath : this.PortalSettings.HomeDirectoryMapPath;
            }
        }

        /// <summary>
        ///   Gets the file PortalId to use.
        /// </summary>
        /// <remarks>
        ///   Defaults to PortalSettings.PortalId.
        /// </remarks>
        /// <value>An Integer.</value>
        protected int PortalId
        {
            get
            {
                if ((this.Page.Request.QueryString["pid"] != null) && (Globals.IsHostTab(this.PortalSettings.ActiveTab.TabID) || UserController.Instance.GetCurrentUserInfo().IsSuperUser))
                {
                    return int.Parse(this.Page.Request.QueryString["pid"]);
                }

                if (!this.IsHost)
                {
                    return this.PortalSettings.PortalId;
                }

                return Null.NullInteger;
            }
        }

        /// <summary>
        ///   Gets the current Portal Settings.
        /// </summary>
        protected PortalSettings PortalSettings
        {
            get
            {
                return PortalController.Instance.GetCurrentPortalSettings();
            }
        }

        /// <summary>
        ///   Gets or sets the current mode of the control.
        /// </summary>
        /// <remarks>
        ///   Defaults to FileControlMode.Normal.
        /// </remarks>
        /// <value>A FileControlMode enum.</value>
        protected FileControlMode Mode
        {
            get
            {
                return this.ViewState["Mode"] == null ? FileControlMode.Normal : (FileControlMode)this.ViewState["Mode"];
            }

            set
            {
                this.ViewState["Mode"] = value;
            }
        }

        public virtual void LocalizeStrings()
        {
        }

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            this.LocalResourceFile = Utilities.GetLocalResourceFile(this);
            this.EnsureChildControls();
        }

        /// <summary>
        ///   CreateChildControls overrides the Base class's method to correctly build the
        ///   control based on the configuration.
        /// </summary>
        protected override void CreateChildControls()
        {
            // First clear the controls collection
            this.Controls.Clear();

            this._pnlContainer = new Panel { CssClass = "dnnFilePicker" };

            this._pnlLeftDiv = new Panel { CssClass = "dnnLeft" };

            this.AddFolderArea();
            this.AddFileAndUploadArea();
            this.AddButtonArea();
            this.AddMessageRow();

            this._pnlContainer.Controls.Add(this._pnlLeftDiv);

            this._pnlRightDiv = new Panel { CssClass = "dnnLeft" };

            this.GeneratePreviewImage();

            this._pnlContainer.Controls.Add(this._pnlRightDiv);

            this.Controls.Add(this._pnlContainer);

            base.CreateChildControls();
        }

        /// <summary>
        ///   OnPreRender runs just before the control is rendered.
        /// </summary>
        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);

            if (this._cboFolders.Items.Count > 0)
            {
                // Configure Labels
                this._lblFolder.Text = Utilities.GetLocalizedString("Folder");
                this._lblFolder.CssClass = this.LabelCssClass;
                this._lblFile.Text = Utilities.GetLocalizedString("File");
                this._lblFile.CssClass = this.LabelCssClass;

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
                    folderPath = string.Empty;
                }

                if (this._cboFolders.Items.FindByValue(folderPath) != null)
                {
                    this._cboFolders.SelectedIndex = -1;
                    this._cboFolders.Items.FindByValue(folderPath).Selected = true;
                }

                // Get Files
                this.LoadFiles();
                if (this._cboFiles.Items.FindByText(fileName) != null)
                {
                    this._cboFiles.Items.FindByText(fileName).Selected = true;
                }

                if (this._cboFiles.SelectedItem == null || string.IsNullOrEmpty(this._cboFiles.SelectedItem.Value))
                {
                    this.FileID = -1;
                }
                else
                {
                    this.FileID = int.Parse(this._cboFiles.SelectedItem.Value);
                }

                if (this._cboFolders.Items.Count > 1 && this.ShowFolders)
                {
                    this._pnlFolder.Visible = true;
                }
                else
                {
                    this._pnlFolder.Visible = false;
                }

                // Configure Mode
                switch (this.Mode)
                {
                    case FileControlMode.Normal:
                        this._pnlFile.Visible = true;
                        this._pnlUpload.Visible = false;
                        this._pnlRightDiv.Visible = true;
                        this.ShowImage();

                        if ((FolderPermissionController.HasFolderPermission(this.PortalId, this._cboFolders.SelectedItem.Value, "ADD") || this.IsUserFolder(this._cboFolders.SelectedItem.Value)) && this.ShowUpLoad)
                        {
                            this.ShowButton(this._cmdUpload, "Upload");
                        }

                        break;

                    case FileControlMode.UpLoadFile:
                        this._pnlFile.Visible = false;
                        this._pnlUpload.Visible = true;
                        this._pnlRightDiv.Visible = false;
                        this.ShowButton(this._cmdSave, "Save");
                        this.ShowButton(this._cmdCancel, "Cancel");
                        break;
                }
            }
            else
            {
                this._lblMessage.Text = Utilities.GetLocalizedString("NoPermission");
            }

            // Show message Row
            this._pnlMessage.Visible = !string.IsNullOrEmpty(this._lblMessage.Text);
        }

        /// <summary>
        ///   AddButton adds a button to the Command Row.
        /// </summary>
        /// <param name = "button">The button to add to the Row.</param>
        private void AddButton(ref LinkButton button)
        {
            button = new LinkButton { EnableViewState = false, CausesValidation = false };
            button.ControlStyle.CssClass = this.CommandCssClass;
            button.Visible = false;

            this._pnlButtons.Controls.Add(button);
        }

        /// <summary>
        ///   AddCommandRow adds the Command Row.
        /// </summary>
        private void AddButtonArea()
        {
            this._pnlButtons = new Panel { Visible = false };

            this.AddButton(ref this._cmdUpload);
            this._cmdUpload.Click += this.UploadFile;

            this.AddButton(ref this._cmdSave);
            this._cmdSave.Click += this.SaveFile;

            this.AddButton(ref this._cmdCancel);
            this._cmdCancel.Click += this.CancelUpload;

            this._pnlLeftDiv.Controls.Add(this._pnlButtons);
        }

        /// <summary>
        ///   AddFileRow adds the Files Row.
        /// </summary>
        private void AddFileAndUploadArea()
        {
            // Create Url Div
            this._pnlFile = new Panel { CssClass = "dnnFormItem" };

            // Create File Label
            this._lblFile = new Label { EnableViewState = false };
            this._pnlFile.Controls.Add(this._lblFile);

            // Create Files Combo
            this._cboFiles = new DropDownList { ID = "File", DataTextField = "Text", DataValueField = "Value", AutoPostBack = true };
            this._cboFiles.SelectedIndexChanged += this.FileChanged;
            this._pnlFile.Controls.Add(this._cboFiles);

            this._pnlLeftDiv.Controls.Add(this._pnlFile);

            // Create Upload Div
            this._pnlUpload = new Panel { CssClass = "dnnFormItem" };

            // Create Upload Box
            this._txtFile = new HtmlInputFile();
            this._txtFile.Attributes.Add("size", "13");
            this._pnlUpload.Controls.Add(this._txtFile);

            this._pnlLeftDiv.Controls.Add(this._pnlUpload);
        }

        /// <summary>
        ///   AddFolderRow adds the Folders Row.
        /// </summary>
        private void AddFolderArea()
        {
            // Create Url Div
            this._pnlFolder = new Panel { CssClass = "dnnFormItem" };

            // Create Folder Label
            this._lblFolder = new Label { EnableViewState = false };
            this._pnlFolder.Controls.Add(this._lblFolder);

            // Create Folders Combo
            this._cboFolders = new DropDownList { ID = "Folder", AutoPostBack = true };
            this._cboFolders.SelectedIndexChanged += this.FolderChanged;
            this._pnlFolder.Controls.Add(this._cboFolders);

            // add to left div
            this._pnlLeftDiv.Controls.Add(this._pnlFolder);

            // Load Folders
            this.LoadFolders();
        }

        /// <summary>
        ///
        /// </summary>
        private void GeneratePreviewImage()
        {
            this._imgPreview = new Image();
            this._pnlRightDiv.Controls.Add(this._imgPreview);
        }

        /// <summary>
        ///   AddMessageRow adds the Message Row.
        /// </summary>
        private void AddMessageRow()
        {
            this._pnlMessage = new Panel { CssClass = "dnnFormMessage dnnFormWarning" };

            // Create Label
            this._lblMessage = new Label { EnableViewState = false, Text = string.Empty };
            this._pnlMessage.Controls.Add(this._lblMessage);

            this._pnlLeftDiv.Controls.Add(this._pnlMessage);
        }

        private bool IsUserFolder(string folderPath)
        {
            UserInfo user = this.User ?? UserController.Instance.GetCurrentUserInfo();
            return folderPath.StartsWith("users/", StringComparison.InvariantCultureIgnoreCase) && folderPath.EndsWith(string.Format("/{0}/", user.UserID));
        }

        private void LoadFiles()
        {
            int effectivePortalId = this.PortalId;
            if (this.IsUserFolder(this._cboFolders.SelectedItem.Value))
            {
                effectivePortalId = PortalController.GetEffectivePortalId(this.PortalId);
            }

            this._cboFiles.DataSource = Globals.GetFileList(effectivePortalId, this.FileFilter, !this.Required, this._cboFolders.SelectedItem.Value);
            this._cboFiles.DataBind();
        }

        /// <summary>
        ///   LoadFolders fetches the list of folders from the Database.
        /// </summary>
        private void LoadFolders()
        {
            UserInfo user = this.User ?? UserController.Instance.GetCurrentUserInfo();
            this._cboFolders.Items.Clear();

            // Add Personal Folder
            if (this.UsePersonalFolder)
            {
                var userFolder = FolderManager.Instance.GetUserFolder(user).FolderPath;
                var userFolderItem = this._cboFolders.Items.FindByValue(userFolder);
                if (userFolderItem != null)
                {
                    userFolderItem.Text = Utilities.GetLocalizedString("MyFolder");
                }
                else
                {
                    // Add DummyFolder
                    this._cboFolders.Items.Add(new ListItem(Utilities.GetLocalizedString("MyFolder"), userFolder));
                }
            }
            else
            {
                var folders = FolderManager.Instance.GetFolders(this.PortalId, "READ,ADD", user.UserID);
                foreach (FolderInfo folder in folders)
                {
                    var folderItem = new ListItem
                    {
                        Text =
                                                 folder.FolderPath == Null.NullString
                                                     ? Utilities.GetLocalizedString("PortalRoot")
                                                     : folder.DisplayPath,
                        Value = folder.FolderPath,
                    };
                    this._cboFolders.Items.Add(folderItem);
                }
            }
        }

        /// <summary>
        ///   SetFilePath sets the FilePath property.
        /// </summary>
        /// <remarks>
        ///   This overload uses the selected item in the Folder combo.
        /// </remarks>
        private void SetFilePath()
        {
            this.SetFilePath(this._cboFiles.SelectedItem.Text);
        }

        /// <summary>
        ///   SetFilePath sets the FilePath property.
        /// </summary>
        /// <remarks>
        ///   This overload allows the caller to specify a file.
        /// </remarks>
        /// <param name = "fileName">The filename to use in setting the property.</param>
        private void SetFilePath(string fileName)
        {
            if (string.IsNullOrEmpty(this._cboFolders.SelectedItem.Value))
            {
                this.FilePath = fileName;
            }
            else
            {
                this.FilePath = (this._cboFolders.SelectedItem.Value + "/") + fileName;
            }
        }

        /// <summary>
        ///   ShowButton configures and displays a button.
        /// </summary>
        /// <param name = "button">The button to configure.</param>
        /// <param name = "command">The command name (amd key) of the button.</param>
        private void ShowButton(LinkButton button, string command)
        {
            button.Visible = true;
            if (!string.IsNullOrEmpty(command))
            {
                button.Text = Utilities.GetLocalizedString(command);
            }

            AJAX.RegisterPostBackControl(button);
            this._pnlButtons.Visible = true;
        }

        /// <summary>
        ///   ShowImage displays the Preview Image.
        /// </summary>
        private void ShowImage()
        {
            var image = (FileInfo)FileManager.Instance.GetFile(this.FileID);

            if (image != null)
            {
                this._imgPreview.ImageUrl = FileManager.Instance.GetUrl(image);
                try
                {
                    Utilities.CreateThumbnail(image, this._imgPreview, this.MaxWidth, this.MaxHeight);
                }
                catch (Exception)
                {
                    Logger.WarnFormat("Unable to create thumbnail for {0}", image.PhysicalPath);
                    this._pnlRightDiv.Visible = false;
                }
            }
            else
            {
                this._imgPreview.Visible = false;

                Panel imageHolderPanel = new Panel { CssClass = "dnnFilePickerImageHolder" };
                this._pnlRightDiv.Controls.Add(imageHolderPanel);
                this._pnlRightDiv.Visible = true;
            }
        }

        private void CancelUpload(object sender, EventArgs e)
        {
            this.Mode = FileControlMode.Normal;
        }

        private void FileChanged(object sender, EventArgs e)
        {
            this.SetFilePath();
        }

        private void FolderChanged(object sender, EventArgs e)
        {
            this.LoadFiles();
            this.SetFilePath();
        }

        private void SaveFile(object sender, EventArgs e)
        {
            // if file is selected exit
            if (!string.IsNullOrEmpty(this._txtFile.PostedFile.FileName))
            {
                var extension = Path.GetExtension(this._txtFile.PostedFile.FileName).Replace(".", string.Empty);

                if (!string.IsNullOrEmpty(this.FileFilter) && !this.FileFilter.ToLowerInvariant().Contains(extension.ToLowerInvariant()))
                {
                    // trying to upload a file not allowed for current filter
                    var localizedString = Localization.GetString("UploadError", this.LocalResourceFile);
                    if (string.IsNullOrEmpty(localizedString))
                    {
                        localizedString = Utilities.GetLocalizedString("UploadError");
                    }

                    this._lblMessage.Text = string.Format(localizedString, this.FileFilter, extension);
                }
                else
                {
                    var folderManager = FolderManager.Instance;

                    var folderPath = PathUtils.Instance.GetRelativePath(this.PortalId, this.ParentFolder) + this._cboFolders.SelectedItem.Value;

                    // Check if this is a User Folder
                    IFolderInfo folder;
                    if (this.IsUserFolder(this._cboFolders.SelectedItem.Value))
                    {
                        // Make sure the user folder exists
                        folder = folderManager.GetFolder(PortalController.GetEffectivePortalId(this.PortalId), folderPath);
                        if (folder == null)
                        {
                            // Add User folder
                            var user = this.User ?? UserController.Instance.GetCurrentUserInfo();

                            // fix user's portal id
                            user.PortalID = this.PortalId;
                            folder = ((FolderManager)folderManager).AddUserFolder(user);
                        }
                    }
                    else
                    {
                        folder = folderManager.GetFolder(this.PortalId, folderPath);
                    }

                    var fileName = Path.GetFileName(this._txtFile.PostedFile.FileName);

                    try
                    {
                        FileManager.Instance.AddFile(folder, fileName, this._txtFile.PostedFile.InputStream, true);
                    }
                    catch (PermissionsNotMetException)
                    {
                        this._lblMessage.Text += "<br />" + string.Format(Localization.GetString("InsufficientFolderPermission"), folder.FolderPath);
                    }
                    catch (NoSpaceAvailableException)
                    {
                        this._lblMessage.Text += "<br />" + string.Format(Localization.GetString("DiskSpaceExceeded"), fileName);
                    }
                    catch (InvalidFileExtensionException)
                    {
                        this._lblMessage.Text += "<br />" + string.Format(Localization.GetString("RestrictedFileType"), fileName, Host.AllowedExtensionWhitelist.ToDisplayString());
                    }
                    catch (Exception ex)
                    {
                        Logger.Error(ex);

                        this._lblMessage.Text += "<br />" + string.Format(Localization.GetString("SaveFileError"), fileName);
                    }
                }

                if (string.IsNullOrEmpty(this._lblMessage.Text))
                {
                    var fileName = this._txtFile.PostedFile.FileName.Substring(this._txtFile.PostedFile.FileName.LastIndexOf("\\") + 1);
                    this.SetFilePath(fileName);
                }
            }

            this.Mode = FileControlMode.Normal;
        }

        private void UploadFile(object sender, EventArgs e)
        {
            this.Mode = FileControlMode.UpLoadFile;
        }
    }
}
