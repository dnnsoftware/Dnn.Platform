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
using DotNetNuke.Common.Utilities;

using System;
using System.Collections.Specialized;
using System.Web;

using DotNetNuke.Entities.Portals;
using DotNetNuke.Instrumentation;
using DotNetNuke.Services.FileSystem;

namespace DotNetNuke.Providers.RadEditorProvider
{

	/// <summary>
	/// 
	/// </summary>
	/// <remarks>
	/// </remarks>
	/// <history>
	/// </history>
	public partial class RenderTemplate : System.Web.UI.Page
	{
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof (RenderTemplate));

#region Event Handlers

		protected void Page_Load(object sender, System.EventArgs e)
		{
			try
			{
				string renderUrl = Request.QueryString["rurl"];

				if (! (string.IsNullOrEmpty(renderUrl)))
				{
					string fileContents = string.Empty;
					FileController fileCtrl = new FileController();
					FileInfo fileInfo = null;
					int portalID = PortalController.GetCurrentPortalSettings().PortalId;

					if (renderUrl.ToLower().Contains("linkclick.aspx") && renderUrl.ToLower().Contains("fileticket"))
					{
						//File Ticket
						int fileID = GetFileIDFromURL(renderUrl);

						if (fileID > -1)
						{
							fileInfo = fileCtrl.GetFileById(fileID, portalID);
						}
					}
					else
					{
						//File URL
						string dbPath = (string)(string)FileSystemValidation.ToDBPath(renderUrl);
						string fileName = System.IO.Path.GetFileName(renderUrl);

						if (! (string.IsNullOrEmpty(fileName)))
						{
							FolderInfo dnnFolder = GetDNNFolder(dbPath);
							if (dnnFolder != null)
							{
								fileInfo = fileCtrl.GetFile(fileName, portalID, dnnFolder.FolderID);
							}
						}
					}

					if (fileInfo != null)
					{
						if (CanViewFile(fileInfo.Folder) && fileInfo.Extension.ToLower() == "htmtemplate")
						{
							byte[] fileBytes = FileSystemUtils.GetFileContent(fileInfo);
							fileContents = System.Text.Encoding.ASCII.GetString(fileBytes);
						}
					}

					if (! (string.IsNullOrEmpty(fileContents)))
					{
						Content.Text = Server.HtmlEncode(fileContents);
					}
				}
			}
			catch (Exception ex)
			{
				Services.Exceptions.Exceptions.LogException(ex);
				Content.Text = string.Empty;
			}
		}

#endregion

#region Methods

		private int GetFileIDFromURL(string url)
		{
			int returnValue = -1;
			//add http
			if (! (url.ToLower().StartsWith("http")))
			{
				if (url.ToLower().StartsWith("/"))
				{
					url = "http:/" + url;
				}
				else
				{
					url = "http://" + url;
				}
			}

			Uri u = new Uri(url);

			if (u != null && u.Query != null)
			{
				NameValueCollection @params = HttpUtility.ParseQueryString(u.Query);

				if (@params != null && @params.Count > 0)
				{
					string fileTicket = @params.Get("fileticket");

					if (! (string.IsNullOrEmpty(fileTicket)))
					{
						try
						{
							returnValue = FileLinkClickController.Instance.GetFileIdFromLinkClick(@params); 
						}
						catch (Exception ex)
						{
							returnValue = -1;
                            Logger.Error(ex);
						}
					}
				}
			}

			return returnValue;
		}

		protected bool CanViewFile(string dbPath)
		{
			return DotNetNuke.Security.Permissions.FolderPermissionController.CanViewFolder(GetDNNFolder(dbPath));
		}

		private DotNetNuke.Services.FileSystem.FolderInfo GetDNNFolder(string dbPath)
		{
			return new DotNetNuke.Services.FileSystem.FolderController().GetFolder(PortalController.GetCurrentPortalSettings().PortalId, dbPath, false);
		}

		private string DNNHomeDirectory
		{
			get
			{
				//todo: host directory
				string homeDir = PortalController.GetCurrentPortalSettings().HomeDirectory;
				homeDir = homeDir.Replace("\\", "/");

				if (homeDir.EndsWith("/"))
				{
					homeDir = homeDir.Remove(homeDir.Length - 1, 1);
				}

				return homeDir;
			}
		}

#endregion


	    override protected void OnInit(EventArgs e)
	    {
		    base.OnInit(e);

		    this.Load += Page_Load;
	    }
	}

}