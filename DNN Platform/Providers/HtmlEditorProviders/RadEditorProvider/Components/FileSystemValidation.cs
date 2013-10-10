#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2013
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

using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;

using DotNetNuke.Common;
using DotNetNuke.Instrumentation;
using DotNetNuke.Services.Exceptions;
using DotNetNuke.Services.FileSystem;
using DotNetNuke.Services.Localization;

using System;
using System.Collections.Generic;
using System.Web;
using System.IO;
using DotNetNuke.Security.Permissions;
using DotNetNuke.Entities.Portals;

using Telerik.Web.UI.Widgets;

// ReSharper disable CheckNamespace
namespace DotNetNuke.Providers.RadEditorProvider
// ReSharper restore CheckNamespace
{

	public class FileSystemValidation
	{
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof (FileSystemValidation));

		public bool EnableDetailedLogging = true;

#region Public Folder Validate Methods

		public virtual string OnCreateFolder(string virtualPath, string folderName)
		{
			string returnValue;
			try
			{
				returnValue = Check_CanAddToFolder(virtualPath);
			}
			catch (Exception ex)
			{
				return LogUnknownError(ex, virtualPath, folderName);
			}

			return returnValue;
		}

		public virtual string OnDeleteFolder(string virtualPath)
		{
			string returnValue;
			try
			{
				returnValue = Check_CanDeleteFolder(virtualPath);
			}
			catch (Exception ex)
			{
				return LogUnknownError(ex, virtualPath);
			}

			return returnValue;
		}

		public virtual string OnMoveFolder(string virtualPath, string virtualDestinationPath)
		{
			string returnValue;
			try
			{
				returnValue = Check_CanDeleteFolder(virtualPath);
				if (! (string.IsNullOrEmpty(returnValue)))
				{
					return returnValue;
				}

				returnValue = Check_CanAddToFolder(virtualDestinationPath);
				if (! (string.IsNullOrEmpty(returnValue)))
				{
					return returnValue;
				}
			}
			catch (Exception ex)
			{
				return LogUnknownError(ex, virtualPath, virtualDestinationPath);
			}

			return returnValue;
		}

		public virtual string OnRenameFolder(string virtualPath)
		{
			string returnValue;
			try
			{
				returnValue = Check_CanAddToFolder(GetDestinationFolder(virtualPath));
				if (! (string.IsNullOrEmpty(returnValue)))
				{
					return returnValue;
				}

				returnValue = Check_CanDeleteFolder(virtualPath);
				if (! (string.IsNullOrEmpty(returnValue)))
				{
					return returnValue;
				}
			}
			catch (Exception ex)
			{
				return LogUnknownError(ex, virtualPath);
			}

			return returnValue;
		}

		public virtual string OnCopyFolder(string virtualPath, string virtualDestinationPath)
		{
			string returnValue;
			try
			{
				returnValue = Check_CanCopyFolder(virtualPath);
				if (! (string.IsNullOrEmpty(returnValue)))
				{
					return returnValue;
				}

				returnValue = Check_CanAddToFolder(virtualDestinationPath);
				if (! (string.IsNullOrEmpty(returnValue)))
				{
					return returnValue;
				}
			}
			catch (Exception ex)
			{
				return LogUnknownError(ex, virtualPath, virtualDestinationPath);
			}

			return returnValue;
		}

#endregion

#region Public File Validate Methods

        public virtual void OnFolderRenamed(string oldFolderPath, string newFolderPath)
        {
            try
            {
                var folder = GetDNNFolder(oldFolderPath);
                folder.FolderPath = ToDBPath(newFolderPath);
                DNNFolderCtrl.UpdateFolder(folder);
            }
            catch (Exception ex)
            {
                LogUnknownError(ex, newFolderPath);
            }

        }

        public virtual void OnFolderCreated(string virtualFolderPath, string virtualParentPath)
        {
            try
            {
                //rename secure files
                var folder = GetDNNFolder(virtualFolderPath);
                var parent = GetDNNFolder(virtualParentPath);

                folder.StorageLocation = parent.StorageLocation;
                folder.FolderMappingID = parent.FolderMappingID;
                DNNFolderCtrl.UpdateFolder(folder);
            }
            catch (Exception ex)
            {
                LogUnknownError(ex, virtualFolderPath);
            }

        }

        public virtual void OnFileCreated(string virtualPathAndFile, int contentLength)
        {
            try
            {
                //rename secure files
                var folder = GetDNNFolder(virtualPathAndFile);
                if (folder.StorageLocation == (int)FolderController.StorageLocationTypes.SecureFileSystem)
                {
                    var securedFile = virtualPathAndFile + Globals.glbProtectedExtension;
                    var absolutePathAndFile = HttpContext.Current.Request.MapPath(virtualPathAndFile);
                    var securedFileAbsolute = HttpContext.Current.Request.MapPath(securedFile);

                    File.Move(absolutePathAndFile, securedFileAbsolute);
                }

                FolderManager.Instance.Synchronize(folder.PortalID, folder.FolderPath, false, true);
            }
            catch (Exception ex)
            {
                LogUnknownError(ex, virtualPathAndFile, contentLength.ToString(CultureInfo.InvariantCulture));
            }

        }

		public virtual string OnCreateFile(string virtualPathAndFile, long contentLength)
		{
			string returnValue;
			try
			{
				var virtualPath = RemoveFileName(virtualPathAndFile);
				returnValue = Check_CanAddToFolder(virtualPath, true);
				if (! (string.IsNullOrEmpty(returnValue)))
				{
					return returnValue;
				}

				returnValue = Check_FileName(virtualPathAndFile);
				if (! (string.IsNullOrEmpty(returnValue)))
				{
					return returnValue;
				}

				returnValue = (string)Check_DiskSpace(virtualPathAndFile, contentLength);
				if (! (string.IsNullOrEmpty(returnValue)))
				{
					return returnValue;
				}
			}
			catch (Exception ex)
			{
				return LogUnknownError(ex, virtualPathAndFile, contentLength.ToString(CultureInfo.InvariantCulture));
			}

			return returnValue;
		}

		public virtual string OnDeleteFile(string virtualPathAndFile)
		{
			string returnValue;
			try
			{
				string virtualPath = RemoveFileName(virtualPathAndFile);

				returnValue = Check_CanDeleteFolder(virtualPath, true);
			}
			catch (Exception ex)
			{
				return LogUnknownError(ex, virtualPathAndFile);
			}

			return returnValue;
		}

		public virtual string OnRenameFile(string virtualPathAndFile)
		{
		    try
			{
				string virtualPath = RemoveFileName(virtualPathAndFile);

				string returnValue = Check_CanAddToFolder(virtualPath, true);
				if (! (string.IsNullOrEmpty(returnValue)))
				{
					return returnValue;
				}

				returnValue = Check_CanDeleteFolder(virtualPath, true);
				if (! (string.IsNullOrEmpty(returnValue)))
				{
					return returnValue;
				}

				return returnValue;
			}
			catch (Exception ex)
			{
				return LogUnknownError(ex, virtualPathAndFile);
			}
		}

		public virtual string OnMoveFile(string virtualPathAndFile, string virtualNewPathAndFile)
		{
		    try
			{
				string virtualPath = RemoveFileName(virtualPathAndFile);

				string returnValue = Check_CanDeleteFolder(virtualPath, true);
				if (! (string.IsNullOrEmpty(returnValue)))
				{
					return returnValue;
				}

				return OnCreateFile(virtualNewPathAndFile, 0);
			}
			catch (Exception ex)
			{
				return LogUnknownError(ex, virtualPathAndFile, virtualNewPathAndFile);
			}
		}

		public virtual string OnCopyFile(string virtualPathAndFile, string virtualNewPathAndFile)
		{
		    try
			{
				int existingFileSize = GetFileSize(virtualPathAndFile);
				if (existingFileSize < 0)
				{
					return LogDetailError(ErrorCodes.FileDoesNotExist, virtualPathAndFile, true);
				}

				string virtualPath = RemoveFileName(virtualPathAndFile);
				string returnValue = Check_CanCopyFolder(virtualPath, true);
				if (! (string.IsNullOrEmpty(returnValue)))
				{
					return returnValue;
				}

				return OnCreateFile(virtualNewPathAndFile, existingFileSize);
			}
			catch (Exception ex)
			{
				return LogUnknownError(ex, virtualPathAndFile, virtualNewPathAndFile);
			}
		}

#endregion

#region Public Shared Path Properties and Convert Methods

		/// <summary>
		/// Gets the DotNetNuke Portal Directory Virtual path
		/// </summary>
		/// <value></value>
		/// <returns></returns>
		/// <remarks></remarks>
		public static string HomeDirectory
		{
			get
			{
				string homeDir = PortalController.GetCurrentPortalSettings().HomeDirectory;
				homeDir = homeDir.Replace("\\", "/");

				if (homeDir.EndsWith("/"))
				{
					homeDir = homeDir.Remove(homeDir.Length - 1, 1);
				}

				return homeDir;
			}
		}

		/// <summary>
		/// Gets the DotNetNuke Portal Directory Root localized text to display to the end user
		/// </summary>
		/// <value></value>
		/// <returns></returns>
		/// <remarks></remarks>
		public static string EndUserHomeDirectory
		{
			get
			{
				//Dim text As String = Localization.Localization.GetString("PortalRoot.Text")
				//If (String.IsNullOrEmpty(text)) Then
				//    Return "Portal Root"
				//End If

				//Return text.Replace("/", " ").Replace("\", " ").Trim()

				string homeDir = PortalController.GetCurrentPortalSettings().HomeDirectory;
				homeDir = homeDir.Replace("\\", "/");

				if (homeDir.EndsWith("/"))
				{
					homeDir = homeDir.Remove(homeDir.Length - 1, 1);
				}

				return homeDir;

			}
		}

		/// <summary>
		/// Gets the DotNetNuke Portal Directory Root as stored in the database
		/// </summary>
		/// <value></value>
		/// <returns></returns>
		/// <remarks></remarks>
		public static string DBHomeDirectory
		{
			get
			{
				return string.Empty;
			}
		}

		/// <summary>
		/// Results in a virtual path to a folder or file
		/// </summary>
		/// <param name="path"></param>
		/// <returns></returns>
		/// <remarks></remarks>
		public static string ToVirtualPath(string path)
		{
			path = path.Replace("\\", "/");

			if (path.StartsWith(EndUserHomeDirectory))
			{
				path = HomeDirectory + path.Substring(EndUserHomeDirectory.Length);
			}

			if (! (path.StartsWith(HomeDirectory)))
			{
				path = CombineVirtualPath(HomeDirectory, path);
			}

			if (string.IsNullOrEmpty(Path.GetExtension(path)) && ! (path.EndsWith("/")))
			{
				path = path + "/";
			}

			return path.Replace("\\", "/");
		}

		/// <summary>
		/// Results in the path displayed to the end user
		/// </summary>
		/// <param name="path"></param>
		/// <returns></returns>
		/// <remarks></remarks>
		public static object ToEndUserPath(string path)
		{
			path = path.Replace("\\", "/");

			if (path.StartsWith(HomeDirectory))
			{
				path = EndUserHomeDirectory + path.Substring(HomeDirectory.Length);
			}

			if (! (path.StartsWith(EndUserHomeDirectory)))
			{
				if (! (path.StartsWith("/")))
				{
					path = "/" + path;
				}
				path = EndUserHomeDirectory + path;
			}

			if (string.IsNullOrEmpty(Path.GetExtension(path)) && ! (path.EndsWith("/")))
			{
				path = path + "/";
			}

			return path;
		}

		/// <summary>
		/// Results in a path that can be used in database calls
		/// </summary>
		/// <param name="path"></param>
		/// <returns></returns>
		/// <remarks></remarks>
        public static string ToDBPath(string path)
		{
			string returnValue = path;

			returnValue = returnValue.Replace("\\", "/");
			returnValue = RemoveFileName(returnValue);

			if (returnValue.StartsWith(HomeDirectory))
			{
				returnValue = returnValue.Substring(HomeDirectory.Length);
			}

			if (returnValue.StartsWith(EndUserHomeDirectory))
			{
				returnValue = returnValue.Substring(EndUserHomeDirectory.Length);
			}

			//folders in dnn db do not start with /
			if (returnValue.StartsWith("/"))
			{
				returnValue = returnValue.Remove(0, 1);
			}

			//Root directory is an empty string
			if (returnValue == "/" || returnValue == "\\")
			{
				returnValue = string.Empty;
			}

			//root folder (empty string) does not contain / - all other folders must contain a slash at the end
			if (! (string.IsNullOrEmpty(returnValue)) && ! (returnValue.EndsWith("/")))
			{
				returnValue = returnValue + "/";
			}

			return returnValue;
		}

		public static string CombineVirtualPath(string virtualPath, string folderOrFileName)
		{
			string returnValue = Path.Combine(virtualPath, folderOrFileName);
			returnValue = returnValue.Replace("\\", "/");

			if (string.IsNullOrEmpty(Path.GetExtension(returnValue)) && ! (returnValue.EndsWith("/")))
			{
				returnValue = returnValue + "/";
			}

			return returnValue;
		}

        public static string RemoveFileName(string path)
		{
			if (! (string.IsNullOrEmpty(Path.GetExtension(path))))
			{
			    var directoryName = Path.GetDirectoryName(path);
			    if (directoryName != null)
			    {
			        path = directoryName.Replace("\\", "/") + "/";
			    }
			}

            return path;
		}

#endregion

#region Public Data Access

        //public virtual IDictionary<string, FolderInfo> GetUserFolders()
        //{
        //    return UserFolders;
        //}

		public virtual FolderInfo GetUserFolder(string path)
		{
		    var returnFolder = FolderManager.Instance.GetFolder(PortalSettings.PortalId, ToDBPath(path));
		    return HasPermission(returnFolder, "BROWSE,READ") ? (FolderInfo)returnFolder : null;
		}

	    public virtual IDictionary<string, FolderInfo> GetChildUserFolders(string parentPath)
		{
			string dbPath = ToDBPath(parentPath);
			IDictionary<string, FolderInfo> returnValue = new Dictionary<string, FolderInfo>();

            var dnnParentFolder = FolderManager.Instance.GetFolder(PortalSettings.PortalId, dbPath);
            var dnnChildFolders = FolderManager.Instance.GetFolders(dnnParentFolder).Where(folder => (HasPermission(folder, "BROWSE,READ")));
	        foreach (var dnnChildFolder in dnnChildFolders)
	        {
	            returnValue.Add(dnnChildFolder.FolderPath,(FolderInfo)dnnChildFolder);
	        }			
            return returnValue;
		}

		public static string GetDestinationFolder(string virtualPath)
		{
			string splitPath = virtualPath;
			if (splitPath.Substring(splitPath.Length - 1) == "/")
			{
				splitPath = splitPath.Remove(splitPath.Length - 1, 1);
			}

			if (splitPath == HomeDirectory)
			{
				return splitPath;
			}

			string[] pathList = splitPath.Split('/');
			if (pathList.Length > 0)
			{
				string folderName = pathList[pathList.Length - 1];

				string folderSubString = splitPath.Substring(splitPath.Length - folderName.Length);
				if (folderSubString == folderName)
				{
					return splitPath.Substring(0, splitPath.Length - folderName.Length);
				}
			}

			return string.Empty;
		}

#endregion

#region Public Permissions Checks

        public static bool HasPermission(IFolderInfo folder, string permissionKey)
        {
            var hasPermission = PortalSettings.Current.UserInfo.IsSuperUser;

            if (!hasPermission && folder != null)
            {
                hasPermission = FolderPermissionController.HasFolderPermission(folder.FolderPermissions, permissionKey);
            }

            return hasPermission;
        }

        public static PathPermissions TelerikPermissions(IFolderInfo folder)
        {
            var folderPermissions = PathPermissions.Read;

            if (FolderPermissionController.CanViewFolder((FolderInfo)folder))
            {
                if (FolderPermissionController.CanAddFolder((FolderInfo)folder))
                {
                    folderPermissions = folderPermissions | PathPermissions.Upload;
                }

                if (FolderPermissionController.CanDeleteFolder((FolderInfo)folder))
                {
                    folderPermissions = folderPermissions | PathPermissions.Delete;
                }
            }

            return folderPermissions;
        } 

		public virtual bool CanViewFolder(string path)
		{
		    return GetUserFolder(path) != null;
		}

		public virtual bool CanViewFolder(FolderInfo dnnFolder)
		{
            return GetUserFolder(dnnFolder.FolderPath) != null;
		}

		public virtual bool CanViewFilesInFolder(string path)
		{
			return CanViewFilesInFolder(GetUserFolder(path));
		}

		public virtual bool CanViewFilesInFolder(FolderInfo dnnFolder)
		{
			if ((dnnFolder == null))
			{
				return false;
			}

			if (! (CanViewFolder(dnnFolder)))
			{
				return false;
			}

			if (! (FolderPermissionController.CanViewFolder(dnnFolder)))
			{
				return false;
			}

			return true;
		}

		public virtual bool CanAddToFolder(FolderInfo dnnFolder)
		{
            if(!FolderPermissionController.CanAddFolder(dnnFolder))
            {
                return false;
            }

		    return true;
		}

		public virtual bool CanDeleteFolder(FolderInfo dnnFolder)
		{
			if (! (FolderPermissionController.CanDeleteFolder(dnnFolder)))
			{
				return false;
			}

			return true;
		}

		//In Addition to Permissions:
		//don't allow upload or delete for database or secured file folders, because this provider does not handle saving to db or adding .resource extensions
		//is protected means it is a system folder that cannot be deleted
		private string Check_CanAddToFolder(string virtualPath)
		{
			return Check_CanAddToFolder(virtualPath,  EnableDetailedLogging);
		}

        private string Check_CanAddToFolder(string virtualPath, bool logDetail)
        {
            var dnnFolder = GetDNNFolder(virtualPath);

			if (dnnFolder == null)
			{
                return LogDetailError(ErrorCodes.FolderDoesNotExist, ToVirtualPath(virtualPath), logDetail);
			}

			//check permissions
			if (! (FolderPermissionController.CanAddFolder(dnnFolder)))
			{
				return LogDetailError(ErrorCodes.AddFolder_NoPermission, ToVirtualPath(dnnFolder.FolderPath), logDetail);
			}

			return string.Empty;
		}

		private string Check_CanCopyFolder(string virtualPath)
		{
			return Check_CanCopyFolder(virtualPath,  EnableDetailedLogging);
		}

        private string Check_CanCopyFolder(string virtualPath, bool logDetail)
		{
            var dnnFolder = GetDNNFolder(virtualPath);

            if (dnnFolder == null)
			{
                return LogDetailError(ErrorCodes.FolderDoesNotExist, virtualPath, logDetail);
			}

			//check permissions 
			if (! (FolderPermissionController.CanCopyFolder(dnnFolder)))
			{
				return LogDetailError(ErrorCodes.CopyFolder_NoPermission, ToVirtualPath(dnnFolder.FolderPath), logDetail);
			}

			return string.Empty;
		}

		private string Check_CanDeleteFolder(string virtualPath)
		{
			return Check_CanDeleteFolder(virtualPath, false, EnableDetailedLogging);
		}

		private string Check_CanDeleteFolder(string virtualPath, bool isFileCheck)
		{
			return Check_CanDeleteFolder(virtualPath, isFileCheck, EnableDetailedLogging);
		}

        private string Check_CanDeleteFolder(string virtualPath, bool isFileCheck, bool logDetail)
		{
            var dnnFolder = GetDNNFolder(virtualPath);
            
            if (dnnFolder == null)
			{
                return LogDetailError(ErrorCodes.FolderDoesNotExist, virtualPath, logDetail);
			}

			//skip additional folder checks when it is a file
			if (! isFileCheck)
			{
				//Don't allow delete of root folder, root is a protected folder, but show a special message
				if (dnnFolder.FolderPath == DBHomeDirectory)
				{
					return LogDetailError(ErrorCodes.DeleteFolder_Root, ToVirtualPath(dnnFolder.FolderPath));
				}

				//Don't allow deleting of any protected folder
				if (dnnFolder.IsProtected)
				{
					return LogDetailError(ErrorCodes.DeleteFolder_Protected, ToVirtualPath(dnnFolder.FolderPath), logDetail);
				}
			}

			//check permissions 
			if (! (FolderPermissionController.CanDeleteFolder(dnnFolder)))
			{
				return LogDetailError(ErrorCodes.DeleteFolder_NoPermission, ToVirtualPath(dnnFolder.FolderPath), logDetail);
			}

			return string.Empty;
		}

#endregion

#region Private Check Methods

		private string Check_FileName(string virtualPathAndName)
		{
			try
			{
				string fileName = Path.GetFileName(virtualPathAndName);
                if (string.IsNullOrEmpty(fileName))
                    Logger.DebugFormat("filename is empty, call stack: {0}", new StackTrace().ToString());

    		    var rawExtension = Path.GetExtension(fileName);
			    if (rawExtension != null)
			    {
			        string extension = rawExtension.Replace(".", "").ToLowerInvariant();
			        string validExtensions = Entities.Host.Host.FileExtensions.ToLowerInvariant();

			        if (fileName != null && (string.IsNullOrEmpty(extension) || ("," + validExtensions + ",").IndexOf("," + extension + ",", StringComparison.Ordinal) == -1 || Regex.IsMatch(fileName, @"\..+;")))
			        {
			            if (HttpContext.Current != null)
			            {
			                return string.Format(Localization.GetString("RestrictedFileType"), ToEndUserPath(virtualPathAndName), validExtensions.Replace(",", ", *."));
			            }
			            return "RestrictedFileType";
			        }
			    }
			}
			catch (Exception ex)
			{
				return LogUnknownError(ex, virtualPathAndName);
			}

			return string.Empty;
		}

		/// <summary>
		/// Validates disk space available
		/// </summary>
		/// <param name="virtualPathAndName">The system path. ie: C:\WebSites\DotNetNuke_Community\Portals\0\sample.gif</param>
		/// <param name="contentLength">Content Length</param>
		/// <returns>The error message or empty string</returns>
		/// <remarks></remarks>
		private object Check_DiskSpace(string virtualPathAndName, long contentLength)
		{
			try
			{
				var portalCtrl = new PortalController(); 
				if (! (portalCtrl.HasSpaceAvailable(PortalController.GetCurrentPortalSettings().PortalId, contentLength)))
				{
					return string.Format(Localization.GetString("DiskSpaceExceeded"), ToEndUserPath(virtualPathAndName));
				}
			}
			catch (Exception ex)
			{
				return LogUnknownError(ex, virtualPathAndName, contentLength.ToString(CultureInfo.InvariantCulture));
			}

			return string.Empty;
		}

#endregion

#region Misc Helper Methods

		private int GetFileSize(string virtualPathAndFile)
		{
			int returnValue = -1;

			if  (File.Exists(virtualPathAndFile))
			{
				FileStream openFile = null;
				try
				{
					openFile = File.OpenRead(virtualPathAndFile);
                    returnValue = (int)openFile.Length;
				}
				finally
				{
					if (openFile != null)
					{
						openFile.Close();
						openFile.Dispose();
					}
                    else
					    returnValue = -1;
				}
			}

			return returnValue;
		}

 private FolderInfo GetDNNFolder(string path)
		{
			return DNNFolderCtrl.GetFolder(PortalSettings.PortalId, ToDBPath(path), false);
		}

		private FolderController _DNNFolderCtrl;
		private FolderController DNNFolderCtrl
		{
			get { return _DNNFolderCtrl ?? (_DNNFolderCtrl = new FolderController()); }
		}

		private PortalSettings PortalSettings
		{
			get
			{
				return PortalSettings.Current;
			}
		}

		protected internal string LogUnknownError(Exception ex, params string[] @params)
		{
			string returnValue = GetUnknownText();
			var exc = new FileManagerException(GetSystemErrorText(@params), ex);
			Exceptions.LogException(exc);
			return returnValue;
		}

		public string LogDetailError(ErrorCodes errorCode)
		{
			return LogDetailError(errorCode, string.Empty, EnableDetailedLogging);
		}

		public string LogDetailError(ErrorCodes errorCode, string virtualPath)
		{
			return LogDetailError(errorCode, virtualPath, EnableDetailedLogging);
		}

		public string LogDetailError(ErrorCodes errorCode, string virtualPath, bool logError)
		{
			string endUserPath = string.Empty;
			if (! (string.IsNullOrEmpty(virtualPath)))
			{
				endUserPath = (string)ToEndUserPath(virtualPath);
			}

			string returnValue = GetPermissionErrorText();
			string logMsg = string.Empty;

			switch (errorCode)
			{
				case ErrorCodes.AddFolder_NoPermission:
				case ErrorCodes.AddFolder_NotInsecureFolder:
				case ErrorCodes.CopyFolder_NoPermission:
				case ErrorCodes.CopyFolder_NotInsecureFolder:
				case ErrorCodes.DeleteFolder_NoPermission:
				case ErrorCodes.DeleteFolder_NotInsecureFolder:
				case ErrorCodes.DeleteFolder_Protected:
				case ErrorCodes.CannotMoveFolder_ChildrenVisible:
				case ErrorCodes.CannotDeleteFolder_ChildrenVisible:
				case ErrorCodes.CannotCopyFolder_ChildrenVisible:
					logMsg = GetString("ErrorCodes." + errorCode);
					break;
				case ErrorCodes.DeleteFolder_Root:
				case ErrorCodes.RenameFolder_Root:
					logMsg = GetString("ErrorCodes." + errorCode);
					returnValue = string.Format("{0} [{1}]", GetString("ErrorCodes." + errorCode), endUserPath);
					break;
				case ErrorCodes.FileDoesNotExist:
				case ErrorCodes.FolderDoesNotExist:
					logMsg = string.Empty;
					returnValue = string.Format("{0} [{1}]", GetString("ErrorCodes." + errorCode), endUserPath);
					break;
			}

			if (! (string.IsNullOrEmpty(logMsg)))
			{
				var objEventLog = new Services.Log.EventLog.EventLogController();
				var objEventLogInfo = new Services.Log.EventLog.LogInfo();

				objEventLogInfo.AddProperty("From", "TelerikHtmlEditorProvider Message");

				if (PortalSettings.ActiveTab != null)
				{
					objEventLogInfo.AddProperty("TabID", PortalSettings.ActiveTab.TabID.ToString(CultureInfo.InvariantCulture));
					objEventLogInfo.AddProperty("TabName", PortalSettings.ActiveTab.TabName);
				}

				Entities.Users.UserInfo user = Entities.Users.UserController.GetCurrentUserInfo();
				if (user != null)
				{
					objEventLogInfo.AddProperty("UserID", user.UserID.ToString(CultureInfo.InvariantCulture));
					objEventLogInfo.AddProperty("UserName", user.Username);
				}

				objEventLogInfo.LogTypeKey = Services.Log.EventLog.EventLogController.EventLogType.ADMIN_ALERT.ToString();
				objEventLogInfo.AddProperty("Message", logMsg);
				objEventLogInfo.AddProperty("Path", virtualPath);
				objEventLog.AddLog(objEventLogInfo);
			}

			return returnValue;
		}

#endregion

#region Localized Messages

		public string GetString(string key)
		{
			string resourceFile = "/DesktopModules/Admin/RadEditorProvider/" + Localization.LocalResourceDirectory + "/FileManager.resx";
			return Localization.GetString(key, resourceFile);
		}

		private string GetUnknownText()
		{
			try
			{
				return GetString("SystemError.Text");
			}
			catch (Exception ex)
			{
                Logger.Error(ex);
				return "An unknown error occurred.";
			}
		}

		private string GetSystemErrorText(params string[] @params)
		{
			try
			{
				return GetString("SystemError.Text") + " " + string.Join(" | ", @params);
			}
			catch (Exception ex)
			{
                Logger.Error(ex);
				return "An unknown error occurred." + " " + string.Join(" | ", @params);
			}
		}

		private string GetPermissionErrorText()
		{
			return GetString("ErrorCodes." + ErrorCodes.General_PermissionDenied);
		}

#endregion

	}

}