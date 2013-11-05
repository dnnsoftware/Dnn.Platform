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

using System.Globalization;
using System.Web.UI;

using DotNetNuke.Common.Utilities;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using DotNetNuke.Instrumentation;

using Telerik.Web.UI.Widgets;
using DotNetNuke.Services.FileSystem;
using System.IO;

// ReSharper disable CheckNamespace
namespace DotNetNuke.Providers.RadEditorProvider
// ReSharper restore CheckNamespace
{

	public class TelerikFileBrowserProvider : FileSystemContentProvider
	{
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(TelerikFileBrowserProvider));

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
                    //make sure that the folder is flagged secure if necessary
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
				var virtualPath = FileSystemValidation.ToVirtualPath(path);
				var virtualNewPath = FileSystemValidation.ToVirtualPath(newPath);
				var virtualDestinationPath = FileSystemValidation.GetDestinationFolder(virtualNewPath);

				string returnValue;
			    var isRename = FileSystemValidation.GetDestinationFolder(virtualPath) == virtualDestinationPath;
				if (isRename)
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

			    if (isRename)
                {
                    var dnnFolderToRename = FolderManager.Instance.GetFolder(PortalSettings.PortalId, FileSystemValidation.ToDBPath(virtualPath));
                    var newFolderName = virtualNewPath.TrimEnd('/').Split('/').LastOrDefault();
                    FolderManager.Instance.RenameFolder(dnnFolderToRename, newFolderName);
			    }
			    else // move
                {
                    var dnnFolderToMove = FolderManager.Instance.GetFolder(PortalSettings.PortalId, FileSystemValidation.ToDBPath(virtualPath));
                    var dnnDestinationFolder = FolderManager.Instance.GetFolder(PortalSettings.PortalId, FileSystemValidation.ToDBPath(virtualDestinationPath));
                    FolderManager.Instance.MoveFolder(dnnFolderToMove, dnnDestinationFolder);
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
				string virtualPath = FileSystemValidation.ToVirtualPath(path);
				string virtualNewPath = FileSystemValidation.ToVirtualPath(newPath);
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
				string virtualPathAndFile = FileSystemValidation.ToVirtualPath(path);

				string returnValue = DNNValidator.OnDeleteFile(virtualPathAndFile);
				if (! (string.IsNullOrEmpty(returnValue)))
				{
					return returnValue;
				}

				returnValue = TelerikContent.DeleteFile(virtualPathAndFile);

				if (string.IsNullOrEmpty(returnValue))
				{
					string virtualPath = FileSystemValidation.RemoveFileName(virtualPathAndFile);
					FolderInfo dnnFolder = DNNValidator.GetUserFolder(virtualPath);
					DNNFileCtrl.DeleteFile(PortalSettings.PortalId, Path.GetFileName(virtualPathAndFile), dnnFolder.FolderID, true);
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

				string returnValue;
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
				string virtualPathAndFile = FileSystemValidation.ToVirtualPath(path);
				string virtualNewPathAndFile = FileSystemValidation.ToVirtualPath(newPath);

				string returnValue = DNNValidator.OnCopyFile(virtualPathAndFile, virtualNewPathAndFile);
				if (! (string.IsNullOrEmpty(returnValue)))
				{
					return returnValue;
				}

				//Returns errors or empty string when successful (ie: NewFileAlreadyExists)
				returnValue = TelerikContent.CopyFile(virtualPathAndFile, virtualNewPathAndFile);

				if (string.IsNullOrEmpty(returnValue))
				{
					string virtualNewPath = FileSystemValidation.RemoveFileName(virtualNewPathAndFile);
					FolderInfo dnnFolder = DNNValidator.GetUserFolder(virtualNewPath);
					var dnnFileInfo = new Services.FileSystem.FileInfo();
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

		public override string StoreFile(HttpPostedFile file, string path, string name, params string[] arguments)
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
                    ShowMessage(string.Format("The file {0} cannot be uploaded because it would create an invalid URL. Please, rename the file before upload.", name));
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

			    var fileInfo = new Services.FileSystem.FileInfo();
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

            if (pageObject != null)
            {
                ScriptManager.RegisterClientScriptBlock(pageObject, pageObject.GetType(), "showAlertFromServer", @"
                    function showradAlertFromServer(message)
                    {
                        function f()
                        {// MS AJAX Framework is loaded
                            Sys.Application.remove_load(f);
                            // RadFileExplorer already contains a RadWindowManager inside, so radalert can be called without problem
                            radalert(message);
                        }

                        Sys.Application.add_load(f);
                    }", true);

                var script = string.Format("showradAlertFromServer('{0}');", message);
                ScriptManager.RegisterStartupScript(pageObject, pageObject.GetType(), "KEY", script, true);
            }
        }

		public override string StoreBitmap(System.Drawing.Bitmap bitmap, string url, System.Drawing.Imaging.ImageFormat format)
		{
			try
			{
				//base calls CheckWritePermissions method			
				string virtualPathAndFile = FileSystemValidation.ToVirtualPath(url);
				string virtualPath = FileSystemValidation.RemoveFileName(virtualPathAndFile);
				string returnValue = DNNValidator.OnCreateFile(virtualPathAndFile, 0);
				if (! (string.IsNullOrEmpty(returnValue)))
				{
					return returnValue;
				}

				returnValue = TelerikContent.StoreBitmap(bitmap, virtualPathAndFile, format);

				var dnnFileInfo = new Services.FileSystem.FileInfo();
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
                Logger.DebugFormat("ResolveDirectory: {0}", path);
                return GetDirectoryItemWithDNNPermissions(path);
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
                Logger.DebugFormat("ResolveRootDirectoryAsTree: {0}", path);
                return GetDirectoryItemWithDNNPermissions(path);
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
                Logger.DebugFormat("ResolveRootDirectoryAsList: {0}", path);
                return GetDirectoryItemWithDNNPermissions(path).Directories;
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

		private FileSystemContentProvider _TelerikContent;
		private FileSystemContentProvider TelerikContent
		{
			get {
			    return _TelerikContent ??
			        (_TelerikContent =
			            new FileSystemContentProvider(Context, SearchPatterns,
			                new[] { FileSystemValidation.HomeDirectory }, new[] { FileSystemValidation.HomeDirectory },
			                new[] { FileSystemValidation.HomeDirectory }, FileSystemValidation.ToVirtualPath(SelectedUrl),
			                FileSystemValidation.ToVirtualPath(SelectedItemTag)));
			}
		}

		private FolderController _DNNFolderCtrl;
		private FolderController DNNFolderCtrl
		{
			get { return _DNNFolderCtrl ?? (_DNNFolderCtrl = new FolderController()); }
		}

		private FileController _DNNFileCtrl;
		private FileController DNNFileCtrl
		{
			get { return _DNNFileCtrl ?? (_DNNFileCtrl = new FileController()); }
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

		private DirectoryItem GetDirectoryItemWithDNNPermissions(string path)
		{
			var radDirectory = TelerikContent.ResolveDirectory(FileSystemValidation.ToVirtualPath(path));
            if (radDirectory.FullPath == PortalSettings.HomeDirectory)
            {
                radDirectory.Name = DNNValidator.GetString("Root");
            }
            Logger.DebugFormat("GetDirectoryItemWithDNNPermissions - path: {0}, radDirectory: {1}", path, radDirectory);
            //var directoryArray = new[] {radDirectory};
            return AddChildDirectoriesToList(radDirectory);
		}

		private DirectoryItem AddChildDirectoriesToList( DirectoryItem radDirectory)
		{
            var parentFolderPath = radDirectory.FullPath.EndsWith("/") ? radDirectory.FullPath : radDirectory.FullPath + "/";
            if (parentFolderPath.StartsWith(PortalSettings.HomeDirectory))
		    {
                parentFolderPath = parentFolderPath.Remove(0, PortalSettings.HomeDirectory.Length);
		    }

		    var dnnParentFolder = FolderManager.Instance.GetFolder(PortalSettings.PortalId, parentFolderPath);
            var dnnChildFolders = FolderManager.Instance.GetFolders(dnnParentFolder).Where(folder => (FileSystemValidation.HasPermission(folder, "BROWSE,READ")));
            var radDirectories = new List<DirectoryItem>();
            foreach (var dnnChildFolder in dnnChildFolders)
            {
                if (!dnnChildFolder.FolderPath.ToLowerInvariant().StartsWith("cache/") 
                    && !dnnChildFolder.FolderPath.ToLowerInvariant().StartsWith("users/")
                    && !dnnChildFolder.FolderPath.ToLowerInvariant().StartsWith("groups/"))
                {
                        var radSubDirectory =
                            TelerikContent.ResolveDirectory(FileSystemValidation.ToVirtualPath(dnnChildFolder.FolderPath));
                        radSubDirectory.Permissions = FileSystemValidation.TelerikPermissions(dnnChildFolder);
                        radDirectories.Add(radSubDirectory);
                }
            }

            if (parentFolderPath == "")
            {
                var userFolder = FolderManager.Instance.GetUserFolder(PortalSettings.UserInfo);
                if (userFolder.PortalID == PortalSettings.PortalId)
                {
                    var radUserFolder = TelerikContent.ResolveDirectory(FileSystemValidation.ToVirtualPath(userFolder.FolderPath));
                    radUserFolder.Name = DNNValidator.GetString("MyFolder");
                    radUserFolder.Permissions = FileSystemValidation.TelerikPermissions(userFolder);
                    radDirectories.Add(radUserFolder);
                }
            }


		    radDirectory.Directories = radDirectories.ToArray();

            return radDirectory;


		}

        private IDictionary<string, Services.FileSystem.FileInfo> GetDNNFiles(int dnnFolderID)
		{
			System.Data.IDataReader drFiles = null;
			IDictionary<string, Services.FileSystem.FileInfo> dnnFiles;

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
			var files = GetDNNFiles(folder.FolderID);
			var visibleFileCount = 0;
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

		private void FillFileInfo(string virtualPathAndFile, ref Services.FileSystem.FileInfo fileInfo)
		{
			fileInfo.FileName = Path.GetFileName(virtualPathAndFile);
			fileInfo.Extension = Path.GetExtension(virtualPathAndFile);
			if (fileInfo.Extension != null && fileInfo.Extension.StartsWith("."))
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

		private void FillFileInfo(Telerik.Web.UI.UploadedFile file, ref Services.FileSystem.FileInfo fileInfo)
		{
			//The core API expects the path to be stripped off the filename
			fileInfo.FileName = ((file.FileName.Contains("\\")) ? Path.GetFileName(file.FileName) : file.FileName);
			fileInfo.Extension = file.GetExtension();
			if (fileInfo.Extension.StartsWith("."))
			{
				fileInfo.Extension = fileInfo.Extension.Remove(0, 1);
			}

			fileInfo.ContentType = FileSystemUtils.GetContentType(fileInfo.Extension);

			FillImageInfo(file.InputStream, ref fileInfo);
		}

		private void FillImageInfo(Stream fileStream, ref Services.FileSystem.FileInfo fileInfo)
		{
		    var imageExtensions = new FileExtensionWhitelist(Common.Globals.glbImageFileTypes);
			if (imageExtensions.IsAllowedExtension(fileInfo.Extension))
			{
				System.Drawing.Image img = null;
				try
				{
					img = System.Drawing.Image.FromStream(fileStream);
					fileInfo.Size = fileStream.Length > int.MaxValue ? int.MaxValue : int.Parse(fileStream.Length.ToString(CultureInfo.InvariantCulture));
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