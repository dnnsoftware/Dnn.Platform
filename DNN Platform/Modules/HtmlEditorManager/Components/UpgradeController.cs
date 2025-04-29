// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Modules.HtmlEditorManager.Components;

using System;
using System.IO;
using System.Text;
using System.Web.Hosting;
using System.Xml;

using DotNetNuke.Abstractions.Application;
using DotNetNuke.Application;
using DotNetNuke.Common;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Modules.Definitions;
using DotNetNuke.Security;
using DotNetNuke.Services.Installer.Packages;
using DotNetNuke.Services.Localization;
using DotNetNuke.Services.Log.EventLog;
using DotNetNuke.Services.Upgrade;

/// <summary>Class that contains upgrade procedures.</summary>
public class UpgradeController : IUpgradeable
{
    /// <summary>The module folder location.</summary>
    private const string ModuleFolder = "~/DesktopModules/Admin/HtmlEditorManager";

    private readonly IApplicationStatusInfo applicationStatusInfo;

    /// <summary> Initializes a new instance of the <see cref="UpgradeController"/> class.</summary>
    public UpgradeController()
        : this(new ApplicationStatusInfo(new Application()))
    {
    }

    /// <summary> Initializes a new instance of the <see cref="UpgradeController"/> class.</summary>
    /// <param name="applicationStatusInfo">The application status info.</param>
    public UpgradeController(IApplicationStatusInfo applicationStatusInfo)
    {
        this.applicationStatusInfo = applicationStatusInfo;
    }

    /// <summary>Called when a module is upgraded.</summary>
    /// <param name="version">The version.</param>
    /// <returns>Success if all goes well, otherwise, Failed.</returns>
    public string UpgradeModule(string version)
    {
        try
        {
            switch (version)
            {
                case "07.04.00":
                    const string ResourceFile = ModuleFolder + "/App_LocalResources/ProviderConfiguration.ascx.resx";
                    var pageName = Localization.GetString("HTMLEditorPageName", ResourceFile);
                    var pageDescription = Localization.GetString("HTMLEditorPageDescription", ResourceFile);

                    // Create HTML Editor Config Page (or get existing one)
                    var editorPage = Upgrade.AddHostPage(pageName, pageDescription, ModuleFolder + "/images/HtmlEditorManager_Standard_16x16.png", ModuleFolder + "/images/HtmlEditorManager_Standard_32x32.png", false);

                    // Find the RadEditor control and remove it
                    Upgrade.RemoveModule("RadEditor Manager", editorPage.TabName, editorPage.ParentId, false);

                    // Add Module To Page
                    var moduleDefId = this.GetModuleDefinitionId("DotNetNuke.HtmlEditorManager", "Html Editor Management");
                    Upgrade.AddModuleToPage(editorPage, moduleDefId, pageName, ModuleFolder + "/images/HtmlEditorManager_Standard_32x32.png", true);

                    foreach (var item in DesktopModuleController.GetDesktopModules(Null.NullInteger))
                    {
                        var moduleInfo = item.Value;

                        if (moduleInfo.ModuleName == "DotNetNuke.HtmlEditorManager")
                        {
                            moduleInfo.Category = "Host";
                            DesktopModuleController.SaveDesktopModule(moduleInfo, false, false);
                        }
                    }

                    break;
                case "09.01.01":
                    if (this.RadEditorProviderInstalled())
                    {
                        UpdateRadCfgFiles();
                    }

                    if (this.TelerikAssemblyExists())
                    {
                        this.UpdateWebConfigFile();
                    }

                    break;
                case "09.02.00":
                    if (this.TelerikAssemblyExists())
                    {
                        this.UpdateTelerikEncryptionKey("Telerik.Web.UI.DialogParametersEncryptionKey");
                    }

                    break;
                case "09.02.01":
                    if (this.TelerikAssemblyExists())
                    {
                        this.UpdateTelerikEncryptionKey("Telerik.Upload.ConfigurationHashKey");
                    }

                    break;
            }
        }
        catch (Exception ex)
        {
            var xlc = new ExceptionLogController();
            xlc.AddLog(ex);

            return "Failed";
        }

        return "Success";
    }

    private static void UpdateRadCfgFiles()
    {
        var folder = Path.Combine(HostingEnvironment.ApplicationPhysicalPath, @"DesktopModules\Admin\RadEditorProvider\ConfigFile");
        UpdateRadCfgFiles(folder, "*ConfigFile*.xml");
    }

