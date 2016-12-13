#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2017
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
using DotNetNuke.Common;
using DotNetNuke.Common.Utilities;

using System;
using System.Collections;
using System.IO;
using System.Text;
using DotNetNuke.Entities.Host;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Security.Permissions;
using DotNetNuke.Services.FileSystem;

using Telerik.Web.UI;
using DotNetNuke.Services.Exceptions;
using System.Web;
using FileInfo = DotNetNuke.Services.FileSystem.FileInfo;

namespace DotNetNuke.Providers.RadEditorProvider
{

	/// <summary>
	/// 
	/// </summary>
	/// <remarks>
	/// </remarks>
	public partial class SaveTemplate : DotNetNuke.Framework.PageBase
	{


#region Event Handlers

		protected void Page_Init(object sender, System.EventArgs e)
		{

			DotNetNuke.Framework.AJAX.RegisterScriptManager();

			if (Request.IsAuthenticated == true)
			{
				Response.Cache.SetCacheability(System.Web.HttpCacheability.ServerAndNoCache);
			}

		}

		protected void Page_Load(object sender, System.EventArgs e)
		{
			try
			{

				SetResStrings();

				if (!IsPostBack)
				{
                    if (!IsValidUser())
                    {
	                    var url = Globals.AccessDeniedURL();
	                    url = string.Format("{0}{1}popUp=true", url, url.Contains("?") ? "&" : "?");
                        Response.Redirect(url, true);
                    }

				    FixAllowedExtensions();

					int portalID = DotNetNuke.Entities.Portals.PortalController.Instance.GetCurrentPortalSettings().PortalId;
					ArrayList folders = DotNetNuke.Common.Utilities.FileSystemUtils.GetFoldersByUser(portalID, true, true, "Add");

					//filter out only folders below the editor's template path
					string strStartFolder = "";
					try
					{
                        if (!String.IsNullOrEmpty(Request.QueryString["path"]))
						    strStartFolder = Request.QueryString["path"];
					}
					catch
					{
					}
					ArrayList tmpFolders = new ArrayList();
					foreach (DotNetNuke.Services.FileSystem.FolderInfo folder in folders)
					{
						if (folder.FolderPath.StartsWith(strStartFolder))
						{
							tmpFolders.Add(folder);
						}
					}

					if (tmpFolders.Count == 0)
					{
						msgError.InnerHtml = GetString("msgNoFolders.Text");
						divInputArea.Visible = false;
						cmdClose.Visible = true;
					}
					else
					{
						FolderList.Items.Clear();

						FolderList.DataTextField = "FolderPath";
						FolderList.DataValueField = "FolderPath";
						FolderList.DataSource = tmpFolders;
						FolderList.DataBind();

						RadComboBoxItem rootFolder = FolderList.FindItemByText(string.Empty);
						if (rootFolder != null)
						{
							rootFolder.Text = GetString("lblRootFolder.Text");
						}

					}
				}
			}
			catch (Exception ex)
			{
				DotNetNuke.Services.Exceptions.Exceptions.LogException(ex);
				throw ex;
			}
		}
        
        private bool IsValidUser()
        {
			int moduleId = Null.NullInteger;
			bool result = User.Identity.IsAuthenticated && int.TryParse(Request.QueryString["ModuleId"], out moduleId);
	        if (result)
	        {
                var module = ModuleController.Instance.GetModule(moduleId, Null.NullInteger, true);
		        result = module != null && ModulePermissionController.CanEditModuleContent(module);
	        }
	        return result;
        }

