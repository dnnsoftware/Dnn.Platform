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
                    var installer = GetInstaller(stream, fileName);

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
                    }
                    else
                    {
                        parseResult.Failed("InvalidFile");
                    }

                    DeleteTempInstallFiles(installer);
                }
                catch (ICSharpCode.SharpZipLib.ZipException)
                {
                    parseResult.Failed("ZipCriticalError");
                }
            }

            return parseResult;
        }

        public InstallResultDto InstallPackage(PortalSettings portalSettings, UserInfo user, string filePath, Stream stream)
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
                    var installer = GetInstaller(stream, fileName);

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

                        var logs = installer.InstallerInfo.Log.Logs.Select(l => l.ToString()).ToList();
                        if (!installer.IsValid)
                        {
                            installResult.Failed("InstallError", logs);
                        }
                        else
                        {
                            installResult.NewPackageId = installer.Packages.First().Value.Package.PackageID;
                            installResult.Succeed(logs);
                            DeleteInstallFile(filePath);
                        }
                    }
                    else
                    {
                        installResult.Failed("InstallError");
                    }

                    DeleteTempInstallFiles(installer);
                }
                catch (ICSharpCode.SharpZipLib.ZipException)
                {
                    installResult.Failed("ZipCriticalError");
                }
            }

            return installResult;
        }

        public Installer GetInstaller(Stream stream, string fileName)
        {
            var installer = new Installer(stream, Globals.ApplicationMapPath, true, false);
            if (string.IsNullOrEmpty(installer.InstallerInfo.ManifestFile.TempFileName))
            {
                CreateManifest(installer, fileName);
            }

            return installer;
        }

        private void CreateManifest(Installer installer, string fileName)
        {
            var manifestFile = Path.Combine(installer.TempInstallFolder, Path.GetFileNameWithoutExtension(fileName) + ".dnn");
            StreamWriter manifestWriter = new StreamWriter(manifestFile);
            manifestWriter.Write(LegacyUtil.CreateSkinManifest(fileName, "Skin", installer.TempInstallFolder));
            manifestWriter.Close();
        }

        private static void DeleteTempInstallFiles(Installer installer)
        {
            try
            {
                var tempFolder = installer.TempInstallFolder;
                if (!String.IsNullOrEmpty(tempFolder) && Directory.Exists(tempFolder))
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

        private bool? AzureCompact(Installer installer)
        {
            bool? compact = null;
            string manifestFile = null;
            if (installer.InstallerInfo.ManifestFile != null)
            {
                manifestFile = installer.InstallerInfo.ManifestFile.TempFileName;
            }
            if (installer.Packages.Count > 0)
            {
                if (installer.Packages[0].Package.PackageType.Equals("CoreLanguagePack", StringComparison.InvariantCultureIgnoreCase)
                        || installer.Packages[0].Package.PackageType.Equals("ExtensionLanguagePack", StringComparison.InvariantCultureIgnoreCase))
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
                    var document = new XmlDocument();
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

        private bool IsAzureDatabase()
        {
            return PetaPocoHelper.ExecuteScalar<int>(DataProvider.Instance().ConnectionString, CommandType.Text, "SELECT CAST(ServerProperty('EngineEdition') as INT)") == 5;
        }
    }
}