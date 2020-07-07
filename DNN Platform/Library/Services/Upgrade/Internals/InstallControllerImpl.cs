// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.Upgrade.Internals
{
    using System;
    using System.Collections.Generic;
    using System.Data.Common;
    using System.Data.SqlClient;
    using System.Globalization;
    using System.Net;
    using System.Threading;
    using System.Web;
    using System.Xml;

    using DotNetNuke.Application;
    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Data;
    using DotNetNuke.Services.FileSystem;
    using DotNetNuke.Services.Installer;
    using DotNetNuke.Services.Upgrade.Internals.InstallConfiguration;

    using Localization = DotNetNuke.Services.Localization.Localization;

    /// -----------------------------------------------------------------------------
    /// <summary>
    ///   The Controller class for Installer.
    /// </summary>
    /// <remarks>
    /// </remarks>
    internal class InstallControllerImpl : IInstallController
    {
        public string InstallerLogName
        {
            get { return "InstallerLog" + DateTime.Now.ToString("yyyyMMdd") + ".resources"; }
        }

        /// <summary>
        /// GetConnectionFromWebConfig - Returns Connection Configuration in web.config file.
        /// </summary>
        /// <returns>ConnectionConfig object. Null if information is not present in the config file.</returns>
        public ConnectionConfig GetConnectionFromWebConfig()
        {
            var connectionConfig = new ConnectionConfig();

            string connection = Config.GetConnectionString();
            foreach (string connectionParam in connection.Split(';'))
            {
                int index = connectionParam.IndexOf("=");
                if (index > 0)
                {
                    string key = connectionParam.Substring(0, index);
                    string value = connectionParam.Substring(index + 1);
                    switch (key.ToLowerInvariant())
                    {
                        case "server":
                        case "data source":
                        case "address":
                        case "addr":
                        case "network address":
                            connectionConfig.Server = value;
                            break;
                        case "database":
                        case "initial catalog":
                            connectionConfig.Database = value;
                            break;
                        case "uid":
                        case "user id":
                        case "user":
                            connectionConfig.User = value;
                            break;
                        case "pwd":
                        case "password":
                            connectionConfig.Password = value;
                            break;
                        case "integrated security":
                            connectionConfig.Integrated = value.ToLowerInvariant() == "true";
                            break;
                        case "attachdbfilename":
                            connectionConfig.File = value.Replace("|DataDirectory|", string.Empty);
                            break;
                    }
                }
            }

            connectionConfig.Qualifier = Config.GetObjectQualifer();
            connectionConfig.RunAsDbowner = Config.GetDataBaseOwner() == "dbo.";
            connectionConfig.UpgradeConnectionString = Config.GetUpgradeConnectionString();

            return connectionConfig;
        }

        /// <summary>
        /// SetInstallConfig - Saves configuration n DotNetNuke.Install.Config.
        /// </summary>
        public void SetInstallConfig(InstallConfig installConfig)
        {
            if (installConfig == null)
            {
                return;
            }

            // Load Template
            var installTemplate = new XmlDocument { XmlResolver = null };
            Upgrade.GetInstallTemplate(installTemplate);
            XmlNode dotnetnukeNode = installTemplate.SelectSingleNode("//dotnetnuke");

            // Set Version
            if (!string.IsNullOrEmpty(installConfig.Version))
            {
                XmlNode versionNode = installTemplate.SelectSingleNode("//dotnetnuke/version");

                if (versionNode == null)
                {
                    versionNode = AppendNewXmlNode(ref installTemplate, ref dotnetnukeNode, "version", installConfig.Version);
                }

                versionNode.InnerText = installConfig.Version;
            }

            // Set installer culture
            if (!string.IsNullOrEmpty(installConfig.InstallCulture))
            {
                XmlNode versionNode = installTemplate.SelectSingleNode("//dotnetnuke/installCulture");

                if (versionNode == null)
                {
                    versionNode = AppendNewXmlNode(ref installTemplate, ref dotnetnukeNode, "installCulture", installConfig.InstallCulture);
                }

                versionNode.InnerText = installConfig.InstallCulture;
            }

            // Set SuperUser
            if (installConfig.SuperUser != null)
            {
                XmlNode superUserNode = installTemplate.SelectSingleNode("//dotnetnuke/superuser");
                if (superUserNode == null)
                {
                    superUserNode = AppendNewXmlNode(ref installTemplate, ref dotnetnukeNode, "superuser", installConfig.Version);
                }
                else
                {
                    superUserNode.RemoveAll();
                }

                AppendNewXmlNode(ref installTemplate, ref superUserNode, "firstname", installConfig.SuperUser.FirstName);
                AppendNewXmlNode(ref installTemplate, ref superUserNode, "lastname", installConfig.SuperUser.LastName);
                AppendNewXmlNode(ref installTemplate, ref superUserNode, "username", installConfig.SuperUser.UserName);
                AppendNewXmlNode(ref installTemplate, ref superUserNode, "password", installConfig.SuperUser.Password);
                AppendNewXmlNode(ref installTemplate, ref superUserNode, "email", installConfig.SuperUser.Email);
                AppendNewXmlNode(ref installTemplate, ref superUserNode, "updatepassword", "false");
            }

            // Set Folder Mappings Settings
            if (!string.IsNullOrEmpty(installConfig.FolderMappingsSettings))
            {
                XmlNode folderMappingsNode = installTemplate.SelectSingleNode("//dotnetnuke/" + FolderMappingsConfigController.Instance.ConfigNode);

                if (folderMappingsNode == null)
                {
                    folderMappingsNode = AppendNewXmlNode(ref installTemplate, ref dotnetnukeNode, FolderMappingsConfigController.Instance.ConfigNode, installConfig.FolderMappingsSettings);
                }

                folderMappingsNode.InnerText = installConfig.FolderMappingsSettings;
            }

            // Set Portals
            if (installConfig.Portals != null && installConfig.Portals.Count > 0)
            {
                XmlNode portalsNode = installTemplate.SelectSingleNode("//dotnetnuke/portals");
                if (portalsNode == null)
                {
                    portalsNode = AppendNewXmlNode(ref installTemplate, ref dotnetnukeNode, "portals", installConfig.Version);
                }
                else
                {
                    portalsNode.RemoveAll();
                }

                foreach (PortalConfig portalConfig in installConfig.Portals)
                {
                    XmlNode portalNode = AppendNewXmlNode(ref installTemplate, ref portalsNode, "portal", null);
                    XmlNode administratorNode = AppendNewXmlNode(ref installTemplate, ref portalNode, "administrator", null);
                    XmlNode portalAliasesNode = AppendNewXmlNode(ref installTemplate, ref portalNode, "portalaliases", null);
                    AppendNewXmlNode(ref installTemplate, ref portalNode, "portalname", portalConfig.PortalName);
                    AppendNewXmlNode(ref installTemplate, ref administratorNode, "firstname", portalConfig.AdminFirstName);
                    AppendNewXmlNode(ref installTemplate, ref administratorNode, "lastname", portalConfig.AdminLastName);
                    AppendNewXmlNode(ref installTemplate, ref administratorNode, "username", portalConfig.AdminUserName);
                    AppendNewXmlNode(ref installTemplate, ref administratorNode, "password", portalConfig.AdminPassword);
                    AppendNewXmlNode(ref installTemplate, ref administratorNode, "email", portalConfig.AdminEmail);
                    AppendNewXmlNode(ref installTemplate, ref portalNode, "description", portalConfig.Description);
                    AppendNewXmlNode(ref installTemplate, ref portalNode, "keywords", portalConfig.Keywords);
                    AppendNewXmlNode(ref installTemplate, ref portalNode, "templatefile", portalConfig.TemplateFileName);
                    AppendNewXmlNode(ref installTemplate, ref portalNode, "ischild", portalConfig.IsChild.ToString().ToLowerInvariant());
                    AppendNewXmlNode(ref installTemplate, ref portalNode, "homedirectory", portalConfig.HomeDirectory);

                    foreach (string portalAliase in portalConfig.PortAliases)
                    {
                        AppendNewXmlNode(ref installTemplate, ref portalAliasesNode, "portalalias", portalAliase);
                    }
                }
            }

            // Set the settings
            if (installConfig.Settings != null && installConfig.Settings.Count > 0)
            {
                XmlNode settingsNode = installTemplate.SelectSingleNode("//dotnetnuke/settings");
                if (settingsNode == null)
                {
                    settingsNode = AppendNewXmlNode(ref installTemplate, ref dotnetnukeNode, "settings", null);
                }

                // DNN-8833: for this node specifically we should append/overwrite existing but not clear all
                // else
                // {
                //    settingsNode.RemoveAll();
                // }
                foreach (HostSettingConfig setting in installConfig.Settings)
                {
                    XmlNode settingNode = AppendNewXmlNode(ref installTemplate, ref settingsNode, setting.Name, setting.Value);
                    if (setting.IsSecure)
                    {
                        XmlAttribute attribute = installTemplate.CreateAttribute("Secure");
                        attribute.Value = "true";
                        settingNode.Attributes.Append(attribute);
                    }
                }
            }

            // Set Connection
            if (installConfig.Connection != null)
            {
                XmlNode connectionNode = installTemplate.SelectSingleNode("//dotnetnuke/connection");
                if (connectionNode == null)
                {
                    connectionNode = AppendNewXmlNode(ref installTemplate, ref dotnetnukeNode, "connection", null);
                }
                else
                {
                    connectionNode.RemoveAll();
                }

                AppendNewXmlNode(ref installTemplate, ref connectionNode, "server", installConfig.Connection.Server);
                AppendNewXmlNode(ref installTemplate, ref connectionNode, "database", installConfig.Connection.Database);
                AppendNewXmlNode(ref installTemplate, ref connectionNode, "file", installConfig.Connection.File);
                AppendNewXmlNode(ref installTemplate, ref connectionNode, "integrated", installConfig.Connection.Integrated.ToString().ToLowerInvariant());
                AppendNewXmlNode(ref installTemplate, ref connectionNode, "user", installConfig.Connection.User);
                AppendNewXmlNode(ref installTemplate, ref connectionNode, "password", installConfig.Connection.Password);
                AppendNewXmlNode(ref installTemplate, ref connectionNode, "runasdbowner", installConfig.Connection.RunAsDbowner.ToString().ToLowerInvariant());
                AppendNewXmlNode(ref installTemplate, ref connectionNode, "qualifier", installConfig.Connection.Qualifier);
                AppendNewXmlNode(ref installTemplate, ref connectionNode, "upgradeconnectionstring", installConfig.Connection.UpgradeConnectionString);
            }

            Upgrade.SetInstallTemplate(installTemplate);
        }

        public void RemoveFromInstallConfig(string xmlNodePath)
        {
            InstallConfig config = this.GetInstallConfig();
            if (config == null)
            {
                return;
            }

            var installTemplate = new XmlDocument { XmlResolver = null };
            Upgrade.GetInstallTemplate(installTemplate);
            XmlNodeList nodes = installTemplate.SelectNodes(xmlNodePath);
            if (nodes != null && nodes.Count > 0 && nodes[0].ParentNode != null)
            {
                nodes[0].ParentNode.RemoveChild(nodes[0]);
            }

            Upgrade.SetInstallTemplate(installTemplate);
        }

        /// <summary>
        /// GetInstallConfig - Returns configuration stored in DotNetNuke.Install.Config.
        /// </summary>
        /// <returns>ConnectionConfig object. Null if information is not present in the config file.</returns>
        public InstallConfig GetInstallConfig()
        {
            var installConfig = new InstallConfig();

            // Load Template
            var installTemplate = new XmlDocument { XmlResolver = null };
            Upgrade.GetInstallTemplate(installTemplate);

            // Parse the root node
            XmlNode rootNode = installTemplate.SelectSingleNode("//dotnetnuke");
            if (rootNode != null)
            {
                installConfig.Version = XmlUtils.GetNodeValue(rootNode.CreateNavigator(), "version");
                installConfig.SupportLocalization = XmlUtils.GetNodeValueBoolean(rootNode.CreateNavigator(), "supportLocalization");
                installConfig.InstallCulture = XmlUtils.GetNodeValue(rootNode.CreateNavigator(), "installCulture") ?? Localization.SystemLocale;
            }

            // Parse the scripts node
            XmlNode scriptsNode = installTemplate.SelectSingleNode("//dotnetnuke/scripts");
            if (scriptsNode != null)
            {
                foreach (XmlNode scriptNode in scriptsNode)
                {
                    if (scriptNode != null)
                    {
                        installConfig.Scripts.Add(scriptNode.InnerText);
                    }
                }
            }

            // Parse the connection node
            XmlNode connectionNode = installTemplate.SelectSingleNode("//dotnetnuke/connection");
            if (connectionNode != null)
            {
                var connectionConfig = new ConnectionConfig();

                // Build connection string from the file
                connectionConfig.Server = XmlUtils.GetNodeValue(connectionNode.CreateNavigator(), "server");
                connectionConfig.Database = XmlUtils.GetNodeValue(connectionNode.CreateNavigator(), "database");
                connectionConfig.File = XmlUtils.GetNodeValue(connectionNode.CreateNavigator(), "file");
                connectionConfig.Integrated = XmlUtils.GetNodeValue(connectionNode.CreateNavigator(), "integrated").ToLowerInvariant() == "true";
                connectionConfig.User = XmlUtils.GetNodeValue(connectionNode.CreateNavigator(), "user");
                connectionConfig.Password = XmlUtils.GetNodeValue(connectionNode.CreateNavigator(), "password");
                connectionConfig.RunAsDbowner = XmlUtils.GetNodeValue(connectionNode.CreateNavigator(), "runasdbowner").ToLowerInvariant() == "true";
                connectionConfig.Qualifier = XmlUtils.GetNodeValue(connectionNode.CreateNavigator(), "qualifier");
                connectionConfig.UpgradeConnectionString = XmlUtils.GetNodeValue(connectionNode.CreateNavigator(), "upgradeconnectionstring");

                installConfig.Connection = connectionConfig;
            }

            // Parse the superuser node
            XmlNode superUserNode = installTemplate.SelectSingleNode("//dotnetnuke/superuser");
            if (superUserNode != null)
            {
                var superUserConfig = new SuperUserConfig();

                superUserConfig.FirstName = XmlUtils.GetNodeValue(superUserNode.CreateNavigator(), "firstname");
                superUserConfig.LastName = XmlUtils.GetNodeValue(superUserNode.CreateNavigator(), "lastname");
                superUserConfig.UserName = XmlUtils.GetNodeValue(superUserNode.CreateNavigator(), "username");
                superUserConfig.Password = XmlUtils.GetNodeValue(superUserNode.CreateNavigator(), "password");
                superUserConfig.Email = XmlUtils.GetNodeValue(superUserNode.CreateNavigator(), "email");
                superUserConfig.Locale = XmlUtils.GetNodeValue(superUserNode.CreateNavigator(), "locale");
                superUserConfig.UpdatePassword = XmlUtils.GetNodeValue(superUserNode.CreateNavigator(), "updatepassword").ToLowerInvariant() == "true";

                installConfig.SuperUser = superUserConfig;
            }

            // Parse the license node
            XmlNode licenseNode = installTemplate.SelectSingleNode("//dotnetnuke/license");
            if (licenseNode != null)
            {
                var licenseConfig = new LicenseConfig();

                licenseConfig.AccountEmail = XmlUtils.GetNodeValue(licenseNode.CreateNavigator(), "accountEmail");
                licenseConfig.InvoiceNumber = XmlUtils.GetNodeValue(licenseNode.CreateNavigator(), "invoiceNumber");
                licenseConfig.WebServer = XmlUtils.GetNodeValue(licenseNode.CreateNavigator(), "webServer");
                licenseConfig.LicenseType = XmlUtils.GetNodeValue(licenseNode.CreateNavigator(), "licenseType");

                if (!string.IsNullOrEmpty(XmlUtils.GetNodeValue(licenseNode.CreateNavigator(), "trial")))
                {
                    licenseConfig.TrialRequest = bool.Parse(XmlUtils.GetNodeValue(licenseNode.CreateNavigator(), "trial"));
                }

                installConfig.License = licenseConfig;
            }

            // Parse the settings node
            XmlNode settingsNode = installTemplate.SelectSingleNode("//dotnetnuke/settings");
            if (settingsNode != null)
            {
                foreach (XmlNode settingNode in settingsNode.ChildNodes)
                {
                    if (settingNode != null && !string.IsNullOrEmpty(settingNode.Name))
                    {
                        bool settingIsSecure = false;
                        if (settingNode.Attributes != null)
                        {
                            XmlAttribute secureAttrib = settingNode.Attributes["Secure"];
                            if (secureAttrib != null)
                            {
                                if (secureAttrib.Value.ToLowerInvariant() == "true")
                                {
                                    settingIsSecure = true;
                                }
                            }
                        }

                        installConfig.Settings.Add(new HostSettingConfig { Name = settingNode.Name, Value = settingNode.InnerText, IsSecure = settingIsSecure });
                    }
                }
            }

            var folderMappingsNode = installTemplate.SelectSingleNode("//dotnetnuke/" + FolderMappingsConfigController.Instance.ConfigNode);
            installConfig.FolderMappingsSettings = (folderMappingsNode != null) ? folderMappingsNode.InnerXml : string.Empty;

            // Parse the portals node
            XmlNodeList portalsNode = installTemplate.SelectNodes("//dotnetnuke/portals/portal");
            if (portalsNode != null)
            {
                foreach (XmlNode portalNode in portalsNode)
                {
                    if (portalNode != null)
                    {
                        var portalConfig = new PortalConfig();
                        portalConfig.PortalName = XmlUtils.GetNodeValue(portalNode.CreateNavigator(), "portalname");

                        XmlNode adminNode = portalNode.SelectSingleNode("administrator");
                        if (adminNode != null)
                        {
                            portalConfig.AdminFirstName = XmlUtils.GetNodeValue(adminNode.CreateNavigator(), "firstname");
                            portalConfig.AdminLastName = XmlUtils.GetNodeValue(adminNode.CreateNavigator(), "lastname");
                            portalConfig.AdminUserName = XmlUtils.GetNodeValue(adminNode.CreateNavigator(), "username");
                            portalConfig.AdminPassword = XmlUtils.GetNodeValue(adminNode.CreateNavigator(), "password");
                            portalConfig.AdminEmail = XmlUtils.GetNodeValue(adminNode.CreateNavigator(), "email");
                        }

                        portalConfig.Description = XmlUtils.GetNodeValue(portalNode.CreateNavigator(), "description");
                        portalConfig.Keywords = XmlUtils.GetNodeValue(portalNode.CreateNavigator(), "keywords");
                        portalConfig.TemplateFileName = XmlUtils.GetNodeValue(portalNode.CreateNavigator(), "templatefile");
                        portalConfig.IsChild = XmlUtils.GetNodeValue(portalNode.CreateNavigator(), "ischild").ToLowerInvariant() == "true";
                        portalConfig.HomeDirectory = XmlUtils.GetNodeValue(portalNode.CreateNavigator(), "homedirectory");

                        // Get the Portal Alias
                        XmlNodeList portalAliases = portalNode.SelectNodes("portalaliases/portalalias");
                        if (portalAliases != null)
                        {
                            foreach (XmlNode portalAliase in portalAliases)
                            {
                                if (!string.IsNullOrEmpty(portalAliase.InnerText))
                                {
                                    portalConfig.PortAliases.Add(portalAliase.InnerText);
                                }
                            }
                        }

                        installConfig.Portals.Add(portalConfig);
                    }
                }
            }

            return installConfig;
        }

        public bool IsValidSqlServerVersion(string connectionString)
        {
            // todo: check if we can use globals.DatabaseEngineVersion instead
            bool isValidVersion = false;
            var sqlConnection = new SqlConnection(connectionString);
            try
            {
                sqlConnection.Open();

                string serverVersion = sqlConnection.ServerVersion;
                if (serverVersion != null)
                {
                    string[] serverVersionDetails = serverVersion.Split(new[] { "." }, StringSplitOptions.None);

                    int versionNumber = int.Parse(serverVersionDetails[0]);

                    switch (versionNumber)
                    {
                        case 8:
                        // sql 2000
                        case 9:
                            // sql 2005
                            isValidVersion = false;
                            break;
                        case 10:
                        // sql 2008
                        case 11:
                        // sql 2010
                        case 12:
                            // sql 2012
                            isValidVersion = true;
                            break;
                        default:
                            // covers unknown versions and later releases
                            isValidVersion = true;
                            break;
                    }
                }
            }
            catch (Exception)
            {
                // cannot connect with the details
                isValidVersion = false;
            }
            finally
            {
                sqlConnection.Close();
            }

            return isValidVersion;
        }

        public bool IsAbleToPerformDatabaseActions(string connectionString)
        {
            var fakeName = "{databaseOwner}[{objectQualifier}FakeTable_" + DateTime.Now.Ticks.ToString("x16") + "]";
            var databaseActions = string.Format(@"CREATE TABLE {0}([fakeColumn] [int] NULL); SELECT * FROM {0}; DROP TABLE {0};", fakeName);
            var strExceptions = DataProvider.Instance().ExecuteScript(connectionString, databaseActions);

            // if no exceptions we have necessary drop etc permissions
            return string.IsNullOrEmpty(strExceptions);
        }

        public bool IsValidDotNetVersion()
        {
            // todo: check that this works for 4.5 etc.
            return Upgrade.IsNETFrameworkCurrent("4.0");
        }

        public bool IsSqlServerDbo()
        {
            string dbo = DataProvider.Instance().Settings["databaseOwner"];
            if (dbo.Trim().ToLowerInvariant() != "dbo.")
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool IsAvailableLanguagePack(string cultureCode)
        {
            try
            {
                string downloadUrl = UpdateService.GetLanguageDownloadUrl(cultureCode);

                string installFolder = HttpContext.Current.Server.MapPath("~/Install/language");

                // no need to download english, always there
                if (cultureCode != "en-us" && string.IsNullOrEmpty(downloadUrl) != true)
                {
                    var newCulture = new CultureInfo(cultureCode);
                    Thread.CurrentThread.CurrentCulture = newCulture;
                    this.GetLanguagePack(downloadUrl, installFolder);
                    return true;
                }

                return false;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public CultureInfo GetCurrentLanguage()
        {
            CultureInfo pageCulture = null;

            // 1. querystring
            pageCulture = this.GetCultureFromQs();

            // 2. cookie
            pageCulture = this.GetCultureFromCookie();

            // 3. browser
            pageCulture = this.GetCultureFromBrowser();

            return pageCulture;
        }

        /// <summary>
        /// Tests the Database Connection using the database connection config.
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <returns></returns>
        public string TestDatabaseConnection(ConnectionConfig config)
        {
            DbConnectionStringBuilder builder = DataProvider.Instance().GetConnectionStringBuilder();
            string owner = string.Empty;
            string objectQualifier = config.Qualifier;

            builder["data source"] = config.Server;
            builder["integrated security"] = config.Integrated;
            builder["uid"] = config.User;
            builder["pwd"] = config.Password;

            if (!string.IsNullOrEmpty(config.File))
            {
                builder["attachDbFilename"] = "|DataDirectory|" + config.File;
                builder["user instance"] = true;
            }
            else
            {
                builder["initial catalog"] = config.Database;
            }

            if (config.RunAsDbowner)
            {
                owner = "dbo.";
            }

            return DataProvider.Instance().TestDatabaseConnection(builder, owner, objectQualifier);
        }

        public CultureInfo GetCultureFromCookie()
        {
            var langCookie = HttpContext.Current.Request.Cookies["language"];
            var language = langCookie != null ? langCookie.Value : @"en-US";
            var culture = new CultureInfo(language);
            return culture;
        }

        public CultureInfo GetCultureFromBrowser()
        {
            CultureInfo culture = null;
            foreach (string userLang in HttpContext.Current.Request.UserLanguages)
            {
                // split userlanguage by ";"... all but the first language will contain a preferrence index eg. ;q=.5
                string language = userLang.Split(';')[0];
                culture = new CultureInfo(language);
            }

            return culture;
        }

        private static XmlNode AppendNewXmlNode(ref XmlDocument document, ref XmlNode parentNode, string elementName, string elementValue)
        {
            XmlNode newNode = document.CreateNode(XmlNodeType.Element, elementName, null);
            if (!string.IsNullOrEmpty(elementValue))
            {
                newNode.InnerText = elementValue;
            }

            parentNode.AppendChild(newNode);
            return newNode;
        }

        private CultureInfo GetCultureFromQs()
        {
            if (HttpContext.Current == null || HttpContext.Current.Request["language"] == null)
            {
                return null;
            }

            string language = HttpContext.Current.Request["language"];
            var culture = new CultureInfo(language);
            return culture;
        }

        private void GetLanguagePack(string downloadUrl, string installFolder)
        {
            string myfile = string.Empty;
            WebResponse wr = Util.GetExternalRequest(
                downloadUrl,
                null,
                null,
                null,
                null,
                null,
                -1,
                null,
                null,
                false,
                "DotNetNuke-Appgallery/1.0.0.0(Microsoft Windows NT 6.1.7600.0",
                "wpi://2.1.0.0/Microsoft Windows NT 6.1.7600.0",
                out myfile,
                10000);

            // use fixed name for later installation
            myfile = "installlanguage.resources";
            Util.DeployExtension(wr, myfile, installFolder);
        }
    }
}
