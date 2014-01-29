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
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;

using DotNetNuke.Common;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Instrumentation;
using DotNetNuke.Services.Exceptions;
using DotNetNuke.Services.Localization;
using DotNetNuke.UI.Skins;
using DotNetNuke.UI.Skins.Controls;

using Image = System.Drawing.Image;

#endregion

namespace DotNetNuke.Modules.Admin.Skins
{
	/// -----------------------------------------------------------------------------
	/// <summary>
	/// The EditSkins PortalModuleBase is used to manage the Available Skins
	/// </summary>
	/// <remarks>
	/// </remarks>
	/// <history>
	/// 	[cnurse]	9/13/2004	Updated to reflect design changes for Help, 508 support
	///                       and localisation
	/// </history>
	/// -----------------------------------------------------------------------------
	public partial class EditSkins : PortalModuleBase
	{
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof (EditSkins));

		#region Private Members

		private readonly string _notSpecified = "<" + Localization.GetString("Not_Specified") + ">";

		#endregion

		#region Private Methods

		protected string CurrentContainer
		{
			get
			{
				var currentContainer = Null.NullString;
				if (ViewState["CurrentContainer"] != null)
				{
					currentContainer = Convert.ToString(ViewState["CurrentContainer"]);
				}
				return currentContainer;
			}
			set
			{
				ViewState["CurrentContainer"] = value;
			}
		}

		protected string CurrentSkin
		{
			get
			{
				var currentSkin = Null.NullString;
				if (ViewState["CurrentSkin"] != null)
				{
					currentSkin = Convert.ToString(ViewState["CurrentSkin"]);
				}
				return currentSkin;
			}
			set
			{
				ViewState["CurrentSkin"] = value;
			}
		}

		private static void AddSkinstoCombo(DotNetNuke.Web.UI.WebControls.DnnComboBox combo, string strRoot)
		{
			if (Directory.Exists(strRoot))
			{
				foreach (var strFolder in Directory.GetDirectories(strRoot))
				{
					var strName = strFolder.Substring(strFolder.LastIndexOf("\\") + 1);
					if (strName != "_default")
					{
						combo.AddItem(strName, strFolder.Replace(Globals.ApplicationMapPath, "").ToLower());
					}
				}
			}
		}

		private static string CreateThumbnail(string strImage)
		{
			var blnCreate = true;

			var strThumbnail = strImage.Replace(Path.GetFileName(strImage), "thumbnail_" + Path.GetFileName(strImage));

			//check if image has changed
			if (File.Exists(strThumbnail))
			{
				if (File.GetLastWriteTime(strThumbnail) == File.GetLastWriteTime(strImage))
				{
					blnCreate = false;
				}
			}
			if (blnCreate)
			{
				const int intSize = 150; //size of the thumbnail 
				Image objImage;
				try
				{
					objImage = Image.FromFile(strImage);
					
					//scale the image to prevent distortion
					int intHeight;
					int intWidth;
					double dblScale;
					if (objImage.Height > objImage.Width)
					{
						//The height was larger, so scale the width 
						dblScale = (double) intSize/objImage.Height;
						intHeight = intSize;
						intWidth = Convert.ToInt32(objImage.Width*dblScale);
					}
					else
					{
						//The width was larger, so scale the height 
						dblScale = (double) intSize/objImage.Width;
						intWidth = intSize;
						intHeight = Convert.ToInt32(objImage.Height*dblScale);
					}
					
					//create the thumbnail image
					var objThumbnail = objImage.GetThumbnailImage(intWidth, intHeight, null, IntPtr.Zero);

					//delete the old file ( if it exists )
					if (File.Exists(strThumbnail))
					{
						File.Delete(strThumbnail);
					}
					
					//save the thumbnail image 
					objThumbnail.Save(strThumbnail, objImage.RawFormat);

					//set the file attributes
					File.SetAttributes(strThumbnail, FileAttributes.Normal);
					File.SetLastWriteTime(strThumbnail, File.GetLastWriteTime(strImage));

					//tidy up
					objImage.Dispose();
					objThumbnail.Dispose();
				}
				catch (Exception ex) //problem creating thumbnail
				{
					Logger.Error(ex);
				}
			}

			strThumbnail = Globals.ApplicationPath + "/" + strThumbnail.Substring(strThumbnail.IndexOf("portals\\"));
			strThumbnail = strThumbnail.Replace("\\", "/");

			//return thumbnail filename
			return strThumbnail;
		}

