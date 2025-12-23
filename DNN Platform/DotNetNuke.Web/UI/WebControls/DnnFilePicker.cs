// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Web.UI.WebControls
{
    using System;
    using System.Diagnostics.CodeAnalysis;
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

    /// <summary>  The FilePicker Class provides a File Picker Control for DotNetNuke.</summary>
    public class DnnFilePicker : CompositeControl, ILocalizable
    {
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(DnnFilePicker));

        private Panel pnlContainer;
        private Panel pnlLeftDiv;
        private Panel pnlFolder;

        private Label lblFolder;
        private DropDownList cboFolders;
        private Panel pnlFile;
        private Label lblFile;
        private DropDownList cboFiles;
        private Panel pnlUpload;
        private HtmlInputFile txtFile;
        private Panel pnlButtons;
        private LinkButton cmdCancel;
        private LinkButton cmdSave;
        private LinkButton cmdUpload;
        private Panel pnlMessage;
        private Label lblMessage;
        private Panel pnlRightDiv;
        private Image imgPreview;
        private bool localize = true;

        /// <summary>  Represents a possible mode for the File Control.</summary>
        protected enum FileControlMode
        {
            /// <summary>  The File Control is in its Normal mode.</summary>
            Normal = 0,

            /// <summary>  The File Control is in the Upload File mode.</summary>
            UpLoadFile = 1,

            /// <summary>  The File Control is in the Preview mode.</summary>
            Preview = 2,
        }

        public int MaxHeight { get; set; } = 100;

        public int MaxWidth { get; set; } = 135;

        /// <summary>  Gets or sets the class to be used for the Labels.</summary>
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

        /// <summary>  Gets or sets the file Filter to use.</summary>
        /// <remarks>
        ///   Defaults to ''.
        /// </remarks>
        /// <value>a comma seperated list of file extensions no wildcards or periods e.g. "jpg,png,gif".</value>
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

        /// <summary>  Gets or sets the FileID for the control.</summary>
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
                    if (this.cboFiles.SelectedItem != null)
                    {
                        fileId = int.Parse(this.cboFiles.SelectedItem.Value);
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

        /// <summary>  Gets or sets the FilePath for the control.</summary>
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

        /// <summary>  Gets or sets a value indicating whether to Include Personal Folder.</summary>
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

        /// <summary>  Gets or sets the class to be used for the Labels.</summary>
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

        /// <summary>  Gets or sets a value indicating whether the combos have a "Not Specified" option.</summary>
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

        /// <summary>  Gets or sets a value indicating whether to Show the Upload Button.</summary>
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

        /// <inheritdoc/>
        public bool Localize
        {
            get
            {
                return this.localize;
            }

            set
            {
                this.localize = value;
            }
        }

        /// <inheritdoc/>
        public string LocalResourceFile { get; set; }

        /// <summary>Gets a value indicating whether the control is on a Host or Portal Tab.</summary>
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

        /// <summary>  Gets the root folder for the control.</summary>
        /// <value>A String.</value>
        protected string ParentFolder
        {
            get
            {
                return this.IsHost ? Globals.HostMapPath : this.PortalSettings.HomeDirectoryMapPath;
            }
        }

        /// <summary>  Gets the file PortalId to use.</summary>
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

        /// <summary>  Gets the current Portal Settings.</summary>
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Breaking change")]
        protected PortalSettings PortalSettings => PortalController.Instance.GetCurrentPortalSettings();

        /// <summary>  Gets or sets the current mode of the control.</summary>
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

        /// <inheritdoc/>
        public virtual void LocalizeStrings()
        {
        }

        /// <inheritdoc/>
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

            this.pnlContainer = new Panel { CssClass = "dnnFilePicker" };

            this.pnlLeftDiv = new Panel { CssClass = "dnnLeft" };

            this.AddFolderArea();
            this.AddFileAndUploadArea();
            this.AddButtonArea();
            this.AddMessageRow();

            this.pnlContainer.Controls.Add(this.pnlLeftDiv);

            this.pnlRightDiv = new Panel { CssClass = "dnnLeft" };

            this.GeneratePreviewImage();

            this.pnlContainer.Controls.Add(this.pnlRightDiv);

            this.Controls.Add(this.pnlContainer);

            base.CreateChildControls();
        }

        /// <summary>OnPreRender runs just before the control is rendered.</summary>
        /// <param name="e">The event arguments.</param>
        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);

            if (this.cboFolders.Items.Count > 0)
            {
                // Configure Labels
                this.lblFolder.Text = Utilities.GetLocalizedString("Folder");
                this.lblFolder.CssClass = this.LabelCssClass;
                this.lblFile.Text = Utilities.GetLocalizedString("File");
                this.lblFile.CssClass = this.LabelCssClass;

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

                if (this.cboFolders.Items.FindByValue(folderPath) != null)
                {
                    this.cboFolders.SelectedIndex = -1;
                    this.cboFolders.Items.FindByValue(folderPath).Selected = true;
                }

                // Get Files
                this.LoadFiles();
                if (this.cboFiles.Items.FindByText(fileName) != null)
                {
                    this.cboFiles.Items.FindByText(fileName).Selected = true;
                }

                if (this.cboFiles.SelectedItem == null || string.IsNullOrEmpty(this.cboFiles.SelectedItem.Value))
                {
                    this.FileID = -1;
                }
                else
                {
                    this.FileID = int.Parse(this.cboFiles.SelectedItem.Value);
                }

                if (this.cboFolders.Items.Count > 1 && this.ShowFolders)
                {
                    this.pnlFolder.Visible = true;
                }
                else
                {
                    this.pnlFolder.Visible = false;
                }

                // Configure Mode
                switch (this.Mode)
                {
                    case FileControlMode.Normal:
                        this.pnlFile.Visible = true;
                        this.pnlUpload.Visible = false;
                        this.pnlRightDiv.Visible = true;
                        this.ShowImage();

                        if ((FolderPermissionController.HasFolderPermission(this.PortalId, this.cboFolders.SelectedItem.Value, "ADD") || this.IsUserFolder(this.cboFolders.SelectedItem.Value)) && this.ShowUpLoad)
                        {
                            this.ShowButton(this.cmdUpload, "Upload");
                        }

                        break;

                    case FileControlMode.UpLoadFile:
                        this.pnlFile.Visible = false;
                        this.pnlUpload.Visible = true;
                        this.pnlRightDiv.Visible = false;
                        this.ShowButton(this.cmdSave, "Save");
                        this.ShowButton(this.cmdCancel, "Cancel");
                        break;
                }
            }
            else
            {
                this.lblMessage.Text = Utilities.GetLocalizedString("NoPermission");
            }

            // Show message Row
            this.pnlMessage.Visible = !string.IsNullOrEmpty(this.lblMessage.Text);
        }

        /// <summary>  AddButton adds a button to the Command Row.</summary>
        /// <param name="button">The button to add to the Row.</param>
        private void AddButton(ref LinkButton button)
        {
            button = new LinkButton { EnableViewState = false, CausesValidation = false };
            button.ControlStyle.CssClass = this.CommandCssClass;
            button.Visible = false;

            this.pnlButtons.Controls.Add(button);
        }

        /// <summary>  AddCommandRow adds the Command Row.</summary>
        private void AddButtonArea()
        {
            this.pnlButtons = new Panel { Visible = false };

            this.AddButton(ref this.cmdUpload);
            this.cmdUpload.Click += this.UploadFile;

            this.AddButton(ref this.cmdSave);
            this.cmdSave.Click += this.SaveFile;

            this.AddButton(ref this.cmdCancel);
            this.cmdCancel.Click += this.CancelUpload;

            this.pnlLeftDiv.Controls.Add(this.pnlButtons);
        }

        /// <summary>  AddFileRow adds the Files Row.</summary>
        private void AddFileAndUploadArea()
        {
            // Create Url Div
            this.pnlFile = new Panel { CssClass = "dnnFormItem" };

            // Create File Label
            this.lblFile = new Label { EnableViewState = false };
            this.pnlFile.Controls.Add(this.lblFile);

            // Create Files Combo
            this.cboFiles = new DropDownList { ID = "File", DataTextField = "Text", DataValueField = "Value", AutoPostBack = true };
            this.cboFiles.SelectedIndexChanged += this.FileChanged;
            this.pnlFile.Controls.Add(this.cboFiles);

            this.pnlLeftDiv.Controls.Add(this.pnlFile);

            // Create Upload Div
            this.pnlUpload = new Panel { CssClass = "dnnFormItem" };

            // Create Upload Box
            this.txtFile = new HtmlInputFile();
            this.txtFile.Attributes.Add("size", "13");
            this.pnlUpload.Controls.Add(this.txtFile);

            this.pnlLeftDiv.Controls.Add(this.pnlUpload);
        }

        /// <summary>  AddFolderRow adds the Folders Row.</summary>
        private void AddFolderArea()
        {
            // Create Url Div
            this.pnlFolder = new Panel { CssClass = "dnnFormItem" };

            // Create Folder Label
            this.lblFolder = new Label { EnableViewState = false };
            this.pnlFolder.Controls.Add(this.lblFolder);

            // Create Folders Combo
            this.cboFolders = new DropDownList { ID = "Folder", AutoPostBack = true };
            this.cboFolders.SelectedIndexChanged += this.FolderChanged;
            this.pnlFolder.Controls.Add(this.cboFolders);

            // add to left div
            this.pnlLeftDiv.Controls.Add(this.pnlFolder);

            // Load Folders
            this.LoadFolders();
        }

        /// <summary>Adds an empty preview image to the <see cref="pnlRightDiv"/> control.</summary>
        private void GeneratePreviewImage()
        {
            this.imgPreview = new Image();
            this.pnlRightDiv.Controls.Add(this.imgPreview);
        }

        /// <summary>  AddMessageRow adds the Message Row.</summary>
        private void AddMessageRow()
        {
            this.pnlMessage = new Panel { CssClass = "dnnFormMessage dnnFormWarning" };

            // Create Label
            this.lblMessage = new Label { EnableViewState = false, Text = string.Empty };
            this.pnlMessage.Controls.Add(this.lblMessage);

            this.pnlLeftDiv.Controls.Add(this.pnlMessage);
        }

        private bool IsUserFolder(string folderPath)
        {
            UserInfo user = this.User ?? UserController.Instance.GetCurrentUserInfo();
            return folderPath.StartsWith("users/", StringComparison.InvariantCultureIgnoreCase) && folderPath.EndsWith(string.Format("/{0}/", user.UserID));
        }

        private void LoadFiles()
        {
            int effectivePortalId = this.PortalId;
            if (this.IsUserFolder(this.cboFolders.SelectedItem.Value))
            {
                effectivePortalId = PortalController.GetEffectivePortalId(this.PortalId);
            }

            this.cboFiles.DataSource = Globals.GetFileList(effectivePortalId, this.FileFilter, !this.Required, this.cboFolders.SelectedItem.Value);
            this.cboFiles.DataBind();
        }

        /// <summary>  LoadFolders fetches the list of folders from the Database.</summary>
        private void LoadFolders()
        {
            UserInfo user = this.User ?? UserController.Instance.GetCurrentUserInfo();
            this.cboFolders.Items.Clear();

            // Add Personal Folder
            if (this.UsePersonalFolder)
            {
                var userFolder = FolderManager.Instance.GetUserFolder(user).FolderPath;
                var userFolderItem = this.cboFolders.Items.FindByValue(userFolder);
                if (userFolderItem != null)
                {
                    userFolderItem.Text = Utilities.GetLocalizedString("MyFolder");
                }
                else
                {
                    // Add DummyFolder
                    this.cboFolders.Items.Add(new ListItem(Utilities.GetLocalizedString("MyFolder"), userFolder));
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
                    this.cboFolders.Items.Add(folderItem);
                }
            }
        }

        /// <summary>  SetFilePath sets the FilePath property.</summary>
        /// <remarks>
        ///   This overload uses the selected item in the Folder combo.
        /// </remarks>
        private void SetFilePath()
        {
            this.SetFilePath(this.cboFiles.SelectedItem.Text);
        }

        /// <summary>  SetFilePath sets the FilePath property.</summary>
        /// <remarks>
        ///   This overload allows the caller to specify a file.
        /// </remarks>
        /// <param name="fileName">The filename to use in setting the property.</param>
        private void SetFilePath(string fileName)
        {
            if (string.IsNullOrEmpty(this.cboFolders.SelectedItem.Value))
            {
                this.FilePath = fileName;
            }
            else
            {
                this.FilePath = (this.cboFolders.SelectedItem.Value + "/") + fileName;
            }
        }

        /// <summary>  ShowButton configures and displays a button.</summary>
        /// <param name="button">The button to configure.</param>
        /// <param name="command">The command name (amd key) of the button.</param>
        private void ShowButton(LinkButton button, string command)
        {
            button.Visible = true;
            if (!string.IsNullOrEmpty(command))
            {
                button.Text = Utilities.GetLocalizedString(command);
            }

            AJAX.RegisterPostBackControl(button);
            this.pnlButtons.Visible = true;
        }

        /// <summary>  ShowImage displays the Preview Image.</summary>
        private void ShowImage()
        {
            var image = (FileInfo)FileManager.Instance.GetFile(this.FileID);

            if (image != null)
            {
                this.imgPreview.ImageUrl = FileManager.Instance.GetUrl(image);
                try
                {
                    Utilities.CreateThumbnail(image, this.imgPreview, this.MaxWidth, this.MaxHeight);
                }
                catch (Exception)
                {
                    Logger.WarnFormat("Unable to create thumbnail for {0}", image.PhysicalPath);
                    this.pnlRightDiv.Visible = false;
                }
            }
            else
            {
                this.imgPreview.Visible = false;

                Panel imageHolderPanel = new Panel { CssClass = "dnnFilePickerImageHolder" };
                this.pnlRightDiv.Controls.Add(imageHolderPanel);
                this.pnlRightDiv.Visible = true;
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
            if (!string.IsNullOrEmpty(this.txtFile.PostedFile.FileName))
            {
                var extension = Path.GetExtension(this.txtFile.PostedFile.FileName).Replace(".", string.Empty);

                if (!string.IsNullOrEmpty(this.FileFilter) && !this.FileFilter.ToLowerInvariant().Contains(extension.ToLowerInvariant()))
                {
                    // trying to upload a file not allowed for current filter
                    var localizedString = Localization.GetString("UploadError", this.LocalResourceFile);
                    if (string.IsNullOrEmpty(localizedString))
                    {
                        localizedString = Utilities.GetLocalizedString("UploadError");
                    }

                    this.lblMessage.Text = string.Format(localizedString, this.FileFilter, extension);
                }
                else
                {
                    var folderManager = FolderManager.Instance;

                    var folderPath = PathUtils.Instance.GetRelativePath(this.PortalId, this.ParentFolder) + this.cboFolders.SelectedItem.Value;

                    // Check if this is a User Folder
                    IFolderInfo folder;
                    if (this.IsUserFolder(this.cboFolders.SelectedItem.Value))
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

                    var fileName = Path.GetFileName(this.txtFile.PostedFile.FileName);

                    try
                    {
                        FileManager.Instance.AddFile(folder, fileName, this.txtFile.PostedFile.InputStream, true, true, FileContentTypeManager.Instance.GetContentType(Path.GetExtension(fileName)));
                    }
                    catch (PermissionsNotMetException)
                    {
                        this.lblMessage.Text += "<br />" + string.Format(Localization.GetString("InsufficientFolderPermission"), folder.FolderPath);
                    }
                    catch (NoSpaceAvailableException)
                    {
                        this.lblMessage.Text += "<br />" + string.Format(Localization.GetString("DiskSpaceExceeded"), fileName);
                    }
                    catch (InvalidFileExtensionException)
                    {
                        this.lblMessage.Text += "<br />" + string.Format(Localization.GetString("RestrictedFileType"), fileName, Host.AllowedExtensionWhitelist.ToDisplayString());
                    }
                    catch (Exception ex)
                    {
                        Logger.Error(ex);

                        this.lblMessage.Text += "<br />" + string.Format(Localization.GetString("SaveFileError"), fileName);
                    }
                }

                if (string.IsNullOrEmpty(this.lblMessage.Text))
                {
                    var fileName = this.txtFile.PostedFile.FileName.Substring(this.txtFile.PostedFile.FileName.LastIndexOf("\\") + 1);
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
