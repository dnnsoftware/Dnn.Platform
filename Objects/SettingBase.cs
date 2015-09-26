using System;
using System.Collections.Generic;
using System.Web.UI.WebControls;
using DNNConnect.CKEditorProvider.Constants;

namespace DNNConnect.CKEditorProvider.Objects
{

    /// <summary>
    /// The Settings Base
    /// </summary>
    [Obsolete("This class is phasing out please use EditorProviderSettings Class instead")]
    public class SettingBase : object
    {
        #region Properties

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
        /// Gets or sets a value indicating whether [auto create file thumb nails].
        /// </summary>
        /// <value>
        ///  <c>true</c> if [auto create file thumb nails]; otherwise, <c>false</c>.
        /// </value>
        public bool AutoCreateFileThumbNails { get; set; }

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
        /// Gets or sets a value indicating whether Use Subdirs for Users.
        /// </summary>
        public bool bSubDirs { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether Use jQuery Adapter
        /// </summary>
        public bool bUseJquery { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether Inject the Syntax Jquery Js
        /// </summary>
        public bool injectSyntaxJs { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether The Browser Root Dir Id
        /// </summary>
        public int BrowserRootDirId { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether The Upload Dir Folder Id
        /// </summary>
        public int UploadDirId { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether Default Resize Image Height
        /// </summary>
        public int iResizeHeight { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether Default Resize Image Width
        /// </summary>
        public int iResizeWidth { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether Toolbar Roles
        /// </summary>
        public List<ToolbarRoles> listToolbRoles { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether Blank Initialtext
        /// </summary>
        public string sBlankText { get; set; }

        /// <summary>
        /// Gets or sets the browser.
        /// </summary>
        /// <value>
        /// The browser.
        /// </value>
        public BrowserType browser { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether Editor File Browser
        /// </summary>
        public string sBrowser { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether Allowed Browser Roles
        /// </summary>
        public string sBrowserRoles { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether Custom Config file
        /// </summary>
        public string sConfig { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether Css File
        /// </summary>
        public string sCss { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether Editor Skin
        /// </summary>
        public string sSkin { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether Styles File
        /// </summary>
        public string sStyles { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether Templates File
        /// </summary>
        public string sTemplates { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether Current Setting Mode
        /// </summary>
        public SettingsMode settingMode { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether Browser Height
        /// </summary>
        public Unit uHeight { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether Browser Width
        /// </summary>
        public Unit uWidth { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether Browser Height
        /// </summary>
        public string BrowserHeight { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether Browser Width
        /// </summary>
        public string BrowserWidth { get; set; }

        #endregion
    }
}