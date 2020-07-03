// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.PersonaBar.Themes.Components
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Xml;

    using Dnn.PersonaBar.Themes.Components.DTO;
    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Framework;
    using DotNetNuke.Instrumentation;
    using DotNetNuke.Services.Exceptions;
    using DotNetNuke.UI.Skins;

    using Image = System.Drawing.Image;

    public class ThemesController : ServiceLocator<IThemesController, ThemesController>, IThemesController
    {
        internal static readonly IList<string> ImageExtensions = new List<string>() { ".jpg", ".png", ".jpeg" };

        internal static readonly IList<string> DefaultLayoutNames = new List<string>() { "Default", "2-Col", "Home", "Index", "Main" };
        internal static readonly IList<string> DefaultContainerNames = new List<string>() { "Title-h2", "NoTitle", "Main", "Default" };
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(ThemesController));

        private static readonly object _threadLocker = new object();

        /// <summary>
        /// Get Skins.
        /// </summary>
        /// <param name="portalSettings"></param>
        /// <param name="level">portal level or host level.</param>
        /// <returns></returns>
        public IList<ThemeInfo> GetLayouts(PortalSettings portalSettings, ThemeLevel level)
        {
            var themes = new List<ThemeInfo>();
            if ((level & ThemeLevel.Site) == ThemeLevel.Site)
            {
                themes.AddRange(GetThemes(ThemeType.Skin, Path.Combine(portalSettings.HomeDirectoryMapPath, SkinController.RootSkin)));
            }

            if ((level & ThemeLevel.SiteSystem) == ThemeLevel.SiteSystem)
            {
                themes.AddRange(GetThemes(ThemeType.Skin, Path.Combine(portalSettings.HomeSystemDirectoryMapPath, SkinController.RootSkin)));
            }

            if ((level & ThemeLevel.Global) == ThemeLevel.Global)
            {
                themes.AddRange(GetThemes(ThemeType.Skin, Path.Combine(Globals.HostMapPath, SkinController.RootSkin)));
            }

            return themes;
        }

        /// <summary>
        /// Get Containers.
        /// </summary>
        /// <param name="portalSettings"></param>
        /// <param name="level">portal level or host level.</param>
        /// <returns></returns>
        public IList<ThemeInfo> GetContainers(PortalSettings portalSettings, ThemeLevel level)
        {
            var themes = new List<ThemeInfo>();
            if ((level & ThemeLevel.Site) == ThemeLevel.Site)
            {
                themes.AddRange(GetThemes(ThemeType.Container, Path.Combine(portalSettings.HomeDirectoryMapPath, SkinController.RootContainer)));
            }

            if ((level & ThemeLevel.SiteSystem) == ThemeLevel.SiteSystem)
            {
                themes.AddRange(GetThemes(ThemeType.Container, Path.Combine(portalSettings.HomeSystemDirectoryMapPath, SkinController.RootContainer)));
            }

            if ((level & ThemeLevel.Global) == ThemeLevel.Global)
            {
                themes.AddRange(GetThemes(ThemeType.Container, Path.Combine(Globals.HostMapPath, SkinController.RootContainer)));
            }

            return themes;
        }

        /// <summary>
        /// get skin files in the skin.
        /// </summary>
        /// <param name="portalSettings"></param>
        /// <param name="theme"></param>
        /// <returns></returns>
        public IList<ThemeFileInfo> GetThemeFiles(PortalSettings portalSettings, ThemeInfo theme)
        {
            var themePath = Path.Combine(Globals.ApplicationMapPath, theme.Path);
            var themeFiles = new List<ThemeFileInfo>();

            if (Directory.Exists(themePath))
            {
                bool fallbackSkin;
                if (theme.Type == ThemeType.Skin)
                {
                    fallbackSkin = IsFallbackSkin(themePath);
                }
                else
                {
                    fallbackSkin = IsFallbackContainer(themePath);
                }

                var strSkinType = themePath.IndexOf(Globals.HostMapPath, StringComparison.OrdinalIgnoreCase) != -1 ? "G" : "L";

                var canDeleteSkin = SkinController.CanDeleteSkin(themePath, portalSettings.HomeDirectoryMapPath);
                var arrFiles = Directory.GetFiles(themePath, "*.ascx");

                foreach (var strFile in arrFiles)
                {
                    var file = strFile.ToLowerInvariant();

                    var themeFile = new ThemeFileInfo();
                    themeFile.ThemeName = theme.PackageName;
                    themeFile.Type = theme.Type;
                    themeFile.Level = theme.Level;

                    var imagePath = string.Empty;
                    foreach (var ext in ImageExtensions)
                    {
                        var path = Path.ChangeExtension(file, ext);
                        if (File.Exists(path))
                        {
                            imagePath = path;
                            break;
                        }
                    }

                    if (!string.IsNullOrEmpty(imagePath))
                    {
                        themeFile.Thumbnail = CreateThumbnail(imagePath);
                    }

                    themeFile.Name = Path.GetFileNameWithoutExtension(file);
                    themeFile.Path = FormatThemePath(portalSettings, themePath, Path.GetFileName(strFile), theme.Type);
                    themeFile.CanDelete = (UserController.Instance.GetCurrentUserInfo().IsSuperUser || strSkinType == "L")
                                          && (!fallbackSkin && canDeleteSkin);

                    themeFiles.Add(themeFile);
                }
            }

            return themeFiles;
        }

        public ThemeFileInfo GetThemeFile(PortalSettings portalSettings, string filePath, ThemeType type)
        {
            var themeName = SkinController.FormatSkinPath(filePath)
                            .Substring(filePath.IndexOf("/", StringComparison.OrdinalIgnoreCase) + 1)
                            .Replace("/", string.Empty);
            var themeLevel = GetThemeLevel(filePath);

            var themeInfo = (type == ThemeType.Skin ? this.GetLayouts(portalSettings, ThemeLevel.All)
                                                    : this.GetContainers(portalSettings, ThemeLevel.All))
                            .FirstOrDefault(t => t.PackageName.Equals(themeName, StringComparison.OrdinalIgnoreCase) && t.Level == themeLevel);

            if (themeInfo != null)
            {
                return this.GetThemeFiles(portalSettings, themeInfo).FirstOrDefault(f => (f.Path + ".ascx").Equals(filePath, StringComparison.OrdinalIgnoreCase));
            }

            return null;
        }

        /// <summary>
        /// update portal skin.
        /// </summary>
        /// <param name="portalId">portal id.</param>
        /// <param name="themeFile">skin info.</param>
        /// <param name="scope">change skin or container.</param>
        public void ApplyTheme(int portalId, ThemeFileInfo themeFile, ApplyThemeScope scope)
        {
            var skinPath = themeFile.Path + ".ascx";

            switch (themeFile.Type)
            {
                case ThemeType.Container:
                    if ((scope & ApplyThemeScope.Site) == ApplyThemeScope.Site)
                    {
                        SkinController.SetSkin(SkinController.RootContainer, portalId, SkinType.Portal, skinPath);
                    }

                    if ((scope & ApplyThemeScope.Edit) == ApplyThemeScope.Edit)
                    {
                        SkinController.SetSkin(SkinController.RootContainer, portalId, SkinType.Admin, skinPath);
                    }
                    break;
                case ThemeType.Skin:
                    if ((scope & ApplyThemeScope.Site) == ApplyThemeScope.Site)
                    {
                        SkinController.SetSkin(SkinController.RootSkin, portalId, SkinType.Portal, skinPath);
                    }
                    if ((scope & ApplyThemeScope.Edit) == ApplyThemeScope.Edit)
                    {
                        SkinController.SetSkin(SkinController.RootSkin, portalId, SkinType.Admin, skinPath);
                    }
                    DataCache.ClearPortalCache(portalId, true);
                    break;
            }
        }

        /// <summary>
        /// update portal skin.
        /// </summary>
        /// <param name="portalSettings">portal settings.</param>
        /// <param name="themeName"></param>
        /// <param name="level"></param>
        public void ApplyDefaultTheme(PortalSettings portalSettings, string themeName, ThemeLevel level)
        {
            var skin = this.GetLayouts(portalSettings, ThemeLevel.All)
                .FirstOrDefault(t => t.PackageName.Equals(themeName, StringComparison.OrdinalIgnoreCase) && t.Level == level);
            if (skin != null)
            {
                var skinFile = this.GetThemeFiles(portalSettings, skin).FirstOrDefault(t => t.Path == skin.DefaultThemeFile);
                if (skinFile != null)
                {
                    this.ApplyTheme(portalSettings.PortalId, skinFile, ApplyThemeScope.Site | ApplyThemeScope.Edit);
                }
            }

            var container = this.GetContainers(portalSettings, ThemeLevel.All)
                .FirstOrDefault(t => t.PackageName.Equals(themeName, StringComparison.OrdinalIgnoreCase) && t.Level == level);
            if (container != null)
            {
                var containerFile = this.GetThemeFiles(portalSettings, container).FirstOrDefault(t => t.Path == container.DefaultThemeFile);
                if (containerFile != null)
                {
                    this.ApplyTheme(portalSettings.PortalId, containerFile, ApplyThemeScope.Site | ApplyThemeScope.Edit);
                }
            }
        }

        /// <summary>
        /// delete a skin or container.
        /// </summary>
        /// <param name="portalSettings"></param>
        /// <param name="themeFile"></param>
        public void DeleteTheme(PortalSettings portalSettings, ThemeFileInfo themeFile)
        {
            var themePath = SkinController.FormatSkinSrc(themeFile.Path, portalSettings);
            var user = UserController.Instance.GetCurrentUserInfo();

            if (!user.IsSuperUser && themePath.IndexOf("\\portals\\_default\\", StringComparison.OrdinalIgnoreCase) != Null.NullInteger)
            {
                throw new SecurityException("NoPermission");
            }

            File.Delete(Path.Combine(Globals.ApplicationMapPath, themePath));
            DataCache.ClearPortalCache(portalSettings.PortalId, true);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="portalSettings"></param>
        /// <param name="theme"></param>
        public void DeleteThemePackage(PortalSettings portalSettings, ThemeInfo theme)
        {
            var themePath = Path.Combine(Globals.ApplicationMapPath, theme.Path);
            var user = UserController.Instance.GetCurrentUserInfo();

            if (!user.IsSuperUser && themePath.IndexOf("\\portals\\_default\\", StringComparison.OrdinalIgnoreCase) != Null.NullInteger)
            {
                throw new SecurityException("NoPermission");
            }

            if (theme.Type == ThemeType.Skin)
            {
                var skinPackage = SkinController.GetSkinPackage(portalSettings.PortalId, theme.PackageName, "Skin");
                if (skinPackage != null)
                {
                    throw new InvalidOperationException("UsePackageUninstall");
                }

                if (Directory.Exists(themePath))
                {
                    Globals.DeleteFolderRecursive(themePath);
                }
                if (Directory.Exists(themePath.Replace("\\" + SkinController.RootSkin.ToLower() + "\\", "\\" + SkinController.RootContainer + "\\")))
                {
                    Globals.DeleteFolderRecursive(themePath.Replace("\\" + SkinController.RootSkin.ToLower() + "\\", "\\" + SkinController.RootContainer + "\\"));
                }
            }
            else if (theme.Type == ThemeType.Container)
            {
                var skinPackage = SkinController.GetSkinPackage(portalSettings.PortalId, theme.PackageName, "Container");
                if (skinPackage != null)
                {
                    throw new InvalidOperationException("UsePackageUninstall");
                }

                if (Directory.Exists(themePath))
                {
                    Globals.DeleteFolderRecursive(themePath);
                }
            }
        }

        /// <summary>
        /// Update Theme Attributes.
        /// </summary>
        /// <param name="portalSettings"></param>
        /// <param name="updateTheme"></param>
        public void UpdateTheme(PortalSettings portalSettings, UpdateThemeInfo updateTheme)
        {
            var themePath = SkinController.FormatSkinSrc(updateTheme.Path + ".ascx", portalSettings);
            themePath = Path.Combine(Globals.ApplicationMapPath, themePath.TrimStart('/'));

            var objStreamReader = File.OpenText(themePath);
            var strSkin = objStreamReader.ReadToEnd();
            objStreamReader.Close();
            var strTag = "<dnn:" + updateTheme.Token + " runat=\"server\" id=\"dnn" + updateTheme.Token + "\"";
            var intOpenTag = strSkin.IndexOf(strTag);
            if (intOpenTag != -1)
            {
                var intCloseTag = strSkin.IndexOf(" />", intOpenTag);
                var strAttribute = updateTheme.Setting;
                var intStartAttribute = strSkin.IndexOf(strAttribute, intOpenTag);
                string strValue = updateTheme.Value;
                if (intStartAttribute != -1 && intStartAttribute < intCloseTag)
                {
                    //remove attribute
                    var intEndAttribute = strSkin.IndexOf("\" ", intStartAttribute);
                    strSkin = strSkin.Substring(0, intStartAttribute) + strSkin.Substring(intEndAttribute + 2);
                }
                //add attribute
                strSkin = strSkin.Insert(intOpenTag + strTag.Length, " " + strAttribute + "=\"" + strValue + "\"");

                File.SetAttributes(themePath, FileAttributes.Normal);
                var objStream = File.CreateText(themePath);
                objStream.WriteLine(strSkin);
                objStream.Close();

                this.UpdateManifest(portalSettings, updateTheme);
            }
        }

        /// <summary>
        /// Parse skin package.
        /// </summary>
        /// <param name="portalSettings"></param>
        /// <param name="theme"></param>
        /// <param name="parseType"></param>
        public void ParseTheme(PortalSettings portalSettings, ThemeInfo theme, ParseType parseType)
        {
            var strRootPath = Null.NullString;
            switch (theme.Level)
            {
                case ThemeLevel.Global: //global
                    strRootPath = Globals.HostMapPath;
                    break;
                case ThemeLevel.Site: //local
                    strRootPath = portalSettings.HomeDirectoryMapPath;
                    break;
            }
            var objSkinFiles = new SkinFileProcessor(strRootPath, theme.Type == ThemeType.Container ? SkinController.RootContainer : SkinController.RootSkin, theme.PackageName);
            var arrSkinFiles = new ArrayList();

            var strFolder = Path.Combine(Globals.ApplicationMapPath, theme.Path);
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
            switch (parseType)
            {
                case ParseType.Localized: //localized
                    objSkinFiles.ProcessList(arrSkinFiles, SkinParser.Localized);
                    break;
                case ParseType.Portable: //portable
                    objSkinFiles.ProcessList(arrSkinFiles, SkinParser.Portable);
                    break;
            }
        }

        internal static string CreateThumbnail(string strImage)
        {
            var imageFileName = Path.GetFileName(strImage);
            if (string.IsNullOrEmpty(imageFileName) || imageFileName.StartsWith("thumbnail_"))
            {
                strImage = Globals.ApplicationPath + "/" + strImage.Substring(strImage.IndexOf("portals\\"));
                strImage = strImage.Replace("\\", "/");
                return strImage;
            }

            var strThumbnail = strImage.Replace(Path.GetFileName(strImage), "thumbnail_" + imageFileName);

            if (NeedCreateThumbnail(strThumbnail, strImage))
            {
                lock (_threadLocker)
                {
                    if (NeedCreateThumbnail(strThumbnail, strImage))
                    {
                        const int intSize = 150; //size of the thumbnail 
                        try
                        {
                            var objImage = Image.FromFile(strImage);

                            //scale the image to prevent distortion
                            int intHeight;
                            int intWidth;
                            double dblScale;
                            if (objImage.Height > objImage.Width)
                            {
                                //The height was larger, so scale the width 
                                dblScale = (double)intSize / objImage.Height;
                                intHeight = intSize;
                                intWidth = Convert.ToInt32(objImage.Width * dblScale);
                            }
                            else
                            {
                                //The width was larger, so scale the height 
                                dblScale = (double)intSize / objImage.Width;
                                intWidth = intSize;
                                intHeight = Convert.ToInt32(objImage.Height * dblScale);
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
                }
            }

            strThumbnail = Globals.ApplicationPath + "/" + strThumbnail.Substring(strThumbnail.IndexOf("portals\\"));
            strThumbnail = strThumbnail.Replace("\\", "/");

            //return thumbnail filename
            return strThumbnail;
        }

        internal static ThemeLevel GetThemeLevel(string themeFilePath)
        {
            themeFilePath = themeFilePath.Replace("\\", "/");
            if (!string.IsNullOrEmpty(Globals.ApplicationPath)
                && !themeFilePath.StartsWith("[")
                && !themeFilePath.StartsWith(Globals.ApplicationPath, StringComparison.InvariantCultureIgnoreCase))
            {
                var needSlash = !Globals.ApplicationPath.EndsWith("/") && !themeFilePath.StartsWith("/");
                themeFilePath = $"{Globals.ApplicationPath}{(needSlash ? "/" : "")}{themeFilePath}";
            }

            if (themeFilePath.IndexOf(Globals.HostPath.TrimStart('/'), StringComparison.OrdinalIgnoreCase) > Null.NullInteger
                || themeFilePath.StartsWith("[G]", StringComparison.OrdinalIgnoreCase))
            {
                return ThemeLevel.Global;
            }
            else if ((PortalSettings.Current != null &&
                        themeFilePath.IndexOf(PortalSettings.Current.HomeSystemDirectory.TrimStart('/'), StringComparison.OrdinalIgnoreCase) > Null.NullInteger)
                        || themeFilePath.StartsWith("[S]", StringComparison.OrdinalIgnoreCase))
            {
                return ThemeLevel.SiteSystem;
            }
            else
            {
                return ThemeLevel.Site;
            }
        }

        protected override Func<IThemesController> GetFactory()
        {
            return () => new ThemesController();
        }

        private static IList<ThemeInfo> GetThemes(ThemeType type, string strRoot)
        {
            var themes = new List<ThemeInfo>();
            if (Directory.Exists(strRoot))
            {
                foreach (var strFolder in Directory.GetDirectories(strRoot))
                {
                    var strName = strFolder.Substring(strFolder.LastIndexOf("\\") + 1);
                    if (strName != "_default")
                    {
                        var themePath = strFolder.Replace(Globals.ApplicationMapPath, "").TrimStart('\\').ToLowerInvariant();
                        var isFallback = type == ThemeType.Skin ? IsFallbackSkin(themePath) : IsFallbackContainer(themePath);
                        var canDelete = !isFallback && SkinController.CanDeleteSkin(strFolder, PortalSettings.Current.HomeDirectoryMapPath);
                        var defaultThemeFile = GetDefaultThemeFileName(themePath, type);
                        themes.Add(new ThemeInfo()
                        {
                            PackageName = strName,
                            Type = type,
                            Path = themePath,
                            DefaultThemeFile = FormatThemePath(PortalSettings.Current, strFolder, defaultThemeFile, type),
                            Thumbnail = GetThumbnail(themePath, defaultThemeFile),
                            CanDelete = canDelete
                        });
                    }
                }
            }

            return themes;
        }

        private static string GetDefaultThemeFileName(string themePath, ThemeType type)
        {
            var themeFiles = new List<string>();
            var folderPath = Path.Combine(Globals.ApplicationMapPath, themePath);
            themeFiles.AddRange(Directory.GetFiles(folderPath, "*.ascx"));

            var defaultFile = themeFiles.FirstOrDefault(i =>
            {
                var fileName = Path.GetFileNameWithoutExtension(i);
                return type == ThemeType.Skin ? DefaultLayoutNames.Contains(fileName, StringComparer.OrdinalIgnoreCase)
                                              : DefaultContainerNames.Contains(fileName, StringComparer.OrdinalIgnoreCase);
            });

            if (string.IsNullOrEmpty(defaultFile))
            {
                defaultFile = themeFiles.FirstOrDefault();
            }

            return !string.IsNullOrEmpty(defaultFile) ? Path.GetFileName(defaultFile) : string.Empty;
        }

        private static string GetThumbnail(string themePath, string themeFileName)
        {
            var folderPath = Path.Combine(Globals.ApplicationMapPath, themePath);
            var filePath = Path.Combine(folderPath, themeFileName);
            var imagePath = string.Empty;
            foreach (var ext in ImageExtensions)
            {
                var path = Path.ChangeExtension(filePath, ext);
                if (File.Exists(path))
                {
                    imagePath = path;
                    break;
                }
            }

            return !string.IsNullOrEmpty(imagePath) ? CreateThumbnail(imagePath) : string.Empty;
        }

        private static bool NeedCreateThumbnail(string thumbnailPath, string imagePath)
        {
            return !File.Exists(thumbnailPath) || File.GetLastWriteTime(thumbnailPath) != File.GetLastWriteTime(imagePath);
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

        private static string FormatThemePath(PortalSettings portalSettings, string themePath, string fileName, ThemeType type)
        {
            var filePath = Path.Combine(themePath, fileName);
            var lowercasePath = filePath.ToLowerInvariant();
            string strRootSkin;
            if (type == ThemeType.Skin)
            {
                strRootSkin = SkinController.RootSkin.ToLower();
            }
            else
            {
                strRootSkin = SkinController.RootContainer.ToLower();
            }

            var strSkinType = themePath.IndexOf(Globals.HostMapPath, StringComparison.OrdinalIgnoreCase) != -1 ? "G" : "L";
            if (themePath.IndexOf(portalSettings.HomeSystemDirectoryMapPath, StringComparison.OrdinalIgnoreCase) > -1)
            {
                strSkinType = "S";
            }

            var strUrl = lowercasePath.Substring(filePath.IndexOf("\\" + strRootSkin + "\\", StringComparison.OrdinalIgnoreCase))
                        .Replace(".ascx", "")
                        .Replace("\\", "/")
                        .TrimStart('/');

            return "[" + strSkinType + "]" + strUrl;
        }

        private void UpdateManifest(PortalSettings portalSettings, UpdateThemeInfo updateTheme)
        {
            var themePath = SkinController.FormatSkinSrc(updateTheme.Path, portalSettings);
            if (File.Exists(themePath.Replace(".ascx", ".htm")))
            {
                var strFile = themePath.Replace(".ascx", ".xml");
                if (File.Exists(strFile) == false)
                {
                    strFile = strFile.Replace(Path.GetFileName(strFile), "skin.xml");
                }
                XmlDocument xmlDoc = null;
                try
                {
                    xmlDoc = new XmlDocument { XmlResolver = null };
                    xmlDoc.Load(strFile);
                }
                catch
                {
                    xmlDoc.InnerXml = "<Objects></Objects>";
                }
                var xmlToken = xmlDoc.DocumentElement.SelectSingleNode("descendant::Object[Token='[" + updateTheme.Token + "]']");
                if (xmlToken == null)
                {
                    //add token
                    string strToken = "<Token>[" + updateTheme.Token + "]</Token><Settings></Settings>";
                    xmlToken = xmlDoc.CreateElement("Object");
                    xmlToken.InnerXml = strToken;
                    xmlDoc.SelectSingleNode("Objects").AppendChild(xmlToken);
                    xmlToken = xmlDoc.DocumentElement.SelectSingleNode("descendant::Object[Token='[" + updateTheme.Token + "]']");
                }
                var strValue = updateTheme.Value;

                var blnUpdate = false;
                foreach (XmlNode xmlSetting in xmlToken.SelectNodes(".//Settings/Setting"))
                {
                    if (xmlSetting.SelectSingleNode("Name").InnerText == updateTheme.Setting)
                    {
                        xmlSetting.SelectSingleNode("Value").InnerText = strValue;
                        blnUpdate = true;
                    }
                }
                if (blnUpdate == false)
                {
                    var strSetting = "<Name>" + updateTheme.Setting + "</Name><Value>" + strValue + "</Value>";
                    XmlNode xmlSetting = xmlDoc.CreateElement("Setting");
                    xmlSetting.InnerXml = strSetting;
                    xmlToken.SelectSingleNode("Settings").AppendChild(xmlSetting);
                }
                try
                {
                    if (File.Exists(strFile))
                    {
                        File.SetAttributes(strFile, FileAttributes.Normal);
                    }
                    var objStream = File.CreateText(strFile);
                    var strXML = xmlDoc.InnerXml;
                    strXML = strXML.Replace("><", ">" + Environment.NewLine + "<");
                    objStream.WriteLine(strXML);
                    objStream.Close();
                }
                catch (Exception ex)
                {
                    Logger.Error(ex);
                }
            }

        }
    }
}
