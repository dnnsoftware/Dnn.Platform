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

using System.Web.UI;

using DotNetNuke.Common;
using DotNetNuke.Common.Utilities;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using DotNetNuke.Entities.Portals;

using Telerik.Web.UI.Widgets;
using DotNetNuke.Services.FileSystem;
using System.IO;

using FileItem = Telerik.Web.UI.Widgets.FileItem;

namespace DotNetNuke.Providers.RadEditorProvider
{

	public class TelerikFileBrowserProvider : FileSystemContentProvider
	{

		/// <summary>
		/// The current portal will be used for file access.
		/// </summary>
		/// <param name="context"></param>
		/// <param name="searchPatterns"></param>
		/// <param name="viewPaths"></param>
		/// <param name="uploadPaths"></param>
		/// <param name="deletePaths"></param>
		/// <param name="selectedUrl"></param>
		/// <param name="selectedItemTag"></param>
		/// <remarks></remarks>
		public TelerikFileBrowserProvider(HttpContext context, string[] searchPatterns, string[] viewPaths, string[] uploadPaths, string[] deletePaths, string selectedUrl, string selectedItemTag) : base(context, searchPatterns, viewPaths, uploadPaths, deletePaths, selectedUrl, selectedItemTag)
		{
		}

#region Overrides

		public override Stream GetFile(string url)
		{
			//base calls CheckWritePermissions method
            Stream fileContent = null;
		    var folderPath = FileSystemValidation.ToDBPath(url);
		    var fileName = GetFileName(url);
            var folder = DNNValidator.GetUserFolder(folderPath);
            if (folder != null)
            {
                var file = FileManager.Instance.GetFile(folder, fileName);
                if (file != null)
                {
                    fileContent = FileManager.Instance.GetFileContent(file);
                }
            }
            return fileContent;
		}

		public override string GetPath(string url)
		{
			return TelerikContent.GetPath(FileSystemValidation.ToVirtualPath(url));
		}

		public override string GetFileName(string url)
		{
			return TelerikContent.GetFileName(FileSystemValidation.ToVirtualPath(url));
		}

		public override string CreateDirectory(string path, string name)
		{
			try
			{
                var directoryName = name.Trim();
				var virtualPath = FileSystemValidation.ToVirtualPath(path);

                var returnValue = DNNValidator.OnCreateFolder(virtualPath, directoryName);
				if (! (string.IsNullOrEmpty(returnValue)))
				{
					return returnValue;
				}

				//Returns errors or empty string when successful (ie: DirectoryAlreadyExists, InvalidCharactersInPath)
                returnValue = TelerikContent.CreateDirectory(virtualPath, directoryName);

				if (! (string.IsNullOrEmpty(returnValue)))
				{
					return GetTelerikMessage(returnValue);
				}

				if (string.IsNullOrEmpty(returnValue))
				{
                    var virtualNewPath = FileSystemValidation.CombineVirtualPath(virtualPath, directoryName);
					var newFolderID = DNNFolderCtrl.AddFolder(PortalSettings.PortalId, FileSystemValidation.ToDBPath(virtualNewPath));
					FileSystemUtils.SetFolderPermissions(PortalSettings.PortalId, newFolderID, FileSystemValidation.ToDBPath(virtualNewPath));
                    //make sure that the folder is flaged secure if necessary
                    DNNValidator.OnFolderCreated(virtualNewPath, virtualPath);
				}

				return returnValue;
			}
			catch (Exception ex)
			{
                return DNNValidator.LogUnknownError(ex, path, name);
			}
		}

