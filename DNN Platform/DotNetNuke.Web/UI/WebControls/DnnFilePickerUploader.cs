using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using DotNetNuke.Framework.JavaScriptLibraries;
using DotNetNuke.Services.Localization;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;
using DotNetNuke.Framework;
using DotNetNuke.Services.FileSystem;

namespace DotNetNuke.Web.UI.WebControls
{
    public class DnnFilePickerUploader: UserControl
	{
		#region Private Fields
        private const string MyFileName = "filepickeruploader.ascx";
	    private int? _portalId = null;

		#endregion

		#region protected properties

		protected HiddenField dnnFileUploadFilePath;
        protected HiddenField dnnFileUploadFileId;
        protected DnnComboBox FilesComboBox;
        protected DnnComboBox FoldersComboBox;

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
        
		#endregion

		#region Public Properties

		public bool UsePersonalFolder { get; set; }
        public string FilePath
        {
            get
            {
                EnsureChildControls();
                return dnnFileUploadFilePath.Value;
            }
            set
            {
                EnsureChildControls();
                dnnFileUploadFilePath.Value = value;
            }
        }
        public int FileID
        {
            get
            {
                EnsureChildControls();
                try
                {
                    return int.Parse(dnnFileUploadFileId.Value);
                }
                catch
                {
                    return 0;
                }
            }
            set
            {
                EnsureChildControls();
                dnnFileUploadFileId.Value = value.ToString();

                // set select item
                var fileSelectedItem = FilesComboBox.FindItemByValue(value.ToString());
                if (fileSelectedItem != null)
                {
                    fileSelectedItem.Selected = true;
                }
            }
        }


        public string FolderPath { get; set; }
        public string FileFilter { get; set; }
        public bool Required { get; set; }
        public UserInfo User { get; set; }

	    public int PortalId
	    {
		    get
		    {
			    return !_portalId.HasValue ? PortalSettings.Current.PortalId : _portalId.Value;
		    }
			set
			{
				_portalId = value;
			}
	    }

        #endregion

        protected void Page_Load(object sender, EventArgs e)
        {
            LoadFolders();
            jQuery.RegisterFileUpload(Page);
            JavaScript.RequestRegistration(CommonJs.DnnPlugins);
            ServicesFramework.Instance.RequestAjaxAntiForgerySupport();

            if (!IsPostBack && FileID == 0)
            {
                // set file id
                if (!string.IsNullOrEmpty(FilesComboBox.SelectedValue))
                {
                    FileID = int.Parse(FilesComboBox.SelectedValue);
                }
                else FileID = 0;
            }
        }

        private void LoadFolders()
        {
            UserInfo user = User ?? UserController.GetCurrentUserInfo();
            FoldersComboBox.Items.Clear();

            //Add Personal Folder
            if (UsePersonalFolder)
            {
                var userFolder = FolderManager.Instance.GetUserFolder(user).FolderPath;                
                FoldersComboBox.AddItem(FolderManager.Instance.MyFolderName , userFolder);
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
                                ? "Site Root"
                                : folder.DisplayPath,
                        Value = folder.FolderPath
                    };
                    FoldersComboBox.AddItem(folderItem.Text, folderItem.Value);
                }
            }

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

                if(UsePersonalFolder)
                {
                    folderPath = FolderManager.Instance.GetUserFolder(user).FolderPath;
                    FilePath = folderPath;
                }
            }

            if (FoldersComboBox.FindItemByValue(folderPath) != null)
            {
                FoldersComboBox.FindItemByValue(folderPath).Selected = true;
            }

            FolderPath = folderPath;

            //select file
            LoadFiles();

            var fileSelectedItem = FilesComboBox.FindItemByText(fileName);
            if (fileSelectedItem != null)
            {
                fileSelectedItem.Selected = true;
            }
        }

        private void LoadFiles()
        {
            int effectivePortalId = PortalId;
            var user = User ?? UserController.GetCurrentUserInfo();
            if (IsUserFolder(FoldersComboBox.SelectedItem.Value))
            {
                if (!user.IsSuperUser)
                    effectivePortalId = PortalController.GetEffectivePortalId(effectivePortalId);
                else effectivePortalId = -1;

            }
            FilesComboBox.DataSource = DotNetNuke.Common.Globals.GetFileList(effectivePortalId, FileFilter, Required, FoldersComboBox.SelectedItem.Value);
            FilesComboBox.DataBind();
        }

        private bool IsUserFolder(string folderPath)
        {
            UserInfo user = UserController.GetCurrentUserInfo();
            return (folderPath.ToLowerInvariant().StartsWith("users/") && folderPath.EndsWith(string.Format("/{0}/", user.UserID)));
        }

        private void SetFilePath(string fileName)
        {
            if (FoldersComboBox.SelectedItem == null || string.IsNullOrEmpty(FoldersComboBox.SelectedItem.Value))
            {
                FilePath = fileName;
            }
            else
            {
                FilePath = (FoldersComboBox.SelectedItem.Value + "/") + fileName;
            }

            var fileSelectedItem = FilesComboBox.FindItemByText(fileName);
            if (fileSelectedItem != null)
            {
                fileSelectedItem.Selected = true;
            }
        }
    }
}
