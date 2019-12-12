using System;
using System.Data;
using System.IO;
using System.Linq;
using System.Xml;
using Dnn.PersonaBar.Extensions.Components.Dto;
using DotNetNuke.Common;
using DotNetNuke.Data;
using DotNetNuke.Data.PetaPoco;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;
using DotNetNuke.Framework;
using DotNetNuke.Instrumentation;
using DotNetNuke.Services.Installer;
using DotNetNuke.Common.Utilities;
using ICSharpCode.SharpZipLib;

namespace Dnn.PersonaBar.Extensions.Components
{
    public class InstallController : ServiceLocator<IInstallController, InstallController>, IInstallController
    {
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(InstallController));

        protected override Func<IInstallController> GetFactory()
        {
            return () => new InstallController();
        }

        public ParseResultDto ParsePackage(PortalSettings portalSettings, UserInfo user, string filePath, Stream stream)
        {
            var parseResult = new ParseResultDto();
            var fileName = Path.GetFileName(filePath);
            var extension = Path.GetExtension(fileName ?? "").ToLowerInvariant();

            if (extension != ".zip" && extension != ".resources")
            {
                parseResult.Failed("InvalidExt");
            }
            else
            {
                try
                {
                    var installer = GetInstaller(stream, fileName, portalSettings.PortalId);

                    try
                    {
                        if (installer.IsValid)
                        {
                            if (installer.Packages.Count > 0)
                            {
                                parseResult = new ParseResultDto(installer.Packages[0].Package);
                            }

                            parseResult.AzureCompact = AzureCompact(installer).GetValueOrDefault(false);
                            parseResult.NoManifest = string.IsNullOrEmpty(installer.InstallerInfo.ManifestFile.TempFileName);
                            parseResult.LegacyError = installer.InstallerInfo.LegacyError;
                            parseResult.HasInvalidFiles = !installer.InstallerInfo.HasValidFiles;
                            parseResult.AlreadyInstalled = installer.InstallerInfo.Installed;
                            parseResult.AddLogs(installer.InstallerInfo.Log.Logs);
                        }
                        else
                        {
                            if (installer.InstallerInfo.ManifestFile == null)
                            {
                                parseResult.LegacySkinInstalled = CheckIfSkinAlreadyInstalled(fileName, installer, "Skin");
                                parseResult.LegacyContainerInstalled = CheckIfSkinAlreadyInstalled(fileName, installer, "Container");
                            }

                            parseResult.Failed("InvalidFile", installer.InstallerInfo.Log.Logs);
                            parseResult.NoManifest = string.IsNullOrEmpty(installer.InstallerInfo.ManifestFile?.TempFileName);
                            if (parseResult.NoManifest)
                            {
                                // we still can install when the manifest is missing
                                parseResult.Success = true;
                            }
                        }
                    }
                    finally
                    {
                        DeleteTempInstallFiles(installer);
                    }
                }
                catch (SharpZipBaseException)
                {
                    parseResult.Failed("ZipCriticalError");
                }
            }

            return parseResult;
        }

        public InstallResultDto InstallPackage(PortalSettings portalSettings, UserInfo user, string legacySkin, string filePath, Stream stream, bool isPortalPackage = false)
        {
            var installResult = new InstallResultDto();
            var fileName = Path.GetFileName(filePath);
            var extension = Path.GetExtension(fileName ?? "").ToLowerInvariant();

            if (extension != ".zip" && extension != ".resources")
            {
                installResult.Failed("InvalidExt");
            }
            else
            {
                try
                {
                    var installer = GetInstaller(stream, fileName, portalSettings.PortalId, legacySkin, isPortalPackage);

                    try
                    {
                        if (installer.IsValid)
                        {
                            //Reset Log
                            installer.InstallerInfo.Log.Logs.Clear();

                            //Set the IgnnoreWhiteList flag
                            installer.InstallerInfo.IgnoreWhiteList = true;

                            //Set the Repair flag
                            installer.InstallerInfo.RepairInstall = true;

                            //Install
                            installer.Install();

                            installResult.AddLogs(installer.InstallerInfo.Log.Logs);
                            if (!installer.IsValid)
                            {
                                installResult.Failed("InstallError");
                            }
                            else
                            {
                                installResult.NewPackageId = installer.Packages.Count == 0
                                    ? Null.NullInteger
                                    : installer.Packages.First().Value.Package.PackageID;
                                installResult.Succeed();
                                DeleteInstallFile(filePath);
                            }
                        }
                        else
                        {
                            installResult.Failed("InstallError");
                        }
                    }
                    finally
                    {
                        DeleteTempInstallFiles(installer);
                    }
                }
                catch (SharpZipBaseException)
                {
                    installResult.Failed("ZipCriticalError");
                }
            }

            return installResult;
        }