		private string GetSkinPath(string type, string root, string name)
		{
			var strPath = Null.NullString;
			switch (type)
			{
				case "G":  //global
					strPath = Globals.HostPath + root + "/" + name;
					break;
				case "L": //local
					strPath = PortalSettings.HomeDirectory + root + "/" + name;
					break;
			}
			return strPath;
		}

		private static bool IsFallbackContainer(string skinPath)
		{
			var strDefaultContainerPath = (Globals.HostMapPath + SkinController.RootContainer + SkinDefaults.GetSkinDefaults(SkinDefaultType.SkinInfo).Folder).Replace("/", "\\");
			if (strDefaultContainerPath.EndsWith("\\"))
			{
				strDefaultContainerPath = strDefaultContainerPath.Substring(0, strDefaultContainerPath.Length - 1);
			}
			return skinPath.IndexOf(strDefaultContainerPath, StringComparison.CurrentCultureIgnoreCase) != -1;
		}

		private static bool IsFallbackSkin(string skinPath)
		{
			var strDefaultSkinPath = (Globals.HostMapPath + SkinController.RootSkin + SkinDefaults.GetSkinDefaults(SkinDefaultType.SkinInfo).Folder).Replace("/", "\\");
			if (strDefaultSkinPath.EndsWith("\\"))
			{
				strDefaultSkinPath = strDefaultSkinPath.Substring(0, strDefaultSkinPath.Length - 1);
			}
			return skinPath.ToLowerInvariant() == strDefaultSkinPath.ToLowerInvariant();
		}

		private void LoadCombos()
		{
			cboSkins.Items.Clear();
			CurrentSkin = _notSpecified;
            cboSkins.AddItem(CurrentSkin, CurrentSkin);

			//load host skins
			if (chkHost.Checked)
			{
				AddSkinstoCombo(cboSkins, Request.MapPath(Globals.HostPath + SkinController.RootSkin));
			}
			
			//load portal skins
			if (chkSite.Checked)
			{
				AddSkinstoCombo(cboSkins, PortalSettings.HomeDirectoryMapPath + SkinController.RootSkin);
			}

			cboContainers.Items.Clear();
			CurrentContainer = _notSpecified;
            cboContainers.AddItem(CurrentContainer,CurrentContainer);

			//load host containers
			if (chkHost.Checked)
			{
				AddSkinstoCombo(cboContainers, Request.MapPath(Globals.HostPath + SkinController.RootContainer));
			}
			
			//load portal containers
			if (chkSite.Checked)
			{
				AddSkinstoCombo(cboContainers, PortalSettings.HomeDirectoryMapPath + SkinController.RootContainer);
			}
		}

		private string ParseSkinPackage(string strType, string strRoot, string strName, string strFolder, string strParse)
		{
			var strRootPath = Null.NullString;
			switch (strType)
			{
				case "G": //global
					strRootPath = Request.MapPath(Globals.HostPath);
					break;
				case "L": //local
					strRootPath = Request.MapPath(PortalSettings.HomeDirectory);
					break;
			}
			var objSkinFiles = new SkinFileProcessor(strRootPath, strRoot, strName);
			var arrSkinFiles = new ArrayList();

			if (Directory.Exists(strFolder))
			{
				var arrFiles = Directory.GetFiles(strFolder);
				foreach (var strFile in arrFiles)
				{
					switch (Path.GetExtension(strFile))
					{
						case ".htm":
						case ".html":
						case ".css":
							if (strFile.ToLower().IndexOf(Globals.glbAboutPage.ToLower()) < 0)
							{
								arrSkinFiles.Add(strFile);
							}
							break;
						case ".ascx":
							if (File.Exists(strFile.Replace(".ascx", ".htm")) == false && File.Exists(strFile.Replace(".ascx", ".html")) == false)
							{
								arrSkinFiles.Add(strFile);
							}
							break;
					}
				}
			}
			switch (strParse)
			{
				case "L": //localized
					return objSkinFiles.ProcessList(arrSkinFiles, SkinParser.Localized);
				case "P": //portable
					return objSkinFiles.ProcessList(arrSkinFiles, SkinParser.Portable);
			}
			return Null.NullString;
		}