		public override string MoveDirectory(string path, string newPath)
		{
			try
			{
				string virtualPath = (string)(string)FileSystemValidation.ToVirtualPath(path);
				string virtualNewPath = (string)(string)FileSystemValidation.ToVirtualPath(newPath);
				string virtualDestinationPath = FileSystemValidation.GetDestinationFolder(virtualNewPath);

				string returnValue = string.Empty;
				if (FileSystemValidation.GetDestinationFolder(virtualPath) == virtualDestinationPath)
				{
					//rename directory
					returnValue = DNNValidator.OnRenameFolder(virtualPath);
					if (! (string.IsNullOrEmpty(returnValue)))
					{
						return returnValue;
					}
				}
				else
				{
					//move directory
					returnValue = DNNValidator.OnMoveFolder(virtualPath, virtualDestinationPath);
					if (! (string.IsNullOrEmpty(returnValue)))
					{
						return returnValue;
					}
				}

				//Are all items visible to user?
				FolderInfo folder = DNNValidator.GetUserFolder(virtualPath);
				if (! (CheckAllChildrenVisible(ref folder)))
				{
					return DNNValidator.LogDetailError(ErrorCodes.CannotMoveFolder_ChildrenVisible);
				}

				//Returns errors or empty string when successful (ie: Cannot create a file when that file already exists)
				returnValue = TelerikContent.MoveDirectory(virtualPath, virtualNewPath);

				if (string.IsNullOrEmpty(returnValue))
				{
                    //make sure folder name is being updated in database
                    DNNValidator.OnFolderRenamed(virtualPath, virtualNewPath);
					//Sync to remove old folder & files        
					FileSystemUtils.SynchronizeFolder(PortalSettings.PortalId, HttpContext.Current.Request.MapPath(virtualPath), FileSystemValidation.ToDBPath(virtualPath), true, true, true);
					//Sync to add new folder & files
					FileSystemUtils.SynchronizeFolder(PortalSettings.PortalId, HttpContext.Current.Request.MapPath(virtualNewPath), FileSystemValidation.ToDBPath(virtualNewPath), true, true, true);
				}

				return returnValue;
			}
			catch (Exception ex)
			{
				return DNNValidator.LogUnknownError(ex, path, newPath);
			}
		}

		public override string CopyDirectory(string path, string newPath)
		{
			try
			{
				string virtualPath = (string)(string)FileSystemValidation.ToVirtualPath(path);
				string virtualNewPath = (string)(string)FileSystemValidation.ToVirtualPath(newPath);
				string virtualDestinationPath = FileSystemValidation.GetDestinationFolder(virtualNewPath);

				string returnValue = DNNValidator.OnCopyFolder(virtualPath, virtualDestinationPath);
				if (! (string.IsNullOrEmpty(returnValue)))
				{
					return returnValue;
				}

				//Are all items visible to user?
				//todo: copy visible files and folders only?
				FolderInfo folder = DNNValidator.GetUserFolder(virtualPath);
				if (! (CheckAllChildrenVisible(ref folder)))
				{
					return DNNValidator.LogDetailError(ErrorCodes.CannotCopyFolder_ChildrenVisible);
				}

				returnValue = TelerikContent.CopyDirectory(virtualPath, virtualNewPath);

				if (string.IsNullOrEmpty(returnValue))
				{
					//Sync to add new folder & files
					FileSystemUtils.SynchronizeFolder(PortalSettings.PortalId, HttpContext.Current.Request.MapPath(virtualNewPath), FileSystemValidation.ToDBPath(virtualNewPath), true, true, true);
				}

				return returnValue;
			}
			catch (Exception ex)
			{
				return DNNValidator.LogUnknownError(ex, path, newPath);
			}
		}

		public override string DeleteDirectory(string path)
		{
			try
			{
				string virtualPath = FileSystemValidation.ToVirtualPath(path);

				string returnValue = DNNValidator.OnDeleteFolder(virtualPath);
				if (! (string.IsNullOrEmpty(returnValue)))
				{
					return returnValue;
				}

				//Are all items visible to user?
				FolderInfo folder = DNNValidator.GetUserFolder(virtualPath);
				if (!CheckAllChildrenVisible(ref folder))
				{
					return DNNValidator.LogDetailError(ErrorCodes.CannotDeleteFolder_ChildrenVisible);
				}

				returnValue = TelerikContent.DeleteDirectory(virtualPath);

				if (string.IsNullOrEmpty(returnValue))
				{
                    FolderManager.Instance.DeleteFolder(folder);
				}

				return returnValue;
			}
			catch (Exception ex)
			{
				return DNNValidator.LogUnknownError(ex, path);
			}
		}