        private static Installer GetInstaller(Stream stream, string fileName, int portalId, string legacySkin = null, bool isPortalPackage = false)
        {
            var installer = new Installer(stream, Globals.ApplicationMapPath, false, false);
            if (string.IsNullOrEmpty(installer.InstallerInfo.ManifestFile?.TempFileName) && !string.IsNullOrEmpty(legacySkin))
            {
                var manifestFile = CreateManifest(installer, fileName, legacySkin);
                //Re-evaluate the package after creating a temporary manifest
                installer = new Installer(installer.TempInstallFolder, manifestFile, Globals.ApplicationMapPath, false);
            }

            // We always assume we are installing from //Host/Extensions (in the previous releases)
            // This will not work when we try to install a skin/container under a specific portal.
            installer.InstallerInfo.PortalID = isPortalPackage ? portalId: Null.NullInteger;

            //Read the manifest
            if (installer.InstallerInfo.ManifestFile != null)
            {
                installer.ReadManifest(true);
            }
            return installer;
        }

        private static string CreateManifest(Installer installer, string fileName, string legacySkin)
        {
            var manifestFile = Path.Combine(installer.TempInstallFolder, Path.GetFileNameWithoutExtension(fileName) + ".dnn");
            using (var manifestWriter = new StreamWriter(manifestFile))
            {
                manifestWriter.Write(LegacyUtil.CreateSkinManifest(fileName, legacySkin ?? "Skin", installer.TempInstallFolder));
                manifestWriter.Close();
            }
            return manifestFile;
        }

        private static bool CheckIfSkinAlreadyInstalled(string fileName, Installer installer, string legacySkin)
        {
            // this whole thing is to check for if already installed
            var manifestFile = CreateManifest(installer, fileName, legacySkin);
            var installer2 = new Installer(installer.TempInstallFolder, manifestFile, Globals.ApplicationMapPath, false);
            installer2.InstallerInfo.PortalID = installer.InstallerInfo.PortalID;
            if (installer2.InstallerInfo.ManifestFile != null)
            {
                installer2.ReadManifest(true);
            }
            return installer2.IsValid && installer2.InstallerInfo.Installed;
        }

        private static void DeleteTempInstallFiles(Installer installer)
        {
            try
            {
                var tempFolder = installer.TempInstallFolder;
                if (!string.IsNullOrEmpty(tempFolder) && Directory.Exists(tempFolder))
                {
                    Globals.DeleteFolderRecursive(tempFolder);
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }
        }

        private static void DeleteInstallFile(string installerFile)
        {
            try
            {
                if (File.Exists(installerFile))
                {
                        File.SetAttributes(installerFile, FileAttributes.Normal);
                        File.Delete(installerFile);
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }
        }

        private static bool? AzureCompact(Installer installer)
        {
            bool? compact = null;
            string manifestFile = null;
            if (installer.InstallerInfo.ManifestFile != null)
            {
                manifestFile = installer.InstallerInfo.ManifestFile.TempFileName;
            }
            if (installer.Packages.Count > 0)
            {
                if (installer.Packages[0].Package.PackageType.Equals("CoreLanguagePack", StringComparison.OrdinalIgnoreCase)
                        || installer.Packages[0].Package.PackageType.Equals("ExtensionLanguagePack", StringComparison.OrdinalIgnoreCase))
                {
                    compact = true;
                }
            }
            if (!IsAzureDatabase())
            {
                compact = true;
            }
            else if (manifestFile != null && File.Exists(manifestFile))
            {
                try
                {
                    var document = new XmlDocument { XmlResolver = null };
                    document.Load(manifestFile);
                    var compactNode = document.SelectSingleNode("/dotnetnuke/packages/package/azureCompatible");
                    if (compactNode != null && !string.IsNullOrEmpty(compactNode.InnerText))
                    {
                        compact = compactNode.InnerText.ToLowerInvariant() == "true";
                    }
                }
                catch (Exception ex)
                {
                    Logger.Error(ex);
                }

            }

            return compact;
        }

        private static bool IsAzureDatabase()
        {
            return PetaPocoHelper.ExecuteScalar<int>(DataProvider.Instance().ConnectionString, CommandType.Text, "SELECT CAST(ServerProperty('EngineEdition') as INT)") == 5;
        }
    }
}