		private void ProcessSkins(string strFolderPath, string type)
		{
		    const int kColSpan = 5;

			HtmlTable tbl;
			HtmlTableRow row = null;
			HtmlTableCell cell;
		    Panel pnlMsg;

			string[] arrFiles;
			string strURL;
			var intIndex = 0;

			if (Directory.Exists(strFolderPath))
			{
				bool fallbackSkin;
				string strRootSkin;
				if (type == "Skin")
				{
					tbl = tblSkins;
					strRootSkin = SkinController.RootSkin.ToLower();
					fallbackSkin = IsFallbackSkin(strFolderPath);
				    pnlMsg = pnlMsgSkins;
				}
				else
				{
					tbl = tblContainers;
					strRootSkin = SkinController.RootContainer.ToLower();
					fallbackSkin = IsFallbackContainer(strFolderPath);
				    pnlMsg = pnlMsgContainers;
				}
				var strSkinType = strFolderPath.ToLower().IndexOf(Globals.HostMapPath.ToLower()) != -1 ? "G" : "L";

				var canDeleteSkin = SkinController.CanDeleteSkin(strFolderPath, PortalSettings.HomeDirectoryMapPath);
                arrFiles = Directory.GetFiles(strFolderPath, "*.ascx");
                int colSpan = arrFiles.Length ==0 ? 1: arrFiles.Length;
			    tbl.Width = "auto";
				if (fallbackSkin || !canDeleteSkin)
                {                
					var pnl = new Panel {CssClass = "dnnFormMessage dnnFormWarning"};
				    var lbl = new Label {Text = Localization.GetString(type == "Skin" ? "CannotDeleteSkin.ErrorMessage" : "CannotDeleteContainer.ErrorMessage", LocalResourceFile)};
				    pnl.Controls.Add(lbl);
                    pnlMsg.Controls.Add(pnl);
                 
					cmdDelete.Visible = false;
				}
				if (arrFiles.Length == 0)
				{
                 	var pnl = new Panel {CssClass = "dnnFormMessage dnnFormWarning"};
				    var lbl = new Label {Text = Localization.GetString(type == "Skin" ? "NoSkin.ErrorMessage" : "NoContainer.ErrorMessage", LocalResourceFile)};
				    pnl.Controls.Add(lbl);
                    pnlMsg.Controls.Add(pnl);                 
				}
                
				var strFolder = strFolderPath.Substring(strFolderPath.LastIndexOf("\\") + 1);
				foreach (var strFile in arrFiles)
				{
					var file = strFile.ToLower();
                    intIndex += 1;
                    if (intIndex == kColSpan+ 1)
                    {
                        intIndex = 1;
                    }
                    if (intIndex == 1)
                    {
                        //Create new row
                        row = new HtmlTableRow();
                        tbl.Rows.Add(row);
                    }
					cell = new HtmlTableCell {Align = "center", VAlign = "bottom"};
					cell.Attributes["class"] = "NormalBold";

					
					//thumbnail
					if (File.Exists(file.Replace(".ascx", ".jpg")))
					{
						var imgLink = new HyperLink();
						strURL = file.Substring(strFile.LastIndexOf("\\portals\\"));
						imgLink.NavigateUrl = ResolveUrl("~" + strURL.Replace(".ascx", ".jpg"));
						imgLink.Target = "_new";


						var img = new System.Web.UI.WebControls.Image {ImageUrl = CreateThumbnail(file.Replace(".ascx", ".jpg")), BorderWidth = new Unit(1)};

						imgLink.Controls.Add(img);
						cell.Controls.Add(imgLink);
					}
					else
					{
						var img = new System.Web.UI.WebControls.Image {ImageUrl = ResolveUrl("~/images/thumbnail_black.png"), BorderWidth = new Unit(1)};
						cell.Controls.Add(img);
					}
					cell.Controls.Add(new LiteralControl("<br />"));

					strURL = file.Substring(strFile.IndexOf("\\" + strRootSkin + "\\"));
					strURL.Replace(".ascx", "");

                    //name
                    var label = new Label { Text = getReducedFileName(Path.GetFileNameWithoutExtension(file)), ToolTip = Path.GetFileNameWithoutExtension(file) ,CssClass = "skinTitle"};
                    cell.Controls.Add(label);
                    cell.Controls.Add(new LiteralControl("<br />"));

                    //Actions
					var previewLink = new HyperLink();
					
					if (type == "Skin")
					{
						previewLink.NavigateUrl = Globals.NavigateURL(PortalSettings.HomeTabId,
																	  Null.NullString,
																	  "SkinSrc=" + "[" + strSkinType + "]" + Globals.QueryStringEncode(strURL.Replace(".ascx", "").Replace("\\", "/")));
					}
					else
					{
						previewLink.NavigateUrl = Globals.NavigateURL(PortalSettings.HomeTabId,
																	  Null.NullString,
																	  "ContainerSrc=" + "[" + strSkinType + "]" + Globals.QueryStringEncode(strURL.Replace(".ascx", "").Replace("\\", "/")));
					}

				    previewLink.CssClass = "dnnSecondaryAction";
					previewLink.Target = "_new";
					previewLink.Text = Localization.GetString("cmdPreview", LocalResourceFile);
					cell.Controls.Add(previewLink);

					cell.Controls.Add(new LiteralControl("&nbsp;"));

					var applyButton = new LinkButton
										  {
											  Text = Localization.GetString("cmdApply", LocalResourceFile),
											  CommandName = "Apply" + type,
											  CommandArgument = "[" + strSkinType + "]" + strRootSkin + "/" + strFolder + "/" + Path.GetFileName(strFile),
											  CssClass = "dnnSecondaryAction applyAction"
										  };
					applyButton.Command += OnCommand;
					cell.Controls.Add(applyButton);

					if ((UserInfo.IsSuperUser || strSkinType == "L") && (!fallbackSkin && canDeleteSkin))
					{
						cell.Controls.Add(new LiteralControl("&nbsp;"));

						var deleteButton = new LinkButton
											   {
												   Text = Localization.GetString("cmdDelete"),
												   CommandName = "Delete",
												   CommandArgument = "[" + strSkinType + "]" + strRootSkin + "/" + strFolder + "/" + Path.GetFileName(strFile),
												   CssClass = "dnnSecondaryAction"
											   };
						deleteButton.Command += OnCommand;
						cell.Controls.Add(deleteButton);
					}
					row.Cells.Add(cell);
				}
				if (File.Exists(strFolderPath + "/" + Globals.glbAboutPage))
				{
					row = new HtmlTableRow();
					cell = new HtmlTableCell {ColSpan = colSpan, Align = "center"};
					var strFile = strFolderPath + "/" + Globals.glbAboutPage;
					strURL = strFile.Substring(strFile.IndexOf("\\portals\\"));

					var copyrightLink = new HyperLink
											{
												NavigateUrl = ResolveUrl("~" + strURL),
												CssClass = "dnnSecondaryAction",
												Target = "_new",
												Text = string.Format(Localization.GetString("About", LocalResourceFile), strFolder)
											};
					cell.Controls.Add(copyrightLink);

					row.Cells.Add(cell);
					tbl.Rows.Add(row);
				}
			}
		}

