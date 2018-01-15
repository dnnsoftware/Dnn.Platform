#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2018
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
using System.IO;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;

using DotNetNuke.Common;
using DotNetNuke.Framework;
using DotNetNuke.Instrumentation;
using DotNetNuke.Services.Localization;

using Image = System.Drawing.Image;

#endregion

namespace DotNetNuke.UI.Skins
{
    /// -----------------------------------------------------------------------------
    /// <summary>
    /// SkinThumbNailControl is a user control that provides that displays the skins
    ///	as a Radio ButtonList with Thumbnail Images where available
    /// </summary>
    /// <remarks>
    /// </remarks>
    /// -----------------------------------------------------------------------------
    public abstract class SkinThumbNailControl : UserControlBase
    {
    	private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof (SkinThumbNailControl));
		#region "Private Members"
		
        protected HtmlGenericControl ControlContainer;
        protected RadioButtonList OptSkin;
		
		#endregion

		#region "Properties"

        public string Border
        {
            get
            {
                return Convert.ToString(ViewState["SkinControlBorder"]);
            }
            set
            {
                ViewState["SkinControlBorder"] = value;
                if (!String.IsNullOrEmpty(value))
                {
                    ControlContainer.Style.Add("border-top", value);
                    ControlContainer.Style.Add("border-bottom", value);
                    ControlContainer.Style.Add("border-left", value);
                    ControlContainer.Style.Add("border-right", value);
                }
            }
        }

        public int Columns
        {
            get
            {
                return Convert.ToInt32(ViewState["SkinControlColumns"]);
            }
            set
            {
                ViewState["SkinControlColumns"] = value;
                if (value > 0)
                {
                    OptSkin.RepeatColumns = value;
                }
            }
        }

        public string Height
        {
            get
            {
                return Convert.ToString(ViewState["SkinControlHeight"]);
            }
            set
            {
                ViewState["SkinControlHeight"] = value;
                if (!String.IsNullOrEmpty(value))
                {
                    ControlContainer.Style.Add("height", value);
                }
            }
        }

        public string SkinRoot
        {
            get
            {
                return Convert.ToString(ViewState["SkinRoot"]);
            }
            set
            {
                ViewState["SkinRoot"] = value;
            }
        }

        public string SkinSrc
        {
            get
            {
                return OptSkin.SelectedItem != null ? OptSkin.SelectedItem.Value : "";
            }
            set
            {
				//select current skin
                int intIndex;
                for (intIndex = 0; intIndex <= OptSkin.Items.Count - 1; intIndex++)
                {
                    if (OptSkin.Items[intIndex].Value == value)
                    {
                        OptSkin.Items[intIndex].Selected = true;
                        break;
                    }
                }
            }
        }

        public string Width
        {
            get
            {
                return Convert.ToString(ViewState["SkinControlWidth"]);
            }
            set
            {
                ViewState["SkinControlWidth"] = value;
                if (!String.IsNullOrEmpty(value))
                {
                    ControlContainer.Style.Add("width", value);
                }
            }
        }
		
		#endregion

