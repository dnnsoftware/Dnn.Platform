#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2014
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
using System.Collections;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.UI.WebControls;

using DotNetNuke.Common;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Host;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Users;
using DotNetNuke.Instrumentation;
using DotNetNuke.Security.Permissions;
using DotNetNuke.Services.Exceptions;
using DotNetNuke.Services.FileSystem;
using DotNetNuke.Services.Localization;
using DotNetNuke.UI.Skins;
using DotNetNuke.UI.Skins.Controls;
using DotNetNuke.Web.Common;

#endregion

namespace DotNetNuke.Modules.Admin.FileManager
{
    /// -----------------------------------------------------------------------------
    /// Project	 : DotNetNuke
    /// Class	 : WebUpload
    /// -----------------------------------------------------------------------------
    /// <summary>
    /// Supplies the functionality for uploading files to the Portal
    /// </summary>
    /// <remarks>
    /// </remarks>
    /// <history>
    ///   [cnurse] 16/9/2004  Updated for localization, Help and 508
    /// </history>
    /// -----------------------------------------------------------------------------
    public partial class WebUpload : PortalModuleBase
    {
    	private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof (WebUpload));
		#region "Members"

        private string _DestinationFolder;
        private UploadType _FileType;
        private string _FileTypeName;
        private string _RootFolder;
        private string _UploadRoles;

		#endregion

		#region "Public Properties"

        public string DestinationFolder
        {
            get
            {
                if (_DestinationFolder == null)
                {
                    _DestinationFolder = string.Empty;
                    if ((Request.QueryString["dest"] != null))
                    {
                        _DestinationFolder = Globals.QueryStringDecode(Request.QueryString["dest"]);
                    }
                }
                return PathUtils.Instance.RemoveTrailingSlash(_DestinationFolder.Replace("\\", "/"));
            }
        }

        public UploadType FileType
        {
            get
            {
                _FileType = UploadType.File;
                if ((Request.QueryString["ftype"] != null))
                {
					//The select statement ensures that the parameter can be converted to UploadType
                    switch (Request.QueryString["ftype"].ToLower())
                    {
                        case "file":
                            _FileType = (UploadType) Enum.Parse(typeof (UploadType), Request.QueryString["ftype"]);
                            break;
                    }
                }
                return _FileType;
            }
        }

        public string FileTypeName
        {
            get
            {
                if (_FileTypeName == null)
                {
                    _FileTypeName = Localization.GetString(FileType.ToString(), LocalResourceFile);
                }
                return _FileTypeName;
            }
        }

        public int FolderPortalID
        {
            get
            {
                if (IsHostMenu)
                {
                    return Null.NullInteger;
                }
                else
                {
                    return PortalId;
                }
            }
        }

        public string RootFolder
        {
            get
            {
                if (_RootFolder == null)
                {
                    if (IsHostMenu)
                    {
                        _RootFolder = Globals.HostMapPath;
                    }
                    else
                    {
                        _RootFolder = PortalSettings.HomeDirectoryMapPath;
                    }
                }
                return _RootFolder;
            }
        }

        public string UploadRoles
        {
            get
            {
                if (_UploadRoles == null)
                {
                    _UploadRoles = string.Empty;

                    if (Convert.ToString(Settings["uploadroles"]) != null)
                    {
                        _UploadRoles = Convert.ToString(Settings["uploadroles"]);
                    }
                }
                return _UploadRoles;
            }
        }

		#endregion