		public override string DeleteFile(string path)
		{
			try
			{
				string virtualPathAndFile = (string)(string)FileSystemValidation.ToVirtualPath(path);

				string returnValue = DNNValidator.OnDeleteFile(virtualPathAndFile);
				if (! (string.IsNullOrEmpty(returnValue)))
				{
					return returnValue;
				}

				returnValue = TelerikContent.DeleteFile(virtualPathAndFile);

				if (string.IsNullOrEmpty(returnValue))
				{
					string virtualPath = (string)(string)FileSystemValidation.RemoveFileName(virtualPathAndFile);
					FolderInfo dnnFolder = DNNValidator.GetUserFolder(virtualPath);
					DNNFileCtrl.DeleteFile(PortalSettings.PortalId, System.IO.Path.GetFileName(virtualPathAndFile), dnnFolder.FolderID, true);
				}

				return returnValue;
			}
			catch (Exception ex)
			{
				return DNNValidator.LogUnknownError(ex, path);
			}
		}

		public override string MoveFile(string path, string newPath)
		{
			try
			{
				string virtualPathAndFile = FileSystemValidation.ToVirtualPath(path);
				string virtualNewPathAndFile = FileSystemValidation.ToVirtualPath(newPath);

				string virtualPath = FileSystemValidation.RemoveFileName(virtualPathAndFile);
				string virtualNewPath = FileSystemValidation.RemoveFileName(virtualNewPathAndFile);

				string returnValue = string.Empty;
				if (virtualPath == virtualNewPath)
				{
					//rename file
					returnValue = DNNValidator.OnRenameFile(virtualPathAndFile);
					if (! (string.IsNullOrEmpty(returnValue)))
					{
						return returnValue;
					}
				}
				else
				{
					//move file
					returnValue = DNNValidator.OnMoveFile(virtualPathAndFile, virtualNewPathAndFile);
					if (! (string.IsNullOrEmpty(returnValue)))
					{
						return returnValue;
					}
				}

				//Returns errors or empty string when successful (ie: NewFileAlreadyExists)
				//returnValue = TelerikContent.MoveFile(virtualPathAndFile, virtualNewPathAndFile);
                var folderPath = FileSystemValidation.ToDBPath(path);
                var folder = FolderManager.Instance.GetFolder(PortalSettings.PortalId, folderPath);
                if (folder != null)
                {
                    var file = FileManager.Instance.GetFile(folder, GetFileName(virtualPathAndFile));

                    if (file != null)
                    {
                        var destFolderPath = FileSystemValidation.ToDBPath(newPath);
                        var destFolder = FolderManager.Instance.GetFolder(PortalSettings.PortalId, destFolderPath);
                        var destFileName = GetFileName(virtualNewPathAndFile);
                        
                        if (destFolder != null)
                        {
                            if (file.FolderId != destFolder.FolderID
                                 && FileManager.Instance.GetFile(destFolder, file.FileName) != null)
                            {
                                returnValue = "FileExists";
                            }
                            else
                            {
                                FileManager.Instance.MoveFile(file, destFolder);
                                FileManager.Instance.RenameFile(file, destFileName);
                            }
                        }
                    }
                    else
                    {
                        returnValue = "FileNotFound";
                    }
                }
			    if (! (string.IsNullOrEmpty(returnValue)))
				{
					return GetTelerikMessage(returnValue);
				}

				return returnValue;
			}
			catch (Exception ex)
			{
				return DNNValidator.LogUnknownError(ex, path, newPath);
			}
		}

		public override string CopyFile(string path, string newPath)
		{
			try
			{
				string virtualPathAndFile = (string)(string)FileSystemValidation.ToVirtualPath(path);
				string virtualNewPathAndFile = (string)(string)FileSystemValidation.ToVirtualPath(newPath);

				string returnValue = DNNValidator.OnCopyFile(virtualPathAndFile, virtualNewPathAndFile);
				if (! (string.IsNullOrEmpty(returnValue)))
				{
					return returnValue;
				}

				//Returns errors or empty string when successful (ie: NewFileAlreadyExists)
				returnValue = TelerikContent.CopyFile(virtualPathAndFile, virtualNewPathAndFile);

				if (string.IsNullOrEmpty(returnValue))
				{
					string virtualNewPath = (string)(string)FileSystemValidation.RemoveFileName(virtualNewPathAndFile);
					FolderInfo dnnFolder = DNNValidator.GetUserFolder(virtualNewPath);
					DotNetNuke.Services.FileSystem.FileInfo dnnFileInfo = new DotNetNuke.Services.FileSystem.FileInfo();
					FillFileInfo(virtualNewPathAndFile, ref dnnFileInfo);

					DNNFileCtrl.AddFile(PortalSettings.PortalId, dnnFileInfo.FileName, dnnFileInfo.Extension, dnnFileInfo.Size, dnnFileInfo.Width, dnnFileInfo.Height, dnnFileInfo.ContentType, dnnFolder.FolderPath, dnnFolder.FolderID, true);
				}

				return returnValue;
			}
			catch (Exception ex)
			{
				return DNNValidator.LogUnknownError(ex, path, newPath);
			}
		}