        private string getReducedFileName(string fileName   )
        {
            const int kMaxLength = 13;
            string result = fileName;
            if(fileName.Length > kMaxLength)
            {
                result = fileName.Substring(0, kMaxLength - 2) + "...";
            }
            return result;
        }

		private void SetContainer(string strContainer)
		{
			if (cboContainers.FindItemByValue(CurrentContainer) != null)
			{
				cboContainers.FindItemByValue(CurrentContainer).Selected = false;
			}
			if (cboContainers.FindItemByValue(strContainer) != null)
			{
				cboContainers.FindItemByValue(strContainer).Selected = true;
				CurrentContainer = strContainer;
			}
			else
			{
				CurrentContainer = _notSpecified;
			}
		}

		private void SetSkin(string strSkin)
		{
			if (cboSkins.FindItemByValue(CurrentSkin) != null)
			{
				cboSkins.FindItemByValue(CurrentSkin).Selected = false;
			}
			if (cboSkins.FindItemByValue(strSkin) != null)
			{
				cboSkins.FindItemByValue(strSkin).Selected = true;
				CurrentSkin = strSkin;
			}
			else
			{
				CurrentSkin = _notSpecified;
			}
		}

		private void ShowContainers()
		{
			tblContainers.Rows.Clear();
			var intPortalId = PortalId;

			var strContainerPath = Globals.ApplicationMapPath.ToLower() + cboContainers.SelectedItem.Value;
			if (strContainerPath.ToLowerInvariant().Contains(Globals.HostMapPath.ToLowerInvariant()))
			{
				intPortalId = Null.NullInteger;
			}
			var skinPackage = SkinController.GetSkinPackage(intPortalId, cboContainers.SelectedItem.Text, "Container");
			if (skinPackage == null && !lblLegacy.Visible)
			{
				lblLegacy.Visible = (cboContainers.SelectedIndex > 0);
			}
			if (cboContainers.SelectedIndex > 0)
			{
				ProcessSkins(strContainerPath, "Container");
				pnlSkin.Visible = true;
				if (UserInfo.IsSuperUser || strContainerPath.IndexOf(Globals.HostMapPath.ToLower()) == -1)
				{
					cmdParse.Visible = true;
					pnlParse.Visible = true;
				}
				else
				{
					cmdParse.Visible = false;
					pnlParse.Visible = false;
				}
			}
			else
			{
				pnlSkin.Visible = false;
				pnlParse.Visible = false;
			}
		}

