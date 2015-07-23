<%@ Control Language="C#" EnableViewState="false" AutoEventWireup="false" Inherits="CKFinder.Settings.ConfigFile" %>
<%@ Import Namespace="System.IO"%>
<%@ Import Namespace="DotNetNuke.Security"%>
<%@ Import Namespace="DotNetNuke.Entities.Portals"%>
<%@ Import Namespace="CKFinder.Settings" %>
<script runat="server">

    readonly HttpRequest request = HttpContext.Current.Request;
    private PortalSettings _portalSettings;
    private bool bUseSubDirs;

    /// <summary>
    /// This function must check the user session to be sure that he/she is
    /// authorized to upload and access files using CKFinder.
    /// </summary>
    /// <returns></returns>
    public override bool CheckAuthentication()
    {
        return (Session["CKDNNIsAuthorized"] != null && (bool)Session["CKDNNIsAuthorized"] &&
                HttpContext.Current.Request.IsAuthenticated);
    }

    /// <summary>
    /// All configuration settings must be defined here.
    /// </summary>
    public override void SetConfig()
    {
        if (Session["CKDNNSubDirs"] != null)
        {
            bUseSubDirs = (bool)Session["CKDNNSubDirs"];
        }
        
        _portalSettings = GetPortalSettings();

        // Paste your license name and key here. If left blank, CKFinder will
        // be fully functional, in Demo Mode.
        LicenseName = string.Empty;
        LicenseKey = string.Empty;

        string sStartDir;
        string sStartDirMapPath;

        if (PortalSecurity.IsInRole("Administrators"))
        {
            sStartDir = _portalSettings.HomeDirectory;
            sStartDirMapPath = _portalSettings.HomeDirectoryMapPath;
        }
        else
        {
            if (bUseSubDirs)
            {
                sStartDir = Path.Combine(_portalSettings.HomeDirectory,
                                         string.Format("userfiles/{0}/", _portalSettings.UserInfo.Username));
                sStartDirMapPath = Path.Combine(_portalSettings.HomeDirectoryMapPath,
                                                string.Format("userfiles/{0}/", _portalSettings.UserInfo.Username));
            }
            else
            {
                sStartDir = _portalSettings.HomeDirectory;
                sStartDirMapPath = _portalSettings.HomeDirectoryMapPath;
            }

            if (!Directory.Exists(sStartDirMapPath))
            {
                Directory.CreateDirectory(sStartDirMapPath);
            }
        }

        // The base URL used to reach files in CKFinder through the browser.
        BaseUrl = sStartDir;

        // The phisical directory in the server where the file will end up. If
        // blank, CKFinder attempts to resolve BaseUrl.
        BaseDir = sStartDirMapPath;

                // Optional: enable extra plugins (remember to copy .dll files first).
		Plugins = new string[] {
			// "CKFinder.Plugins.FileEditor, CKFinder_FileEditor",
			// "CKFinder.Plugins.ImageResize, CKFinder_ImageResize"
		};
		// Settings for extra plugins.
		PluginSettings = new Hashtable();
		PluginSettings.Add("ImageResize_smallThumb", "90x90" );
		PluginSettings.Add("ImageResize_mediumThumb", "120x120" );
		PluginSettings.Add("ImageResize_largeThumb", "180x180" );
        
		// Thumbnail settings.
		// "Url" is used to reach the thumbnails with the browser, while "Dir"
		// points to the physical location of the thumbnail files in the server.
        Thumbnails.Url = string.Format("{0}_thumbs/", BaseUrl);
        Thumbnails.Dir = string.Format("{0}_thumbs/", BaseDir);
		Thumbnails.Enabled = true;
		Thumbnails.DirectAccess = false;
		Thumbnails.MaxWidth = 100;
		Thumbnails.MaxHeight = 100;
		Thumbnails.Quality = 80;

		// Set the maximum size of uploaded images. If an uploaded image is
		// larger, it gets scaled down proportionally. Set to 0 to disable this
		// feature.
		Images.MaxWidth = 1600;
		Images.MaxHeight = 1200;
		Images.Quality = 80;

		// Indicates that the file size (MaxSize) for images must be checked only
		// after scaling them. Otherwise, it is checked right after uploading.
		CheckSizeAfterScaling = true;

		// Due to security issues with Apache modules, it is recommended to leave the
		// following setting enabled. It can be safely disabled on IIS.
		ForceSingleExtension = true;

		// For security, HTML is allowed in the first Kb of data for files having the
		// following extensions only.
		HtmlExtensions = new[] { "html", "htm", "xml", "js" };

		// Folders to not display in CKFinder, no matter their location. No
		// paths are accepted, only the folder name.
		// The * and ? wildcards are accepted.
		HideFolders = new[] { ".svn", "CVS" };

		// Files to not display in CKFinder, no matter their location. No
		// paths are accepted, only the file name, including extension.
		// The * and ? wildcards are accepted.
		HideFiles = new[] { ".*" };

		// Perform additional checks for image files.
		SecureImageUploads = true;

		// The session variable name that CKFinder must use to retrieve the
		// "role" of the current user. The "role" is optional and can be used
		// in the "AccessControl" settings (bellow in this file).
		RoleSessionVar = "CKFinder_UserRole";

		// ACL (Access Control) settings. Used to restrict access or features
		// to specific folders.
		// Several "AccessControl.Add()" calls can be made, which return a
		// single ACL setting object to be configured. All properties settings
		// are optional in that object.
		// Subfolders inherit their default settings from their parents' definitions.
		//
		//	- The "Role" property accepts the special "*" value, which means
		//	  "everybody".
		//	- The "ResourceType" attribute accepts the special value "*", which
		//	  means "all resource types".
		AccessControl acl = AccessControl.Add();
		acl.Role = "*";
		acl.ResourceType = "*";
		acl.Folder = "/";

		acl.FolderView = true;
		acl.FolderCreate = true;
		acl.FolderRename = true;
		acl.FolderDelete = true;

		acl.FileView = true;
		acl.FileUpload = true;
		acl.FileRename = true;
		acl.FileDelete = true;

		// Resource Type settings.
		// A resource type is nothing more than a way to group files under
		// different paths, each one having different configuration settings.
		// Each resource type name must be unique.
		// When loading CKFinder, the "type" querystring parameter can be used
		// to display a specific type only. If "type" is omitted in the URL,
		// the "DefaultResourceTypes" settings is used (may contain the
		// resource type names separated by a comma). If left empty, all types
		// are loaded.

		DefaultResourceTypes = string.Empty;

        ResourceType type = ResourceType.Add( "Files" );
		type.Url = BaseUrl + "files/";
		type.Dir = BaseDir == string.Empty ? string.Empty : BaseDir + "files/";
		type.MaxSize = 0;
		type.AllowedExtensions = new[] { "7z", "aiff", "asf", "avi", "bmp", "csv", "doc", "fla", "flv", "gif", "gz", "gzip", "jpeg", "jpg", "mid", "mov", "mp3", "mp4", "mpc", "mpeg", "mpg", "ods", "odt", "pdf", "png", "ppt", "pxd", "qt", "ram", "rar", "rm", "rmi", "rmvb", "rtf", "sdc", "sitd", "swf", "sxc", "sxw", "tar", "tgz", "tif", "tiff", "txt", "vsd", "wav", "wma", "wmv", "xls", "zip" };
		type.DeniedExtensions = new string[] { };

		type = ResourceType.Add( "Images" );
		type.Url = BaseUrl + "images/";
		type.Dir = BaseDir == "" ? "" : BaseDir + "images/";
		type.MaxSize = 0;
		type.AllowedExtensions = new[] { "bmp", "gif", "jpeg", "jpg", "png" };
		type.DeniedExtensions = new string[] { };

		type = ResourceType.Add( "Flash" );
		type.Url = BaseUrl + "flash/";
        type.Dir = BaseDir == string.Empty ? string.Empty : BaseDir + "flash/";
		type.MaxSize = 0;
		type.AllowedExtensions = new[] { "swf", "flv" };
		type.DeniedExtensions = new string[] { };
	}
    private PortalSettings GetPortalSettings()
    {
        int iTabId = 0, iPortalId = 0;

        PortalSettings portalSettings;

        try
        {
            if (request.QueryString["tabid"] != null)
            {
                iTabId = int.Parse(request.QueryString["tabid"]);
            }

            if (request.QueryString["PortalID"] != null)
            {
                iPortalId = int.Parse(request.QueryString["PortalID"]);
            }

            if (HttpContext.Current.Session["CKDNNtabid"] != null)
            {
                iTabId = int.Parse(HttpContext.Current.Session["CKDNNtabid"].ToString());
            }

            if (HttpContext.Current.Session["CKDNNporid"] != null)
            {
                iPortalId = int.Parse(HttpContext.Current.Session["CKDNNporid"].ToString());
            }


            string sDomainName = DotNetNuke.Common.Globals.GetDomainName(Request, true);

            string sPortalAlias = PortalAliasController.GetPortalAliasByPortal(iPortalId, sDomainName);

            PortalAliasInfo objPortalAliasInfo = PortalAliasController.Instance.GetPortalAlias(sPortalAlias);

            portalSettings = new PortalSettings(iTabId, objPortalAliasInfo);
        }
        catch (Exception)
        {
            portalSettings = (PortalSettings)(HttpContext.Current.Items["PortalSettings"]);
        }

        return portalSettings;
    }

</script>