    private static void UpdateRadCfgFiles(string folder, string mask)
    {
        var allowedDocExtensions = "doc|docx|xls|xlsx|ppt|pptx|pdf|txt".Split('|');

        var files = Directory.GetFiles(folder, mask);
        foreach (var fileName in files)
        {
            if (fileName.Contains(".Original.xml"))
            {
                continue;
            }

            try
            {
                var doc = new XmlDocument { XmlResolver = null };
                doc.Load(fileName);
                var root = doc.DocumentElement;
                var docFilters = root?.SelectNodes("/configuration/property[@name='DocumentsFilters']");
                if (docFilters != null)
                {
                    var changed = false;
                    foreach (XmlNode filter in docFilters)
                    {
                        if (filter.InnerText == "*.*")
                        {
                            changed = true;
                            filter.InnerXml = string.Empty;
                            foreach (var extension in allowedDocExtensions)
                            {
                                var node = doc.CreateElement("item");
                                node.InnerText = "*." + extension;
                                filter.AppendChild(node);
                            }
                        }
                    }

                    if (changed)
                    {
                        File.Copy(fileName, fileName + ".bak.resources", true);
                        doc.Save(fileName);
                    }
                }
            }
            catch (Exception ex)
            {
                var xlc = new ExceptionLogController();
                xlc.AddLog(ex);
            }
        }
    }

    private string UpdateWebConfigFile()
    {
        return this.UpdateTelerikEncryptionKey("Telerik.AsyncUpload.ConfigurationEncryptionKey");
    }

    private string UpdateTelerikEncryptionKey(string keyName)
    {
        const string DefaultValue = "MDEyMzQ1Njc4OUFCQ0RFRjAxMjM0NTY3ODlBQkNERUYwMTIzNDU2Nzg5QUJDREVG";

        var strError = string.Empty;
        var currentKey = Config.GetSetting(keyName);
        if (string.IsNullOrEmpty(currentKey) || DefaultValue.Equals(currentKey) || currentKey.Length < 40)
        {
            try
            {
                // open the web.config
                var xmlConfig = Config.Load(this.applicationStatusInfo);

                // save the current config file
                Config.BackupConfig(this.applicationStatusInfo);

                // create a random Telerik encryption key and add it under <appSettings>
                var newKey = PortalSecurity.Instance.CreateKey(32);
                newKey = Convert.ToBase64String(Encoding.ASCII.GetBytes(newKey));
                Config.AddAppSetting(xmlConfig, keyName, newKey);

                // save a copy of the existing web.config
                var backupFolder = string.Concat(Globals.glbConfigFolder, "Backup_", DateTime.Now.ToString("yyyyMMddHHmm"), "\\");
                strError += Config.Save(this.applicationStatusInfo, xmlConfig, backupFolder + "web_.config") + Environment.NewLine;

                // save the web.config
                strError += Config.Save(this.applicationStatusInfo, xmlConfig) + Environment.NewLine;
            }
            catch (Exception ex)
            {
                strError += ex.Message;
            }
        }

        return strError;
    }

    /// <summary>Gets the module definition identifier.</summary>
    /// <param name="moduleName">Name of the module.</param>
    /// <param name="moduleDefinitionName">Name of the module definition.</param>
    /// <returns>The Module Id for the HTML Editor Management module.</returns>
    private int GetModuleDefinitionId(string moduleName, string moduleDefinitionName)
    {
        // get desktop module
        var desktopModule = DesktopModuleController.GetDesktopModuleByModuleName(moduleName, Null.NullInteger);
        if (desktopModule == null)
        {
            return -1;
        }

        // get module definition
        var moduleDefinition = ModuleDefinitionController.GetModuleDefinitionByDefinitionName(
            moduleDefinitionName,
            desktopModule.DesktopModuleID);
        if (moduleDefinition == null)
        {
            return -1;
        }

        return moduleDefinition.ModuleDefID;
    }

    private bool RadEditorProviderInstalled()
    {
        return PackageController.Instance.GetExtensionPackage(Null.NullInteger, p => p.Name.Equals("DotNetNuke.RadEditorProvider")) != null;
    }

    private bool TelerikAssemblyExists()
    {
        return File.Exists(Path.Combine(HostingEnvironment.ApplicationPhysicalPath, "bin\\Telerik.Web.UI.dll"));
    }
}
