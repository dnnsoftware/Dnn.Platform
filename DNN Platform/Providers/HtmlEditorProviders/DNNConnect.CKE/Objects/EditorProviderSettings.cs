using System.Collections.Generic;
using DNNConnect.CKEditorProvider.Constants;

namespace DNNConnect.CKEditorProvider.Objects
{

    /// <summary>
    /// The Editor Provider Settings
    /// </summary>
    public class EditorProviderSettings : object
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EditorProviderSettings"/> class. 
        /// </summary>
        public EditorProviderSettings()
        {
            OverrideFileOnUpload = false;
            UseAnchorSelector = true;
            FileListPageSize = 20;
            FileListViewMode = FileListView.DetailView;
            SettingMode = SettingsMode.Portal;
            DefaultLinkMode = LinkMode.RelativeURL;
            InjectSyntaxJs = true;
            BrowserRootDirId = -1;
            UploadDirId = -1;
            ResizeHeight = -1;
            ResizeWidth = -1;
            BrowserRoles = "0;Administrators;";
            Browser = "standard";
            ToolBarRoles = new List<ToolbarRoles> { new ToolbarRoles { RoleId = 0, Toolbar = "Full" } };
            UploadSizeRoles = new List<UploadSizeRoles>
                                       {
                                           new UploadSizeRoles { RoleId = 0, UploadFileLimit = -1 }
                                       };

            Config = new EditorConfig();
        }
        #region Properties

        /// <summary>
        /// Gets or sets a value indicating whether [override file on upload].
        /// </summary>
        /// <value>
        /// <c>true</c> if [override file on upload]; otherwise, <c>false</c>.
        /// </value>
        public bool OverrideFileOnUpload { get; set; }

        /// <summary>
        /// Gets or sets How many Items to Show per Page on the File List.
        /// </summary>
        /// <value>
        /// How many Items to Show per Page on the File List.
        /// </value>
        public int FileListPageSize { get; set; }

        /// <summary>
        /// Gets or sets the file list view mode.
        /// </summary>
        /// <value>
        /// The file list view mode.
        /// </value>
        public FileListView FileListViewMode { get; set; }

        /// <summary>
        /// Gets or sets the default link mode.
        /// </summary>
        /// <value>
        /// The default link mode.
        /// </value>
        public LinkMode DefaultLinkMode { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [use anchor selector].
        /// </summary>
        /// <value>
        ///  <c>true</c> if [use anchor selector]; otherwise, <c>false</c>.
        /// </value>
        public bool UseAnchorSelector { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [show page links tab first].
        /// </summary>
        /// <value>
        ///  <c>true</c> if [show page links tab first]; otherwise, <c>false</c>.
        /// </value>
        public bool ShowPageLinksTabFirst { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether Use Sub directory for Users.
        /// </summary>
        public bool SubDirs { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether Inject the Syntax 
        /// jQuery Java Script
        /// </summary>
        public bool InjectSyntaxJs { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether The Browser Root Directory Id
        /// </summary>
        public int BrowserRootDirId { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether The Upload Directory Id
        /// </summary>
        public int UploadDirId { get; set; }

        /// <summary>
        /// Gets or sets the custom JS file.
        /// </summary>
        /// <value>
        /// The custom JS file.
        /// </value>
        public string CustomJsFile { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether Default Resize Image Height
        /// </summary>
        public int ResizeHeight { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether Default Resize Image Width
        /// </summary>
        public int ResizeWidth { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether Toolbar Roles
        /// </summary>
        public List<ToolbarRoles> ToolBarRoles { get; set; }

        /// <summary>
        /// Gets or sets a value setting the Upload Sizes for each Role
        /// </summary>
        public List<UploadSizeRoles> UploadSizeRoles { get; set; }

        /// <summary>
        /// Gets or sets the upload file size limit.
        /// </summary>
        /// <value>
        /// The upload file size limit.
        /// </value>
        public int UploadFileSizeLimit { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether Blank Initial text
        /// </summary>
        public string BlankText { get; set; }

        /// <summary>
        /// Gets or sets the browser.
        /// </summary>
        /// <value>
        /// The browser.
        /// </value>
        public BrowserType BrowserMode { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether Editor File Browser
        /// </summary>
        public string Browser { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether Allowed Browser Roles
        /// </summary>
        public string BrowserRoles { get; set; }

        /// <summary>
        /// Gets or sets the width of the editor as string.
        /// </summary>
        /// <value>
        /// The width of the browser.
        /// </value>
        public string EditorWidth { get; set; }

        /// <summary>
        /// Gets or sets the height of the editor as string.
        /// </summary>
        /// <value>
        /// The height of the browser.
        /// </value>
        public string EditorHeight { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether Current Setting Mode
        /// </summary>
        public SettingsMode SettingMode { get; set; }

        /// <summary>
        /// Gets or sets the Editor configuration.
        /// </summary>
        /// <value>
        /// The configuration.
        /// </value>
        public EditorConfig Config { get; set; }

        #endregion
    }
}