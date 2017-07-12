using System;
using System.IO;
using static DotNetNuke.Common.Globals;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Services.Exceptions;
using Dnn.PersonaBar.Library.Prompt.Attributes;
using Dnn.PersonaBar.Library.Prompt;
using Dnn.PersonaBar.Library.Prompt.Models;
using System.Text;
using System.Web.UI;

namespace Dnn.PersonaBar.Prompt.Commands.Extensions
{
    [ConsoleCommand("new-extension", "Creates a new extension from a manifest or package", new[] { "path" })]
    public class NewExtension : ConsoleCommandBase
    {
        private const string FlagPath = "path";
        private string _path = "";
        private bool _isPackage;

        private bool _isManifest;
        public override void Init(string[] args, PortalSettings portalSettings, UserInfo userInfo, int activeTabId)
        {
            base.Init(args, portalSettings, userInfo, activeTabId);
            var sbErrors = new StringBuilder();

            if (HasFlag(FlagPath))
            {
                _path = Flag(FlagPath);
            }
            else if (args.Length >= 2 && !IsFlag(args[1]))
            {
                // assume first argument is the module name
                _path = args[1];
            }
            else
            {
                sbErrors.AppendFormat("You must supply the path to the extension package or manifest");
            }
            _path = _path.ToLower().Replace("/", "\\");
            if (_path.EndsWith(".dnn"))
            {
                _isManifest = true;
            }
            else if (_path.EndsWith(".zip"))
            {
                _isPackage = true;
            }
            else
            {
                sbErrors.AppendFormat("You must supply a path to a package (.zip) or a manifest (.dnn)");
            }

            _path = _path.TrimStart("~".ToCharArray()).TrimStart("/".ToCharArray());
            if (_path.StartsWith("desktopmodules"))
            {
                _path = _path.Substring(15);
            }
            _path = Path.Combine(ApplicationMapPath, "desktopmodules/" + _path);
            if (File.Exists(_path))
            {
            }
            else
            {
                sbErrors.AppendFormat("Cannot find {0}", _path);
            }

            ValidationMessage = sbErrors.ToString();
        }

        public override ConsoleResultModel Run()
        {

            var res = "";
            try
            {
                if (_isPackage)
                {
                    res = InstallPackage(PortalSettings, User, _path);
                }
                else if (_isManifest)
                {
                    var installer = new DotNetNuke.Services.Installer.Installer(_path, ApplicationMapPath, true);
                    if (installer.IsValid)
                    {
                        installer.InstallerInfo.Log.Logs.Clear();
                        installer.Install();
                        if (installer.IsValid)
                        {
                            res = string.Format("<strong>Successfully added {0}</strong>", _path);
                            // Return installer.InstallerInfo.PackageID
                        }
                        else
                        {
                            return new ConsoleErrorResultModel("An error occurred while attempting to add the extension. Please see the DNN Event Viewer for details.");
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                Exceptions.LogException(ex);
                return new ConsoleErrorResultModel("An error occurred while attempting to add the extension. Please see the DNN Event Viewer for details.");
            }
            return new ConsoleResultModel(res) { IsHtml = true };
        }

        public string InstallPackage(PortalSettings portalSettings, UserInfo user, string filePath)
        {
            //Dim installResult = New InstallResultDto()
            var fileName = Path.GetFileName(_path);
            string res;
            try
            {
                using (var stream = File.OpenRead(_path))
                {
                    var installer = GetInstaller(stream, fileName, portalSettings.PortalId);

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
                            if (!installer.IsValid)
                            {
                                res = "Install Error";
                            }
                            else
                            {
                                using (var sw = new StringWriter())
                                {
                                    installer.InstallerInfo.Log.GetLogsTable().RenderControl(new HtmlTextWriter(sw));
                                    res = sw.ToString();
                                }
                            }
                        }
                        else
                        {
                            res = "Install Error";
                        }
                    }
                    finally
                    {
                        DeleteTempInstallFiles(installer);
                    }
                }
            }
            catch (Exception ex)
            {
                res = "ZipCriticalError";
            }
            return res;
        }

        private static DotNetNuke.Services.Installer.Installer GetInstaller(Stream stream, string fileName, int portalId, string legacySkin = null)
        {
            var installer = new DotNetNuke.Services.Installer.Installer(stream, ApplicationMapPath, false, false);
            // We always assume we are installing from //Host/Extensions (in the previous releases)
            // This will not work when we try to install a skin/container under a specific portal.
            installer.InstallerInfo.PortalID = Null.NullInteger;
            //Read the manifest
            if (installer.InstallerInfo.ManifestFile != null)
            {
                installer.ReadManifest(true);
            }
            return installer;
        }

        private static void DeleteTempInstallFiles(DotNetNuke.Services.Installer.Installer installer)
        {
            try
            {
                var tempFolder = installer.TempInstallFolder;
                if (!string.IsNullOrEmpty(tempFolder) && Directory.Exists(tempFolder))
                {
                    DeleteFolderRecursive(tempFolder);
                }
            }
            catch (Exception ex)
            {
                //
            }
        }
    }
}