		#region "Private Methods"

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// AddDefaultSkin adds the not-specified skin to the radio button list
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// -----------------------------------------------------------------------------
        private void AddDefaultSkin()
        {
            var strDefault = Localization.GetString("Not_Specified") + "<br />";
            strDefault += "<img src=\"" + Globals.ApplicationPath.Replace("\\", "/") + "/images/spacer.gif\" width=\"140\" height=\"135\" border=\"0\">";
            OptSkin.Items.Insert(0, new ListItem(strDefault, ""));
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// AddSkin adds the skin to the radio button list
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <param name="root">Root Path.</param>
        /// <param name="strFolder">The Skin Folder</param>
        /// <param name="strFile">The Skin File</param>
        /// -----------------------------------------------------------------------------
        private void AddSkin(string root, string strFolder, string strFile)
        {
            var strImage = "";
            if (File.Exists(strFile.Replace(".ascx", ".jpg")))
            {
                strImage += "<a href=\"" + CreateThumbnail(strFile.Replace(".ascx", ".jpg")).Replace("thumbnail_", "") + "\" target=\"_blank\"><img src=\"" +
                            CreateThumbnail(strFile.Replace(".ascx", ".jpg")).Replace("\\", "/") + "\" border=\"1\"></a>";
            }
            else
            {
                strImage += "<img src=\"" + Globals.ApplicationPath.Replace("\\", "/") + "/images/thumbnail.jpg\" border=\"1\">";
            }
            OptSkin.Items.Add(new ListItem(FormatSkinName(strFolder, Path.GetFileNameWithoutExtension(strFile)) + "<br />" + strImage, root + "/" + strFolder + "/" + Path.GetFileName(strFile)));
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// format skin name
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <param name="strSkinFolder">The Folder Name</param>
        /// <param name="strSkinFile">The File Name without extension</param>
        private static string FormatSkinName(string strSkinFolder, string strSkinFile)
        {
            if (strSkinFolder.ToLower() == "_default") //host folder
            {
                return strSkinFile;
            }
			
			//portal folder
            switch (strSkinFile.ToLower())
            {
                case "skin":
                case "container":
                case "default":
                    return strSkinFolder;
                default:
                    return string.Format("<span class=\"NormalBold\">{0} - {1}</span>", strSkinFolder, strSkinFile);
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// CreateThumbnail creates a thumbnail of the Preview Image
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <param name="strImage">The Image File Name</param>
        /// -----------------------------------------------------------------------------
        private static string CreateThumbnail(string strImage)
        {
            var blnCreate = true;

            var strThumbnail = strImage.Replace(Path.GetFileName(strImage), "thumbnail_" + Path.GetFileName(strImage));

            //check if image has changed
            if (File.Exists(strThumbnail))
            {
                //var d1 = File.GetLastWriteTime(strThumbnail);
                //var d2 = File.GetLastWriteTime(strImage);
                if (File.GetLastWriteTime(strThumbnail) == File.GetLastWriteTime(strImage))
                {
                    blnCreate = false;
                }
            }
            if (blnCreate)
            {
                const int intSize = 140; //size of the thumbnail 
                Image objImage;
                try
                {
                    objImage = Image.FromFile(strImage);
					
					//scale the image to prevent distortion
                    int intWidth;
                    int intHeight;
                    double dblScale;
                    if (objImage.Height > objImage.Width)
                    {
						//The height was larger, so scale the width 
                        dblScale = (double)intSize / objImage.Height;
                        intHeight = intSize;
                        intWidth = Convert.ToInt32(objImage.Width*dblScale);
                    }
                    else
                    {
						//The width was larger, so scale the height 
                        dblScale = (double)intSize / objImage.Width;
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
				catch (Exception ex)
				{
					Logger.Error(ex);
				}
            }
            strThumbnail = Globals.ApplicationPath + "\\" + strThumbnail.Substring(strThumbnail.ToLower().IndexOf("portals\\"));
            return strThumbnail;
        }

		#endregion

		#region "Public Methods"

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Clear clears the radio button list
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// -----------------------------------------------------------------------------
        public void Clear()
        {
            OptSkin.Items.Clear();
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// LoadAllSkins loads all the available skins (Host and Site) to the radio button list
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <param name="includeNotSpecified">Optionally include the "Not Specified" option</param>
        /// -----------------------------------------------------------------------------
        public void LoadAllSkins(bool includeNotSpecified)
        {
            //default value
            if (includeNotSpecified)
            {
                AddDefaultSkin();
            }
			
            //load host skins (includeNotSpecified = false as we have already added it)
            LoadHostSkins(false);

            //load portal skins (includeNotSpecified = false as we have already added it)
            LoadPortalSkins(false);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// LoadHostSkins loads all the available Host skins to the radio button list
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <param name="includeNotSpecified">Optionally include the "Not Specified" option</param>
        /// -----------------------------------------------------------------------------
        public void LoadHostSkins(bool includeNotSpecified)
        {

            //default value
            if (includeNotSpecified)
            {
                AddDefaultSkin();
            }
			
			//load host skins
            var strRoot = Globals.HostMapPath + SkinRoot;
            if (Directory.Exists(strRoot))
            {
                var arrFolders = Directory.GetDirectories(strRoot);
                foreach (var strFolder in arrFolders)
                {
                    if (!strFolder.EndsWith(Globals.glbHostSkinFolder))
                    {
                        LoadSkins(strFolder, "[G]", false);
                    }
                }
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// LoadHostSkins loads all the available Site/Portal skins to the radio button list
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <param name="includeNotSpecified">Optionally include the "Not Specified" option</param>
        /// -----------------------------------------------------------------------------
        public void LoadPortalSkins(bool includeNotSpecified)
        {
            //default value
            if (includeNotSpecified)
            {
                AddDefaultSkin();
            }
			
			//load portal skins
            var strRoot = PortalSettings.HomeDirectoryMapPath + SkinRoot;
            if (Directory.Exists(strRoot))
            {
                var arrFolders = Directory.GetDirectories(strRoot);
                foreach (var strFolder in arrFolders)
                {
                    LoadSkins(strFolder, "[L]", false);
                }
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// LoadSkins loads all the available skins in a specific folder to the radio button list
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <param name="strFolder">The folder to search for skins</param>
        /// <param name="skinType">A string that identifies whether the skin is Host "[G]" or Site "[L]"</param>
        /// <param name="includeNotSpecified">Optionally include the "Not Specified" option</param>
        /// -----------------------------------------------------------------------------
        public void LoadSkins(string strFolder, string skinType, bool includeNotSpecified)
        {
            //default value
            if (includeNotSpecified)
            {
                AddDefaultSkin();
            }
            if (Directory.Exists(strFolder))
            {
                var arrFiles = Directory.GetFiles(strFolder, "*.ascx");
                strFolder = strFolder.Substring(strFolder.LastIndexOf("\\") + 1);

                foreach (var strFile in arrFiles)
                {
                    AddSkin(skinType + SkinRoot, strFolder, strFile);
                }
            }
        }
		
		#endregion

    }
}