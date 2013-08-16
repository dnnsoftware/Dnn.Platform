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
using System.Text.RegularExpressions;

using DotNetNuke.Common;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Users;
using DotNetNuke.Instrumentation;
using DotNetNuke.Services.Exceptions;
using DotNetNuke.Services.FileSystem;
using DotNetNuke.Services.Localization;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Web;
using System.IO;
using DotNetNuke.Security.Permissions;
using DotNetNuke.Entities.Portals;

namespace DotNetNuke.Providers.RadEditorProvider
{

	public class FileSystemValidation
	{
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof (FileSystemValidation));

		public bool EnableDetailedLogging = true;

#region Public Folder Validate Methods

		public virtual string OnCreateFolder(string virtualPath, string folderName)
		{
			string returnValue = string.Empty;
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
			string returnValue = string.Empty;
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
			string returnValue = string.Empty;
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
			string returnValue = string.Empty;
			try
			{
				returnValue = Check_CanAddToFolder(FileSystemValidation.GetDestinationFolder(virtualPath));
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
			string returnValue = string.Empty;
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
                LogUnknownError(ex, virtualPathAndFile, contentLength.ToString());
            }

        }

		public virtual string OnCreateFile(string virtualPathAndFile, long contentLength)
		{
			string returnValue = string.Empty;
			try
			{
				string virtualPath = (string)RemoveFileName(virtualPathAndFile);
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
				return LogUnknownError(ex, virtualPathAndFile, contentLength.ToString());
			}

			return returnValue;
		}

		public virtual string OnDeleteFile(string virtualPathAndFile)
		{
			string returnValue = string.Empty;
			try
			{
				string virtualPath = (string)RemoveFileName(virtualPathAndFile);

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
				path = (string)CombineVirtualPath(HomeDirectory, path);
			}

			if (string.IsNullOrEmpty(System.IO.Path.GetExtension(path)) && ! (path.EndsWith("/")))
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

			if (string.IsNullOrEmpty(System.IO.Path.GetExtension(path)) && ! (path.EndsWith("/")))
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
			return ToDBPath(path, true);
		}

        private static string ToDBPath(string path, bool removeFileName)
		{
			string returnValue = path;

			returnValue = returnValue.Replace("\\", "/");
			returnValue = (string)(string)FileSystemValidation.RemoveFileName(returnValue);

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
			if (! (string.IsNullOrEmpty(System.IO.Path.GetExtension(path))))
			{
				path = System.IO.Path.GetDirectoryName(path).Replace("\\", "/") + "/";
			}

			return path;
		}

#endregion

#region Public Data Access

		public virtual IDictionary<string, FolderInfo> GetUserFolders()
		{
			return UserFolders;
		}

		public virtual FolderInfo GetUserFolder(string path)
		{
			string dbPath = (string)(string)FileSystemValidation.ToDBPath(path);

			if (UserFolders.ContainsKey(dbPath))
			{
				return UserFolders[dbPath];
			}

			return null;
		}