		public override string StoreFile(System.Web.HttpPostedFile file, string path, string name, params string[] arguments)
		{
			return StoreFile(Telerik.Web.UI.UploadedFile.FromHttpPostedFile(file), path, name, arguments);
		}

		public override string StoreFile(Telerik.Web.UI.UploadedFile file, string path, string name, params string[] arguments)
		{
		    try
		    {
                // TODO: Create entries in .resx for these messages
			    Uri uri;
			    if (!Uri.TryCreate(name, UriKind.Relative, out uri))
			    {                    
                    ShowMessage(string.Format("The file {0} cannot be uplodaded because it would create an invalid URL. Please, rename the file before upload.", name));
			        return "";
			    }

                var invalidChars = new[] {'<', '>', '*', '%', '&', ':', '\\', '?', '+'};
			    if (invalidChars.Any(uri.ToString().Contains))
			    {
                    ShowMessage(string.Format("The file {0} contains some invalid characters. The file name cannot contain any of the following characters: {1}", name, new String(invalidChars)));
                    return "";
                }

			    string virtualPath = FileSystemValidation.ToVirtualPath(path);

				string returnValue = DNNValidator.OnCreateFile(FileSystemValidation.CombineVirtualPath(virtualPath, name), file.ContentLength);
				if (!string.IsNullOrEmpty(returnValue))
				{
					return returnValue;
				}

                var folder = DNNValidator.GetUserFolder(virtualPath);

			    var fileInfo = new DotNetNuke.Services.FileSystem.FileInfo();
                FillFileInfo(file, ref fileInfo);

				//Add or update file
				FileManager.Instance.AddFile(folder, name, file.InputStream);

				return returnValue;
			}
			catch (Exception ex)
			{
				return DNNValidator.LogUnknownError(ex, path, name);
			}
		}

