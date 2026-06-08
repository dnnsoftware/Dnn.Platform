// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace Dnn.PersonaBar.Themes.Components;

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Xml;

using Dnn.PersonaBar.Themes.Components.DTO;

using DotNetNuke.Abstractions.Application;
using DotNetNuke.Common;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;
using DotNetNuke.Framework;
using DotNetNuke.Instrumentation;
using DotNetNuke.Services.Exceptions;
using DotNetNuke.UI.Skins;

using Microsoft.Extensions.DependencyInjection;

using Image = System.Drawing.Image;

/// <summary>The default <see cref="IThemesController"/> implementation.</summary>
public class ThemesController : ServiceLocator<IThemesController, ThemesController>, IThemesController
{
    /// <summary>The image extensions.</summary>
    internal static readonly IList<string> ImageExtensions = [".jpg", ".png", ".jpeg",];

    /// <summary>The default theme layout names.</summary>
    internal static readonly IList<string> DefaultLayoutNames = ["Default", "2-Col", "Home", "Index", "Main",];

    /// <summary>The default container names.</summary>
    internal static readonly IList<string> DefaultContainerNames = ["Title-h2", "NoTitle", "Main", "Default",];
    private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(ThemesController));
    private static readonly object ThreadLocker = new object();

    private readonly IHostSettings hostSettings = Globals.GetCurrentServiceProvider().GetRequiredService<IHostSettings>();
    private readonly IApplicationStatusInfo appStatus = Globals.GetCurrentServiceProvider().GetRequiredService<IApplicationStatusInfo>();

    /// <inheritdoc />
    public IList<ThemeInfo> GetLayouts(PortalSettings portalSettings, ThemeLevel level)
    {
        var themes = new List<ThemeInfo>();
        if ((level & ThemeLevel.Site) == ThemeLevel.Site)
        {
            themes.AddRange(this.GetThemes(ThemeType.Skin, Path.Combine(portalSettings.HomeDirectoryMapPath, SkinController.RootSkin)));
        }

        if ((level & ThemeLevel.SiteSystem) == ThemeLevel.SiteSystem)
        {
            themes.AddRange(this.GetThemes(ThemeType.Skin, Path.Combine(portalSettings.HomeSystemDirectoryMapPath, SkinController.RootSkin)));
        }

        if ((level & ThemeLevel.Global) == ThemeLevel.Global)
        {
            themes.AddRange(this.GetThemes(ThemeType.Skin, Path.Combine(Globals.HostMapPath, SkinController.RootSkin)));
        }

        return themes;
    }

    /// <inheritdoc />
    public IList<ThemeInfo> GetContainers(PortalSettings portalSettings, ThemeLevel level)
    {
        var themes = new List<ThemeInfo>();
        if ((level & ThemeLevel.Site) == ThemeLevel.Site)
        {
            themes.AddRange(this.GetThemes(ThemeType.Container, Path.Combine(portalSettings.HomeDirectoryMapPath, SkinController.RootContainer)));
        }

        if ((level & ThemeLevel.SiteSystem) == ThemeLevel.SiteSystem)
        {
            themes.AddRange(this.GetThemes(ThemeType.Container, Path.Combine(portalSettings.HomeSystemDirectoryMapPath, SkinController.RootContainer)));
        }

        if ((level & ThemeLevel.Global) == ThemeLevel.Global)
        {
            themes.AddRange(this.GetThemes(ThemeType.Container, Path.Combine(Globals.HostMapPath, SkinController.RootContainer)));
        }

        return themes;
    }

    /// <inheritdoc />
    public IList<ThemeFileInfo> GetThemeFiles(PortalSettings portalSettings, ThemeInfo theme)
    {
        var themePath = Path.Combine(this.appStatus.ApplicationMapPath, theme.Path);
        var themeFiles = new List<ThemeFileInfo>();

        if (Directory.Exists(themePath))
        {
            var fallbackSkin = theme.Type == ThemeType.Skin ? this.IsFallbackSkin(themePath) : this.IsFallbackContainer(themePath);

            var strSkinType = themePath.IndexOf(Globals.HostMapPath, StringComparison.OrdinalIgnoreCase) != -1 ? "G" : "L";

            var canDeleteSkin = SkinController.CanDeleteSkin(this.hostSettings, themePath, portalSettings.HomeDirectoryMapPath);
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
                    themeFile.Thumbnail = this.CreateThumbnail(imagePath);
                }

                themeFile.Name = Path.GetFileNameWithoutExtension(file);
                themeFile.Path = FormatThemePath(portalSettings, themePath, Path.GetFileName(strFile), theme.Type);
                themeFile.CanDelete = (UserController.Instance.GetCurrentUserInfo().IsSuperUser || strSkinType == "L") && !fallbackSkin && canDeleteSkin;

                themeFiles.Add(themeFile);
            }
        }

        return themeFiles;
    }

    /// <inheritdoc />
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

    /// <inheritdoc />
    public void ApplyTheme(int portalId, ThemeFileInfo themeFile, ApplyThemeScope scope)
    {
        var skinPath = $"{themeFile.Path}.ascx";

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

    /// <inheritdoc />
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

    /// <inheritdoc />
    public void DeleteTheme(PortalSettings portalSettings, ThemeFileInfo themeFile)
    {
        var themePath = SkinController.FormatSkinSrc(themeFile.Path, portalSettings);
        var user = UserController.Instance.GetCurrentUserInfo();

        if (!user.IsSuperUser && themePath.IndexOf("\\portals\\_default\\", StringComparison.OrdinalIgnoreCase) != Null.NullInteger)
        {
            throw new SecurityException("NoPermission");
        }

        File.Delete(Path.Combine(this.appStatus.ApplicationMapPath, themePath));
        DataCache.ClearPortalCache(portalSettings.PortalId, true);
    }

    /// <inheritdoc />
    public void DeleteThemePackage(PortalSettings portalSettings, ThemeInfo theme)
    {
        var themePath = Path.Combine(this.appStatus.ApplicationMapPath, theme.Path);
        var user = UserController.Instance.GetCurrentUserInfo();

        if (!user.IsSuperUser && themePath.IndexOf(@"\portals\_default\", StringComparison.OrdinalIgnoreCase) != Null.NullInteger)
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

            if (Directory.Exists(themePath.Replace($@"\{SkinController.RootSkin.ToLowerInvariant()}\", $@"\{SkinController.RootContainer}\")))
            {
                Globals.DeleteFolderRecursive(themePath.Replace($@"\{SkinController.RootSkin.ToLowerInvariant()}\", $@"\{SkinController.RootContainer}\"));
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

    /// <inheritdoc />
    public void UpdateTheme(PortalSettings portalSettings, UpdateThemeInfo updateTheme)
    {
        var themePath = SkinController.FormatSkinSrc(updateTheme.Path + ".ascx", portalSettings);
        themePath = Path.Combine(this.appStatus.ApplicationMapPath, themePath.TrimStart('/'));

        var objStreamReader = File.OpenText(themePath);
        var strSkin = objStreamReader.ReadToEnd();
        objStreamReader.Close();
        var strTag = $"<dnn:{updateTheme.Token} runat=\"server\" id=\"dnn{updateTheme.Token}\"";
        var intOpenTag = strSkin.IndexOf(strTag, StringComparison.OrdinalIgnoreCase);
        if (intOpenTag == -1)
        {
            return;
        }

        var intCloseTag = strSkin.IndexOf(" />", intOpenTag, StringComparison.Ordinal);
        var strAttribute = updateTheme.Setting;
        var intStartAttribute = strSkin.IndexOf(strAttribute, intOpenTag, StringComparison.OrdinalIgnoreCase);
        var strValue = updateTheme.Value;
        if (intStartAttribute != -1 && intStartAttribute < intCloseTag)
        {
            // remove attribute
            var intEndAttribute = strSkin.IndexOf("\" ", intStartAttribute, StringComparison.Ordinal);
            strSkin = strSkin.Substring(0, intStartAttribute) + strSkin.Substring(intEndAttribute + 2);
        }

        // add attribute
        strAttribute = HttpUtility.HtmlAttributeEncode(strAttribute);
        strValue = HttpUtility.HtmlAttributeEncode(strValue);
        strSkin = strSkin.Insert(intOpenTag + strTag.Length, $" {strAttribute}=\"{strValue}\"");

        File.SetAttributes(themePath, FileAttributes.Normal);
        var objStream = File.CreateText(themePath);
        objStream.WriteLine(strSkin);
        objStream.Close();

        UpdateManifest(portalSettings, updateTheme);
    }

    /// <inheritdoc />
    public void ParseTheme(PortalSettings portalSettings, ThemeInfo theme, ParseType parseType)
    {
        var strRootPath = theme.Level switch
        {
            ThemeLevel.Global => // global
                Globals.HostMapPath,
            ThemeLevel.Site => // local
                portalSettings.HomeDirectoryMapPath,
            _ => Null.NullString,
        };

        var objSkinFiles = new SkinFileProcessor(strRootPath, theme.Type == ThemeType.Container ? SkinController.RootContainer : SkinController.RootSkin, theme.PackageName);
        var arrSkinFiles = new ArrayList();

        var strFolder = Path.Combine(this.appStatus.ApplicationMapPath, theme.Path);
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
                        if (strFile.IndexOf(Globals.glbAboutPage, StringComparison.CurrentCultureIgnoreCase) < 0)
                        {
                            arrSkinFiles.Add(strFile);
                        }

                        break;
                    case ".ascx":
                        if (!File.Exists(strFile.Replace(".ascx", ".htm")) && !File.Exists(strFile.Replace(".ascx", ".html")))
                        {
                            arrSkinFiles.Add(strFile);
                        }

                        break;
                }
            }
        }

        switch (parseType)
        {
            case ParseType.Localized: // localized
                objSkinFiles.ProcessList(arrSkinFiles, SkinParser.Localized);
                break;
            case ParseType.Portable: // portable
                objSkinFiles.ProcessList(arrSkinFiles, SkinParser.Portable);
                break;
        }
    }

    /// <summary>Gets the <see cref="ThemeLevel"/> for the given <paramref name="themeFilePath"/>.</summary>
    /// <param name="themeFilePath">The file path to the theme.</param>
    /// <returns>The <see cref="ThemeLevel"/> value.</returns>
    internal static ThemeLevel GetThemeLevel(string themeFilePath)
    {
        themeFilePath = themeFilePath.Replace(@"\", "/");
        if (!string.IsNullOrEmpty(Globals.ApplicationPath)
            && !themeFilePath.StartsWith("[", StringComparison.Ordinal)
            && !themeFilePath.StartsWith(Globals.ApplicationPath, StringComparison.InvariantCultureIgnoreCase))
        {
            var needSlash = !Globals.ApplicationPath.EndsWith("/", StringComparison.Ordinal) && !themeFilePath.StartsWith("/", StringComparison.Ordinal);
            themeFilePath = $"{Globals.ApplicationPath}{(needSlash ? "/" : string.Empty)}{themeFilePath}";
        }

        if (themeFilePath.IndexOf(Globals.HostPath.TrimStart('/'), StringComparison.OrdinalIgnoreCase) > Null.NullInteger
            || themeFilePath.StartsWith("[G]", StringComparison.OrdinalIgnoreCase))
        {
            return ThemeLevel.Global;
        }

        if ((PortalSettings.Current != null &&
             themeFilePath.IndexOf(PortalSettings.Current.HomeSystemDirectory.TrimStart('/'), StringComparison.OrdinalIgnoreCase) > Null.NullInteger)
            || themeFilePath.StartsWith("[S]", StringComparison.OrdinalIgnoreCase))
        {
            return ThemeLevel.SiteSystem;
        }

        return ThemeLevel.Site;
    }

    /// <summary>Creates a thumbnail version of the theme image.</summary>
    /// <param name="strImage">The path to the theme image.</param>
    /// <returns>The path to the thumbnail image.</returns>
    internal string CreateThumbnail(string strImage)
    {
        var imageFileName = Path.GetFileName(strImage);
        if (string.IsNullOrEmpty(imageFileName) || imageFileName.StartsWith("thumbnail_", StringComparison.OrdinalIgnoreCase))
        {
            strImage = strImage.Substring(this.appStatus.ApplicationMapPath.Length);
            strImage = strImage.Replace(@"\", "/");
            return strImage;
        }

        var strThumbnail = strImage.Replace(Path.GetFileName(strImage), "thumbnail_" + imageFileName);

        if (NeedCreateThumbnail(strThumbnail, strImage))
        {
            lock (ThreadLocker)
            {
                if (NeedCreateThumbnail(strThumbnail, strImage))
                {
                    const int intSize = 150; // size of the thumbnail
                    try
                    {
                        var objImage = Image.FromFile(strImage);

                        // scale the image to prevent distortion
                        int intHeight;
                        int intWidth;
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
                        // problem creating thumbnail
                        Logger.Error(ex);
                    }
                }
            }
        }

        strThumbnail = strThumbnail.Substring(this.appStatus.ApplicationMapPath.Length);
        strThumbnail = strThumbnail.Replace(@"\", "/");

        // return thumbnail filename
        return strThumbnail;
    }

    /// <inheritdoc/>
    protected override Func<IThemesController> GetFactory()
    {
        return () => new ThemesController();
    }

    private static bool NeedCreateThumbnail(string thumbnailPath, string imagePath)
    {
        return !File.Exists(thumbnailPath) || File.GetLastWriteTime(thumbnailPath) != File.GetLastWriteTime(imagePath);
    }

    private static string FormatThemePath(PortalSettings portalSettings, string themePath, string fileName, ThemeType type)
    {
        var filePath = Path.Combine(themePath, fileName);
        var lowercasePath = filePath.ToLowerInvariant();
        var strRootSkin = type == ThemeType.Skin ? SkinController.RootSkin : SkinController.RootContainer;

        var strSkinType = themePath.IndexOf(Globals.HostMapPath, StringComparison.OrdinalIgnoreCase) != -1 ? "G" : "L";
        if (themePath.IndexOf(portalSettings.HomeSystemDirectoryMapPath, StringComparison.OrdinalIgnoreCase) > -1)
        {
            strSkinType = "S";
        }

        var strUrl = lowercasePath.Substring(filePath.IndexOf($@"\{strRootSkin}\", StringComparison.OrdinalIgnoreCase))
            .Replace(".ascx", string.Empty)
            .Replace(@"\", "/")
            .TrimStart('/');

        return $"[{strSkinType}]{strUrl}";
    }

    private static void UpdateManifest(PortalSettings portalSettings, UpdateThemeInfo updateTheme)
    {
        var themePath = SkinController.FormatSkinSrc(updateTheme.Path, portalSettings);
        if (!File.Exists(themePath.Replace(".ascx", ".htm")))
        {
            return;
        }

        var strFile = themePath.Replace(".ascx", ".xml");
        if (!File.Exists(strFile))
        {
            strFile = strFile.Replace(Path.GetFileName(strFile), "skin.xml");
        }

        var xmlDoc = new XmlDocument { XmlResolver = null };
        try
        {
            using var manifestReader = XmlReader.Create(strFile, new XmlReaderSettings { XmlResolver = null, });
            xmlDoc.Load(manifestReader);
        }
        catch
        {
            using var objectsReader = XmlReader.Create(new StringReader("<Objects></Objects>"), new XmlReaderSettings { XmlResolver = null, });
            xmlDoc.Load(objectsReader);
        }

        var xmlToken = xmlDoc.DocumentElement?.CreateNavigator().SelectSingleNode(XmlUtils.CreateXPathExpression("descendant::Object[Token='[$token]']", new KeyValuePair<string, object>("token", updateTheme.Token)));
        if (xmlToken == null)
        {
            // add token
            string strToken = $"<Token>[{updateTheme.Token}]</Token><Settings></Settings>";
            xmlToken = xmlDoc.CreateElement("Object").CreateNavigator();
            xmlToken.InnerXml = strToken;
            xmlDoc.SelectSingleNode("Objects")?.CreateNavigator().AppendChild(xmlToken);
            xmlToken = xmlDoc.DocumentElement?.CreateNavigator().SelectSingleNode(XmlUtils.CreateXPathExpression("descendant::Object[Token='[$token]']", new KeyValuePair<string, object>("token", updateTheme.Token)));
        }

        var strValue = updateTheme.Value;

        var blnUpdate = false;
        var settings = xmlToken?.Select(".//Settings/Setting");
        if (settings is not null)
        {
            foreach (XmlNode xmlSetting in settings)
            {
                if (xmlSetting.SelectSingleNode("Name")?.InnerText == updateTheme.Setting)
                {
                    xmlSetting.SelectSingleNode("Value")?.InnerText = strValue;
                    blnUpdate = true;
                }
            }
        }

        if (!blnUpdate)
        {
            var strSetting = $"<Name>{updateTheme.Setting}</Name><Value>{strValue}</Value>";
            var xmlSetting = xmlDoc.CreateElement("Setting");
            xmlSetting.InnerXml = strSetting;
            xmlToken.SelectSingleNode("Settings").AppendChild(xmlSetting.CreateNavigator());
        }

        try
        {
            if (File.Exists(strFile))
            {
                File.SetAttributes(strFile, FileAttributes.Normal);
            }

            var objStream = File.CreateText(strFile);
            objStream.WriteLine(xmlDoc.InnerXml.Replace("><", ">" + Environment.NewLine + "<"));
            objStream.Close();
        }
        catch (Exception ex)
        {
            Logger.Error(ex);
        }
    }

    private List<ThemeInfo> GetThemes(ThemeType type, string strRoot)
    {
        if (!Directory.Exists(strRoot))
        {
            return [];
        }

        return (
            from strFolder in Directory.GetDirectories(strRoot)
            let strName = strFolder.Substring(strFolder.LastIndexOf(@"\", StringComparison.Ordinal) + 1)
            where strName != "_default"
            let themePath = strFolder.Replace(this.appStatus.ApplicationMapPath, string.Empty).TrimStart('\\').ToLowerInvariant()
            let isFallback = type == ThemeType.Skin ? this.IsFallbackSkin(themePath) : this.IsFallbackContainer(themePath)
            let canDelete = !isFallback && SkinController.CanDeleteSkin(this.hostSettings, strFolder, PortalSettings.Current.HomeDirectoryMapPath)
            let defaultThemeFile = this.GetDefaultThemeFileName(themePath, type)
            select new ThemeInfo
            {
                PackageName = strName,
                Type = type,
                Path = themePath,
                DefaultThemeFile = FormatThemePath(PortalSettings.Current, strFolder, defaultThemeFile, type),
                Thumbnail = this.GetThumbnail(themePath, defaultThemeFile),
                CanDelete = canDelete,
            }).ToList();
    }

    private string GetDefaultThemeFileName(string themePath, ThemeType type)
    {
        var themeFiles = new List<string>();
        var folderPath = Path.Combine(this.appStatus.ApplicationMapPath, themePath);
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

    private string GetThumbnail(string themePath, string themeFileName)
    {
        var folderPath = Path.Combine(this.appStatus.ApplicationMapPath, themePath);
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

        return !string.IsNullOrEmpty(imagePath) ? this.CreateThumbnail(imagePath) : string.Empty;
    }

    private bool IsFallbackContainer(string skinPath)
    {
        var strDefaultContainerPath = (Globals.HostMapPath + SkinController.RootContainer + SkinDefaults.GetSkinDefaults(this.hostSettings, SkinDefaultType.SkinInfo).Folder).Replace("/", @"\");
        if (strDefaultContainerPath.EndsWith(@"\", StringComparison.Ordinal))
        {
            strDefaultContainerPath = strDefaultContainerPath.Substring(0, strDefaultContainerPath.Length - 1);
        }

        return skinPath.IndexOf(strDefaultContainerPath, StringComparison.CurrentCultureIgnoreCase) != -1;
    }

    private bool IsFallbackSkin(string skinPath)
    {
        var strDefaultSkinPath = (Globals.HostMapPath + SkinController.RootSkin + SkinDefaults.GetSkinDefaults(this.hostSettings, SkinDefaultType.SkinInfo).Folder).Replace("/", @"\");
        if (strDefaultSkinPath.EndsWith(@"\", StringComparison.Ordinal))
        {
            strDefaultSkinPath = strDefaultSkinPath.Substring(0, strDefaultSkinPath.Length - 1);
        }

        return skinPath.Equals(strDefaultSkinPath, StringComparison.OrdinalIgnoreCase);
    }
}
