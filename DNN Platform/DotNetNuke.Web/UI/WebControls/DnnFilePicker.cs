#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2017
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
#region Usings

using System;
using System.IO;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using DotNetNuke.Common;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;
using DotNetNuke.Framework;
using DotNetNuke.Instrumentation;
using DotNetNuke.Security.Permissions;
using DotNetNuke.Services.FileSystem;
using DotNetNuke.Services.Localization;

using FileInfo = DotNetNuke.Services.FileSystem.FileInfo;
using DotNetNuke.Entities.Host;

#endregion

namespace DotNetNuke.Web.UI.WebControls
{

	/// <summary>
	///   The FilePicker Class provides a File Picker Control for DotNetNuke
	/// </summary>
	public class DnnFilePicker : CompositeControl, ILocalizable
	{
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof (DnnFilePicker));

		#region Public Enums

		/// <summary>
		///   Represents a possible mode for the File Control
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
			Preview
		}

		#endregion

		#region Private Members

		#region Controls

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

		#endregion

		private bool _localize = true;
		private int _maxHeight = 100;
		private int _maxWidth = 135;

		#endregion

		#region Protected Properties

		/// <summary>
		///   Gets whether the control is on a Host or Portal Tab
		/// </summary>
		/// <value>A Boolean</value>
		protected bool IsHost
		{
			get
			{
				var isHost = Globals.IsHostTab(PortalSettings.ActiveTab.TabID);
                //if not host tab but current edit user is a host user, then return true
                if(!isHost && User != null && User.IsSuperUser)
                {
                    isHost = true;
                }

			    return isHost;
			}
		}

		public int MaxHeight
		{
			get
			{
				return _maxHeight;
			}
			set
			{
				_maxHeight = value;
			}
		}

		public int MaxWidth
		{
			get
			{
				return _maxWidth;
			}
			set
			{
				_maxWidth = value;
			}
		}

		/// <summary>
		///   Gets or sets the current mode of the control
		/// </summary>
		/// <remarks>
		///   Defaults to FileControlMode.Normal
		/// </remarks>
		/// <value>A FileControlMode enum</value>
		protected FileControlMode Mode
		{
			get
			{
				return ViewState["Mode"] == null ? FileControlMode.Normal : (FileControlMode) ViewState["Mode"];
			}
			set
			{
				ViewState["Mode"] = value;
			}
		}

		/// <summary>
		///   Gets the root folder for the control
		/// </summary>
		/// <value>A String</value>
		protected string ParentFolder
		{
			get
			{
                return IsHost ? Globals.HostMapPath : PortalSettings.HomeDirectoryMapPath;
			}
		}

	    /// <summary>
	    ///   Gets or sets the file PortalId to use
	    /// </summary>
	    /// <remarks>
	    ///   Defaults to PortalSettings.PortalId
	    /// </remarks>
	    /// <value>An Integer</value>        
        protected int PortalId
		{
			get
			{
                if ((Page.Request.QueryString["pid"] != null) && (Globals.IsHostTab(PortalSettings.ActiveTab.TabID) || UserController.Instance.GetCurrentUserInfo().IsSuperUser))
                {
                    return Int32.Parse(Page.Request.QueryString["pid"]);
                }
			    if (!IsHost)
			    {
			        return PortalSettings.PortalId;
			    }

			    return Null.NullInteger;			    
			}
		}

		/// <summary>
		///   Gets the current Portal Settings
		/// </summary>
		protected PortalSettings PortalSettings
		{
			get
			{
				return PortalController.Instance.GetCurrentPortalSettings();
			}
		}

		#endregion

		#region Public Properties

		/// <summary>
		///   Gets or sets the class to be used for the Labels
		/// </summary>
		/// <remarks>
		///   Defaults to 'CommandButton'
		/// </remarks>
		/// <value>A String</value>
		public string CommandCssClass
		{
			get
			{
				var cssClass = Convert.ToString(ViewState["CommandCssClass"]);
				return string.IsNullOrEmpty(cssClass) ? "dnnSecondaryAction" : cssClass;
			}
			set
			{
				ViewState["CommandCssClass"] = value;
			}
		}

		/// <summary>
		///   Gets or sets the file Filter to use
		/// </summary>
		/// <remarks>
		///   Defaults to ''
		/// </remarks>
		/// <value>a comma seperated list of file extenstions no wildcards or periods e.g. "jpg,png,gif"</value>
		public string FileFilter
		{
			get
			{
				return ViewState["FileFilter"] != null ? (string) ViewState["FileFilter"] : "";
			}
			set
			{
				ViewState["FileFilter"] = value;
			}
		}

		/// <summary>
		///   Gets or sets the FileID for the control
		/// </summary>
		/// <value>An Integer</value>
		public int FileID
		{
			get
			{
				EnsureChildControls();
				if (ViewState["FileID"] == null)
				{
					//Get FileId from the file combo
					var fileId = Null.NullInteger;
					if (_cboFiles.SelectedItem != null)
					{
						fileId = Int32.Parse(_cboFiles.SelectedItem.Value);
					}
					ViewState["FileID"] = fileId;
				}
				return Convert.ToInt32(ViewState["FileID"]);
			}
			set
			{
				EnsureChildControls();
				ViewState["FileID"] = value;
				if (string.IsNullOrEmpty(FilePath))
				{
					var fileInfo = FileManager.Instance.GetFile(value);
					if (fileInfo != null)
					{
						SetFilePath(fileInfo.Folder + fileInfo.FileName);
					}
				}
			}
		}

		/// <summary>
		///   Gets or sets the FilePath for the control
		/// </summary>
		/// <value>A String</value>
		public string FilePath
		{
			get
			{
				return Convert.ToString(ViewState["FilePath"]);
			}
			set
			{
				ViewState["FilePath"] = value;
			}
		}

		/// <summary>
		///   Gets or sets whether to Include Personal Folder
		/// </summary>
		/// <remarks>
		///   Defaults to false
		/// </remarks>
		/// <value>A Boolean</value>
		public bool UsePersonalFolder
		{
			get
			{
				return ViewState["UsePersonalFolder"] != null && Convert.ToBoolean(ViewState["UsePersonalFolder"]);
			}
			set
			{
				ViewState["UsePersonalFolder"] = value;
			}
		}

		/// <summary>
		///   Gets or sets the class to be used for the Labels
		/// </summary>
		/// <value>A String</value>
		public string LabelCssClass
		{
			get
			{
				var cssClass = Convert.ToString(ViewState["LabelCssClass"]);
				return string.IsNullOrEmpty(cssClass) ? "" : cssClass;
			}
			set
			{
				ViewState["LabelCssClass"] = value;
			}
		}

		public string Permissions
		{
			get
			{
				var permissions = Convert.ToString(ViewState["Permissions"]);
				return string.IsNullOrEmpty(permissions) ? "BROWSE,ADD" : permissions;
			}
			set
			{
				ViewState["Permissions"] = value;
			}
		}

		/// <summary>
		///   Gets or sets whether the combos have a "Not Specified" option
		/// </summary>
		/// <remarks>
		///   Defaults to True (ie no "Not Specified")
		/// </remarks>
		/// <value>A Boolean</value>
		public bool Required
		{
			get
			{
				return ViewState["Required"] != null && Convert.ToBoolean(ViewState["Required"]);
			}
			set
			{
				ViewState["Required"] = value;
			}
		}

		public bool ShowFolders
		{
			get
			{
				return ViewState["ShowFolders"] == null || Convert.ToBoolean(ViewState["ShowFolders"]);
			}
			set
			{
				ViewState["ShowFolders"] = value;
			}
		}

		/// <summary>
		///   Gets or sets whether to Show the Upload Button
		/// </summary>
		/// <remarks>
		///   Defaults to True
		/// </remarks>
		/// <value>A Boolean</value>
		public bool ShowUpLoad
		{
			get
			{
				return ViewState["ShowUpLoad"] == null || Convert.ToBoolean(ViewState["ShowUpLoad"]);
			}
			set
			{
				ViewState["ShowUpLoad"] = value;
			}
		}

        public UserInfo User { get; set; }

		#endregion

		#region Private Methods

		/// <summary>
		///   AddButton adds a button to the Command Row
		/// </summary>
		/// <param name = "button">The button to add to the Row</param>
		private void AddButton(ref LinkButton button)
		{
		    button = new LinkButton {EnableViewState = false, CausesValidation = false};
		    button.ControlStyle.CssClass = CommandCssClass;
			button.Visible = false;

			_pnlButtons.Controls.Add(button);
		}

		/// <summary>
		///   AddCommandRow adds the Command Row
		/// </summary>
		private void AddButtonArea()
		{
		    _pnlButtons = new Panel {Visible = false};

		    AddButton(ref _cmdUpload);
			_cmdUpload.Click += UploadFile;

			AddButton(ref _cmdSave);
			_cmdSave.Click += SaveFile;

			AddButton(ref _cmdCancel);
			_cmdCancel.Click += CancelUpload;

			_pnlLeftDiv.Controls.Add(_pnlButtons);
		}

		/// <summary>
		///   AddFileRow adds the Files Row
		/// </summary>
		private void AddFileAndUploadArea()
		{
			//Create Url Div
		    _pnlFile = new Panel {CssClass = "dnnFormItem"};

		    //Create File Label
		    _lblFile = new Label {EnableViewState = false};
		    _pnlFile.Controls.Add(_lblFile);

			//Create Files Combo
		    _cboFiles = new DropDownList {ID = "File", DataTextField = "Text", DataValueField = "Value", AutoPostBack = true};
		    _cboFiles.SelectedIndexChanged += FileChanged;
			_pnlFile.Controls.Add(_cboFiles);

			_pnlLeftDiv.Controls.Add(_pnlFile);

			//Create Upload Div
		    _pnlUpload = new Panel {CssClass = "dnnFormItem"};

		    //Create Upload Box
			_txtFile = new HtmlInputFile();
			_txtFile.Attributes.Add("size", "13");
			_pnlUpload.Controls.Add(_txtFile);

			_pnlLeftDiv.Controls.Add(_pnlUpload);
		}

		/// <summary>
		///   AddFolderRow adds the Folders Row
		/// </summary>
		private void AddFolderArea()
		{
			//Create Url Div
		    _pnlFolder = new Panel {CssClass = "dnnFormItem"};

		    //Create Folder Label
		    _lblFolder = new Label {EnableViewState = false};
		    _pnlFolder.Controls.Add(_lblFolder);

			//Create Folders Combo
		    _cboFolders = new DropDownList {ID = "Folder", AutoPostBack = true};
		    _cboFolders.SelectedIndexChanged += FolderChanged;
			_pnlFolder.Controls.Add(_cboFolders);

			// add to left div
			_pnlLeftDiv.Controls.Add(_pnlFolder);

			//Load Folders
			LoadFolders();
		}

		/// <summary>
		/// 
		/// </summary>
		private void GeneratePreviewImage()
		{
			_imgPreview = new Image();
			_pnlRightDiv.Controls.Add(_imgPreview);
		}

		/// <summary>
		///   AddMessageRow adds the Message Row
		/// </summary>
		private void AddMessageRow()
		{
		    _pnlMessage = new Panel {CssClass = "dnnFormMessage dnnFormWarning"};

		    //Create Label
		    _lblMessage = new Label {EnableViewState = false, Text = ""};
		    _pnlMessage.Controls.Add(_lblMessage);

			_pnlLeftDiv.Controls.Add(_pnlMessage);
		}

		private bool IsUserFolder(string folderPath)
		{
            UserInfo user = User ?? UserController.Instance.GetCurrentUserInfo();
            return (folderPath.ToLowerInvariant().StartsWith("users/") && folderPath.EndsWith(string.Format("/{0}/", user.UserID)));
		}

		private void LoadFiles()
		{
		    int effectivePortalId = PortalId;
            if (IsUserFolder(_cboFolders.SelectedItem.Value))
            {
                effectivePortalId = PortalController.GetEffectivePortalId(PortalId);
            }
            _cboFiles.DataSource = Globals.GetFileList(effectivePortalId, FileFilter, !Required, _cboFolders.SelectedItem.Value);							
			_cboFiles.DataBind();
		}

		/// <summary>
		///   LoadFolders fetches the list of folders from the Database
		/// </summary>
		private void LoadFolders()
		{
            UserInfo user = User ?? UserController.Instance.GetCurrentUserInfo();
            _cboFolders.Items.Clear();

			//Add Personal Folder
			if (UsePersonalFolder)
			{
			    var userFolder = FolderManager.Instance.GetUserFolder(user).FolderPath;
				var userFolderItem = _cboFolders.Items.FindByValue(userFolder);
				if (userFolderItem != null)
				{
					userFolderItem.Text = Utilities.GetLocalizedString("MyFolder");
				}
				else
				{
					//Add DummyFolder
					_cboFolders.Items.Add(new ListItem(Utilities.GetLocalizedString("MyFolder"), userFolder));
				}
			}
			else
			{
                var folders = FolderManager.Instance.GetFolders(PortalId, "READ,ADD", user.UserID);
				foreach (FolderInfo folder in folders)
				{
				    var folderItem = new ListItem
				                         {
				                             Text =
				                                 folder.FolderPath == Null.NullString
				                                     ? Utilities.GetLocalizedString("PortalRoot")
				                                     : folder.DisplayPath,
				                             Value = folder.FolderPath
				                         };
				    _cboFolders.Items.Add(folderItem);
				}
			}
		}

		/// <summary>
		///   SetFilePath sets the FilePath property
		/// </summary>
		/// <remarks>
		///   This overload uses the selected item in the Folder combo
		/// </remarks>
		private void SetFilePath()
		{
			SetFilePath(_cboFiles.SelectedItem.Text);
		}

		/// <summary>
		///   SetFilePath sets the FilePath property
		/// </summary>
		/// <remarks>
		///   This overload allows the caller to specify a file
		/// </remarks>
		/// <param name = "fileName">The filename to use in setting the property</param>
		private void SetFilePath(string fileName)
		{
			if (string.IsNullOrEmpty(_cboFolders.SelectedItem.Value))
			{
				FilePath = fileName;
			}
			else
			{
				FilePath = (_cboFolders.SelectedItem.Value + "/") + fileName;
			}
		}

		/// <summary>
		///   ShowButton configures and displays a button
		/// </summary>
		/// <param name = "button">The button to configure</param>
		/// <param name = "command">The command name (amd key) of the button</param>
		private void ShowButton(LinkButton button, string command)
		{
			button.Visible = true;
			if (!string.IsNullOrEmpty(command))
			{
				button.Text = Utilities.GetLocalizedString(command);
			}
			AJAX.RegisterPostBackControl(button);
			_pnlButtons.Visible = true;
		}

		/// <summary>
		///   ShowImage displays the Preview Image
		/// </summary>
		private void ShowImage()
		{
			var image = (FileInfo)FileManager.Instance.GetFile(FileID);

            if (image != null)
            {
                _imgPreview.ImageUrl = FileManager.Instance.GetUrl(image);
                try
                {
                    Utilities.CreateThumbnail(image, _imgPreview, MaxWidth, MaxHeight);
                }
                catch (Exception)
                {
                    Logger.WarnFormat("Unable to create thumbnail for {0}", image.PhysicalPath);
                    _pnlRightDiv.Visible = false;
                }

            }
            else
            {
                _imgPreview.Visible = false;

                Panel imageHolderPanel = new Panel { CssClass = "dnnFilePickerImageHolder" };
                _pnlRightDiv.Controls.Add(imageHolderPanel);
                _pnlRightDiv.Visible = true;
            }
		}

		#endregion

		#region Protected Methods

		protected override void OnInit(EventArgs e)
		{
			base.OnInit(e);
			LocalResourceFile = Utilities.GetLocalResourceFile(this);
			EnsureChildControls();
		}

	    /// <summary>
		///   CreateChildControls overrides the Base class's method to correctly build the
		///   control based on the configuration
		/// </summary>
		protected override void CreateChildControls()
		{
			//First clear the controls collection
			Controls.Clear();

	        _pnlContainer = new Panel {CssClass = "dnnFilePicker"};

	        _pnlLeftDiv = new Panel {CssClass = "dnnLeft"};

	        AddFolderArea();
			AddFileAndUploadArea();
			AddButtonArea();
			AddMessageRow();

			_pnlContainer.Controls.Add(_pnlLeftDiv);

	        _pnlRightDiv = new Panel {CssClass = "dnnLeft"};

	        GeneratePreviewImage();
		
			_pnlContainer.Controls.Add(_pnlRightDiv);

			Controls.Add(_pnlContainer);

			base.CreateChildControls();
		}

		/// <summary>
		///   OnPreRender runs just before the control is rendered
		/// </summary>
		protected override void OnPreRender(EventArgs e)
		{
			base.OnPreRender(e);

			if (_cboFolders.Items.Count > 0)
			{
				//Configure Labels
				_lblFolder.Text = Utilities.GetLocalizedString("Folder");
				_lblFolder.CssClass = LabelCssClass;
				_lblFile.Text = Utilities.GetLocalizedString("File");
				_lblFile.CssClass = LabelCssClass;

				//select folder
				string fileName;
				string folderPath;
				if (!string.IsNullOrEmpty(FilePath))
				{
					fileName = FilePath.Substring(FilePath.LastIndexOf("/") + 1);
					folderPath = string.IsNullOrEmpty(fileName) ? FilePath : FilePath.Replace(fileName, "");
				}
				else
				{
					fileName = FilePath;
					folderPath = string.Empty;
				}

				if (_cboFolders.Items.FindByValue(folderPath) != null)
				{
					_cboFolders.SelectedIndex = -1;
					_cboFolders.Items.FindByValue(folderPath).Selected = true;
				}

				//Get Files
				LoadFiles();
				if (_cboFiles.Items.FindByText(fileName) != null)
				{
					_cboFiles.Items.FindByText(fileName).Selected = true;
				}
				if (_cboFiles.SelectedItem == null || string.IsNullOrEmpty(_cboFiles.SelectedItem.Value))
				{
					FileID = -1;
				}
				else
				{
					FileID = Int32.Parse(_cboFiles.SelectedItem.Value);
				}

				if (_cboFolders.Items.Count > 1 && ShowFolders)
				{
					_pnlFolder.Visible = true;
				}
				else
				{
					_pnlFolder.Visible = false;
				}
				//Configure Mode
				switch (Mode)
				{
					case FileControlMode.Normal:
						_pnlFile.Visible = true;
						_pnlUpload.Visible = false;
						_pnlRightDiv.Visible = true;
						ShowImage();

						if ((FolderPermissionController.HasFolderPermission(PortalId, _cboFolders.SelectedItem.Value, "ADD") || IsUserFolder(_cboFolders.SelectedItem.Value)) && ShowUpLoad)
						{
							ShowButton(_cmdUpload, "Upload");
						}
						break;

					case FileControlMode.UpLoadFile:
						_pnlFile.Visible = false;
						_pnlUpload.Visible = true;
                        _pnlRightDiv.Visible = false;
						ShowButton(_cmdSave, "Save");
						ShowButton(_cmdCancel, "Cancel");
						break;
				}
			}
			else
			{
				_lblMessage.Text = Utilities.GetLocalizedString("NoPermission");
			}

			//Show message Row
			_pnlMessage.Visible = (!string.IsNullOrEmpty(_lblMessage.Text));
		}

		#endregion

		#region Event Handlers

		private void CancelUpload(object sender, EventArgs e)
		{
			Mode = FileControlMode.Normal;
		}

		private void FileChanged(object sender, EventArgs e)
		{
			SetFilePath();
		}

		private void FolderChanged(object sender, EventArgs e)
		{
			LoadFiles();
			SetFilePath();
		}

		private void SaveFile(object sender, EventArgs e)
		{
			//if file is selected exit
			if (!string.IsNullOrEmpty(_txtFile.PostedFile.FileName))
			{
				var extension = Path.GetExtension(_txtFile.PostedFile.FileName).Replace(".", "");

				if (!string.IsNullOrEmpty(FileFilter) && !FileFilter.ToLower().Contains(extension.ToLower()))
				{
					// trying to upload a file not allowed for current filter
					var localizedString = Localization.GetString("UploadError", LocalResourceFile);
					if(String.IsNullOrEmpty(localizedString))
					{
						localizedString = Utilities.GetLocalizedString("UploadError");
					}

					_lblMessage.Text = string.Format(localizedString, FileFilter, extension);
				}
				else
				{
					var folderManager = FolderManager.Instance;

					var folderPath = PathUtils.Instance.GetRelativePath(PortalId, ParentFolder) + _cboFolders.SelectedItem.Value;

					//Check if this is a User Folder
				    IFolderInfo folder;
					if (IsUserFolder(_cboFolders.SelectedItem.Value))
					{
						//Make sure the user folder exists
                        folder = folderManager.GetFolder(PortalController.GetEffectivePortalId(PortalId), folderPath);
						if (folder == null)
						{
							//Add User folder
                            var user = User ?? UserController.Instance.GetCurrentUserInfo();
                            //fix user's portal id
						    user.PortalID = PortalId;
                            folder = ((FolderManager)folderManager).AddUserFolder(user);
						}
					}
					else
					{
                        folder = folderManager.GetFolder(PortalId, folderPath);
                    }

					var fileName = Path.GetFileName(_txtFile.PostedFile.FileName);

					try
					{
                        FileManager.Instance.AddFile(folder, fileName, _txtFile.PostedFile.InputStream, true);
					}
					catch (PermissionsNotMetException)
					{
                        _lblMessage.Text += "<br />" + string.Format(Localization.GetString("InsufficientFolderPermission"), folder.FolderPath);
					}
					catch (NoSpaceAvailableException)
					{
						_lblMessage.Text += "<br />" + string.Format(Localization.GetString("DiskSpaceExceeded"), fileName);
					}
					catch (InvalidFileExtensionException)
					{
						_lblMessage.Text += "<br />" + string.Format(Localization.GetString("RestrictedFileType"), fileName, Host.AllowedExtensionWhitelist.ToDisplayString());
					}
					catch (Exception ex)
					{
						Logger.Error(ex);

						_lblMessage.Text += "<br />" + string.Format(Localization.GetString("SaveFileError"), fileName);
					}
				}

				if (string.IsNullOrEmpty(_lblMessage.Text))
				{
					var fileName = _txtFile.PostedFile.FileName.Substring(_txtFile.PostedFile.FileName.LastIndexOf("\\") + 1);
					SetFilePath(fileName);
				}
			}
			Mode = FileControlMode.Normal;
		}

		private void UploadFile(object sender, EventArgs e)
		{
			Mode = FileControlMode.UpLoadFile;
		}

		#endregion

		#region ILocalizable Implementation

		public bool Localize
		{
			get
			{
				return _localize;
			}
			set
			{
				_localize = value;
			}
		}

		public string LocalResourceFile { get; set; }

		public virtual void LocalizeStrings()
		{
		}

		#endregion

	}
}