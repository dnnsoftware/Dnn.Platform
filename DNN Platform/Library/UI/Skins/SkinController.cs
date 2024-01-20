// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.UI.Skins
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.IO;
    using System.IO.Compression;
    using System.Text.RegularExpressions;

    using DotNetNuke.Abstractions.Portals;
    using DotNetNuke.Abstractions.Skins;
    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Data;
    using DotNetNuke.Entities.Controllers;
    using DotNetNuke.Entities.Host;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Instrumentation;
    using DotNetNuke.Internal.SourceGenerators;
    using DotNetNuke.Services.Localization;
    using DotNetNuke.Services.Log.EventLog;

    /// Project  : DotNetNuke
    /// Class    : SkinController
    ///
    /// <summary>    Handles the Business Control Layer for Skins.</summary>
    public partial class SkinController : ISkinService
    {
        private const string GlobalSkinPrefix = "[G]";
        private const string PortalSystemSkinPrefix = "[S]";
        private const string PortalSkinPrefix = "[L]";
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(SkinController));
        private static readonly Regex GdirRegex = new Regex("\\[g]", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        private static readonly Regex SdirRegex = new Regex("\\[s]", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        private static readonly Regex LdirRegex = new Regex("\\[l]", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        /// <inheritdoc cref="ISkinService.RootSkin" />
        [Obsolete($"Deprecated in DotNetNuke 9.13.1. Use {nameof(SkinType)}.{nameof(ISkinService.GetFolderName)} instead. Scheduled removal in v12.0.0.")]
        public static string RootSkin
        {
            get
            {
                return "Skins";
            }
        }

        /// <inheritdoc cref="ISkinService.RootContainer" />
        [Obsolete($"Deprecated in DotNetNuke 9.13.1. Use {nameof(SkinType)}.{nameof(ISkinService.GetFolderName)} instead. Scheduled removal in v12.0.0.")]
        public static string RootContainer
        {
            get
            {
                return "Containers";
            }
        }

        /// <inheritdoc cref="ISkinService.AddSkin" />
        [DnnDeprecated(9, 13, 1, $"Use {nameof(ISkinService)}.{nameof(ISkinService.AddSkin)} instead.")]
        public static partial int AddSkin(int skinPackageID, string skinSrc)
        {
            return DataProvider.Instance().AddSkin(skinPackageID, skinSrc);
        }

        /// <inheritdoc cref="ISkinService.AddSkinPackage" />
        [DnnDeprecated(9, 13, 1, $"Use {nameof(ISkinService)}.{nameof(ISkinService.AddSkinPackage)} instead.")]
        public static partial int AddSkinPackage(SkinPackageInfo skinPackage)
        {
            return AddSkinPackage((ISkinPackageInfo)skinPackage);
        }

        /// <inheritdoc cref="ISkinService.CanDeleteSkinFolder" />
        [DnnDeprecated(9, 13, 1, $"Use {nameof(ISkinService)}.{nameof(ISkinService.CanDeleteSkinFolder)} instead.")]
        public static partial bool CanDeleteSkin(string folderPath, string portalHomeDirMapPath)
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
            else
            {
                // to be compliant with all versions
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

        /// <inheritdoc cref="ISkinService.DeleteSkin" />
        [DnnDeprecated(9, 13, 1, $"Use {nameof(ISkinService)}.{nameof(ISkinService.DeleteSkin)} instead.")]
        public static partial void DeleteSkin(int skinID)
        {
            DataProvider.Instance().DeleteSkin(skinID);
        }

        /// <inheritdoc cref="ISkinService.DeleteSkinPackage" />
        [DnnDeprecated(9, 13, 1, $"Use {nameof(ISkinService)}.{nameof(ISkinService.DeleteSkinPackage)} instead.")]
        public static partial void DeleteSkinPackage(SkinPackageInfo skinPackage)
        {
            DeleteSkinPackage((ISkinPackageInfo)skinPackage);
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

        /// <inheritdoc cref="ISkinService.FormatSkinPath" />
        [DnnDeprecated(9, 13, 1, $"Use {nameof(ISkinService)}.{nameof(ISkinService.FormatSkinPath)} instead.")]
        public static partial string FormatSkinPath(string skinSrc)
        {
            string strSkinSrc = skinSrc;
            if (!string.IsNullOrEmpty(strSkinSrc))
            {
                strSkinSrc = strSkinSrc.Substring(0, strSkinSrc.LastIndexOf("/") + 1);
            }

            return strSkinSrc;
        }

        /// <inheritdoc cref="ISkinService.FormatSkinSrc" />
        [DnnDeprecated(9, 13, 1, $"Use {nameof(ISkinService)}.{nameof(ISkinService.FormatSkinSrc)} instead.")]
        public static partial string FormatSkinSrc(string skinSrc, PortalSettings portalSettings)
        {
            return FormatSkinSrc(skinSrc, (IPortalSettings)portalSettings);
        }

        /// <summary>Gets the global default admin container.</summary>
        /// <remarks>
        /// This is not the default admin container for the portal.
        /// To get the default admin container for the portal use <see cref="IPortalSettings.DefaultAdminContainer"/> instead.
        /// </remarks>
        /// <returns>The global default admin container.</returns>
        [DnnDeprecated(9, 13, 1, $"Use {nameof(ISkinService)}.{nameof(ISkinService.GetDefaultSkinSrc)} instead.")]
        public static partial string GetDefaultAdminContainer()
        {
            SkinDefaults defaultContainer = SkinDefaults.GetSkinDefaults(SkinDefaultType.ContainerInfo);
            return "[G]" + RootContainer + defaultContainer.Folder + defaultContainer.AdminDefaultName;
        }

        /// <summary>Gets the global default admin skin.</summary>
        /// <remarks>
        /// This is not the default admin skin for the portal.
        /// To get the default admin skin for the portal use <see cref="IPortalSettings.DefaultAdminSkin"/> instead.
        /// </remarks>
        /// <returns>The global default admin skin.</returns>
        [DnnDeprecated(9, 13, 1, $"Use {nameof(ISkinService)}.{nameof(ISkinService.GetDefaultSkinSrc)} instead.")]
        public static partial string GetDefaultAdminSkin()
        {
            SkinDefaults defaultSkin = SkinDefaults.GetSkinDefaults(SkinDefaultType.SkinInfo);
            return "[G]" + RootSkin + defaultSkin.Folder + defaultSkin.AdminDefaultName;
        }

        /// <summary>Gets the global default skin.</summary>
        /// <remarks>
        /// This is not the default skin for the portal.
        /// To get the default skin for the portal use <see cref="IPortalSettings.DefaultPortalSkin"/> instead.
        /// </remarks>
        /// <returns>The global default skin.</returns>
        [DnnDeprecated(9, 13, 1, $"Use {nameof(ISkinService)}.{nameof(ISkinService.GetDefaultSkinSrc)} instead.")]
        public static partial string GetDefaultPortalContainer()
        {
            SkinDefaults defaultContainer = SkinDefaults.GetSkinDefaults(SkinDefaultType.ContainerInfo);
            return "[G]" + RootContainer + defaultContainer.Folder + defaultContainer.DefaultName;
        }

        /// <summary>Gets the global default skin.</summary>
        /// <remarks>
        /// This is not the default skin for the portal.
        /// To get the default skin for the portal use <see cref="IPortalSettings.DefaultPortalSkin"/> instead.
        /// </remarks>
        /// <returns>The global default skin.</returns>
        [DnnDeprecated(9, 13, 1, $"Use {nameof(ISkinService)}.{nameof(ISkinService.GetDefaultSkinSrc)} instead.")]
        public static partial string GetDefaultPortalSkin()
        {
            SkinDefaults defaultSkin = SkinDefaults.GetSkinDefaults(SkinDefaultType.SkinInfo);
            return "[G]" + RootSkin + defaultSkin.Folder + defaultSkin.DefaultName;
        }

        /// <inheritdoc cref="ISkinService.GetSkinByPackageID" />
        [DnnDeprecated(9, 13, 1, $"Use {nameof(ISkinService)}.{nameof(ISkinService.GetSkinPackageById)} instead.")]
        public static partial SkinPackageInfo GetSkinByPackageID(int packageID)
        {
            return CBO.FillObject<SkinPackageInfo>(DataProvider.Instance().GetSkinByPackageID(packageID));
        }

        /// <inheritdoc cref="ISkinService.GetSkinPackage" />]
        [DnnDeprecated(9, 13, 1, $"Use {nameof(ISkinService)}.{nameof(ISkinService.GetSkinPackage)} instead.")]
        public static partial SkinPackageInfo GetSkinPackage(int portalId, string skinName, string skinType)
        {
            return CBO.FillObject<SkinPackageInfo>(DataProvider.Instance().GetSkinPackage(portalId, skinName, skinType));
        }

        /// <inheritdoc cref="ISkinService.GetSkinsInFolder" />
        [DnnDeprecated(9, 13, 1, $"Use {nameof(ISkinService)}.{nameof(ISkinService.GetSkinsInFolder)} instead.")]
        public static partial List<KeyValuePair<string, string>> GetSkins(PortalInfo portalInfo, string skinRoot, SkinScope scope)
        {
            return GetSkins((IPortalInfo)portalInfo, skinRoot, scope);
        }

        /// <inheritdoc cref="ISkinService.GetSkinsInFolder" />
        [DnnDeprecated(9, 13, 1, $"Use {nameof(ISkinService)}.{nameof(ISkinService.GetSkinsInFolder)} instead.")]
        public static partial List<KeyValuePair<string, string>> GetSkins(IPortalInfo portalInfo, string skinRoot, SkinScope scope)
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

        /// <inheritdoc cref="ISkinService.IsGlobalSkin" />
        [DnnDeprecated(9, 13, 1, $"Use {nameof(ISkinService)}.{nameof(ISkinService.IsGlobalSkin)} instead.")]
        public static partial bool IsGlobalSkin(string skinSrc)
        {
            return skinSrc.Contains(Globals.HostPath);
        }

        /// <inheritdoc cref="ISkinService.SetSkin" />
        [DnnDeprecated(9, 13, 1, $"Use {nameof(ISkinService)}.{nameof(ISkinService.SetSkin)} instead.")]
        public static partial void SetSkin(string skinRoot, int portalId, SkinType skinType, string skinSrc)
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
                default:
                    throw new ArgumentOutOfRangeException(nameof(skinRoot), skinRoot, "Expected either 'Skins' or 'Containers'");
            }
        }

        /// <inheritdoc cref="ISkinService.UpdateSkin" />
        [DnnDeprecated(9, 13, 1, $"Use {nameof(ISkinService)}.{nameof(ISkinService.UpdateSkin)} instead.")]
        public static partial void UpdateSkin(int skinID, string skinSrc)
        {
            DataProvider.Instance().UpdateSkin(skinID, skinSrc);
        }

        /// <inheritdoc cref="ISkinService.UpdateSkinPackage" />
        [DnnDeprecated(9, 13, 1, $"Use {nameof(ISkinService)}.{nameof(ISkinService.UpdateSkin)} instead.")]
        public static partial void UpdateSkinPackage(SkinPackageInfo skinPackage)
        {
            UpdateSkinPackage((ISkinPackageInfo)skinPackage);
        }

        [DnnDeprecated(10, 0, 0, "No replacement")]
        public static partial string UploadLegacySkin(string rootPath, string skinRoot, string skinName, Stream inputStream)
        {
            var objZipInputStream = new ZipArchive(inputStream, ZipArchiveMode.Read);

            string strExtension;
            string strFileName;
            FileStream objFileStream;
            var arrData = new byte[2048];
            string strMessage = string.Empty;
            var arrSkinFiles = new ArrayList();

            // Localized Strings
            PortalSettings resourcePortalSettings = Globals.GetPortalSettings();
            string bEGIN_MESSAGE = Localization.GetString("BeginZip", resourcePortalSettings);
            string cREATE_DIR = Localization.GetString("CreateDir", resourcePortalSettings);
            string wRITE_FILE = Localization.GetString("WriteFile", resourcePortalSettings);
            string fILE_ERROR = Localization.GetString("FileError", resourcePortalSettings);
            string eND_MESSAGE = Localization.GetString("EndZip", resourcePortalSettings);
            string fILE_RESTICTED = Localization.GetString("FileRestricted", resourcePortalSettings);

            strMessage += FormatMessage(bEGIN_MESSAGE, skinName, -1, false);

            foreach (var objZipEntry in objZipInputStream.FileEntries())
            {
                // validate file extension
                strExtension = objZipEntry.FullName.Substring(objZipEntry.FullName.LastIndexOf(".") + 1);
                var extraExtensions = new List<string> { ".ASCX", ".HTM", ".HTML", ".CSS", ".SWF", ".RESX", ".XAML", ".JS" };
                if (Host.AllowedExtensionWhitelist.IsAllowedExtension(strExtension, extraExtensions))
                {
                    // process embedded zip files
                    if (objZipEntry.FullName.Equals(RootSkin.ToLowerInvariant() + ".zip", StringComparison.InvariantCultureIgnoreCase))
                    {
                        using (var objMemoryStream = new MemoryStream())
                        {
                            objZipEntry.Open().CopyToStream(objMemoryStream, 25000);
                            objMemoryStream.Seek(0, SeekOrigin.Begin);
                            strMessage += UploadLegacySkin(rootPath, RootSkin, skinName, objMemoryStream);
                        }
                    }
                    else if (objZipEntry.FullName.Equals(RootContainer.ToLowerInvariant() + ".zip", StringComparison.InvariantCultureIgnoreCase))
                    {
                        using (var objMemoryStream = new MemoryStream())
                        {
                            objZipEntry.Open().CopyToStream(objMemoryStream, 25000);
                            objMemoryStream.Seek(0, SeekOrigin.Begin);
                            strMessage += UploadLegacySkin(rootPath, RootContainer, skinName, objMemoryStream);
                        }
                    }
                    else
                    {
                        strFileName = rootPath + skinRoot + "\\" + skinName + "\\" + objZipEntry.FullName;

                        // create the directory if it does not exist
                        if (!Directory.Exists(Path.GetDirectoryName(strFileName)))
                        {
                            strMessage += FormatMessage(cREATE_DIR, Path.GetDirectoryName(strFileName), 2, false);
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
                        strMessage += FormatMessage(wRITE_FILE, Path.GetFileName(strFileName), 2, false);
                        objZipEntry.Open().CopyToStream(objFileStream, 25000);
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
                    strMessage += string.Format(fILE_RESTICTED, objZipEntry.FullName, Host.AllowedExtensionWhitelist.ToStorageString(), ",", ", *.").Replace("2", "true");
                }
            }

            strMessage += FormatMessage(eND_MESSAGE, skinName + ".zip", 1, false);
            objZipInputStream.Dispose();

            // process the list of skin files
            var newSkin = new SkinFileProcessor(rootPath, skinRoot, skinName);
            strMessage += newSkin.ProcessList(arrSkinFiles, SkinParser.Portable);

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

        /// <inheritdoc />
        ISkinPackageInfo ISkinService.GetSkinPackageById(int packageId) => GetSkinByPackageID(packageId);

        /// <inheritdoc />
        ISkinPackageInfo ISkinService.GetSkinPackage(int portalId, string skinName, SkinPackageType packageType)
            => GetSkinPackage(portalId, skinName, SkinUtils.ToDatabaseName(packageType));

        /// <inheritdoc />
        ISkinInfo ISkinService.CreateSkin() => new SkinInfo();

        /// <inheritdoc />
        ISkinPackageInfo ISkinService.CreateSkinPackage() => new SkinPackageInfo();

        /// <inheritdoc />
        int ISkinService.AddSkin(ISkinInfo skin) => AddSkin(skin.SkinPackageId, skin.SkinSrc);

        /// <inheritdoc />
        int ISkinService.AddSkinPackage(ISkinPackageInfo skinPackage) => AddSkinPackage(skinPackage);

        /// <inheritdoc />
        bool ISkinService.CanDeleteSkinFolder(string folderPath, string portalHomeDirMapPath)
            => CanDeleteSkin(folderPath, portalHomeDirMapPath);

        /// <inheritdoc />
        void ISkinService.DeleteSkin(ISkinInfo skin) => DeleteSkin(skin.SkinId);

        /// <inheritdoc />
        void ISkinService.DeleteSkinPackage(ISkinPackageInfo skinPackage) => DeleteSkinPackage(skinPackage);

        /// <inheritdoc />
        string ISkinService.GetFolderName(SkinPackageType packageType)
        {
            return packageType switch
            {
                SkinPackageType.Skin => RootSkin,
                SkinPackageType.Container => RootContainer,
                _ => throw new ArgumentOutOfRangeException(nameof(packageType), packageType, "The skin package type is not supported.")
            };
        }

        /// <inheritdoc />
        string ISkinService.GetDefaultSkinSrc(SkinPackageType packageType, DotNetNuke.Abstractions.Skins.SkinType skinType)
        {
            return (packageType, skinType) switch
            {
                (SkinPackageType.Skin, DotNetNuke.Abstractions.Skins.SkinType.Site) => GetDefaultPortalSkin(),
                (SkinPackageType.Skin, DotNetNuke.Abstractions.Skins.SkinType.Edit) => GetDefaultAdminSkin(),
                (SkinPackageType.Container, DotNetNuke.Abstractions.Skins.SkinType.Site) => GetDefaultPortalContainer(),
                (SkinPackageType.Container, DotNetNuke.Abstractions.Skins.SkinType.Edit) => GetDefaultAdminContainer(),
                _ => throw new ArgumentOutOfRangeException(nameof(packageType), packageType, "The skin package type is not supported.")
            };
        }

        /// <inheritdoc />
        string ISkinService.FormatSkinPath(string skinSrc) => FormatSkinPath(skinSrc);

        /// <inheritdoc />
        string ISkinService.FormatSkinSrc(string skinSrc, IPortalSettings portalSettings)
            => FormatSkinSrc(skinSrc, portalSettings);

        /// <inheritdoc />
        bool ISkinService.IsGlobalSkin(string skinSrc) => IsGlobalSkin(skinSrc);

        /// <inheritdoc />
        void ISkinService.SetSkin(SkinPackageType packageType, int portalId, DotNetNuke.Abstractions.Skins.SkinType skinType, string skinSrc)
        {
            var librarySkinType = skinType switch
            {
                Abstractions.Skins.SkinType.Site => SkinType.Portal,
                Abstractions.Skins.SkinType.Edit => SkinType.Admin,
                _ => throw new ArgumentOutOfRangeException(nameof(skinType), skinType, "The skin type is not supported."),
            };

            var skinRoot = packageType switch
            {
                SkinPackageType.Skin => RootSkin,
                SkinPackageType.Container => RootContainer,
                _ => throw new ArgumentOutOfRangeException(nameof(packageType), packageType, "The skin package type is not supported."),
            };

            SetSkin(skinRoot, portalId, librarySkinType, skinSrc);
        }

        /// <inheritdoc />
        void ISkinService.UpdateSkin(ISkinInfo skin) => UpdateSkin(skin.SkinId, skin.SkinSrc);

        /// <inheritdoc />
        void ISkinService.UpdateSkinPackage(ISkinPackageInfo skinPackage) => UpdateSkinPackage(skinPackage);

        /// <inheritdoc />
        IEnumerable<KeyValuePair<string, string>> ISkinService.GetSkinsInFolder(IPortalInfo portalInfo, DotNetNuke.Abstractions.Skins.SkinType skinType, SkinFolder folder)
        {
            var libraryScope = folder switch
            {
                SkinFolder.All => SkinScope.All,
                SkinFolder.Host => SkinScope.Host,
                SkinFolder.Portal => SkinScope.Site,
                _ => throw new ArgumentOutOfRangeException(nameof(folder), folder, "The skin scope is not supported."),
            };

            var skinRoot = skinType switch
            {
                Abstractions.Skins.SkinType.Site => RootSkin,
                Abstractions.Skins.SkinType.Edit => RootContainer,
                _ => throw new ArgumentOutOfRangeException(nameof(skinType), skinType, "The skin type is not supported."),
            };

            return GetSkins(portalInfo, skinRoot, libraryScope);
        }

        /// <inheritdoc cref="ISkinService.FormatSkinSrc" />
        private static string FormatSkinSrc(string skinSrc, IPortalSettings portalSettings)
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

        /// <inheritdoc cref="ISkinService.DeleteSkinPackage" />
        private static void DeleteSkinPackage(ISkinPackageInfo skinPackage)
        {
            DataProvider.Instance().DeleteSkinPackage(skinPackage.SkinPackageId);
            EventLogController.Instance.AddLog(skinPackage, PortalController.Instance.GetCurrentPortalSettings(), UserController.Instance.GetCurrentUserInfo().UserID, string.Empty, EventLogController.EventLogType.SKINPACKAGE_DELETED);
        }

        /// <inheritdoc cref="ISkinService.AddSkinPackage" />
        private static int AddSkinPackage(ISkinPackageInfo skinPackage)
        {
            EventLogController.Instance.AddLog(skinPackage, PortalController.Instance.GetCurrentPortalSettings(), UserController.Instance.GetCurrentUserInfo().UserID, string.Empty, EventLogController.EventLogType.SKINPACKAGE_CREATED);
            return DataProvider.Instance().AddSkinPackage(skinPackage.PackageId, skinPackage.PortalId, skinPackage.SkinName, SkinUtils.ToDatabaseName(skinPackage.SkinType), UserController.Instance.GetCurrentUserInfo().UserID);
        }

        /// <inheritdoc cref="ISkinService.UpdateSkinPackage" />
        private static void UpdateSkinPackage(ISkinPackageInfo skinPackage)
        {
            DataProvider.Instance().UpdateSkinPackage(
                skinPackage.SkinPackageId,
                skinPackage.PackageId,
                skinPackage.PortalId,
                skinPackage.SkinName,
                SkinUtils.ToDatabaseName(skinPackage.SkinType),
                UserController.Instance.GetCurrentUserInfo().UserID);
            EventLogController.Instance.AddLog(skinPackage, PortalController.Instance.GetCurrentPortalSettings(), UserController.Instance.GetCurrentUserInfo().UserID, string.Empty, EventLogController.EventLogType.SKINPACKAGE_UPDATED);
            foreach (var skin in skinPackage.Skins)
            {
                UpdateSkin(skin.SkinId, skin.SkinSrc);
            }
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

        private static List<KeyValuePair<string, string>> GetPortalSkins(IPortalInfo portalInfo, string skinRoot)
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

        /// <summary>format skin name.</summary>
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