		private void ShowSkins()
		{
			tblSkins.Rows.Clear();
			var intPortalId = PortalId;

			var strSkinPath = Globals.ApplicationMapPath.ToLower() + cboSkins.SelectedItem.Value;
			if (strSkinPath.ToLowerInvariant().Contains(Globals.HostMapPath.ToLowerInvariant()))
			{
				intPortalId = Null.NullInteger;
			}
			var skinPackage = SkinController.GetSkinPackage(intPortalId, cboSkins.SelectedItem.Text, "Skin");
			if (skinPackage == null)
			{
				lblLegacy.Visible = (cboSkins.SelectedIndex > 0);
			}
			if (cboSkins.SelectedIndex > 0)
			{
				ProcessSkins(strSkinPath, "Skin");
				pnlSkin.Visible = true;
				if (UserInfo.IsSuperUser || strSkinPath.IndexOf(Globals.HostMapPath.ToLower()) == -1)
				{
					cmdParse.Visible = true;
					pnlParse.Visible = true;
				}
				else
				{
					cmdParse.Visible = false;
					pnlParse.Visible = false;
				}
			}
			else
			{
				pnlSkin.Visible = false;
				pnlParse.Visible = false;
			}
		}

#endregion

		#region Event Handlers

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

			chkHost.CheckedChanged += OnHostCheckChanged;
			chkSite.CheckedChanged += OnSiteCheckChanged;
			cmdDelete.Click += OnDeleteClick;
			cmdParse.Click += OnParseClick;
			cmdRestore.Click += OnRestoreClick;
		    lblLegacy.Visible = false;

