// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DNNConnect.CKEditorProvider.Constants;

using System;
using System.Globalization;

/// <summary>Provider Constants.</summary>
public static class SettingConstants
{
    /// <summary>The Legacy Toolbar Setting XML File Name.</summary>
    [Obsolete("Deprecated in DotNetNuke 7.0.0. Legacy XML file. Scheduled removal in v11.0.0.")]
    public const string ToolbarXmlFileName = "Dnn.CKToolbar.xml";

    /// <summary>The Legacy Toolbar Setting XML File Name.</summary>
    public const string ToolbarSetXmlFileName = "Dnn.CKToolbarSets.xml";

    /// <summary>The Toolbar Buttons XML File Name.</summary>
    public const string ToolbarButtonXmlFileName = "Dnn.CKToolbarButtons.xml";

    /// <summary>The Default Setting XML File Name.</summary>
    public const string XmlDefaultFileName = "Dnn.CKEditorDefaultSettings.xml";

    /// <summary>The Default Setting XML File Name.</summary>
    public const string XmlSettingsFileName = "Dnn.CKEditorSettings.xml";

    /// <summary>The blank text setting name.</summary>
    public const string BLANKTEXT = "blanktext";

    /// <summary>The browser setting name.</summary>
    public const string BROWSER = "browser";

    /// <summary>The image button setting name.</summary>
    public const string IMAGEBUTTON = "imagebutton";

    /// <summary>The Browser Root Directory Host level setting name.</summary>
    public const string HOSTBROWSERROOTDIR = "hostBrowserRootDir";

    /// <summary>The Browser Root Directory for images Host level setting name.</summary>
    public const string HOSTBROWSERROOTDIRFORIMG = "hostBrowserRootDirForImg";

    /// <summary>The browser root folder id setting name.</summary>
    public const string BROWSERROOTDIRID = "browserRootDirId";

    /// <summary>The browser root folder for images id setting name.</summary>
    public const string BROWSERROOTDIRFORIMGID = "browserRootDirForImgId";

    /// <summary>The config setting name.</summary>
    public const string CONFIG = "config";

    /// <summary>The CSS setting name.</summary>
    public const string CSS = "css";

    /// <summary>The OverrideFileOnUpload setting name.</summary>
    public const string OVERRIDEFILEONUPLOAD = "OverrideFileOnUpload";

    /// <summary>The file list page size setting name.</summary>
    public const string FILELISTPAGESIZE = "FileListPageSize";

    /// <summary>The file list view mode setting name.</summary>
    public const string FILELISTVIEWMODE = "FileListViewMode";

    /// <summary>The default link mode setting name.</summary>
    public const string DEFAULTLINKMODE = "DefaultLinkMode";

    /// <summary>The height setting name.</summary>
    public const string HEIGHT = "height";

    /// <summary>The inject JS setting name.</summary>
    public const string INJECTJS = "injectjs";

    /// <summary>The browser allow follow folder permissions setting name.</summary>
    public const string BROWSERALLOWFOLLOWFOLDERPERMS = "BrowsAllowFolderPerms";

    /// <summary>The roles setting name.</summary>
    public const string ROLES = "roles";

    /// <summary>The show page links tab first setting name.</summary>
    public const string SHOWPAGELINKSTABFIRST = "ShowPageLinksTabFirst";

    /// <summary>The skin setting name.</summary>
    public const string SKIN = "skin";

    /// <summary>The CodeMirror theme setting name.</summary>
    public const string CODEMIRRORTHEME = "Codemirror_theme";

    /// <summary>The sub directory setting name.</summary>
    public const string SUBDIRS = "subdirs";

    /// <summary>The templates setting name.</summary>
    public const string TEMPLATEFILES = "templates_files";

    /// <summary>The custom JS File setting name.</summary>
    public const string CUSTOMJSFILE = "customJsFile";

    /// <summary>The toolbar setting name.</summary>
    public const string TOOLB = "toolb";

    /// <summary>The Upload file limits setting name.</summary>
    public const string UPLOADFILELIMITS = "uploadfileRoles";

    /// <summary>The upload folder for all portals setting name.</summary>
    public const string HOSTUPLOADDIR = "hostUploadDir";

    /// <summary>The upload folder setting name.</summary>
    public const string UPLOADDIRID = "uploadDirId";

    /// <summary>The upload folder for images for all portals setting name.</summary>
    public const string HOSTUPLOADDIRFORIMG = "hostUploadDirForImg";

    /// <summary>The upload folder for images id setting name.</summary>
    public const string UPLOADDIRFORIMGID = "uploadDirForImgId";

    /// <summary>The use anchor selector setting name.</summary>
    public const string USEANCHORSELECTOR = "UseAnchorSelector";

    /// <summary>The width setting name.</summary>
    public const string WIDTH = "width";

    /// <summary>The Resize width setting name.</summary>
    public const string RESIZEWIDTH = "resizewidth";

    /// <summary>The resize height setting name.</summary>
    public const string RESIZEHEIGHT = "resizeheight";

    /// <summary>The Resize Upload width setting name.</summary>
    public const string RESIZEWIDTHUPLOAD = "resizewidthupload";

    /// <summary>The resize Upload height setting name.</summary>
    public const string RESIZEHEIGHTUPLOAD = "resizeheightupload";

    /// <summary>Gets the prefix key for host level settings.</summary>
    /// <returns>Host Key.</returns>
    public static string HostKey => PortalKey(-1);

    /// <summary>Gets the prefix key for portal level settings.</summary>
    /// <param name="portalId">The portal id.</param>
    /// <returns>Portal Key.</returns>
    public static string PortalKey(int portalId) => PortalKey(portalId.ToString(CultureInfo.InvariantCulture));

    /// <summary>Gets the prefix key for portal level settings.</summary>
    /// <param name="portalId">The portal id as a string.</param>
    /// <returns>Portal Key.</returns>
    public static string PortalKey(string portalId) => $"DNNCKP#{portalId}#";
}
