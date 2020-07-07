// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Common.Utilities
{
    using System;
    using System.Globalization;
    using System.IO;
    using System.Threading;
    using System.Web.Configuration;
    using System.Xml;
    using System.Xml.XPath;

    using DotNetNuke.Common.Utilities.Internal;
    using DotNetNuke.Framework.Providers;
    using DotNetNuke.Instrumentation;
    using DotNetNuke.Security;
    using DotNetNuke.Services.Exceptions;

    /// -----------------------------------------------------------------------------
    /// <summary>
    /// The Config class provides access to the web.config file.
    /// </summary>
    /// <remarks>
    /// </remarks>
    /// -----------------------------------------------------------------------------
    public class Config
    {
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Config));

        public enum ConfigFileType
        {
            DotNetNuke,

            // compatible with glbDotNetNukeConfig
            SiteAnalytics,
            Compression,
            SiteUrls,
            SolutionsExplorer,
        }

        public enum FcnMode
        {
            Default,
            Disabled,
            NotSet,
            Single,
        }

        /// <summary>
        /// Adds a new AppSetting to Web.Config. The update parameter allows you to define if,
        /// when the key already exists, this need to be updated or not.
        /// </summary>
        /// <param name="xmlDoc">xml representation of the web.config file.</param>
        /// <param name="key">key to be created.</param>
        /// <param name="value">value to be created.</param>
        /// <param name="update">If setting already exists, it will be updated if this parameter true.</param>
        /// <returns></returns>
        public static XmlDocument AddAppSetting(XmlDocument xmlDoc, string key, string value, bool update)
        {
            // retrieve the appSettings node
            XmlNode xmlAppSettings = xmlDoc.SelectSingleNode("//appSettings");
            if (xmlAppSettings != null)
            {
                XmlElement xmlElement;

                // get the node based on key
                XmlNode xmlNode = xmlAppSettings.SelectSingleNode("//add[@key='" + key + "']");
                if (update && xmlNode != null)
                {
                    // update the existing element
                    xmlElement = (XmlElement)xmlNode;
                    xmlElement.SetAttribute("value", value);
                }
                else
                {
                    // create a new element
                    xmlElement = xmlDoc.CreateElement("add");
                    xmlElement.SetAttribute("key", key);
                    xmlElement.SetAttribute("value", value);
                    xmlAppSettings.AppendChild(xmlElement);
                }
            }

            // return the xml doc
            return xmlDoc;
        }

        /// <summary>
        /// Adds a new AppSetting to Web.Config. If the key already exists, it will be updated with the new value.
        /// </summary>
        /// <param name="xmlDoc">xml representation of the web.config file.</param>
        /// <param name="key">key to be created.</param>
        /// <param name="value">value to be created.</param>
        /// <returns></returns>
        public static XmlDocument AddAppSetting(XmlDocument xmlDoc, string key, string value)
        {
            return AddAppSetting(xmlDoc, key, value, true);
        }

        public static void AddCodeSubDirectory(string name)
        {
            XmlDocument xmlConfig = Load();
            XmlNode xmlCompilation = xmlConfig.SelectSingleNode("configuration/system.web/compilation");
            if (xmlCompilation == null)
            {
                // Try location node
                xmlCompilation = xmlConfig.SelectSingleNode("configuration/location/system.web/compilation");
            }

            // Get the CodeSubDirectories Node
            if (xmlCompilation != null)
            {
                XmlNode xmlSubDirectories = xmlCompilation.SelectSingleNode("codeSubDirectories");
                if (xmlSubDirectories == null)
                {
                    // Add Node
                    xmlSubDirectories = xmlConfig.CreateElement("codeSubDirectories");
                    xmlCompilation.AppendChild(xmlSubDirectories);
                }

                var length = name.IndexOf("/", StringComparison.Ordinal);
                var codeSubDirectoryName = name;
                if (length > 0)
                {
                    codeSubDirectoryName = name.Substring(0, length);
                }

                // Check if the node is already present
                XmlNode xmlSubDirectory = xmlSubDirectories.SelectSingleNode("add[@directoryName='" + codeSubDirectoryName + "']");
                if (xmlSubDirectory == null)
                {
                    // Add Node
                    xmlSubDirectory = xmlConfig.CreateElement("add");
                    XmlUtils.CreateAttribute(xmlConfig, xmlSubDirectory, "directoryName", codeSubDirectoryName);
                    xmlSubDirectories.AppendChild(xmlSubDirectory);
                }
            }

            Save(xmlConfig);
        }

        public static void BackupConfig()
        {
            string backupFolder = string.Concat(Globals.glbConfigFolder, "Backup_", DateTime.Now.ToString("yyyyMMddHHmm"), "\\");

            // save the current config files
            try
            {
                if (!Directory.Exists(Globals.ApplicationMapPath + backupFolder))
                {
                    Directory.CreateDirectory(Globals.ApplicationMapPath + backupFolder);
                }

                if (File.Exists(Globals.ApplicationMapPath + "\\web.config"))
                {
                    File.Copy(Globals.ApplicationMapPath + "\\web.config", Globals.ApplicationMapPath + backupFolder + "web_old.config", true);
                }
            }
            catch (Exception e)
            {
                Exceptions.LogException(e);
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets the default connection String as specified in the provider.
        /// </summary>
        /// <returns>The connection String.</returns>
        /// <remarks></remarks>
        /// -----------------------------------------------------------------------------
        public static string GetConnectionString()
        {
            return GetConnectionString(GetDefaultProvider("data").Attributes["connectionStringName"]);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets the specified connection String.
        /// </summary>
        /// <param name="name">Name of Connection String to return.</param>
        /// <returns>The connection String.</returns>
        /// <remarks></remarks>
        /// -----------------------------------------------------------------------------
        public static string GetConnectionString(string name)
        {
            string connectionString = string.Empty;

            // First check if connection string is specified in <connectionstrings> (ASP.NET 2.0 / DNN v4.x)
            if (!string.IsNullOrEmpty(name))
            {
                // ASP.NET 2 version connection string (in <connectionstrings>)
                // This will be for new v4.x installs or upgrades from v4.x
                connectionString = WebConfigurationManager.ConnectionStrings[name].ConnectionString;
            }

            if (string.IsNullOrEmpty(connectionString))
            {
                if (!string.IsNullOrEmpty(name))
                {
                    // Next check if connection string is specified in <appsettings> (ASP.NET 1.1 / DNN v3.x)
                    // This will accomodate upgrades from v3.x
                    connectionString = GetSetting(name);
                }
            }

            return connectionString;
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        ///   Returns the decryptionkey from webconfig machinekey.
        /// </summary>
        /// <returns>decryption key.</returns>
        /// -----------------------------------------------------------------------------
        public static string GetDecryptionkey()
        {
            MachineKeySection key = System.Configuration.ConfigurationManager.GetSection("system.web/machineKey") as MachineKeySection;
            return key?.DecryptionKey.ToString() ?? string.Empty;
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        ///   Returns the fcnMode from webconfig httpRuntime.
        /// </summary>
        /// <returns>decryption key.</returns>
        /// -----------------------------------------------------------------------------
        public static string GetFcnMode()
        {
            var section = System.Configuration.ConfigurationManager.GetSection("system.web/httpRuntime") as HttpRuntimeSection;
            var mode = section?.FcnMode;
            return ((ValueType)mode ?? FcnMode.NotSet).ToString();
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        ///   Returns the maximum file size allowed to be uploaded to the application in bytes.
        /// </summary>
        /// <returns>Size in bytes.</returns>
        /// -----------------------------------------------------------------------------
        public static long GetMaxUploadSize()
        {
            var configNav = Load();

            var httpNode = configNav.SelectSingleNode("configuration//system.web//httpRuntime") ??
                           configNav.SelectSingleNode("configuration//location//system.web//httpRuntime");
            long maxRequestLength = 0;
            if (httpNode != null)
            {
                maxRequestLength = XmlUtils.GetAttributeValueAsLong(httpNode.CreateNavigator(), "maxRequestLength", 0) * 1024;
            }

            httpNode = configNav.SelectSingleNode("configuration//system.webServer//security//requestFiltering//requestLimits") ??
                       configNav.SelectSingleNode("configuration//location//system.webServer//security//requestFiltering//requestLimits");

            if (httpNode == null && Iis7AndAbove())
            {
                const int DefaultMaxAllowedContentLength = 30000000;
                return Math.Min(maxRequestLength, DefaultMaxAllowedContentLength);
            }

            if (httpNode != null)
            {
                var maxAllowedContentLength = XmlUtils.GetAttributeValueAsLong(httpNode.CreateNavigator(), "maxAllowedContentLength", 30000000);
                return Math.Min(maxRequestLength, maxAllowedContentLength);
            }

            return maxRequestLength;
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        ///   Returns the maximum file size allowed to be uploaded based on the request filter limit.
        /// </summary>
        /// <returns>Size in megabytes.</returns>
        /// -----------------------------------------------------------------------------
        public static long GetRequestFilterSize()
        {
            var configNav = Load();
            const int defaultRequestFilter = 30000000 / 1024 / 1024;
            var httpNode = configNav.SelectSingleNode("configuration//system.webServer//security//requestFiltering//requestLimits") ??
                       configNav.SelectSingleNode("configuration//location//system.webServer//security//requestFiltering//requestLimits");
            if (httpNode == null && Iis7AndAbove())
            {
                return defaultRequestFilter;
            }

            if (httpNode != null)
            {
                var maxAllowedContentLength = XmlUtils.GetAttributeValueAsLong(httpNode.CreateNavigator(), "maxAllowedContentLength", 30000000);
                return maxAllowedContentLength / 1024 / 1024;
            }

            return defaultRequestFilter;
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        ///   Sets the maximum file size allowed to be uploaded to the application in bytes.
        /// </summary>
        /// -----------------------------------------------------------------------------
        public static void SetMaxUploadSize(long newSize)
        {
            if (newSize < 12582912)
            {
                newSize = 12582912;
            } // 12 Mb minimum

            var configNav = Load();

            var httpNode = configNav.SelectSingleNode("configuration//system.web//httpRuntime") ??
                            configNav.SelectSingleNode("configuration//location//system.web//httpRuntime");
            if (httpNode != null)
            {
                httpNode.Attributes["maxRequestLength"].InnerText = (newSize / 1024).ToString("#");
                httpNode.Attributes["requestLengthDiskThreshold"].InnerText = (newSize / 1024).ToString("#");
            }

            httpNode = configNav.SelectSingleNode("configuration//system.webServer//security//requestFiltering//requestLimits") ??
                       configNav.SelectSingleNode("configuration//location//system.webServer//security//requestFiltering//requestLimits");
            if (httpNode != null)
            {
                httpNode.Attributes["maxAllowedContentLength"].InnerText = newSize.ToString("#");
            }

            Save(configNav);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets the specified upgrade connection string.
        /// </summary>
        /// <returns>The connection String.</returns>
        /// <remarks></remarks>
        /// -----------------------------------------------------------------------------
        public static string GetUpgradeConnectionString()
        {
            return GetDefaultProvider("data").Attributes["upgradeConnectionString"];
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets the specified database owner.
        /// </summary>
        /// <returns>The database owner.</returns>
        /// <remarks></remarks>
        /// -----------------------------------------------------------------------------
        public static string GetDataBaseOwner()
        {
            string databaseOwner = GetDefaultProvider("data").Attributes["databaseOwner"];
            if (!string.IsNullOrEmpty(databaseOwner) && databaseOwner.EndsWith(".") == false)
            {
                databaseOwner += ".";
            }

            return databaseOwner;
        }

        public static Provider GetDefaultProvider(string type)
        {
            ProviderConfiguration providerConfiguration = ProviderConfiguration.GetProviderConfiguration(type);

            // Read the configuration specific information for this provider
            return (Provider)providerConfiguration.Providers[providerConfiguration.DefaultProvider];
        }

        public static string GetFriendlyUrlProvider()
        {
            string providerToUse;
            ProviderConfiguration fupConfig = ProviderConfiguration.GetProviderConfiguration("friendlyUrl");
            if (fupConfig != null)
            {
                string defaultFriendlyUrlProvider = fupConfig.DefaultProvider;
                var provider = (Provider)fupConfig.Providers[defaultFriendlyUrlProvider];
                string urlFormat = provider.Attributes["urlFormat"];
                if (string.IsNullOrEmpty(urlFormat) == false)
                {
                    switch (urlFormat.ToLowerInvariant())
                    {
                        case "advanced":
                        case "customonly":
                            providerToUse = "advanced";
                            break;
                        default:
                            providerToUse = "standard";
                            break;
                    }
                }
                else
                {
                    providerToUse = "standard";
                }
            }
            else
            {
                providerToUse = "standard";
            }

            return providerToUse;
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets the specified object qualifier.
        /// </summary>
        /// <returns>The object qualifier.</returns>
        /// <remarks></remarks>
        /// -----------------------------------------------------------------------------
        public static string GetObjectQualifer()
        {
            Provider provider = GetDefaultProvider("data");
            string objectQualifier = provider.Attributes["objectQualifier"];
            if (!string.IsNullOrEmpty(objectQualifier) && objectQualifier.EndsWith("_") == false)
            {
                objectQualifier += "_";
            }

            return objectQualifier;
        }

        public static int GetAuthCookieTimeout()
        {
            XPathNavigator configNav = Load().CreateNavigator();

            // Select the location node
            XPathNavigator locationNav = configNav.SelectSingleNode("configuration/location");
            XPathNavigator formsNav;

            // Test for the existence of the location node if it exists then include that in the nodes of the XPath Query
            if (locationNav == null)
            {
                formsNav = configNav.SelectSingleNode("configuration/system.web/authentication/forms");
            }
            else
            {
                formsNav = configNav.SelectSingleNode("configuration/location/system.web/authentication/forms");
            }

            return (formsNav != null) ? XmlUtils.GetAttributeValueAsInteger(formsNav, "timeout", 30) : 30;
        }

        /// <summary>
        ///   Get's optional persistent cookie timeout value from web.config.
        /// </summary>
        /// <returns>persistent cookie value.</returns>
        /// <remarks>
        ///   allows users to override default asp.net values.
        /// </remarks>
        public static int GetPersistentCookieTimeout()
        {
            int persistentCookieTimeout = 0;
            if (!string.IsNullOrEmpty(GetSetting("PersistentCookieTimeout")))
            {
                persistentCookieTimeout = int.Parse(GetSetting("PersistentCookieTimeout"));
            }

            return (persistentCookieTimeout == 0) ? GetAuthCookieTimeout() : persistentCookieTimeout;
        }

        public static Provider GetProvider(string type, string name)
        {
            ProviderConfiguration providerConfiguration = ProviderConfiguration.GetProviderConfiguration(type);

            // Read the configuration specific information for this provider
            return (Provider)providerConfiguration.Providers[name];
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets the specified provider path.
        /// </summary>
        /// <returns>The provider path.</returns>
        /// <remarks></remarks>
        /// -----------------------------------------------------------------------------
        public static string GetProviderPath(string type)
        {
            Provider objProvider = GetDefaultProvider(type);
            string providerPath = objProvider.Attributes["providerPath"];
            return providerPath;
        }

        public static string GetSetting(string setting)
        {
            return WebConfigurationManager.AppSettings[setting];
        }

        public static object GetSection(string section)
        {
            return WebConfigurationManager.GetWebApplicationSection(section);
        }

        public static XmlDocument Load()
        {
            return Load("web.config");
        }

        public static string GetCustomErrorMode()
        {
            XPathNavigator configNav = Load().CreateNavigator();

            // Select the location node
            var customErrorsNav = configNav.SelectSingleNode("//configuration/system.web/customErrors|//configuration/location/system.web/customErrors");

            string customErrorMode = XmlUtils.GetAttributeValue(customErrorsNav, "mode");
            if (string.IsNullOrEmpty(customErrorMode))
            {
                customErrorMode = "RemoteOnly";
            }

            return (customErrorsNav != null) ? customErrorMode : "RemoteOnly";
        }

        public static XmlDocument Load(string filename)
        {
            // open the config file
            var xmlDoc = new XmlDocument { XmlResolver = null };
            xmlDoc.Load(Globals.ApplicationMapPath + "\\" + filename);

            // test for namespace added by Web Admin Tool
            if (!string.IsNullOrEmpty(xmlDoc.DocumentElement.GetAttribute("xmlns")))
            {
                // remove namespace
                string strDoc = xmlDoc.InnerXml.Replace("xmlns=\"http://schemas.microsoft.com/.NetConfiguration/v2.0\"", string.Empty);
                xmlDoc.LoadXml(strDoc);
            }

            return xmlDoc;
        }

        public static void RemoveCodeSubDirectory(string name)
        {
            XmlDocument xmlConfig = Load();

            // Select the location node
            XmlNode xmlCompilation = xmlConfig.SelectSingleNode("configuration/system.web/compilation");
            if (xmlCompilation == null)
            {
                // Try location node
                xmlCompilation = xmlConfig.SelectSingleNode("configuration/location/system.web/compilation");
            }

            // Get the CodeSubDirectories Node
            XmlNode xmlSubDirectories = xmlCompilation.SelectSingleNode("codeSubDirectories");
            if (xmlSubDirectories == null)
            {
                // Parent doesn't exist so subDirectory node can't exist
                return;
            }

            var length = name.IndexOf("/", StringComparison.Ordinal);
            var codeSubDirectoryName = name;
            if (length > 0)
            {
                codeSubDirectoryName = name.Substring(0, length);
            }

            // Check if the node is present
            XmlNode xmlSubDirectory = xmlSubDirectories.SelectSingleNode("add[@directoryName='" + codeSubDirectoryName + "']");
            if (xmlSubDirectory != null)
            {
                // Remove Node
                xmlSubDirectories.RemoveChild(xmlSubDirectory);

                Save(xmlConfig);
            }
        }

        public static string Save(XmlDocument xmlDoc)
        {
            return Save(xmlDoc, "web.config");
        }

        public static string Save(XmlDocument xmlDoc, string filename)
        {
            var retMsg = string.Empty;
            try
            {
                var strFilePath = Globals.ApplicationMapPath + "\\" + filename;
                var objFileAttributes = FileAttributes.Normal;
                if (File.Exists(strFilePath))
                {
                    // save current file attributes
                    objFileAttributes = File.GetAttributes(strFilePath);

                    // change to normal ( in case it is flagged as read-only )
                    File.SetAttributes(strFilePath, FileAttributes.Normal);
                }

                // Attempt a few times in case the file was locked; occurs during modules' installation due
                // to application restarts where IIS can overlap old application shutdown and new one start.
                const int maxRetires = 4;
                const double miltiplier = 2.5;
                for (var retry = maxRetires; retry >= 0; retry--)
                {
                    try
                    {
                        // save the config file
                        var settings = new XmlWriterSettings { CloseOutput = true, Indent = true };
                        using (var writer = XmlWriter.Create(strFilePath, settings))
                        {
                            xmlDoc.WriteTo(writer);
                            writer.Flush();
                            writer.Close();
                        }

                        break;
                    }
                    catch (IOException exc)
                    {
                        if (retry == 0)
                        {
                            Logger.Error(exc);
                            retMsg = exc.Message;
                        }

                        // try incremental delay; maybe the file lock is released by then
                        Thread.Sleep((int)(miltiplier * (maxRetires - retry + 1)) * 1000);
                    }
                }

                // reset file attributes
                File.SetAttributes(strFilePath, objFileAttributes);
            }
            catch (Exception exc)
            {
                // the file permissions may not be set properly
                Logger.Error(exc);
                retMsg = exc.Message;
            }

            return retMsg;
        }

        public static bool Touch()
        {
            try
            {
                RetryableAction.Retry5TimesWith2SecondsDelay(
                    () => File.SetLastWriteTime(Globals.ApplicationMapPath + "\\web.config", DateTime.Now), "Touching config file");
                return true;
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
                return false;
            }
        }

        public static void UpdateConnectionString(string conn)
        {
            XmlDocument xmlConfig = Load();
            string name = GetDefaultProvider("data").Attributes["connectionStringName"];

            // Update ConnectionStrings
            XmlNode xmlConnection = xmlConfig.SelectSingleNode("configuration/connectionStrings/add[@name='" + name + "']");
            XmlUtils.UpdateAttribute(xmlConnection, "connectionString", conn);

            // Update AppSetting
            XmlNode xmlAppSetting = xmlConfig.SelectSingleNode("configuration/appSettings/add[@key='" + name + "']");
            XmlUtils.UpdateAttribute(xmlAppSetting, "value", conn);

            // Save changes
            Save(xmlConfig);
        }

        public static void UpdateDataProvider(string name, string databaseOwner, string objectQualifier)
        {
            XmlDocument xmlConfig = Load();

            // Update provider
            XmlNode xmlProvider = xmlConfig.SelectSingleNode("configuration/dotnetnuke/data/providers/add[@name='" + name + "']");
            XmlUtils.UpdateAttribute(xmlProvider, "databaseOwner", databaseOwner);
            XmlUtils.UpdateAttribute(xmlProvider, "objectQualifier", objectQualifier);

            // Save changes
            Save(xmlConfig);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Updates the specified upgrade connection string.
        /// </summary>
        /// <remarks></remarks>
        /// -----------------------------------------------------------------------------
        public static void UpdateUpgradeConnectionString(string name, string upgradeConnectionString)
        {
            XmlDocument xmlConfig = Load();

            // Update provider
            XmlNode xmlProvider = xmlConfig.SelectSingleNode("configuration/dotnetnuke/data/providers/add[@name='" + name + "']");
            XmlUtils.UpdateAttribute(xmlProvider, "upgradeConnectionString", upgradeConnectionString);

            // Save changes
            Save(xmlConfig);
        }

        public static string UpdateMachineKey()
        {
            string backupFolder = string.Concat(Globals.glbConfigFolder, "Backup_", DateTime.Now.ToString("yyyyMMddHHmm"), "\\");
            var xmlConfig = new XmlDocument { XmlResolver = null };
            string strError = string.Empty;

            // save the current config files
            BackupConfig();
            try
            {
                // open the web.config
                xmlConfig = Load();

                // create random keys for the Membership machine keys
                xmlConfig = UpdateMachineKey(xmlConfig);
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                strError += ex.Message;
            }

            // save a copy of the web.config
            strError += Save(xmlConfig, backupFolder + "web_.config");

            // save the web.config
            strError += Save(xmlConfig);

            return strError;
        }

        public static XmlDocument UpdateMachineKey(XmlDocument xmlConfig)
        {
            var portalSecurity = PortalSecurity.Instance;
            string validationKey = portalSecurity.CreateKey(20);
            string decryptionKey = portalSecurity.CreateKey(24);

            XmlNode xmlMachineKey = xmlConfig.SelectSingleNode("configuration/system.web/machineKey");
            XmlUtils.UpdateAttribute(xmlMachineKey, "validationKey", validationKey);
            XmlUtils.UpdateAttribute(xmlMachineKey, "decryptionKey", decryptionKey);

            xmlConfig = AddAppSetting(xmlConfig, "InstallationDate", DateTime.Today.ToString("d", new CultureInfo("en-US")));

            return xmlConfig;
        }

        public static string UpdateValidationKey()
        {
            string backupFolder = string.Concat(Globals.glbConfigFolder, "Backup_", DateTime.Now.ToString("yyyyMMddHHmm"), "\\");
            var xmlConfig = new XmlDocument { XmlResolver = null };
            string strError = string.Empty;

            // save the current config files
            BackupConfig();
            try
            {
                // open the web.config
                xmlConfig = Load();

                // create random keys for the Membership machine keys
                xmlConfig = UpdateValidationKey(xmlConfig);
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                strError += ex.Message;
            }

            // save a copy of the web.config
            strError += Save(xmlConfig, backupFolder + "web_.config");

            // save the web.config
            strError += Save(xmlConfig);
            return strError;
        }

        public static XmlDocument UpdateValidationKey(XmlDocument xmlConfig)
        {
            XmlNode xmlMachineKey = xmlConfig.SelectSingleNode("configuration/system.web/machineKey");
            if (xmlMachineKey.Attributes["validationKey"].Value == "F9D1A2D3E1D3E2F7B3D9F90FF3965ABDAC304902")
            {
                var objSecurity = PortalSecurity.Instance;
                string validationKey = objSecurity.CreateKey(20);
                XmlUtils.UpdateAttribute(xmlMachineKey, "validationKey", validationKey);
            }

            return xmlConfig;
        }

        /// <summary>
        ///   Gets the path for the specificed Config file.
        /// </summary>
        /// <param name = "file">The config.file to get the path for.</param>
        /// <returns>fully qualified path to the file.</returns>
        /// <remarks>
        ///   Will copy the file from the template directory as requried.
        /// </remarks>
        public static string GetPathToFile(ConfigFileType file)
        {
            return GetPathToFile(file, false);
        }

        /// <summary>
        ///   Gets the path for the specificed Config file.
        /// </summary>
        /// <param name = "file">The config.file to get the path for.</param>
        /// <param name = "overwrite">force an overwrite of the config file.</param>
        /// <returns>fully qualified path to the file.</returns>
        /// <remarks>
        ///   Will copy the file from the template directory as requried.
        /// </remarks>
        public static string GetPathToFile(ConfigFileType file, bool overwrite)
        {
            string fileName = EnumToFileName(file);
            string path = Path.Combine(Globals.ApplicationMapPath, fileName);

            if (!File.Exists(path) || overwrite)
            {
                // Copy from \Config
                string pathToDefault = Path.Combine(Globals.ApplicationMapPath + Globals.glbConfigFolder, fileName);
                if (File.Exists(pathToDefault))
                {
                    File.Copy(pathToDefault, path, true);
                }
            }

            return path;
        }

        /// <summary>
        /// UpdateInstallVersion, but only if the setting does not already exist.
        /// </summary>
        /// <returns></returns>
        public static string UpdateInstallVersion(Version version)
        {
            string strError = string.Empty;

            var installVersion = GetSetting("InstallVersion");
            if (string.IsNullOrEmpty(installVersion))
            {
                // we need to add the InstallVersion
                string backupFolder = string.Concat(Globals.glbConfigFolder, "Backup_", DateTime.Now.ToString("yyyyMMddHHmm"), "\\");
                var xmlConfig = new XmlDocument { XmlResolver = null };

                // save the current config files
                BackupConfig();
                try
                {
                    // open the web.config
                    xmlConfig = Load();

                    // Update the InstallVersion
                    xmlConfig = UpdateInstallVersion(xmlConfig, version);
                }
                catch (Exception ex)
                {
                    Logger.Error(ex);
                    strError += ex.Message;
                }

                // save a copy of the web.config
                strError += Save(xmlConfig, backupFolder + "web_.config");

                // save the web.config
                strError += Save(xmlConfig);
            }

            return strError;
        }

        public static bool IsNet45OrNewer()
        {
            // Class "ReflectionContext" exists from .NET 4.5 onwards.
            return Type.GetType("System.Reflection.ReflectionContext", false) != null;
        }

        public static string AddFCNMode(FcnMode fcnMode)
        {
            try
            {
                // check current .net version and if attribute has been added already
                if (IsNet45OrNewer() && GetFcnMode() != fcnMode.ToString())
                {
                    // open the web.config
                    var xmlConfig = Load();

                    var xmlhttpRunTimeKey = xmlConfig.SelectSingleNode("configuration/system.web/httpRuntime") ??
                                                xmlConfig.SelectSingleNode("configuration/location/system.web/httpRuntime");
                    XmlUtils.CreateAttribute(xmlConfig, xmlhttpRunTimeKey, "fcnMode", fcnMode.ToString());

                    // save the web.config
                    Save(xmlConfig);
                }
            }
            catch (Exception ex)
            {
                // in case of error installation shouldn't be stopped, log into log4net
                Logger.Error(ex);
            }

            return string.Empty;
        }

        private static bool Iis7AndAbove()
        {
            return Environment.OSVersion.Version.Major >= 6;
        }

        private static string EnumToFileName(ConfigFileType file)
        {
            switch (file)
            {
                case ConfigFileType.SolutionsExplorer:
                    return "SolutionsExplorer.opml.config";
                default:
                    return file + ".config";
            }
        }

        private static XmlDocument UpdateInstallVersion(XmlDocument xmlConfig, Version version)
        {
            // only update appsetting if necessary
            xmlConfig = AddAppSetting(xmlConfig, "InstallVersion", Globals.FormatVersion(version), false);

            return xmlConfig;
        }
    }
}