        private void ShowMessage(string message)
        {
            var pageObject = HttpContext.Current.Handler as Page;

            ScriptManager.RegisterClientScriptBlock(pageObject, pageObject.GetType(), "showAlertFromServer", @"
                    function showradAlertFromServer(message)
                    {
                        function f()
                        {// MS AJAX Framework is liaded
                            Sys.Application.remove_load(f);
                            // RadFileExplorer already contains a RadWindowManager inside, so radalert can be called without problem
                            radalert(message);
                        }

                        Sys.Application.add_load(f);
                    }", true); 

            var script = string.Format("showradAlertFromServer('{0}');", message);
            ScriptManager.RegisterStartupScript(pageObject, pageObject.GetType(), "KEY", script, true);
        }

		public override string StoreBitmap(System.Drawing.Bitmap bitmap, string url, System.Drawing.Imaging.ImageFormat format)
		{
			try
			{
				//base calls CheckWritePermissions method			
				string virtualPathAndFile = (string)(string)FileSystemValidation.ToVirtualPath(url);
				string virtualPath = (string)(string)FileSystemValidation.RemoveFileName(virtualPathAndFile);
				string returnValue = DNNValidator.OnCreateFile(virtualPathAndFile, 0);
				if (! (string.IsNullOrEmpty(returnValue)))
				{
					return returnValue;
				}

				returnValue = TelerikContent.StoreBitmap(bitmap, virtualPathAndFile, format);

				Services.FileSystem.FileInfo dnnFileInfo = new Services.FileSystem.FileInfo();
				FillFileInfo(virtualPathAndFile, ref dnnFileInfo);

				//check again with real contentLength
				string errMsg = DNNValidator.OnCreateFile(virtualPathAndFile, dnnFileInfo.Size);
				if (! (string.IsNullOrEmpty(errMsg)))
				{
					TelerikContent.DeleteFile(virtualPathAndFile);
					return errMsg;
				}

				FolderInfo dnnFolder = DNNValidator.GetUserFolder(virtualPath);
				Services.FileSystem.FileInfo dnnFile = DNNFileCtrl.GetFile(dnnFileInfo.FileName, PortalSettings.PortalId, dnnFolder.FolderID);

				if (dnnFile != null)
				{
					DNNFileCtrl.UpdateFile(dnnFile.FileId, dnnFileInfo.FileName, dnnFileInfo.Extension, dnnFileInfo.Size, bitmap.Width, bitmap.Height, dnnFileInfo.ContentType, dnnFolder.FolderPath, dnnFolder.FolderID);
				}
				else
				{
					DNNFileCtrl.AddFile(PortalSettings.PortalId, dnnFileInfo.FileName, dnnFileInfo.Extension, dnnFileInfo.Size, bitmap.Width, bitmap.Height, dnnFileInfo.ContentType, dnnFolder.FolderPath, dnnFolder.FolderID, true);
				}

				return returnValue;
			}
			catch (Exception ex)
			{
				return DNNValidator.LogUnknownError(ex, url);
			}
		}

		public override DirectoryItem ResolveDirectory(string path)
		{
			try
			{
				System.Diagnostics.Debug.WriteLine(DateTime.Now.ToLongTimeString() + "ResolveDirectory: " + path);
				return GetDirectoryItemWithDNNPermissions(path, true);
			}
			catch (Exception ex)
			{
				DNNValidator.LogUnknownError(ex, path);
				return null;
			}
		}

		public override DirectoryItem ResolveRootDirectoryAsTree(string path)
		{
			try
			{
				System.Diagnostics.Debug.WriteLine(DateTime.Now.ToLongTimeString() + "ResolveRootDirectoryAsTree: " + path);
				return GetDirectoryItemWithDNNPermissions(path, false);
			}
			catch (Exception ex)
			{
				DNNValidator.LogUnknownError(ex, path);
				return null;
			}
		}

		public override DirectoryItem[] ResolveRootDirectoryAsList(string path)
		{
			try
			{
				System.Diagnostics.Debug.WriteLine(DateTime.Now.ToLongTimeString() + "ResolveRootDirectoryAsList: " + path);
				return GetDirectoryItemWithDNNPermissions(path, false).Directories;
			}
			catch (Exception ex)
			{
				DNNValidator.LogUnknownError(ex, path);
				return null;
			}
		}

#endregion

#region Properties

		private FileSystemValidation _DNNValidator;
		private FileSystemValidation DNNValidator
		{
			get
			{
			    return _DNNValidator ?? (_DNNValidator = new FileSystemValidation());
			}
		}

		private Entities.Portals.PortalSettings PortalSettings
		{
			get
			{
				return Entities.Portals.PortalSettings.Current;
			}
		}

		private DotNetNuke.Entities.Users.UserInfo CurrentUser
		{
			get
			{
				return DotNetNuke.Entities.Users.UserController.GetCurrentUserInfo();
			}
		}

		private Telerik.Web.UI.Widgets.FileSystemContentProvider _TelerikContent = null;
		private Telerik.Web.UI.Widgets.FileSystemContentProvider TelerikContent
		{
			get
			{
				if (_TelerikContent == null)
				{
					_TelerikContent = new Telerik.Web.UI.Widgets.FileSystemContentProvider(this.Context, this.SearchPatterns, new string[] {FileSystemValidation.HomeDirectory}, new string[] {FileSystemValidation.HomeDirectory}, new string[] {FileSystemValidation.HomeDirectory}, FileSystemValidation.ToVirtualPath(this.SelectedUrl), FileSystemValidation.ToVirtualPath(this.SelectedItemTag));
				}
				return _TelerikContent;
			}
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

		private FileController _DNNFileCtrl = null;
		private FileController DNNFileCtrl
		{
			get
			{
				if (_DNNFileCtrl == null)
				{
					_DNNFileCtrl = new FileController();
				}
				return _DNNFileCtrl;
			}
		}

		public bool NotUseRelativeUrl
		{
			get
			{
				return HttpContext.Current.Request.QueryString["nuru"] == "1";
			}
		}

#endregion

#region Private

		private DirectoryItem GetDirectoryItemWithDNNPermissions(string path, bool loadFiles)
		{
			var radDirectory = TelerikContent.ResolveDirectory(FileSystemValidation.ToVirtualPath(path));
		    var directoryArray = new[] {radDirectory};
            var returnValues = AddChildDirectoriesToList(ref directoryArray, true, loadFiles);

			if (returnValues != null && returnValues.Length > 0)
			{
				return returnValues[0];
			}

			return null;
		}

		private DirectoryItem[] AddChildDirectoriesToList(ref DirectoryItem[] radDirectories, bool recursive, bool loadFiles)
		{
			var newDirectories = new ArrayList();
		    var invalidFolders = new List<DirectoryItem>();

			foreach (var radDirectory in radDirectories)
			{
			    System.Diagnostics.Debug.WriteLine(DateTime.Now.ToLongTimeString() + " AddChildDirectoriesToList " + radDirectory.Name);

			    var endUserPath = (string) FileSystemValidation.ToEndUserPath(radDirectory.FullPath);

			    var folderPath = radDirectory.FullPath.EndsWith("/") ? radDirectory.FullPath : radDirectory.FullPath + "/";

			    var dnnFolder = DNNValidator.GetUserFolder(folderPath);

			    if (dnnFolder == null)
			    {
                    invalidFolders.Add(radDirectory);
			        continue;
			    }
			    
                //Don't show protected folders
			    if (!string.IsNullOrEmpty(dnnFolder.FolderPath) && dnnFolder.IsProtected)
			    {
                    invalidFolders.Add(radDirectory);
			        continue;
			    }

			    //Don't show Cache folder
			    if (dnnFolder.FolderPath.ToLowerInvariant() == "cache/")
			    {
                    invalidFolders.Add(radDirectory);
			        continue;
			    }

			    var showFiles = new ArrayList();
			    var folderPermissions = PathPermissions.Read;

                if (DNNValidator.CanViewFilesInFolder(dnnFolder))
                {
                    if (DNNValidator.CanAddToFolder(dnnFolder))
                    {
                        folderPermissions = folderPermissions | PathPermissions.Upload;
                    }

                    if (DNNValidator.CanDeleteFolder(dnnFolder))
                    {
                        folderPermissions = folderPermissions | PathPermissions.Delete;
                    }

                    if (loadFiles)
                    {
                        var files = FolderManager.Instance.GetFiles(dnnFolder);
                        foreach (var fileInfo in files)
                        {
                            showFiles.Add(new FileItem(fileInfo.FileName, fileInfo.Extension, fileInfo.Size, "", GetFileUrl(fileInfo), "", folderPermissions));
                        }
                    }

                    var folderFiles = (FileItem[]) showFiles.ToArray(typeof (FileItem));

                    //Root folder name
                    var dirName = radDirectory.Name;
                    if (dnnFolder.FolderPath == "" && dnnFolder.FolderName == "")
                    {
                        dirName = FileSystemValidation.EndUserHomeDirectory;
                    }

                    DirectoryItem newDirectory;
                    if (recursive)
                    {
                        var directory = TelerikContent.ResolveRootDirectoryAsTree(radDirectory.Path);
                        var tempVar2 = directory.Directories;                        
                        if (dnnFolder.FolderPath == "" && dnnFolder.FolderName == "") //Replace USERS folder by the particular User Folder
                        {
                            AddUserFolder(ref tempVar2, loadFiles);
                        }
                        AddChildDirectoriesToList(ref tempVar2, false, false);
                        newDirectory = new DirectoryItem(dirName, "", endUserPath, "", folderPermissions, folderFiles, tempVar2);
                        radDirectory.Directories = tempVar2;
                    }
                    else
                    {
                        newDirectory = new DirectoryItem(dirName, "", endUserPath, "", folderPermissions, folderFiles, new DirectoryItem[0]);
                    }

                    newDirectories.Add(newDirectory);
                }
                else
                {
                    invalidFolders.Add(radDirectory);
                }
			}
            //remove invalid folders
		    radDirectories = radDirectories.Where(d => !invalidFolders.Contains(d)).ToArray();

		    return (DirectoryItem[])newDirectories.ToArray(typeof(DirectoryItem));
		}

        private void AddUserFolder(ref DirectoryItem[] directories, bool loadFiles)
        {
            var usersMainFolder = directories.SingleOrDefault(d => d.Name.ToUpper() == "USERS");
            if(usersMainFolder != null)
            {
                var userFolder = FolderManager.Instance.GetUserFolder(CurrentUser);
                usersMainFolder.Name = FolderManager.Instance.MyFolderName;
                string endUserPath = (string) FileSystemValidation.ToEndUserPath(userFolder.FolderPath);
                usersMainFolder.FullPath = endUserPath;

                var folderPermissions = PathPermissions.Read;
                if (DNNValidator.CanViewFilesInFolder((FolderInfo)userFolder))
                {
                    if (DNNValidator.CanAddToFolder((FolderInfo)userFolder))
                    {
                        folderPermissions = folderPermissions | PathPermissions.Upload;
                    }

                    if (DNNValidator.CanDeleteFolder((FolderInfo)userFolder))
                    {
                        folderPermissions = folderPermissions | PathPermissions.Delete;
                    }
                }
                usersMainFolder.Permissions = folderPermissions;

                //var showFiles = new ArrayList();
                //if (loadFiles)
                //{
                //    var files = FolderManager.Instance.GetFiles((FolderInfo)userFolder);
                //    foreach (var fileInfo in files)
                //    {
                //        showFiles.Add(new FileItem(fileInfo.FileName, fileInfo.Extension, fileInfo.Size, "", FileManager.Instance.GetUrl(fileInfo), "", folderPermissions));
                //    }
                //}
                //var folderFiles = (FileItem[]) showFiles.ToArray(typeof (FileItem));

                //var newDirectory = new DirectoryItem(userFolder.DisplayName, "", endUserPath, "", folderPermissions, folderFiles, new DirectoryItem[0]);
                

            }
            
        }

		private IDictionary<string, Services.FileSystem.FileInfo> GetDNNFiles(int dnnFolderID)
		{
			System.Data.IDataReader drFiles = null;
			IDictionary<string, Services.FileSystem.FileInfo> dnnFiles = null;

			try
			{
				drFiles = DNNFileCtrl.GetFiles(PortalSettings.PortalId, dnnFolderID);
				dnnFiles = CBO.FillDictionary<string, Services.FileSystem.FileInfo>("FileName", drFiles);
			}
			finally
			{
				if (drFiles != null)
				{
					if (! drFiles.IsClosed)
					{
						drFiles.Close();
					}
				}
			}

			return dnnFiles;
		}

		private bool CheckAllChildrenVisible(ref FolderInfo folder)
		{
			string virtualPath = FileSystemValidation.ToVirtualPath(folder.FolderPath);

			//check files are visible
			IDictionary<string, Services.FileSystem.FileInfo> files = GetDNNFiles(folder.FolderID);
			int visibleFileCount = 0;
			foreach (Services.FileSystem.FileInfo fileItem in files.Values)
			{
				string[] tempVar = SearchPatterns;
				if (CheckSearchPatterns(fileItem.FileName, ref tempVar))
				{
					visibleFileCount = visibleFileCount + 1;
				}
			}

			if (visibleFileCount != Directory.GetFiles(HttpContext.Current.Request.MapPath(virtualPath)).Length)
			{
				return false;
			}

			//check folders
			if (folder != null)
			{
				IDictionary<string, FolderInfo> childUserFolders = DNNValidator.GetChildUserFolders(virtualPath);

				if (childUserFolders.Count != Directory.GetDirectories(HttpContext.Current.Request.MapPath(virtualPath)).Length)
				{
					return false;
				}

				//check children
				foreach (FolderInfo childFolder in childUserFolders.Values)
				{
					//do recursive check
					FolderInfo tempVar2 = childFolder;
					if (! (CheckAllChildrenVisible(ref tempVar2)))
					{
						return false;
					}
				}
			}

			return true;
		}

		private void FillFileInfo(string virtualPathAndFile, ref DotNetNuke.Services.FileSystem.FileInfo fileInfo)
		{
			fileInfo.FileName = Path.GetFileName(virtualPathAndFile);
			fileInfo.Extension = Path.GetExtension(virtualPathAndFile);
			if (fileInfo.Extension.StartsWith("."))
			{
				fileInfo.Extension = fileInfo.Extension.Remove(0, 1);
			}

			fileInfo.ContentType = FileSystemUtils.GetContentType(fileInfo.Extension);

			FileStream fileStream = null;
			try
			{
				fileStream = File.OpenRead(HttpContext.Current.Request.MapPath(virtualPathAndFile));
				FillImageInfo(fileStream, ref fileInfo);
			}
			finally
			{
				if (fileStream != null)
				{
					fileStream.Close();
					fileStream.Dispose();
				}
			}
		}

		private void FillFileInfo(Telerik.Web.UI.UploadedFile file, ref DotNetNuke.Services.FileSystem.FileInfo fileInfo)
		{
			//The core API expects the path to be stripped off the filename
			fileInfo.FileName = ((file.FileName.Contains("\\")) ? System.IO.Path.GetFileName(file.FileName) : file.FileName);
			fileInfo.Extension = file.GetExtension();
			if (fileInfo.Extension.StartsWith("."))
			{
				fileInfo.Extension = fileInfo.Extension.Remove(0, 1);
			}

			fileInfo.ContentType = FileSystemUtils.GetContentType(fileInfo.Extension);

			FillImageInfo(file.InputStream, ref fileInfo);
		}

		private void FillImageInfo(Stream fileStream, ref DotNetNuke.Services.FileSystem.FileInfo fileInfo)
		{
		    var imageExtensions = new FileExtensionWhitelist(Common.Globals.glbImageFileTypes);
			if (imageExtensions.IsAllowedExtension(fileInfo.Extension))
			{
				System.Drawing.Image img = null;
				try
				{
					img = System.Drawing.Image.FromStream(fileStream);
					if (fileStream.Length > int.MaxValue)
					{
						fileInfo.Size = int.MaxValue;
					}
					else
					{
						fileInfo.Size = int.Parse(fileStream.Length.ToString());
					}
					fileInfo.Width = img.Width;
					fileInfo.Height = img.Height;
				}
				catch
				{
					// error loading image file
					fileInfo.ContentType = "application/octet-stream";
				}
				finally
				{
					if (img != null)
					{
						img.Dispose();
					}
				}
			}
		}

		private string GetFileUrl(IFileInfo file)
		{
			var url = FileManager.Instance.GetUrl(file);
			if (url.Contains("LinkClick") && NotUseRelativeUrl)
			{
				url = string.Format("{0}{1}{2}{3}",
				                    (HttpContext.Current.Request.IsSecureConnection ? "https://" : "http://"),
									HttpContext.Current.Request.Url.Host,
				                    (!HttpContext.Current.Request.Url.IsDefaultPort ? ":" + HttpContext.Current.Request.Url.Port : string.Empty),
				                    url);
			}

			return url;
		}

#endregion

#region Search Patterns

		private bool CheckSearchPatterns(string dnnFileName, ref string[] searchPatterns)
		{
			if (searchPatterns == null | searchPatterns.Length < 1)
			{
				return true;
			}

			bool returnValue = false;
			foreach (string pattern in searchPatterns)
			{
				bool result = new System.Text.RegularExpressions.Regex(ConvertToRegexPattern(pattern), System.Text.RegularExpressions.RegexOptions.IgnoreCase).IsMatch(dnnFileName);

				if (result)
				{
					returnValue = true;
					break;
				}
			}

			return returnValue;
		}

        private string ConvertToRegexPattern(string pattern)
		{
			string returnValue = System.Text.RegularExpressions.Regex.Escape(pattern);
			returnValue = returnValue.Replace("\\*", ".*");
			returnValue = returnValue.Replace("\\?", ".") + "$";
			return returnValue;
		}

		private string GetTelerikMessage(string key)
		{
			string returnValue = key;
			switch (key)
			{
				case "DirectoryAlreadyExists":
					returnValue = DNNValidator.GetString("ErrorCodes.DirectoryAlreadyExists");
					break;
				case "InvalidCharactersInPath":
					returnValue = DNNValidator.GetString("ErrorCodes.InvalidCharactersInPath");
					break;
				case "NewFileAlreadyExists":
					returnValue = DNNValidator.GetString("ErrorCodes.NewFileAlreadyExists");
					break;
					//Case ""
					//	Exit Select
			}

			return returnValue;
		}

#endregion

	}

}