		#region "Private Methods"

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// This routine checks the Access Security
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <history>
        ///   [cnurse] 1/21/2005  Documented
        /// </history>
        /// -----------------------------------------------------------------------------
        private void CheckSecurity()
        {
            if (!ModulePermissionController.HasModulePermission(ModuleConfiguration.ModulePermissions, "CONTENT,EDIT") && !UserController.Instance.GetCurrentUserInfo().IsInRole("Administrators"))
            {
                Response.Redirect(Globals.NavigateURL("Access Denied"), true);
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// This routine populates the Folder List Drop Down
        /// There is no reference to permissions here as all folders should be available to the admin.
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <history>
        ///     [Philip Beadle]     5/10/2004  Added
        ///     [cnurse]            04/24/2006  Converted to use Database as folder source
        /// </history>
        /// -----------------------------------------------------------------------------
        private void LoadFolders()
        {
            var user = UserController.Instance.GetCurrentUserInfo();

            var folders = FolderManager.Instance.GetFolders(FolderPortalID, "ADD", user.UserID);
            ddlFolders.Services.Parameters.Add("permission", "ADD");
            if (!String.IsNullOrEmpty(DestinationFolder))
            {
                ddlFolders.SelectedFolder = folders.SingleOrDefault(f => f.FolderPath == DestinationFolder);
            }
            else
            {
                var rootFolder = folders.SingleOrDefault(f => f.FolderPath == "");
                if (rootFolder != null)
                {
                    ddlFolders.SelectedItem = new ListItem() {Text = SharedConstants.RootFolder, Value = rootFolder.FolderID.ToString()};
                }
            }
        }

		#endregion

		#region "Public Methods"

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// This routine determines the Return Url
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <history>
        ///   [cnurse] 1/21/2005  Documented
        /// </history>
        /// -----------------------------------------------------------------------------
        public string ReturnURL()
        {
            int TabID = PortalSettings.HomeTabId;

            if (Request.Params["rtab"] != null)
            {
                TabID = int.Parse(Request.Params["rtab"]);
            }
            return Globals.NavigateURL(TabID);
        }

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            //Customise the Control Title
            ModuleConfiguration.ModuleTitle = Localization.GetString("UploadType" + FileType, LocalResourceFile);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// The Page_Load runs when the page loads
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <remarks>
        /// </remarks>
        /// <history>
        ///   [cnurse] 16/9/2004  Updated for localization, Help and 508
        ///   [VMasanas]  9/28/2004   Changed redirect to Access Denied
        ///   [Philip Beadle]  5/10/2004  Added folder population section.
        /// </history>
        /// -----------------------------------------------------------------------------
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            cmdAdd.Click += cmdAdd_Click;
            cmdReturn1.Click += cmdReturn_Click;
            cmdReturn2.Click += cmdReturn_Click;

            try
            {
                CheckSecurity();

                //Get localized Strings
                string strHost = Localization.GetString("HostRoot", LocalResourceFile);
                string strPortal = Localization.GetString("PortalRoot", LocalResourceFile);

                maxSizeWarningLabel.Text = String.Format(Localization.GetString("FileSizeRestriction", LocalResourceFile), (Config.GetMaxUploadSize()/(1024 *1024)));

                if (!Page.IsPostBack)
                {
                    cmdAdd.Text = Localization.GetString("UploadType" + FileType, LocalResourceFile);
                    if (FileType == UploadType.File)
                    {
                        foldersRow.Visible = true;
                        rootRow.Visible = true;
                        unzipRow.Visible = true;

                        if (IsHostMenu)
                        {
                            lblRootType.Text = strHost + ":";
                            lblRootFolder.Text = RootFolder;
                        }
                        else
                        {
                            lblRootType.Text = strPortal + ":";
                            lblRootFolder.Text = RootFolder;
                        }
                        LoadFolders();
                    }
                    chkUnzip.Checked = false;
                }
            }
            catch (Exception exc) //Module failed to load
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// The cmdAdd_Click runs when the Add Button is clicked
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <remarks>
        /// </remarks>
        /// <history>
        ///   [cnurse] 16/9/2004  Updated for localization, Help and 508
        /// </history>
        /// -----------------------------------------------------------------------------
        private void cmdAdd_Click(object sender, EventArgs e)
        {
            try
            {
                CheckSecurity();
                var strMessage = "";

                var postedFile = cmdBrowse.PostedFile;

                //Get localized Strings
                Localization.GetString("InvalidExt", LocalResourceFile);
                var strFileName = Path.GetFileName(postedFile.FileName);
                if (!String.IsNullOrEmpty(postedFile.FileName))
                {
                    switch (FileType)
                    {
                        case UploadType.File: //content files
                            try
                            {
                                var folder = FolderManager.Instance.GetFolder(ddlFolders.SelectedItemValueAsInt);
                                var fileManager = Services.FileSystem.FileManager.Instance;
                                var file = fileManager.AddFile(folder, strFileName, postedFile.InputStream, true, true, postedFile.ContentType);
                                if (chkUnzip.Checked && file.Extension == "zip")
                                {
                                    fileManager.UnzipFile(file, folder);
                                }
                            }
                            catch (PermissionsNotMetException exc)
                            {
                                Logger.Warn(exc);
                                strMessage += "<br />" + string.Format(Localization.GetString("InsufficientFolderPermission"), ddlFolders.SelectedItemValueAsInt);
                            }
                            catch (NoSpaceAvailableException exc)
                            {
                                Logger.Warn(exc);
                                strMessage += "<br />" + string.Format(Localization.GetString("DiskSpaceExceeded"), strFileName);
                            }
                            catch (InvalidFileExtensionException exc)
                            {
                                Logger.Warn(exc);
                                strMessage += "<br />" + string.Format(Localization.GetString("RestrictedFileType"), strFileName, Host.AllowedExtensionWhitelist.ToDisplayString());
                            }
                            catch (Exception exc)
                            {
                                Logger.Error(exc);
                                strMessage += "<br />" + string.Format(Localization.GetString("SaveFileError"), strFileName);
                            }
                            break;
                    }
                }
                else
                {
                    strMessage = Localization.GetString("NoFile", LocalResourceFile);
                }
                if (phPaLogs.Controls.Count > 0)
                {
                    tblLogs.Visible = true;
                }
                else if (String.IsNullOrEmpty(strMessage))
                {
                    Skin.AddModuleMessage(this, String.Format(Localization.GetString("FileUploadSuccess", LocalResourceFile), strFileName), ModuleMessage.ModuleMessageType.GreenSuccess);
                }
                else
                {
                    lblMessage.Text = strMessage;
                }
            }
            catch (Exception exc) //Module failed to load
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// The cmdReturn_Click runs when the Return Button is clicked
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <remarks>
        /// </remarks>
        /// <history>
        ///   [cnurse] 16/9/2004  Updated for localization, Help and 508
        /// </history>
        /// -----------------------------------------------------------------------------
        private void cmdReturn_Click(Object sender, EventArgs e)
        {
            Response.Redirect(ReturnURL(), true);
        }
		
		#endregion
    }
}