			string strSkin;
			var strContainer = Null.NullString;
			try
			{
				cmdDelete.Visible = true;

				if (Page.IsPostBack == false)
				{
					LoadCombos();
				}
				typeRow.Visible = !PortalSettings.ActiveTab.IsSuperTab;

				if (!Page.IsPostBack)
				{
					string strURL;

					if (Request.QueryString["Name"] != null)
					{
						strURL = Request.MapPath(GetSkinPath(Convert.ToString(Request.QueryString["Type"]), Convert.ToString(Request.QueryString["Root"]), Convert.ToString(Request.QueryString["Name"])));
						strSkin = strURL.Replace(Globals.ApplicationMapPath, "").ToLowerInvariant();
					}
					else
					{
						//Get the current portal skin
						var skinSrc = !string.IsNullOrEmpty(PortalSettings.DefaultPortalSkin) ? PortalSettings.DefaultPortalSkin : SkinController.GetDefaultPortalSkin();
						strURL = Request.MapPath(SkinController.FormatSkinPath(SkinController.FormatSkinSrc(skinSrc, PortalSettings)));
						strURL = strURL.Substring(0, strURL.LastIndexOf("\\"));
						strSkin = strURL.Replace(Globals.ApplicationMapPath, "").ToLowerInvariant();
					}
					if (!string.IsNullOrEmpty(strSkin))
					{
						strContainer = strSkin.Replace("\\" + SkinController.RootSkin.ToLowerInvariant() + "\\", "\\" + SkinController.RootContainer.ToLowerInvariant() + "\\");
					}
					SetSkin(strSkin);
					SetContainer(strContainer);
				}
				else
				{
					strSkin = cboSkins.SelectedValue;
					strContainer = cboContainers.SelectedValue;
					if (strSkin != CurrentSkin)
					{
						strContainer = strSkin.Replace("\\" + SkinController.RootSkin.ToLowerInvariant() + "\\", "\\" + SkinController.RootContainer.ToLowerInvariant() + "\\");
						SetSkin(strSkin);
						SetContainer(strContainer);
					}
					else if (strContainer != CurrentContainer)
					{
						SetSkin(_notSpecified);
						SetContainer(strContainer);
					}
				}
				ShowSkins();
				ShowContainers();
			}
			catch (Exception exc) //Module failed to load
			{
				Exceptions.ProcessModuleLoadException(this, exc);
			}
		}

		protected void OnHostCheckChanged(object sender, EventArgs e)
		{
			LoadCombos();

			ShowSkins();
			ShowContainers();
		}

		protected void OnSiteCheckChanged(object sender, EventArgs e)
		{
			LoadCombos();

			ShowSkins();
			ShowContainers();
		}

		protected void OnCommand(Object sender, CommandEventArgs e)
		{
			try
			{
				var strSrc = e.CommandArgument.ToString();
				var redirectUrl = Globals.NavigateURL(TabId);
				switch (e.CommandName)
				{
					case "ApplyContainer":
						if (chkPortal.Checked)
						{
							SkinController.SetSkin(SkinController.RootContainer, PortalId, SkinType.Portal, strSrc);
						}
						if (chkAdmin.Checked)
						{
							SkinController.SetSkin(SkinController.RootContainer, PortalId, SkinType.Admin, strSrc);
						}
						break;
					case "ApplySkin":
						if (chkPortal.Checked)
						{
							SkinController.SetSkin(SkinController.RootSkin, PortalId, SkinType.Portal, strSrc);
						}
						if (chkAdmin.Checked)
						{
							SkinController.SetSkin(SkinController.RootSkin, PortalId, SkinType.Admin, strSrc);
						}
						DataCache.ClearPortalCache(PortalId, true);
						break;
					case "Delete":
						File.Delete(Request.MapPath(SkinController.FormatSkinSrc(strSrc, PortalSettings)));
						DataCache.ClearPortalCache(PortalId, true);
						var strType = "G";
						if (strSrc.StartsWith("[L]"))
						{
							strType = "L";
						}
						var strRoot = strSrc.Substring(3, strSrc.IndexOf("/") - 3);

						var strFolder = strSrc.Substring(strSrc.IndexOf("/") + 1, strSrc.LastIndexOf("/") - strSrc.IndexOf("/") - 2);
						redirectUrl = Globals.NavigateURL(TabId, "", "Type=" + strType, "Root=" + strRoot, "Name=" + strFolder);
						break;
				}
				Response.Redirect(redirectUrl, true);
			}
			catch (Exception exc) //Module failed to load
			{
				Exceptions.ProcessModuleLoadException(this, exc);
			}
		}

		protected void OnDeleteClick(object sender, EventArgs e)
		{
			var failure = false;
			var strSkinPath = Globals.ApplicationMapPath.ToLower() + cboSkins.SelectedItem.Value;
			var strContainerPath = Globals.ApplicationMapPath.ToLower() + cboContainers.SelectedItem.Value;

			string strMessage;

			if (UserInfo.IsSuperUser == false && cboSkins.SelectedItem.Value.IndexOf("\\portals\\_default\\", 0) != -1)
			{
				strMessage = Localization.GetString("SkinDeleteFailure", LocalResourceFile);
				UI.Skins.Skin.AddModuleMessage(this, strMessage, ModuleMessage.ModuleMessageType.RedError);
				failure = true;
			}
			else
			{
				if (cboSkins.SelectedIndex > 0)
				{
					SkinPackageInfo skinPackage = SkinController.GetSkinPackage(PortalId, cboSkins.SelectedItem.Text, "Skin");
					if (skinPackage != null)
					{
						strMessage = Localization.GetString("UsePackageUnInstall", LocalResourceFile);
						UI.Skins.Skin.AddModuleMessage(this, strMessage, ModuleMessage.ModuleMessageType.RedError);
						return;
					}
					if (Directory.Exists(strSkinPath))
					{
						Globals.DeleteFolderRecursive(strSkinPath);
					}
					if (Directory.Exists(strSkinPath.Replace("\\" + SkinController.RootSkin.ToLower() + "\\", "\\" + SkinController.RootContainer + "\\")))
					{
						Globals.DeleteFolderRecursive(strSkinPath.Replace("\\" + SkinController.RootSkin.ToLower() + "\\", "\\" + SkinController.RootContainer + "\\"));
					}
				}
				else if (cboContainers.SelectedIndex > 0)
				{
					var skinPackage = SkinController.GetSkinPackage(PortalId, cboContainers.SelectedItem.Text, "Container");
					if (skinPackage != null)
					{
						strMessage = Localization.GetString("UsePackageUnInstall", LocalResourceFile);
						UI.Skins.Skin.AddModuleMessage(this, strMessage, ModuleMessage.ModuleMessageType.RedError);
						return;
					}
					if (Directory.Exists(strContainerPath))
					{
						Globals.DeleteFolderRecursive(strContainerPath);
					}
				}
			}
			if (!failure)
			{
				LoadCombos();
				ShowSkins();
				ShowContainers();
			}
		}

		protected void OnParseClick(object sender, EventArgs e)
		{
			string strFolder;
			string strType;
			string strRoot;
			string strName;
			var strSkinPath = Globals.ApplicationMapPath.ToLower() + cboSkins.SelectedItem.Value;
			var strContainerPath = Globals.ApplicationMapPath.ToLower() + cboContainers.SelectedItem.Value;
			var strParse = "";
			if (cboSkins.SelectedIndex > 0)
			{
				strFolder = strSkinPath;
				strType = strFolder.IndexOf(Globals.HostMapPath.ToLower()) != -1 ? "G" : "L";
				strRoot = SkinController.RootSkin;
				strName = cboSkins.SelectedItem.Text;
				strParse += ParseSkinPackage(strType, strRoot, strName, strFolder, optParse.SelectedItem.Value);

				strFolder = strSkinPath.Replace("\\" + SkinController.RootSkin.ToLower() + "\\", "\\" + SkinController.RootContainer.ToLower() + "\\");
				strRoot = SkinController.RootContainer;
				strParse += ParseSkinPackage(strType, strRoot, strName, strFolder, optParse.SelectedItem.Value);
				DataCache.ClearPortalCache(PortalId, true);
			}
			if (cboContainers.SelectedIndex > 0)
			{
				strFolder = strContainerPath;
				strType = strFolder.IndexOf(Globals.HostMapPath.ToLower()) != -1 ? "G" : "L";
				strRoot = SkinController.RootContainer;
				strName = cboContainers.SelectedItem.Text;
				strParse += ParseSkinPackage(strType, strRoot, strName, strFolder, optParse.SelectedItem.Value);
				DataCache.ClearPortalCache(PortalId, true);
			}
			lblOutput.Text = strParse;

			if (cboSkins.SelectedIndex > 0)
			{
				ShowSkins();
			}
			else if (cboContainers.SelectedIndex > 0)
			{
				ShowContainers();
			}
		}

		protected void OnRestoreClick(object sender, EventArgs e)
		{
			if (chkPortal.Checked)
			{
				SkinController.SetSkin(SkinController.RootSkin, PortalId, SkinType.Portal, "");
				SkinController.SetSkin(SkinController.RootContainer, PortalId, SkinType.Portal, "");
			}
			if (chkAdmin.Checked)
			{
				SkinController.SetSkin(SkinController.RootSkin, PortalId, SkinType.Admin, "");
				SkinController.SetSkin(SkinController.RootContainer, PortalId, SkinType.Admin, "");
			}
			DataCache.ClearPortalCache(PortalId, true);
			Response.Redirect(Request.RawUrl);
		}

		#endregion

    }
}