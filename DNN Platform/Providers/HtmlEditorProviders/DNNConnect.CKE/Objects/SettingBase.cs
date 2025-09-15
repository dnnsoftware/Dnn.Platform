// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DNNConnect.CKEditorProvider.Objects
{
    using System.Collections.Generic;
    using System.Web.UI.WebControls;

    using DNNConnect.CKEditorProvider.Constants;
    using DotNetNuke.Internal.SourceGenerators;

    /// <summary>The Settings Base.</summary>
    [DnnDeprecated(7, 0, 0, "Please use EditorProviderSettings Class instead", RemovalVersion = 11)]
    public partial class SettingBase : object
    {
#pragma warning disable SA1300 // Element should begin with upper-case letter
        /// <summary>Gets or sets How many Items to Show per Page on the File List.</summary>
        /// <value>
        /// How many Items to Show per Page on the File List.
        /// </value>
        public int FileListPageSize { get; set; }

        /// <summary>Gets or sets the file list view mode.</summary>
        /// <value>
        /// The file list view mode.
        /// </value>
        public FileListView FileListViewMode { get; set; }

        /// <summary>Gets or sets a value indicating whether [auto create file thumb nails].</summary>
        /// <value>
        ///  <see langword="true"/> if [auto create file thumb nails]; otherwise, <see langword="false"/>.
        /// </value>
        public bool AutoCreateFileThumbNails { get; set; }

        /// <summary>Gets or sets a value indicating whether [use anchor selector].</summary>
        /// <value>
        ///  <see langword="true"/> if [use anchor selector]; otherwise, <see langword="false"/>.
        /// </value>
        public bool UseAnchorSelector { get; set; }

        /// <summary>Gets or sets a value indicating whether [show page links tab first].</summary>
        /// <value>
        ///  <see langword="true"/> if [show page links tab first]; otherwise, <see langword="false"/>.
        /// </value>
        public bool ShowPageLinksTabFirst { get; set; }

        /// <summary>Gets or sets a value indicating whether Use Subdirs for Users.</summary>
        public bool bSubDirs { get; set; }

        /// <summary>Gets or sets a value indicating whether Use jQuery Adapter.</summary>
        public bool bUseJquery { get; set; }

        /// <summary>Gets or sets a value indicating whether Inject the Syntax Jquery Js.</summary>
        public bool injectSyntaxJs { get; set; }

        /// <summary>Gets or sets a value indicating the Browser Root Dir Id.</summary>
        public int BrowserRootDirId { get; set; }

        /// <summary>Gets or sets a value indicating the Browser Root Dir Id.</summary>
        public int BrowserRootDirForImgId { get; set; }

        /// <summary>Gets or sets a value indicating whether the Upload Dir Folder Id.</summary>
        public int UploadDirId { get; set; }

        /// <summary>Gets or sets a value indicating the Upload Dir Folder Id.</summary>
        public int UploadDirForImgId { get; set; }

        /// <summary>Gets or sets a value indicating the Resize Image Height On Upload.</summary>
        public int iResizeHeightUpload { get; set; }

        /// <summary>Gets or sets a value indicating the Resize Image Width On Upload.</summary>
        public int iResizeWidthUpload { get; set; }

        /// <summary>Gets or sets a value indicating whether Default Resize Image Height.</summary>
        public int iResizeHeight { get; set; }

        /// <summary>Gets or sets a value indicating whether Default Resize Image Width.</summary>
        public int iResizeWidth { get; set; }

        /// <summary>Gets or sets a value indicating whether Toolbar Roles.</summary>
        public List<ToolbarRoles> listToolbRoles { get; set; }

        /// <summary>Gets or sets a value indicating whether Blank Initialtext.</summary>
        public string sBlankText { get; set; }

        /// <summary>Gets or sets the browser.</summary>
        /// <value>
        /// The browser.
        /// </value>
        public BrowserType browser { get; set; }

        /// <summary>Gets or sets a value indicating whether Editor File Browser.</summary>
        public string sBrowser { get; set; }

        /// <summary>Gets or sets a value indicating whether Browser Allow follows Folder Permissions.</summary>
        public bool BrowserAllowFollowFolderPerms { get; set; }

        /// <summary>Gets or sets a value indicating whether Allowed Browser Roles.</summary>
        public string sBrowserRoles { get; set; }

        /// <summary>Gets or sets a value indicating whether Custom Config file.</summary>
        public string sConfig { get; set; }

        /// <summary>Gets or sets a value indicating whether Css File.</summary>
        public string sCss { get; set; }

        /// <summary>Gets or sets a value indicating whether Editor Skin.</summary>
        public string sSkin { get; set; }

        /// <summary>Gets or sets a value indicating whether Styles File.</summary>
        public string sStyles { get; set; }

        /// <summary>Gets or sets a value indicating whether Templates File.</summary>
        public string sTemplates { get; set; }

        /// <summary>Gets or sets a value indicating whether Current Setting Mode.</summary>
        public SettingsMode settingMode { get; set; }

        /// <summary>Gets or sets a value indicating whether Browser Height.</summary>
        public Unit uHeight { get; set; }

        /// <summary>Gets or sets a value indicating whether Browser Width.</summary>
        public Unit uWidth { get; set; }

        /// <summary>Gets or sets a value indicating whether Browser Height.</summary>
        public string BrowserHeight { get; set; }

        /// <summary>Gets or sets a value indicating whether Browser Width.</summary>
        public string BrowserWidth { get; set; }
#pragma warning restore SA1300 // Element should begin with upper-case letter
    }
}
