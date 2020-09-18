// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.UI.Skins
{
    using System;
    using System.IO;
    using System.Web.UI.HtmlControls;
    using System.Web.UI.WebControls;

    using DotNetNuke.Common;
    using DotNetNuke.Framework;
    using DotNetNuke.Instrumentation;
    using DotNetNuke.Services.Localization;

    using Image = System.Drawing.Image;

    /// -----------------------------------------------------------------------------
    /// <summary>
    /// SkinThumbNailControl is a user control that provides that displays the skins
    ///     as a Radio ButtonList with Thumbnail Images where available.
    /// </summary>
    /// <remarks>
    /// </remarks>
    /// -----------------------------------------------------------------------------
    public abstract class SkinThumbNailControl : UserControlBase
    {
        protected HtmlGenericControl ControlContainer;
        protected RadioButtonList OptSkin;
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(SkinThumbNailControl));

        public string Border
        {
            get
            {
                return Convert.ToString(this.ViewState["SkinControlBorder"]);
            }

            set
            {
                this.ViewState["SkinControlBorder"] = value;
                if (!string.IsNullOrEmpty(value))
                {
                    this.ControlContainer.Style.Add("border-top", value);
                    this.ControlContainer.Style.Add("border-bottom", value);
                    this.ControlContainer.Style.Add("border-left", value);
                    this.ControlContainer.Style.Add("border-right", value);
                }
            }
        }

        public int Columns
        {
            get
            {
                return Convert.ToInt32(this.ViewState["SkinControlColumns"]);
            }

            set
            {
                this.ViewState["SkinControlColumns"] = value;
                if (value > 0)
                {
                    this.OptSkin.RepeatColumns = value;
                }
            }
        }

        public string Height
        {
            get
            {
                return Convert.ToString(this.ViewState["SkinControlHeight"]);
            }

            set
            {
                this.ViewState["SkinControlHeight"] = value;
                if (!string.IsNullOrEmpty(value))
                {
                    this.ControlContainer.Style.Add("height", value);
                }
            }
        }

        public string SkinRoot
        {
            get
            {
                return Convert.ToString(this.ViewState["SkinRoot"]);
            }

            set
            {
                this.ViewState["SkinRoot"] = value;
            }
        }

        public string SkinSrc
        {
            get
            {
                return this.OptSkin.SelectedItem != null ? this.OptSkin.SelectedItem.Value : string.Empty;
            }

            set
            {
                // select current skin
                int intIndex;
                for (intIndex = 0; intIndex <= this.OptSkin.Items.Count - 1; intIndex++)
                {
                    if (this.OptSkin.Items[intIndex].Value == value)
                    {
                        this.OptSkin.Items[intIndex].Selected = true;
                        break;
                    }
                }
            }
        }

        public string Width
        {
            get
            {
                return Convert.ToString(this.ViewState["SkinControlWidth"]);
            }

            set
            {
                this.ViewState["SkinControlWidth"] = value;
                if (!string.IsNullOrEmpty(value))
                {
                    this.ControlContainer.Style.Add("width", value);
                }
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Clear clears the radio button list.
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// -----------------------------------------------------------------------------
        public void Clear()
        {
            this.OptSkin.Items.Clear();
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// LoadAllSkins loads all the available skins (Host and Site) to the radio button list.
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <param name="includeNotSpecified">Optionally include the "Not Specified" option.</param>
        /// -----------------------------------------------------------------------------
        public void LoadAllSkins(bool includeNotSpecified)
        {
            // default value
            if (includeNotSpecified)
            {
                this.AddDefaultSkin();
            }

            // load host skins (includeNotSpecified = false as we have already added it)
            this.LoadHostSkins(false);

            // load portal skins (includeNotSpecified = false as we have already added it)
            this.LoadPortalSkins(false);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// LoadHostSkins loads all the available Host skins to the radio button list.
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <param name="includeNotSpecified">Optionally include the "Not Specified" option.</param>
        /// -----------------------------------------------------------------------------
        public void LoadHostSkins(bool includeNotSpecified)
        {
            // default value
            if (includeNotSpecified)
            {
                this.AddDefaultSkin();
            }

            // load host skins
            var strRoot = Globals.HostMapPath + this.SkinRoot;
            if (Directory.Exists(strRoot))
            {
                var arrFolders = Directory.GetDirectories(strRoot);
                foreach (var strFolder in arrFolders)
                {
                    if (!strFolder.EndsWith(Globals.glbHostSkinFolder))
                    {
                        this.LoadSkins(strFolder, "[G]", false);
                    }
                }
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// LoadHostSkins loads all the available Site/Portal skins to the radio button list.
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <param name="includeNotSpecified">Optionally include the "Not Specified" option.</param>
        /// -----------------------------------------------------------------------------
        public void LoadPortalSkins(bool includeNotSpecified)
        {
            // default value
            if (includeNotSpecified)
            {
                this.AddDefaultSkin();
            }

            // load portal skins
            var strRoot = this.PortalSettings.HomeDirectoryMapPath + this.SkinRoot;
            if (Directory.Exists(strRoot))
            {
                var arrFolders = Directory.GetDirectories(strRoot);
                foreach (var strFolder in arrFolders)
                {
                    this.LoadSkins(strFolder, "[L]", false);
                }
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// LoadSkins loads all the available skins in a specific folder to the radio button list.
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <param name="strFolder">The folder to search for skins.</param>
        /// <param name="skinType">A string that identifies whether the skin is Host "[G]" or Site "[L]".</param>
        /// <param name="includeNotSpecified">Optionally include the "Not Specified" option.</param>
        /// -----------------------------------------------------------------------------
        public void LoadSkins(string strFolder, string skinType, bool includeNotSpecified)
        {
            // default value
            if (includeNotSpecified)
            {
                this.AddDefaultSkin();
            }

            if (Directory.Exists(strFolder))
            {
                var arrFiles = Directory.GetFiles(strFolder, "*.ascx");
                strFolder = strFolder.Substring(strFolder.LastIndexOf("\\") + 1);

                foreach (var strFile in arrFiles)
                {
                    this.AddSkin(skinType + this.SkinRoot, strFolder, strFile);
                }
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// format skin name.
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <param name="strSkinFolder">The Folder Name.</param>
        /// <param name="strSkinFile">The File Name without extension.</param>
        private static string FormatSkinName(string strSkinFolder, string strSkinFile)
        {
            if (strSkinFolder.Equals("_default", StringComparison.InvariantCultureIgnoreCase)) // host folder
            {
                return strSkinFile;
            }

            // portal folder
            switch (strSkinFile.ToLowerInvariant())
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
        /// CreateThumbnail creates a thumbnail of the Preview Image.
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <param name="strImage">The Image File Name.</param>
        /// -----------------------------------------------------------------------------
        private static string CreateThumbnail(string strImage)
        {
            var blnCreate = true;

            var strThumbnail = strImage.Replace(Path.GetFileName(strImage), "thumbnail_" + Path.GetFileName(strImage));

            // check if image has changed
            if (File.Exists(strThumbnail))
            {
                // var d1 = File.GetLastWriteTime(strThumbnail);
                // var d2 = File.GetLastWriteTime(strImage);
                if (File.GetLastWriteTime(strThumbnail) == File.GetLastWriteTime(strImage))
                {
                    blnCreate = false;
                }
            }

            if (blnCreate)
            {
                const int intSize = 140; // size of the thumbnail
                Image objImage;
                try
                {
                    objImage = Image.FromFile(strImage);

                    // scale the image to prevent distortion
                    int intWidth;
                    int intHeight;
                    double dblScale;
                    if (objImage.Height > objImage.Width)
                    {
                        // The height was larger, so scale the width
                        dblScale = (double)intSize / objImage.Height;
                        intHeight = intSize;
                        intWidth = Convert.ToInt32(objImage.Width * dblScale);
                    }
                    else
                    {
                        // The width was larger, so scale the height
                        dblScale = (double)intSize / objImage.Width;
                        intWidth = intSize;
                        intHeight = Convert.ToInt32(objImage.Height * dblScale);
                    }

                    // create the thumbnail image
                    var objThumbnail = objImage.GetThumbnailImage(intWidth, intHeight, null, IntPtr.Zero);

                    // delete the old file ( if it exists )
                    if (File.Exists(strThumbnail))
                    {
                        File.Delete(strThumbnail);
                    }

                    // save the thumbnail image
                    objThumbnail.Save(strThumbnail, objImage.RawFormat);

                    // set the file attributes
                    File.SetAttributes(strThumbnail, FileAttributes.Normal);
                    File.SetLastWriteTime(strThumbnail, File.GetLastWriteTime(strImage));

                    // tidy up
                    objImage.Dispose();
                    objThumbnail.Dispose();
                }
                catch (Exception ex)
                {
                    Logger.Error(ex);
                }
            }

            strThumbnail = Globals.ApplicationPath + "\\" + strThumbnail.Substring(strThumbnail.IndexOf("portals\\", StringComparison.InvariantCultureIgnoreCase));
            return strThumbnail;
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// AddDefaultSkin adds the not-specified skin to the radio button list.
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// -----------------------------------------------------------------------------
        private void AddDefaultSkin()
        {
            var strDefault = Localization.GetString("Not_Specified") + "<br />";
            strDefault += "<img src=\"" + Globals.ApplicationPath.Replace("\\", "/") + "/images/spacer.gif\" width=\"140\" height=\"135\" border=\"0\">";
            this.OptSkin.Items.Insert(0, new ListItem(strDefault, string.Empty));
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// AddSkin adds the skin to the radio button list.
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <param name="root">Root Path.</param>
        /// <param name="strFolder">The Skin Folder.</param>
        /// <param name="strFile">The Skin File.</param>
        /// -----------------------------------------------------------------------------
        private void AddSkin(string root, string strFolder, string strFile)
        {
            var strImage = string.Empty;
            if (File.Exists(strFile.Replace(".ascx", ".jpg")))
            {
                strImage += "<a href=\"" + CreateThumbnail(strFile.Replace(".ascx", ".jpg")).Replace("thumbnail_", string.Empty) + "\" target=\"_blank\"><img src=\"" +
                            CreateThumbnail(strFile.Replace(".ascx", ".jpg")).Replace("\\", "/") + "\" border=\"1\"></a>";
            }
            else
            {
                strImage += "<img src=\"" + Globals.ApplicationPath.Replace("\\", "/") + "/images/thumbnail.jpg\" border=\"1\">";
            }

            this.OptSkin.Items.Add(new ListItem(FormatSkinName(strFolder, Path.GetFileNameWithoutExtension(strFile)) + "<br />" + strImage, root + "/" + strFolder + "/" + Path.GetFileName(strFile)));
        }
    }
}
