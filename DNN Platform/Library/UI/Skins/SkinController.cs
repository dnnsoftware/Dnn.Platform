// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.UI.Skins
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Text.RegularExpressions;

    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Data;
    using DotNetNuke.Entities.Controllers;
    using DotNetNuke.Entities.Host;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Instrumentation;
    using DotNetNuke.Services.Localization;
    using DotNetNuke.Services.Log.EventLog;
    using ICSharpCode.SharpZipLib.Zip;

    /// -----------------------------------------------------------------------------
    /// Project  : DotNetNuke
    /// Class    : SkinController
    ///
    /// -----------------------------------------------------------------------------
    /// <summary>
    ///     Handles the Business Control Layer for Skins.
    /// </summary>
    /// <remarks>
    /// </remarks>
    /// -----------------------------------------------------------------------------
    public class SkinController
    {
        private const string GlobalSkinPrefix = "[G]";
        private const string PortalSystemSkinPrefix = "[S]";
        private const string PortalSkinPrefix = "[L]";
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(SkinController));
        private static readonly Regex GdirRegex = new Regex("\\[g]", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        private static readonly Regex SdirRegex = new Regex("\\[s]", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        private static readonly Regex LdirRegex = new Regex("\\[l]", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        public static string RootSkin
        {
            get
            {
                return "Skins";
            }
        }

        public static string RootContainer
        {
            get
            {
                return "Containers";
            }
        }

        public static int AddSkin(int skinPackageID, string skinSrc)
        {
            return DataProvider.Instance().AddSkin(skinPackageID, skinSrc);
        }

        public static int AddSkinPackage(SkinPackageInfo skinPackage)
        {
            EventLogController.Instance.AddLog(skinPackage, PortalController.Instance.GetCurrentPortalSettings(), UserController.Instance.GetCurrentUserInfo().UserID, string.Empty, EventLogController.EventLogType.SKINPACKAGE_CREATED);
            return DataProvider.Instance().AddSkinPackage(skinPackage.PackageID, skinPackage.PortalID, skinPackage.SkinName, skinPackage.SkinType, UserController.Instance.GetCurrentUserInfo().UserID);
        }

        public static bool CanDeleteSkin(string folderPath, string portalHomeDirMapPath)
        {
            string skinType;
            string skinFolder;
            bool canDelete = true;
            if (folderPath.IndexOf(Globals.HostMapPath, StringComparison.InvariantCultureIgnoreCase) != -1)
            {
                skinType = "G";
                skinFolder = folderPath.ToLowerInvariant().Replace(Globals.HostMapPath.ToLowerInvariant(), string.Empty).Replace("\\", "/");
            }
            else if (folderPath.IndexOf(PortalSettings.Current.HomeSystemDirectoryMapPath, StringComparison.InvariantCultureIgnoreCase) != -1)
            {
                skinType = "S";
                skinFolder = folderPath.ToLowerInvariant().Replace(portalHomeDirMapPath.ToLowerInvariant(), string.Empty).Replace("\\", "/");
            }
            else // to be compliant with all versions
            {
                skinType = "L";
                skinFolder = folderPath.ToLowerInvariant().Replace(portalHomeDirMapPath.ToLowerInvariant(), string.Empty).Replace("\\", "/");
            }

            var portalSettings = PortalController.Instance.GetCurrentPortalSettings();

            string skin = "[" + skinType.ToLowerInvariant() + "]" + skinFolder.ToLowerInvariant();
            if (skinFolder.ToLowerInvariant().Contains("skins"))
            {
                if (Host.DefaultAdminSkin.StartsWith(skin, StringComparison.InvariantCultureIgnoreCase) || Host.DefaultPortalSkin.StartsWith(skin, StringComparison.InvariantCultureIgnoreCase) ||
                    portalSettings.DefaultAdminSkin.StartsWith(skin, StringComparison.InvariantCultureIgnoreCase) || portalSettings.DefaultPortalSkin.StartsWith(skin, StringComparison.InvariantCultureIgnoreCase))
                {
                    canDelete = false;
                }
            }
            else
            {
                if (Host.DefaultAdminContainer.StartsWith(skin, StringComparison.InvariantCultureIgnoreCase) || Host.DefaultPortalContainer.StartsWith(skin, StringComparison.InvariantCultureIgnoreCase) ||
                    portalSettings.DefaultAdminContainer.StartsWith(skin, StringComparison.InvariantCultureIgnoreCase) || portalSettings.DefaultPortalContainer.StartsWith(skin, StringComparison.InvariantCultureIgnoreCase))
                {
                    canDelete = false;
                }
            }

            if (canDelete)
            {
                // Check if used for Tabs or Modules
                canDelete = DataProvider.Instance().CanDeleteSkin(skinType, skinFolder);
            }

            return canDelete;
        }

        public static void DeleteSkin(int skinID)
        {
            DataProvider.Instance().DeleteSkin(skinID);
        }

        public static void DeleteSkinPackage(SkinPackageInfo skinPackage)
        {
            DataProvider.Instance().DeleteSkinPackage(skinPackage.SkinPackageID);
            EventLogController.Instance.AddLog(skinPackage, PortalController.Instance.GetCurrentPortalSettings(), UserController.Instance.GetCurrentUserInfo().UserID, string.Empty, EventLogController.EventLogType.SKINPACKAGE_DELETED);
        }

        public static string FormatMessage(string title, string body, int level, bool isError)
        {
            string message = title;
            if (isError)
            {
                message = "<span class=\"NormalRed\">" + title + "</span>";
            }

            switch (level)
            {
                case -1:
                    message = "<hr /><br /><strong>" + message + "</strong>";
                    break;
                case 0:
                    message = "<br /><br /><strong>" + message + "</strong>";
                    break;
                case 1:
                    message = "<br /><strong>" + message + "</strong>";
                    break;
                default:
                    message = "<br /><li>" + message + "</li>";
                    break;
            }

            return message + ": " + body + Environment.NewLine;
        }

        public static string FormatSkinPath(string skinSrc)
        {
            string strSkinSrc = skinSrc;
            if (!string.IsNullOrEmpty(strSkinSrc))
            {
                strSkinSrc = strSkinSrc.Substring(0, strSkinSrc.LastIndexOf("/") + 1);
            }

            return strSkinSrc;
        }

        public static string FormatSkinSrc(string skinSrc, PortalSettings portalSettings)
        {
            string strSkinSrc = skinSrc;
            if (!string.IsNullOrEmpty(strSkinSrc))
            {
                switch (strSkinSrc.Substring(0, 3).ToLowerInvariant())
                {
                    case "[g]":
                        strSkinSrc = GdirRegex.Replace(strSkinSrc, Globals.HostPath);
                        break;
                    case "[s]":
                        strSkinSrc = SdirRegex.Replace(strSkinSrc, portalSettings.HomeSystemDirectory);
                        break;
                    case "[l]": // to be compliant with all versions
                        strSkinSrc = LdirRegex.Replace(strSkinSrc, portalSettings.HomeDirectory);
                        break;
                }
            }

            return strSkinSrc;
        }

        public static string GetDefaultAdminContainer()
        {
            SkinDefaults defaultContainer = SkinDefaults.GetSkinDefaults(SkinDefaultType.ContainerInfo);
            return "[G]" + RootContainer + defaultContainer.Folder + defaultContainer.AdminDefaultName;
        }

        public static string GetDefaultAdminSkin()
        {
            SkinDefaults defaultSkin = SkinDefaults.GetSkinDefaults(SkinDefaultType.SkinInfo);
            return "[G]" + RootSkin + defaultSkin.Folder + defaultSkin.AdminDefaultName;
        }

        public static string GetDefaultPortalContainer()
        {
            SkinDefaults defaultContainer = SkinDefaults.GetSkinDefaults(SkinDefaultType.ContainerInfo);
            return "[G]" + RootContainer + defaultContainer.Folder + defaultContainer.DefaultName;
        }

        public static string GetDefaultPortalSkin()
        {
            SkinDefaults defaultSkin = SkinDefaults.GetSkinDefaults(SkinDefaultType.SkinInfo);
            return "[G]" + RootSkin + defaultSkin.Folder + defaultSkin.DefaultName;
        }

        public static SkinPackageInfo GetSkinByPackageID(int packageID)
        {
            return CBO.FillObject<SkinPackageInfo>(DataProvider.Instance().GetSkinByPackageID(packageID));
        }

        public static SkinPackageInfo GetSkinPackage(int portalId, string skinName, string skinType)
        {
            return CBO.FillObject<SkinPackageInfo>(DataProvider.Instance().GetSkinPackage(portalId, skinName, skinType));
        }

        public static List<KeyValuePair<string, string>> GetSkins(PortalInfo portalInfo, string skinRoot, SkinScope scope)
        {
            var skins = new List<KeyValuePair<string, string>>();
            switch (scope)
            {
                case SkinScope.Host: // load host skins
                    skins = GetHostSkins(skinRoot);
                    break;
                case SkinScope.Site: // load portal skins
                    skins = GetPortalSkins(portalInfo, skinRoot);
                    break;
                case SkinScope.All:
                    skins = GetHostSkins(skinRoot);
                    skins.AddRange(GetPortalSkins(portalInfo, skinRoot));
                    break;
            }

            return skins;
        }

        /// <summary>
        /// Determines if a given skin is defined as a global skin.
        /// </summary>
        /// <param name="skinSrc">This is the app relative path and filename of the skin to be checked.</param>
        /// <returns>True if the skin is located in the HostPath child directories.</returns>
        /// <remarks>This function performs a quick check to detect the type of skin that is
        /// passed as a parameter.  Using this method abstracts knowledge of the actual location
        /// of skins in the file system.
        /// </remarks>
        public static bool IsGlobalSkin(string skinSrc)
        {
            return skinSrc.Contains(Globals.HostPath);
        }

        public static void SetSkin(string skinRoot, int portalId, SkinType skinType, string skinSrc)
        {
            var selectedCultureCode = LocaleController.Instance.GetCurrentLocale(portalId).Code;
            switch (skinRoot)
            {
                case "Skins":
                    if (skinType == SkinType.Admin)
                    {
                        if (portalId == Null.NullInteger)
                        {
                            HostController.Instance.Update("DefaultAdminSkin", skinSrc);
                        }
                        else
                        {
                            PortalController.UpdatePortalSetting(portalId, "DefaultAdminSkin", skinSrc, true, selectedCultureCode);
                        }
                    }
                    else
                    {
                        if (portalId == Null.NullInteger)
                        {
                            HostController.Instance.Update("DefaultPortalSkin", skinSrc);
                        }
                        else
                        {
                            PortalController.UpdatePortalSetting(portalId, "DefaultPortalSkin", skinSrc, true, selectedCultureCode);
                        }
                    }

                    break;
                case "Containers":
                    if (skinType == SkinType.Admin)
                    {
                        if (portalId == Null.NullInteger)
                        {
                            HostController.Instance.Update("DefaultAdminContainer", skinSrc);
                        }
                        else
                        {
                            PortalController.UpdatePortalSetting(portalId, "DefaultAdminContainer", skinSrc, true, selectedCultureCode);
                        }
                    }
                    else
                    {
                        if (portalId == Null.NullInteger)
                        {
                            HostController.Instance.Update("DefaultPortalContainer", skinSrc);
                        }
                        else
                        {
                            PortalController.UpdatePortalSetting(portalId, "DefaultPortalContainer", skinSrc, true, selectedCultureCode);
                        }
                    }

                    break;
            }
        }

        public static void UpdateSkin(int skinID, string skinSrc)
        {
            DataProvider.Instance().UpdateSkin(skinID, skinSrc);
        }

        public static void UpdateSkinPackage(SkinPackageInfo skinPackage)
        {
            DataProvider.Instance().UpdateSkinPackage(
                skinPackage.SkinPackageID,
                skinPackage.PackageID,
                skinPackage.PortalID,
                skinPackage.SkinName,
                skinPackage.SkinType,
                UserController.Instance.GetCurrentUserInfo().UserID);
            EventLogController.Instance.AddLog(skinPackage, PortalController.Instance.GetCurrentPortalSettings(), UserController.Instance.GetCurrentUserInfo().UserID, string.Empty, EventLogController.EventLogType.SKINPACKAGE_UPDATED);
            foreach (KeyValuePair<int, string> kvp in skinPackage.Skins)
            {
                UpdateSkin(kvp.Key, kvp.Value);
            }
        }

        public static string UploadLegacySkin(string rootPath, string skinRoot, string skinName, Stream inputStream)
        {
            var objZipInputStream = new ZipInputStream(inputStream);

            ZipEntry objZipEntry;
            string strExtension;
            string strFileName;
            FileStream objFileStream;
            int intSize = 2048;
            var arrData = new byte[2048];
            string strMessage = string.Empty;
            var arrSkinFiles = new ArrayList();

            // Localized Strings
            PortalSettings ResourcePortalSettings = Globals.GetPortalSettings();
            string BEGIN_MESSAGE = Localization.GetString("BeginZip", ResourcePortalSettings);
            string CREATE_DIR = Localization.GetString("CreateDir", ResourcePortalSettings);
            string WRITE_FILE = Localization.GetString("WriteFile", ResourcePortalSettings);
            string FILE_ERROR = Localization.GetString("FileError", ResourcePortalSettings);
            string END_MESSAGE = Localization.GetString("EndZip", ResourcePortalSettings);
            string FILE_RESTICTED = Localization.GetString("FileRestricted", ResourcePortalSettings);

            strMessage += FormatMessage(BEGIN_MESSAGE, skinName, -1, false);

            objZipEntry = objZipInputStream.GetNextEntry();
            while (objZipEntry != null)
            {
                objZipEntry.CheckZipEntry();
                if (!objZipEntry.IsDirectory)
                {
                    // validate file extension
                    strExtension = objZipEntry.Name.Substring(objZipEntry.Name.LastIndexOf(".") + 1);
                    var extraExtensions = new List<string> { ".ASCX", ".HTM", ".HTML", ".CSS", ".SWF", ".RESX", ".XAML", ".JS" };
                    if (Host.AllowedExtensionWhitelist.IsAllowedExtension(strExtension, extraExtensions))
                    {
                        // process embedded zip files
                        if (objZipEntry.Name.Equals(RootSkin.ToLowerInvariant() + ".zip", StringComparison.InvariantCultureIgnoreCase))
                        {
                            using (var objMemoryStream = new MemoryStream())
                            {
                                intSize = objZipInputStream.Read(arrData, 0, arrData.Length);
                                while (intSize > 0)
                                {
                                    objMemoryStream.Write(arrData, 0, intSize);
                                    intSize = objZipInputStream.Read(arrData, 0, arrData.Length);
                                }

                                objMemoryStream.Seek(0, SeekOrigin.Begin);
                                strMessage += UploadLegacySkin(rootPath, RootSkin, skinName, objMemoryStream);
                            }
                        }
                        else if (objZipEntry.Name.Equals(RootContainer.ToLowerInvariant() + ".zip", StringComparison.InvariantCultureIgnoreCase))
                        {
                            using (var objMemoryStream = new MemoryStream())
                            {
                                intSize = objZipInputStream.Read(arrData, 0, arrData.Length);
                                while (intSize > 0)
                                {
                                    objMemoryStream.Write(arrData, 0, intSize);
                                    intSize = objZipInputStream.Read(arrData, 0, arrData.Length);
                                }

                                objMemoryStream.Seek(0, SeekOrigin.Begin);
                                strMessage += UploadLegacySkin(rootPath, RootContainer, skinName, objMemoryStream);
                            }
                        }
                        else
                        {
                            strFileName = rootPath + skinRoot + "\\" + skinName + "\\" + objZipEntry.Name;

                            // create the directory if it does not exist
                            if (!Directory.Exists(Path.GetDirectoryName(strFileName)))
                            {
                                strMessage += FormatMessage(CREATE_DIR, Path.GetDirectoryName(strFileName), 2, false);
                                Directory.CreateDirectory(Path.GetDirectoryName(strFileName));
                            }

                            // remove the old file
                            if (File.Exists(strFileName))
                            {
                                File.SetAttributes(strFileName, FileAttributes.Normal);
                                File.Delete(strFileName);
                            }

                            // create the new file
                            objFileStream = File.Create(strFileName);

                            // unzip the file
                            strMessage += FormatMessage(WRITE_FILE, Path.GetFileName(strFileName), 2, false);
                            intSize = objZipInputStream.Read(arrData, 0, arrData.Length);
                            while (intSize > 0)
                            {
                                objFileStream.Write(arrData, 0, intSize);
                                intSize = objZipInputStream.Read(arrData, 0, arrData.Length);
                            }

                            objFileStream.Close();

                            // save the skin file
                            switch (Path.GetExtension(strFileName))
                            {
                                case ".htm":
                                case ".html":
                                case ".ascx":
                                case ".css":
                                    if (strFileName.ToLowerInvariant().IndexOf(Globals.glbAboutPage.ToLowerInvariant()) < 0)
                                    {
                                        arrSkinFiles.Add(strFileName);
                                    }

                                    break;
                            }

                            break;
                        }
                    }
                    else
                    {
                        strMessage += string.Format(FILE_RESTICTED, objZipEntry.Name, Host.AllowedExtensionWhitelist.ToStorageString(), ",", ", *.").Replace("2", "true");
                    }
                }

                objZipEntry = objZipInputStream.GetNextEntry();
            }

            strMessage += FormatMessage(END_MESSAGE, skinName + ".zip", 1, false);
            objZipInputStream.Close();

            // process the list of skin files
            var NewSkin = new SkinFileProcessor(rootPath, skinRoot, skinName);
            strMessage += NewSkin.ProcessList(arrSkinFiles, SkinParser.Portable);

            // log installation event
            try
            {
                var log = new LogInfo { LogTypeKey = EventLogController.EventLogType.HOST_ALERT.ToString() };
                log.LogProperties.Add(new LogDetailInfo("Install Skin:", skinName));
                Array arrMessage = strMessage.Split(new[] { "<br />" }, StringSplitOptions.None);
                foreach (string strRow in arrMessage)
                {
                    log.LogProperties.Add(new LogDetailInfo("Info:", HtmlUtils.StripTags(strRow, true)));
                }

                LogController.Instance.AddLog(log);
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
            }

            return strMessage;
        }

        private static void AddSkinFiles(List<KeyValuePair<string, string>> skins, string skinRoot, string skinFolder, string skinPrefix)
        {
            foreach (string skinFile in Directory.GetFiles(skinFolder, "*.ascx"))
            {
                string folder = skinFolder.Substring(skinFolder.LastIndexOf("\\") + 1);

                string key = (skinPrefix == PortalSystemSkinPrefix || skinPrefix == PortalSkinPrefix ? "Site: " : "Host: ")
                                + FormatSkinName(folder, Path.GetFileNameWithoutExtension(skinFile));
                string value = skinPrefix + skinRoot + "/" + folder + "/" + Path.GetFileName(skinFile);
                skins.Add(new KeyValuePair<string, string>(key, value));
            }
        }

        private static List<KeyValuePair<string, string>> GetHostSkins(string skinRoot)
        {
            var skins = new List<KeyValuePair<string, string>>();

            string root = Globals.HostMapPath + skinRoot;
            if (Directory.Exists(root))
            {
                foreach (string skinFolder in Directory.GetDirectories(root))
                {
                    if (!skinFolder.EndsWith(Globals.glbHostSkinFolder))
                    {
                        AddSkinFiles(skins, skinRoot, skinFolder, GlobalSkinPrefix);
                    }
                }
            }

            return skins;
        }

        private static List<KeyValuePair<string, string>> GetPortalSkins(PortalInfo portalInfo, string skinRoot)
        {
            var skins = new List<KeyValuePair<string, string>>();

            if (portalInfo != null)
            {
                ProcessSkinsFolder(skins, portalInfo.HomeSystemDirectoryMapPath + skinRoot, skinRoot, PortalSystemSkinPrefix);
                ProcessSkinsFolder(skins, portalInfo.HomeDirectoryMapPath + skinRoot, skinRoot, PortalSkinPrefix); // to be compliant with all versions
            }

            return skins;
        }

        private static void ProcessSkinsFolder(List<KeyValuePair<string, string>> skins, string skinsFolder, string skinRoot, string skinPrefix)
        {
            if (Directory.Exists(skinsFolder))
            {
                foreach (string skinFolder in Directory.GetDirectories(skinsFolder))
                {
                    AddSkinFiles(skins, skinRoot, skinFolder, skinPrefix);
                }
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// format skin name.
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <param name="skinFolder">The Folder Name.</param>
        /// <param name="skinFile">The File Name without extension.</param>
        private static string FormatSkinName(string skinFolder, string skinFile)
        {
            if (skinFolder.Equals("_default", StringComparison.InvariantCultureIgnoreCase))
            {
                // host folder
                return skinFile;
            }

            // portal folder
            switch (skinFile.ToLowerInvariant())
            {
                case "skin":
                case "container":
                case "default":
                    return skinFolder;
                default:
                    return skinFolder + " - " + skinFile;
            }
        }
    }
}