		public virtual IDictionary<string, FolderInfo> GetChildUserFolders(string parentPath)
		{
			string dbPath = (string)(string)FileSystemValidation.ToDBPath(parentPath);
			IDictionary<string, FolderInfo> returnValue = new Dictionary<string, FolderInfo>();

			if (string.IsNullOrEmpty(dbPath))
			{
				//Get first folder children
				foreach (string folderPath in UserFolders.Keys)
				{
					if (folderPath.IndexOf("/") == folderPath.LastIndexOf("/"))
					{
						returnValue.Add(folderPath, UserFolders[folderPath]);
					}
				}
			}
			else
			{
				foreach (string folderPath in UserFolders.Keys)
				{
					if (folderPath == dbPath || ! (folderPath.StartsWith(dbPath)))
					{
						continue;
					}

					if (folderPath.Contains(dbPath))
					{
						string childPath = folderPath.Substring(dbPath.Length);
						if (childPath.LastIndexOf("/") > -1)
						{
							childPath = childPath.Substring(0, childPath.Length - 1);
						}

						if (! (childPath.Contains("/")))
						{
							returnValue.Add(folderPath, UserFolders[folderPath]);
						}
					}
				}
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

			if (splitPath == FileSystemValidation.HomeDirectory)
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

		public virtual bool CanViewFolder(string path)
		{
			return UserFolders.ContainsKey(ToDBPath(path));
		}

		public virtual bool CanViewFolder(FolderInfo dnnFolder)
		{
			return UserFolders.ContainsKey(dnnFolder.FolderPath);
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
			return Check_CanAddToFolder(GetDNNFolder(virtualPath), false, EnableDetailedLogging);
		}

		private string Check_CanAddToFolder(string virtualPath, bool isFileCheck)
		{
			return Check_CanAddToFolder(GetDNNFolder(virtualPath), isFileCheck, EnableDetailedLogging);
		}

		private string Check_CanAddToFolder(FolderInfo dnnFolder, bool isFileCheck, bool logDetail)
		{
			if (dnnFolder == null)
			{
				return LogDetailError(ErrorCodes.FolderDoesNotExist, ToVirtualPath(dnnFolder.FolderPath), logDetail);
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
			return Check_CanCopyFolder(GetDNNFolder(virtualPath), false, EnableDetailedLogging);
		}

		private string Check_CanCopyFolder(string virtualPath, bool isFileCheck)
		{
			return Check_CanCopyFolder(GetDNNFolder(virtualPath), isFileCheck, EnableDetailedLogging);
		}

		private string Check_CanCopyFolder(FolderInfo dnnFolder, bool isFileCheck, bool logDetail)
		{
			if (dnnFolder == null)
			{
				return LogDetailError(ErrorCodes.FolderDoesNotExist, ToVirtualPath(dnnFolder.FolderPath), logDetail);
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
			return Check_CanDeleteFolder(GetDNNFolder(virtualPath), false, EnableDetailedLogging);
		}

		private string Check_CanDeleteFolder(string virtualPath, bool isFileCheck)
		{
			return Check_CanDeleteFolder(GetDNNFolder(virtualPath), isFileCheck, EnableDetailedLogging);
		}

		private string Check_CanDeleteFolder(string virtualPath, bool isFileCheck, bool logDetail)
		{
			return Check_CanDeleteFolder(GetDNNFolder(virtualPath), isFileCheck, EnableDetailedLogging);
		}

		private string Check_CanDeleteFolder(FolderInfo dnnFolder, bool isFileCheck, bool logDetail)
		{
			if (dnnFolder == null)
			{
				return LogDetailError(ErrorCodes.FolderDoesNotExist, ToVirtualPath(dnnFolder.FolderPath), logDetail);
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
				System.Diagnostics.Debug.Assert(! (string.IsNullOrEmpty(fileName)), "fileName is empty");

				string extension = Path.GetExtension(fileName).Replace(".", "").ToLowerInvariant();
				string validExtensions = DotNetNuke.Entities.Host.Host.FileExtensions.ToLowerInvariant();

                if (string.IsNullOrEmpty(extension) || ("," + validExtensions + ",").IndexOf("," + extension + ",") == -1 || Regex.IsMatch(fileName, @"\..+;"))
				{
					if (HttpContext.Current != null)
					{
						return string.Format(Localization.GetString("RestrictedFileType"), ToEndUserPath(virtualPathAndName), validExtensions.Replace(",", ", *."));
					}
					else
					{
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
				string fileName = Path.GetFileName(virtualPathAndName);
				PortalController portalCtrl = new PortalController();
				if (! (portalCtrl.HasSpaceAvailable(PortalController.GetCurrentPortalSettings().PortalId, contentLength)))
				{
					return string.Format(Localization.GetString("DiskSpaceExceeded"), ToEndUserPath(virtualPathAndName));
				}
			}
			catch (Exception ex)
			{
				return LogUnknownError(ex, virtualPathAndName, contentLength.ToString());
			}

			return string.Empty;
		}

#endregion

#region Misc Helper Methods

		private int GetFileSize(string virtualPathAndFile)
		{
			int returnValue = -1;

			if (! (File.Exists(virtualPathAndFile)))
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
					returnValue = -1;
				}
			}

			return returnValue;
		}

		private IDictionary<string, FolderInfo> _UserFolders = null;
		private IDictionary<string, FolderInfo> UserFolders
		{
			get
			{
				if (_UserFolders == null)
				{
					_UserFolders = new Dictionary<string, FolderInfo>(StringComparer.InvariantCultureIgnoreCase);

                    var folders = FileSystemUtils.GetFoldersByUser(PortalSettings.PortalId, true, true, "READ");

					foreach (var folder in folders)
					{
						FolderInfo dnnFolder = (FolderInfo)folder;
						string folderPath = dnnFolder.FolderPath;

						if (! (string.IsNullOrEmpty(folderPath)) && folderPath.Substring(folderPath.Length - 1, 1) == "/")
						{
							folderPath = folderPath.Remove(folderPath.Length - 1, 1);
						}

						if (! (string.IsNullOrEmpty(folderPath)) && folderPath.Contains("/"))
						{
							string[] folderPaths = folderPath.Split('/');
							//If (folderPaths.Length > 0) Then
							string addPath = string.Empty;
							foreach (var addFolderPath in folderPaths)
							{
								if (string.IsNullOrEmpty(addPath))
								{
									addPath = addFolderPath + "/";
								}
								else
								{
									addPath = addPath + addFolderPath + "/";
								}

								if (_UserFolders.ContainsKey(addPath))
								{
									continue;
								}

								FolderInfo addFolder = GetDNNFolder(addPath);
								if (addFolder == null)
								{
									break;
								}

								_UserFolders.Add(addFolder.FolderPath, addFolder);
							}
						}
						else
						{
							_UserFolders.Add(dnnFolder.FolderPath, dnnFolder);
						}
					}
				}
				return _UserFolders;
			}
		}

		private FolderInfo GetDNNFolder(string path)
		{
			return DNNFolderCtrl.GetFolder(PortalSettings.PortalId, FileSystemValidation.ToDBPath(path), false);
		}

		private FolderController _DNNFolderCtrl = null;
		private FolderController DNNFolderCtrl
		{
			get
			{
				if (_DNNFolderCtrl == null)
				{
					_DNNFolderCtrl = new FolderController();
				}
				return _DNNFolderCtrl;
			}
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
			FileManagerException exc = new FileManagerException(GetSystemErrorText(@params), ex);
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

			string returnValue = GetPermissionErrorText(endUserPath);
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
				Services.Log.EventLog.EventLogController objEventLog = new Services.Log.EventLog.EventLogController();
				Services.Log.EventLog.LogInfo objEventLogInfo = new Services.Log.EventLog.LogInfo();

				objEventLogInfo.AddProperty("From", "TelerikHtmlEditorProvider Message");

				if (PortalSettings.ActiveTab != null)
				{
					objEventLogInfo.AddProperty("TabID", PortalSettings.ActiveTab.TabID.ToString());
					objEventLogInfo.AddProperty("TabName", PortalSettings.ActiveTab.TabName);
				}

				Entities.Users.UserInfo user = Entities.Users.UserController.GetCurrentUserInfo();
				if (user != null)
				{
					objEventLogInfo.AddProperty("UserID", user.UserID.ToString());
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
			string resourceFile = "/DesktopModules/Admin/RadEditorProvider/" + DotNetNuke.Services.Localization.Localization.LocalResourceDirectory + "/FileManager.resx";
			return DotNetNuke.Services.Localization.Localization.GetString(key, resourceFile);
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

		private string GetPermissionErrorText(string path)
		{
			return GetString("ErrorCodes." + ErrorCodes.General_PermissionDenied.ToString());
			//message text is weird in this scenario
			//Return String.Format(Localization.Localization.GetString("InsufficientFolderPermission"), path)
		}

#endregion

	}

}