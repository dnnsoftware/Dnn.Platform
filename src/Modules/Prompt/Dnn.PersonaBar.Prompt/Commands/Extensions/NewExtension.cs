using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using DotNetNuke;
using DotNetNuke.Common;
using static DotNetNuke.Common.Globals;
using DotNetNuke.Services;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Services.Exceptions;
using Dnn.PersonaBar.Prompt.Attributes;
using Dnn.PersonaBar.Prompt.Common;
using Dnn.PersonaBar.Prompt.Interfaces;
using Dnn.PersonaBar.Prompt.Models;
using System.Text;
using System.Web.UI;

namespace Dnn.PersonaBar.Prompt.Commands.Extensions
{
    [ConsoleCommand("new-extension", "Creates a new extension from a manifest or package", new string[]{ "path" })]
public class NewExtension : BaseConsoleCommand, IConsoleCommand
{

    public string ValidationMessage { get; private set; }


    private const string FLAG_PATH = "path";
    private string Path = "";
    private bool IsPackage = false;

    private bool IsManifest = false;
    public void Init(string[] args, PortalSettings portalSettings, UserInfo userInfo, int activeTabId)
    {
        Initialize(args, portalSettings, userInfo, activeTabId);
        StringBuilder sbErrors = new StringBuilder();

        if (HasFlag(FLAG_PATH))
        {
            Path = Flag(FLAG_PATH);
        }
        else if (args.Length >= 2 && !IsFlag(args[1]))
        {
            // assume first argument is the module name
            Path = args[1];
        }
        else
        {
            sbErrors.AppendFormat("You must supply the path to the extension package or manifest");
        }
        Path = Path.ToLower().Replace("/", "\\");
        if (Path.EndsWith(".dnn"))
        {
            IsManifest = true;
        }
        else if (Path.EndsWith(".zip"))
        {
            IsPackage = true;
        }
        else
        {
            sbErrors.AppendFormat("You must supply a path to a package (.zip) or a manifest (.dnn)");
        }

        Path.TrimStart("~".ToCharArray());
        Path.TrimStart("/".ToCharArray());
        if (Path.StartsWith("desktopmodules"))
        {
            Path = Path.Substring(15);
        }
        Path = System.IO.Path.Combine(ApplicationMapPath, "desktopmodules/" + Path);
        if (File.Exists(Path))
        {
        }
        else
        {
            sbErrors.AppendFormat("Cannot find {0}", Path);
        }

        ValidationMessage = sbErrors.ToString();
    }

    public bool IsValid()
    {
        return string.IsNullOrEmpty(ValidationMessage);
    }

    public ConsoleResultModel Run()
    {

        string res = "";
        try
        {
            if (IsPackage)
            {
                res = InstallPackage(PortalSettings, User, Path);
            }
            else if (IsManifest)
            {
                DotNetNuke.Services.Installer.Installer installer = new DotNetNuke.Services.Installer.Installer(Path, ApplicationMapPath, true);
                if (installer.IsValid)
                {
                    installer.InstallerInfo.Log.Logs.Clear();
                    installer.Install();
                    if (installer.IsValid)
                    {
                        res = string.Format("<strong>Successfully added {0}</strong>", Path);
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
        return new ConsoleResultModel(res) { isHtml = true };
    }

    public string InstallPackage(PortalSettings portalSettings, UserInfo user, string filePath)
    {
        //Dim installResult = New InstallResultDto()
        var fileName = System.IO.Path.GetFileName(Path);
        string res = "";
        try
        {
            using (FileStream stream = File.OpenRead(Path))
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
                            using (StringWriter sw = new StringWriter())
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
                Globals.DeleteFolderRecursive(tempFolder);
            }
        }
        catch (Exception ex)
        {
            //
        }
    }
}
}