		protected void Save_OnClick(object sender, EventArgs e)
		{
			try
			{
				if (FolderList.Items.Count == 0)
				{
					return;
				}

				DotNetNuke.Entities.Portals.PortalSettings portalSettings = DotNetNuke.Entities.Portals.PortalSettings.Current;

				string fileContents = htmlText2.Text.Trim();
				string newFileName = FileName.Text;
				if (! (newFileName.EndsWith(".html")))
				{
					newFileName = newFileName + ".html";
				}

				string rootFolder = portalSettings.HomeDirectoryMapPath;
				string dbFolderPath = FolderList.SelectedValue;
				string virtualFolder = (string)(string)FileSystemValidation.ToVirtualPath(dbFolderPath);
				rootFolder = rootFolder + FolderList.SelectedValue;
				rootFolder = rootFolder.Replace("/", "\\");

				string errorMessage = string.Empty;
				FolderController folderCtrl = new FolderController();
				FolderInfo folder = folderCtrl.GetFolder(portalSettings.PortalId, dbFolderPath, false);

				if ((folder == null))
				{
					ShowSaveTemplateMessage(GetString("msgFolderDoesNotExist.Text"));
					return;
				}

				// Check file name is valid
				FileSystemValidation dnnValidator = new FileSystemValidation();
				errorMessage = dnnValidator.OnCreateFile(virtualFolder + newFileName, fileContents.Length);
				if (! (string.IsNullOrEmpty(errorMessage)))
				{
					ShowSaveTemplateMessage(errorMessage);
					return;
				}

				FileController fileCtrl = new FileController();
				DotNetNuke.Services.FileSystem.FileInfo existingFile = fileCtrl.GetFile(newFileName, portalSettings.PortalId, folder.FolderID);

				// error if file exists
				if (!Overwrite.Checked && existingFile != null)
				{
					ShowSaveTemplateMessage(GetString("msgFileExists.Text"));
					return;
				}

				FileInfo newFile = existingFile;
				if (newFile == null)
				{
					newFile = new FileInfo{ FileId = Null.NullInteger };
				}

				newFile.FileName = newFileName;
				newFile.ContentType = "text/plain";
				newFile.Extension = "html";
				newFile.Size = fileContents.Length;
				newFile.FolderId = folder.FolderID;
				newFile.Folder = FileSystemUtils.FormatFolderPath(folder.FolderPath);

                using (var memStream = new MemoryStream())
                {
                    byte[] fileDataBytes = Encoding.UTF8.GetBytes(fileContents);
                    memStream.Write(fileDataBytes, 0, fileDataBytes.Length);
                    memStream.Flush();
                    memStream.Position = 0;

                    if (newFile.FileId != Null.NullInteger)
                    {
                        FileManager.Instance.UpdateFile(newFile, memStream);
                    }
                    else
                    {
                        FileManager.Instance.AddFile(folder, newFileName, memStream, true);
                    }
                }

				ShowSaveTemplateMessage(string.Empty);
			}
			catch (Exception ex)
			{
				DotNetNuke.Services.Exceptions.Exceptions.LogException(ex);
				throw ex;
			}
		}

#endregion

#region Properties

#endregion

#region Methods

		private void FixAllowedExtensions()
		{

			bool blnHTML = true;

			string validExtensions = DotNetNuke.Entities.Host.Host.FileExtensions.ToLowerInvariant();

			if (("," + validExtensions + ",").IndexOf(",html,") == -1)
			{
				blnHTML = false;
			}

			if (blnHTML == false)
			{
				validExtensions = (string)AddComma(validExtensions).ToString() + "html";
				HostSettingsController ctl = new HostSettingsController();
				ctl.UpdateHostSetting("FileExtensions", validExtensions);
				Config.Touch();
			}

		}

		private object AddComma(string strExpression)
		{
			if (strExpression.EndsWith(","))
			{
				return strExpression;
			}
			else
			{
				return strExpression + ",";
			}
		}

		private void ShowSaveTemplateMessage(string errorMessage)
		{
			if (string.IsNullOrEmpty(errorMessage))
			{
				msgSuccess.Visible = true;
				msgError.Visible = false;
			}
			else
			{
				msgSuccess.Visible = false;
				msgError.Visible = true;
				msgError.InnerHtml += errorMessage;
				DotNetNuke.Services.Exceptions.Exceptions.LogException(new FileManagerException("Error creating htmtemplate file [" + errorMessage + "]"));
			}

			divInputArea.Visible = false;
			cmdClose.Visible = true;
		}

		private void SetResStrings()
		{
			this.lblTitle.Text = GetString("lblDialogTitle");
			this.lblFolders.Text = GetString("lblFolders.Text");
			this.lblFileName.Text = GetString("lblFileName.Text");
			this.lblOverwrite.Text = GetString("lblOverwrite.Text");
			this.cmdSave.Text = GetString("cmdSave.Text");
			this.cmdCancel.Text = GetString("cmdCancel.Text");
			this.cmdClose.Text = GetString("cmdClose.Text");
			this.msgSuccess.InnerHtml = GetString("msgSuccess.Text");
			this.msgError.InnerHtml = GetString("msgError.Text");
		}

		public string GetString(string key)
		{
			string resourceFile = System.IO.Path.Combine(this.TemplateSourceDirectory + "/", DotNetNuke.Services.Localization.Localization.LocalResourceDirectory + "/SaveTemplate.resx");
			return DotNetNuke.Services.Localization.Localization.GetString(key, resourceFile);
		}


#endregion



	    public SaveTemplate()
	    {

    	    this.Init += new System.EventHandler(Page_Init);
		    this.Load += new System.EventHandler(Page_Load);
	    }
	}

}
