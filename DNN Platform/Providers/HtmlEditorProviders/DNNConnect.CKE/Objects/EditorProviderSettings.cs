// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DNNConnect.CKEditorProvider.Objects;

using System.Collections.Generic;

using DNNConnect.CKEditorProvider.Constants;

/// <summary>The Editor Provider Settings.</summary>
public class EditorProviderSettings : object
{
    /// <summary>Initializes a new instance of the <see cref="EditorProviderSettings"/> class.</summary>
    public EditorProviderSettings()
    {
        this.OverrideFileOnUpload = false;
        this.UseAnchorSelector = true;
        this.FileListPageSize = 20;
        this.FileListViewMode = FileListView.DetailView;
        this.SettingMode = SettingsMode.Portal;
        this.DefaultLinkMode = LinkMode.RelativeURL;
        this.DefaultLinkProtocol = LinkProtocol.Https;
        this.InjectSyntaxJs = true;
        this.BrowserRootDirId = -1;
        this.BrowserRootDirForImgId = -1;
        this.UploadDirId = -1;
        this.UploadDirForImgId = -1;
        this.ResizeHeight = -1;
        this.ResizeWidthUpload = -1;
        this.ResizeHeightUpload = -1;
        this.ResizeWidth = -1;
        this.BrowserAllowFollowFolderPerms = false;
        this.BrowserRoles = "0;Administrators;";
        this.Browser = "standard";
        this.ImageButton = "standard";
        this.ToolBarRoles = new List<ToolbarRoles> { new ToolbarRoles { RoleId = 0, Toolbar = "Full" } };
        this.UploadSizeRoles = new List<UploadSizeRoles>
        {
            new UploadSizeRoles { RoleId = 0, UploadFileLimit = -1 },
        };

        this.Config = new EditorConfig();
    }

    /// <summary>Gets or sets a value indicating whether [override file on upload].</summary>
    /// <value>
    /// <see langword="true"/> if [override file on upload]; otherwise, <see langword="false"/>.
    /// </value>
    public bool OverrideFileOnUpload { get; set; }

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

    /// <summary>Gets or sets the default link mode.</summary>
    /// <value>
    /// The default link mode.
    /// </value>
    public LinkMode DefaultLinkMode { get; set; }

    /// <summary>Gets or sets the default link protocol.</summary>
    /// <value>
    /// The default link protocol.
    /// </value>
    public LinkProtocol DefaultLinkProtocol { get; set; }

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

    /// <summary>Gets or sets a value indicating whether Use Sub directory for Users.</summary>
    public bool SubDirs { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether Inject the Syntax
    /// jQuery Java Script.
    /// </summary>
    public bool InjectSyntaxJs { get; set; }

    /// <summary>Gets or sets a value indicating the Browser Root Directory Host level setting.</summary>
    public string HostBrowserRootDir { get; set; }

    /// <summary>Gets or sets a value indicating the Browser Root Directory for images Host level setting.</summary>
    public string HostBrowserRootDirForImg { get; set; }

    /// <summary>Gets or sets a value indicating the Browser Root Directory Id.</summary>
    public int BrowserRootDirId { get; set; }

    /// <summary>Gets or sets a value indicating the Browser Root Directory Id for images.</summary>
    public int BrowserRootDirForImgId { get; set; }

    /// <summary>Gets or sets a value indicating the Upload Directory for all portals.</summary>
    public string HostUploadDir { get; set; }

    /// <summary>Gets or sets a value indicating the Upload Directory Id.</summary>
    public int UploadDirId { get; set; }

    /// <summary>Gets or sets a value indicating the Upload Directory for images for all portals.</summary>
    public string HostUploadDirForImg { get; set; }

    /// <summary>Gets or sets a value indicating the Upload Directory Id for images.</summary>
    public int UploadDirForImgId { get; set; }

    /// <summary>Gets or sets the custom JS file.</summary>
    /// <value>
    /// The custom JS file.
    /// </value>
    public string CustomJsFile { get; set; }

    /// <summary>Gets or sets a value indicating the Resize Image Height On Upload.</summary>
    public int ResizeHeightUpload { get; set; }

    /// <summary>Gets or sets a value indicating the Resize Image Width On Upload.</summary>
    public int ResizeWidthUpload { get; set; }

    /// <summary>Gets or sets a value indicating whether Default Resize Image Height.</summary>
    public int ResizeHeight { get; set; }

    /// <summary>Gets or sets a value indicating whether Default Resize Image Width.</summary>
    public int ResizeWidth { get; set; }

    /// <summary>Gets or sets a value indicating whether Toolbar Roles.</summary>
    public List<ToolbarRoles> ToolBarRoles { get; set; }

    /// <summary>Gets or sets a value setting the Upload Sizes for each Role.</summary>
    public List<UploadSizeRoles> UploadSizeRoles { get; set; }

    /// <summary>Gets or sets the upload file size limit.</summary>
    /// <value>
    /// The upload file size limit.
    /// </value>
    public int UploadFileSizeLimit { get; set; }

    /// <summary>Gets or sets a value indicating whether Blank Initial text.</summary>
    public string BlankText { get; set; }

    /// <summary>Gets or sets the browser.</summary>
    /// <value>
    /// The browser.
    /// </value>
    public BrowserType BrowserMode { get; set; }

    /// <summary>Gets or sets a value indicating whether Editor File Browser.</summary>
    public string Browser { get; set; }

    /// <summary>Gets or sets the image button.</summary>
    /// <value>
    /// The image browser.
    /// </value>
    public ImageButtonType ImageButtonMode { get; set; }

    /// <summary>Gets or sets a value indicating whether which Image Button to use.</summary>
    public string ImageButton { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether allowing the file browser will depend on folder permissions.
    /// </summary>
    public bool BrowserAllowFollowFolderPerms { get; set; }

    /// <summary>Gets or sets a value indicating whether Allowed Browser Roles.</summary>
    public string BrowserRoles { get; set; }

    /// <summary>Gets or sets the width of the editor as string.</summary>
    /// <value>
    /// The width of the browser.
    /// </value>
    public string EditorWidth { get; set; }

    /// <summary>Gets or sets the height of the editor as string.</summary>
    /// <value>
    /// The height of the browser.
    /// </value>
    public string EditorHeight { get; set; }

    /// <summary>Gets or sets a value indicating whether Current Setting Mode.</summary>
    public SettingsMode SettingMode { get; set; }

    /// <summary>Gets or sets the Editor configuration.</summary>
    /// <value>
    /// The configuration.
    /// </value>
    public EditorConfig Config { get; set; }
}
