// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Common.Utilities
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.IO;
    using System.Threading;
    using System.Web.Configuration;
    using System.Xml;
    using System.Xml.XPath;

    using DotNetNuke.Abstractions.Application;
    using DotNetNuke.Application;
    using DotNetNuke.Common.Utilities.Internal;
    using DotNetNuke.Framework.Providers;
    using DotNetNuke.Instrumentation;
    using DotNetNuke.Internal.SourceGenerators;
    using DotNetNuke.Security;
    using DotNetNuke.Services.Exceptions;

    using Microsoft.Extensions.DependencyInjection;

    /// <summary>The Config class provides access to the web.config file.</summary>
    public partial class Config
    {
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Config));

        /// <summary>Represents each configuration file.</summary>
        public enum ConfigFileType
        {
            /// <summary>The DotNetNuke.config file.</summary>
            DotNetNuke = 0,

            /// <summary>The SiteAnalytics.config file.</summary>
            // compatible with glbDotNetNukeConfig
            SiteAnalytics = 1,

            /// <summary>The Compression.config file.</summary>
            Compression = 2,

            /// <summary>The SiteUrls.config file.</summary>
            SiteUrls = 3,

            /// <summary>The SolutionsExplorer.opml.config file.</summary>
            SolutionsExplorer = 4,
        }

        /// <summary>Specifies behavior for file change notification(FCN) in the application.</summary>
        public enum FcnMode
        {
            /// <summary>For each subdirectory, the application creates an object that monitors the subdirectory. This is the default behavior.</summary>
            Default = 0,

            /// <summary>File change notification is disabled.</summary>
            Disabled = 1,

            /// <summary>File change notification is not set, so the application creates an object that monitors each subdirectory. This is the default behavior.</summary>
            NotSet = 2,

            /// <summary>The application creates one object to monitor the main directory and uses this object to monitor each subdirectory.</summary>
            [SuppressMessage("Microsoft.Naming", "CA1720:IdentifiersShouldNotContainTypeNames", Justification = "Breaking change")]
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
        /// <returns>An XML document.</returns>
        public static XmlDocument AddAppSetting(XmlDocument xmlDoc, string key, string value, bool update)
        {
            // retrieve the appSettings node
            var xmlAppSettings = xmlDoc.SelectSingleNode("//appSettings");
            if (xmlAppSettings != null)
            {
                XmlElement xmlElement;

                // get the node based on key
                var xmlNode = xmlAppSettings.SelectSingleNode("//add[@key='" + key + "']");
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

        /// <summary>Adds a new AppSetting to Web.Config. If the key already exists, it will be updated with the new value.</summary>
        /// <param name="xmlDoc">xml representation of the web.config file.</param>
        /// <param name="key">key to be created.</param>
        /// <param name="value">value to be created.</param>
        /// <returns>An XML document.</returns>
        public static XmlDocument AddAppSetting(XmlDocument xmlDoc, string key, string value)
        {
            return AddAppSetting(xmlDoc, key, value, true);
        }

        /// <summary>Adds a code subdirectory to the configuration.</summary>
        /// <param name="name">The name of the code subdirectory.</param>
        [DnnDeprecated(9, 11, 1, "Use overload taking an IApplicationStatusInfo")]
        public static partial void AddCodeSubDirectory(string name)
        {
            AddCodeSubDirectory(
                Globals.GetCurrentServiceProvider().GetService<IApplicationStatusInfo>() ?? new ApplicationStatusInfo(new Application()),
                name);
        }

        /// <summary>Adds a code subdirectory to the configuration.</summary>
        /// <param name="appStatus">The application status info.</param>
        /// <param name="name">The name of the code subdirectory.</param>
        public static void AddCodeSubDirectory(IApplicationStatusInfo appStatus, string name)
        {
            var xmlConfig = Load(appStatus);

            // Try location node
            var xmlCompilation = xmlConfig.SelectSingleNode("configuration/system.web/compilation") ??
                                 xmlConfig.SelectSingleNode("configuration/location/system.web/compilation");

            // Get the CodeSubDirectories Node
            if (xmlCompilation != null)
            {
                var xmlSubDirectories = xmlCompilation.SelectSingleNode("codeSubDirectories");
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
                var xmlSubDirectory = xmlSubDirectories.SelectSingleNode($"add[@directoryName='{codeSubDirectoryName}']");
                if (xmlSubDirectory == null)
                {
                    // Add Node
                    xmlSubDirectory = xmlConfig.CreateElement("add");
                    XmlUtils.CreateAttribute(xmlConfig, xmlSubDirectory, "directoryName", codeSubDirectoryName);
                    xmlSubDirectories.AppendChild(xmlSubDirectory);
                }
            }

            Save(appStatus, xmlConfig);
        }

        /// <summary>Creates a backup of the web.config file.</summary>
        [DnnDeprecated(9, 11, 1, "Use overload taking an IApplicationStatusInfo")]
        public static partial void BackupConfig()
        {
            BackupConfig(Globals.GetCurrentServiceProvider().GetService<IApplicationStatusInfo>() ?? new ApplicationStatusInfo(new Application()));
        }

        /// <summary>Creates a backup of the web.config file.</summary>
        /// <param name="statusInfo">The application status info.</param>
        public static void BackupConfig(IApplicationStatusInfo statusInfo)
        {
            var webConfigPath = Path.Combine(statusInfo.ApplicationMapPath, "web.config");
            var backupWebConfigPath = GetTimestampedBackupPath(statusInfo, "web_old.config");
            var backupFolderPath = Path.GetDirectoryName(backupWebConfigPath);

            // save the current config files
            try
            {
                if (!Directory.Exists(backupFolderPath))
                {
                    Directory.CreateDirectory(backupFolderPath);
                }

                if (File.Exists(webConfigPath))
                {
                    File.Copy(webConfigPath, backupWebConfigPath, true);
                }
            }
            catch (Exception e)
            {
                Exceptions.LogException(e);
            }
        }

        /// <summary>Gets the default connection String as specified in the provider.</summary>
        /// <returns>The connection String.</returns>
        public static string GetConnectionString()
        {
            return GetConnectionString(GetDefaultProvider("data").Attributes["connectionStringName"]);
        }

        /// <summary>Gets the specified connection String.</summary>
        /// <param name="name">Name of Connection String to return.</param>
        /// <returns>The connection String.</returns>
        public static string GetConnectionString(string name)
        {
            var connectionString = string.Empty;

            // First check if connection string is specified in <connectionStrings> (ASP.NET 2.0 / DNN v4.x)
            if (!string.IsNullOrEmpty(name))
            {
                // ASP.NET 2 version connection string (in <connectionStrings>)
                // This will be for new v4.x installs or upgrades from v4.x
                connectionString = System.Configuration.ConfigurationManager.ConnectionStrings[name].ConnectionString;
            }

            if (string.IsNullOrEmpty(connectionString))
            {
                if (!string.IsNullOrEmpty(name))
                {
                    // Next check if connection string is specified in <appSettings> (ASP.NET 1.1 / DNN v3.x)
                    // This will accomodate upgrades from v3.x
                    connectionString = GetSetting(name);
                }
            }

            return connectionString;
        }

        /// <summary>Returns the decryptionKey from web.config machineKey.</summary>
        /// <returns>decryption key.</returns>
        public static string GetDecryptionkey()
        {
            var key = System.Configuration.ConfigurationManager.GetSection("system.web/machineKey") as MachineKeySection;
            return key?.DecryptionKey.ToString() ?? string.Empty;
        }

        /// <summary>Returns the fcnMode from web.config httpRuntime.</summary>
        /// <returns>FCN mode.</returns>
        public static string GetFcnMode()
        {
            var section = System.Configuration.ConfigurationManager.GetSection("system.web/httpRuntime") as HttpRuntimeSection;
            var mode = section?.FcnMode;
            return ((ValueType)mode ?? FcnMode.NotSet).ToString();
        }

        /// <summary>Returns the maximum file size allowed to be uploaded to the application in bytes.</summary>
        /// <returns>Size in bytes.</returns>
        [DnnDeprecated(9, 11, 1, "Use overload taking an IApplicationStatusInfo")]
        public static partial long GetMaxUploadSize()
        {
            return GetMaxUploadSize(
                Globals.GetCurrentServiceProvider().GetService<IApplicationStatusInfo>() ?? new ApplicationStatusInfo(new Application()));
        }

        /// <summary>Returns the maximum file size allowed to be uploaded to the application in bytes.</summary>
        /// <param name="appStatus">The application status info.</param>
        /// <returns>Size in bytes.</returns>
        public static long GetMaxUploadSize(IApplicationStatusInfo appStatus)
        {
            var configNav = Load(appStatus);

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

        /// <summary>Returns the maximum file size allowed to be uploaded based on the request filter limit.</summary>
        /// <returns>Size in megabytes.</returns>
        [DnnDeprecated(9, 11, 1, "Use overload taking an IApplicationStatusInfo")]
        public static partial long GetRequestFilterSize()
        {
            return GetRequestFilterSize(
                Globals.GetCurrentServiceProvider().GetService<IApplicationStatusInfo>() ?? new ApplicationStatusInfo(new Application()));
        }

        /// <summary>Returns the maximum file size allowed to be uploaded based on the request filter limit.</summary>
        /// <param name="appStatus">The application status info.</param>
        /// <returns>Size in megabytes.</returns>
        public static long GetRequestFilterSize(IApplicationStatusInfo appStatus)
        {
            var configNav = Load(appStatus);
            const int DefaultRequestFilter = 30000000 / 1024 / 1024;
            var httpNode = configNav.SelectSingleNode("configuration//system.webServer//security//requestFiltering//requestLimits") ??
                       configNav.SelectSingleNode("configuration//location//system.webServer//security//requestFiltering//requestLimits");
            if (httpNode == null && Iis7AndAbove())
            {
                return DefaultRequestFilter;
            }

            if (httpNode != null)
            {
                var maxAllowedContentLength = XmlUtils.GetAttributeValueAsLong(httpNode.CreateNavigator(), "maxAllowedContentLength", 30000000);
                return maxAllowedContentLength / 1024 / 1024;
            }

            return DefaultRequestFilter;
        }

        /// <summary>Sets the maximum file size allowed to be uploaded to the application in bytes.</summary>
        /// <param name="newSize">The new max upload size in bytes.</param>
        [DnnDeprecated(9, 11, 1, "Use overload taking an IApplicationStatusInfo")]
        public static partial void SetMaxUploadSize(long newSize)
        {
            SetMaxUploadSize(
                Globals.GetCurrentServiceProvider().GetService<IApplicationStatusInfo>() ?? new ApplicationStatusInfo(new Application()),
                newSize);
        }

        /// <summary>Sets the maximum file size allowed to be uploaded to the application in bytes.</summary>
        /// <param name="appStatus">The application status info.</param>
        /// <param name="newSize">The new max upload size in bytes.</param>
        public static void SetMaxUploadSize(IApplicationStatusInfo appStatus, long newSize)
        {
            if (newSize < 12582912)
            {
                newSize = 12582912;
            } // 12 Mb minimum

            var configNav = Load(appStatus);

            var httpNode = configNav.SelectSingleNode("configuration//system.web//httpRuntime") ??
                            configNav.SelectSingleNode("configuration//location//system.web//httpRuntime");
            if (httpNode != null)
            {
                httpNode.Attributes["maxRequestLength"].InnerText = (newSize / 1024).ToString("#", CultureInfo.InvariantCulture);
                httpNode.Attributes["requestLengthDiskThreshold"].InnerText = (newSize / 1024).ToString("#", CultureInfo.InvariantCulture);
            }

            httpNode = configNav.SelectSingleNode("configuration//system.webServer//security//requestFiltering//requestLimits") ??
                       configNav.SelectSingleNode("configuration//location//system.webServer//security//requestFiltering//requestLimits");
            if (httpNode != null)
            {
                if (httpNode.Attributes["maxAllowedContentLength"] == null)
                {
                    httpNode.Attributes.Append(configNav.CreateAttribute("maxAllowedContentLength"));
                }

                httpNode.Attributes["maxAllowedContentLength"].InnerText = newSize.ToString("#", CultureInfo.InvariantCulture);
            }

            Save(appStatus, configNav);
        }

        /// <summary>Gets the specified upgrade connection string.</summary>
        /// <returns>The connection String.</returns>
        public static string GetUpgradeConnectionString()
        {
            return GetDefaultProvider("data").Attributes["upgradeConnectionString"];
        }

        /// <summary>Gets the specified database owner.</summary>
        /// <returns>The database owner.</returns>
        public static string GetDataBaseOwner()
        {
            var databaseOwner = GetDefaultProvider("data").Attributes["databaseOwner"];
            if (!string.IsNullOrEmpty(databaseOwner) && !databaseOwner.EndsWith(".", StringComparison.Ordinal))
            {
                databaseOwner += ".";
            }

            return databaseOwner;
        }

        /// <summary>Gets the Dnn default provider for a given type.</summary>
        /// <param name="type">The type for which to get the default provider for.</param>
        /// <returns>The default provider, <see cref="Provider"/>.</returns>
        public static Provider GetDefaultProvider(string type)
        {
            var providerConfiguration = ProviderConfiguration.GetProviderConfiguration(type);

            // Read the configuration specific information for this provider
            return (Provider)providerConfiguration.Providers[providerConfiguration.DefaultProvider];
        }

        /// <summary>Gets the currently configured friendly url provider.</summary>
        /// <returns>The name of the friendly url provider.</returns>
        public static string GetFriendlyUrlProvider()
        {
            string providerToUse;
            var fupConfig = ProviderConfiguration.GetProviderConfiguration("friendlyUrl");
            if (fupConfig != null)
            {
                var defaultFriendlyUrlProvider = fupConfig.DefaultProvider;
                var provider = (Provider)fupConfig.Providers[defaultFriendlyUrlProvider];
                var urlFormat = provider.Attributes["urlFormat"];
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

        /// <summary>Gets the specified object qualifier.</summary>
        /// <returns>The object qualifier.</returns>
        public static string GetObjectQualifer()
        {
            var provider = GetDefaultProvider("data");
            var objectQualifier = provider.Attributes["objectQualifier"];
            if (!string.IsNullOrEmpty(objectQualifier) && !objectQualifier.EndsWith("_", StringComparison.Ordinal))
            {
                objectQualifier += "_";
            }

            return objectQualifier;
        }

        /// <summary>Gets the authentication cookie timeout value.</summary>
        /// <returns>The timeout value.</returns>
        [DnnDeprecated(9, 11, 1, "Use overload taking an IApplicationStatusInfo")]
        public static partial int GetAuthCookieTimeout()
        {
            return GetAuthCookieTimeout(
                Globals.GetCurrentServiceProvider().GetService<IApplicationStatusInfo>() ?? new ApplicationStatusInfo(new Application()));
        }

        /// <summary>Gets the authentication cookie timeout value.</summary>
        /// <param name="appStatus">The application status info.</param>
        /// <returns>The timeout value.</returns>
        public static int GetAuthCookieTimeout(IApplicationStatusInfo appStatus)
        {
            var configNav = Load(appStatus).CreateNavigator();

            // Try to get the forms authentication from the default location
            var formsNav = configNav?.SelectSingleNode("configuration/system.web/authentication/forms");
            if (formsNav is null)
            {
                // If unable, look for a location node, if found try to get the settings from there
                formsNav = configNav?.SelectSingleNode("configuration/location/system.web/authentication/forms");
            }

            const int DefaultTimeout = 30;
            return formsNav is null ? DefaultTimeout : XmlUtils.GetAttributeValueAsInteger(formsNav, "timeout", DefaultTimeout);
        }

        /// <summary>Gets optional persistent cookie timeout value from web.config.</summary>
        /// <returns>The persistent cookie value.</returns>
        /// <remarks>Allows users to override default asp.net values.</remarks>
        [DnnDeprecated(9, 11, 1, "Use overload taking an IApplicationStatusInfo")]
        public static partial int GetPersistentCookieTimeout()
        {
            return GetPersistentCookieTimeout(
                Globals.GetCurrentServiceProvider().GetService<IApplicationStatusInfo>() ?? new ApplicationStatusInfo(new Application()));
        }

        /// <summary>Gets optional persistent cookie timeout value from web.config.</summary>
        /// <param name="appStatus">The application status info.</param>
        /// <returns>The persistent cookie value.</returns>
        /// <remarks>Allows users to override default asp.net values.</remarks>
        public static int GetPersistentCookieTimeout(IApplicationStatusInfo appStatus)
        {
            var persistentCookieTimeout = 0;
            if (!string.IsNullOrEmpty(GetSetting("PersistentCookieTimeout")))
            {
                persistentCookieTimeout = int.Parse(GetSetting("PersistentCookieTimeout"), CultureInfo.InvariantCulture);
            }

            return persistentCookieTimeout == 0 ? GetAuthCookieTimeout(appStatus) : persistentCookieTimeout;
        }

        /// <summary>Gets a provider by its type and name.</summary>
        /// <param name="type">The provider type.</param>
        /// <param name="name">The provider name.</param>
        /// <returns>The found provider, <see cref="Provider"/>.</returns>
        public static Provider GetProvider(string type, string name)
        {
            var providerConfiguration = ProviderConfiguration.GetProviderConfiguration(type);

            // Read the configuration specific information for this provider
            return (Provider)providerConfiguration.Providers[name];
        }

        /// <summary>Gets the specified provider path.</summary>
        /// <param name="type">The name of the <see cref="Type"/> of the <see cref="Provider"/>.</param>
        /// <returns>The provider path.</returns>
        public static string GetProviderPath(string type)
        {
            var objProvider = GetDefaultProvider(type);
            return objProvider.Attributes["providerPath"];
        }

        /// <summary>Gets an application setting.</summary>
        /// <param name="setting">The name of the setting.</param>
        /// <returns>A string representing the application setting.</returns>
        public static string GetSetting(string setting)
        {
            return System.Configuration.ConfigurationManager.AppSettings[setting];
        }

        /// <summary>Gets a configuration section.</summary>
        /// <param name="section">The name of the section.</param>
        /// <returns>An object representing the application section.</returns>
        public static object GetSection(string section)
        {
            return WebConfigurationManager.GetWebApplicationSection(section);
        }

        /// <summary>Loads the web.config file into an XML document.</summary>
        /// <param name="appStatus">The application status info.</param>
        /// <returns>The configuration XML document.</returns>
        public static XmlDocument Load(IApplicationStatusInfo appStatus)
        {
            return Load(appStatus, "web.config");
        }

        /// <summary>Loads the web.config file into an XML document.</summary>
        /// <returns>The configuration XML document.</returns>
        [DnnDeprecated(9, 11, 1, "Use overload taking an IApplicationStatusInfo")]
        public static partial XmlDocument Load()
        {
            return Load("web.config");
        }

        /// <summary>Gets the currently configured custom error mode.</summary>
        /// <returns>The currently configured custom error mode string.</returns>
        [DnnDeprecated(9, 11, 1, "Use overload taking an IApplicationStatusInfo")]
        public static partial string GetCustomErrorMode()
        {
            return GetCustomErrorMode(
                Globals.GetCurrentServiceProvider().GetService<IApplicationStatusInfo>() ?? new ApplicationStatusInfo(new Application()));
        }

        /// <summary>Gets the currently configured custom error mode.</summary>
        /// <param name="appStatus">The application status info.</param>
        /// <returns>The currently configured custom error mode string.</returns>
        public static string GetCustomErrorMode(IApplicationStatusInfo appStatus)
        {
            var configNav = Load(appStatus).CreateNavigator();

            // Select the location node
            var customErrorsNav = configNav.SelectSingleNode("//configuration/system.web/customErrors|//configuration/location/system.web/customErrors");

            var customErrorMode = XmlUtils.GetAttributeValue(customErrorsNav, "mode");
            if (string.IsNullOrEmpty(customErrorMode))
            {
                customErrorMode = "RemoteOnly";
            }

            return customErrorsNav != null ? customErrorMode : "RemoteOnly";
        }

        /// <summary>Loads a configuration file as an XML document.</summary>
        /// <param name="filename">The configuration file name.</param>
        /// <returns>The configuration as an XML document.</returns>
        [DnnDeprecated(9, 11, 1, "Use overload taking an IApplicationStatusInfo")]
        public static partial XmlDocument Load(string filename)
        {
            return Load(
                Globals.GetCurrentServiceProvider().GetService<IApplicationStatusInfo>() ?? new ApplicationStatusInfo(new Application()),
                filename);
        }

        /// <summary>Loads a configuration file as an XML document.</summary>
        /// <param name="appStatus">The application status info.</param>
        /// <param name="filename">The configuration file name.</param>
        /// <returns>The configuration as an XML document.</returns>
        public static XmlDocument Load(IApplicationStatusInfo appStatus, string filename)
        {
            // open the config file
            var xmlDoc = new XmlDocument { XmlResolver = null };
            var configPath = Path.GetFullPath(Path.Combine(appStatus.ApplicationMapPath, filename.TrimStart(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar)));
            if (!configPath.StartsWith(appStatus.ApplicationMapPath, StringComparison.Ordinal))
            {
                throw new SecurityException($"Unable to load config for \"{filename}\"");
            }

            xmlDoc.Load(configPath);

            // test for namespace added by Web Admin Tool
            if (!string.IsNullOrEmpty(xmlDoc.DocumentElement.GetAttribute("xmlns")))
            {
                // remove namespace
                var strDoc = xmlDoc.InnerXml.Replace("xmlns=\"http://schemas.microsoft.com/.NetConfiguration/v2.0\"", string.Empty);
                xmlDoc.LoadXml(strDoc);
            }

            return xmlDoc;
        }

        /// <summary>Removes a code subdirectory for the web.config file.</summary>
        /// <param name="name">The name of the code subdirectory.</param>
        [DnnDeprecated(9, 11, 1, "Use overload taking an IApplicationStatusInfo")]
        public static partial void RemoveCodeSubDirectory(string name)
        {
            RemoveCodeSubDirectory(
                Globals.GetCurrentServiceProvider().GetService<IApplicationStatusInfo>() ?? new ApplicationStatusInfo(new Application()),
                name);
        }

        /// <summary>Removes a code subdirectory for the web.config file.</summary>
        /// <param name="appStatus">The application status info.</param>
        /// <param name="name">The name of the code subdirectory.</param>
        public static void RemoveCodeSubDirectory(IApplicationStatusInfo appStatus, string name)
        {
            var xmlConfig = Load(appStatus);

            // Select the location node
            var xmlCompilation = xmlConfig.SelectSingleNode("configuration/system.web/compilation");
            if (xmlCompilation == null)
            {
                // Try location node
                xmlCompilation = xmlConfig.SelectSingleNode("configuration/location/system.web/compilation");
            }

            // Get the CodeSubDirectories Node
            var xmlSubDirectories = xmlCompilation.SelectSingleNode("codeSubDirectories");
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
            var xmlSubDirectory = xmlSubDirectories.SelectSingleNode($"add[@directoryName='{codeSubDirectoryName}']");
            if (xmlSubDirectory != null)
            {
                // Remove Node
                xmlSubDirectories.RemoveChild(xmlSubDirectory);

                Save(appStatus, xmlConfig);
            }
        }

        /// <summary>Save the web.config file.</summary>
        /// <param name="appStatus">The application status info.</param>
        /// <param name="xmlDoc">The configuration as an XML document.</param>
        /// <returns>An empty string upon success or the error message upon failure.</returns>
        public static string Save(IApplicationStatusInfo appStatus, XmlDocument xmlDoc)
        {
            return Save(appStatus, xmlDoc, "web.config");
        }

        /// <summary>Save the web.config file.</summary>
        /// <param name="xmlDoc">The configuration as an XML document.</param>
        /// <returns>An empty string upon success or the error message upon failure.</returns>
        [DnnDeprecated(9, 11, 1, "Use overload taking an IApplicationStatusInfo")]
        public static partial string Save(XmlDocument xmlDoc)
        {
            return Save(xmlDoc, "web.config");
        }

        /// <summary>Save an XML document to the application root folder.</summary>
        /// <param name="xmlDoc">The configuration as an XML document.</param>
        /// <param name="filename">The file name to save to.</param>
        /// <returns>An empty string upon success or the error message upon failure.</returns>
        [DnnDeprecated(9, 11, 1, "Use overload taking an IApplicationStatusInfo")]
        public static partial string Save(XmlDocument xmlDoc, string filename)
        {
            return Save(
                Globals.GetCurrentServiceProvider().GetService<IApplicationStatusInfo>() ?? new ApplicationStatusInfo(new Application()),
                xmlDoc,
                filename);
        }

        /// <summary>Save an XML document to the application root folder.</summary>
        /// <param name="appStatus">The application status info.</param>
        /// <param name="xmlDoc">The configuration as an XML document.</param>
        /// <param name="filename">The file name to save to.</param>
        /// <returns>An empty string upon success or the error message upon failure.</returns>
        public static string Save(IApplicationStatusInfo appStatus, XmlDocument xmlDoc, string filename)
        {
            var retMsg = string.Empty;
            try
            {
                var strFilePath = Path.Combine(appStatus.ApplicationMapPath, filename.TrimStart(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar));
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
                const int MaxRetries = 4;
                const double Multiplier = 2.5;
                for (var retry = MaxRetries; retry >= 0; retry--)
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
                        Thread.Sleep((int)(Multiplier * (MaxRetries - retry + 1)) * 1000);
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

        /// <summary>Touches the web.config file to force the application to reload.</summary>
        /// <returns>A value indicating whether the operation succeeded.</returns>
        [DnnDeprecated(9, 11, 1, "Use overload taking an IApplicationStatusInfo")]
        public static partial bool Touch()
        {
            return Touch(
                Globals.GetCurrentServiceProvider().GetService<IApplicationStatusInfo>() ?? new ApplicationStatusInfo(new Application()));
        }

        /// <summary>Touches the web.config file to force the application to reload.</summary>
        /// <param name="appStatus">The application status info.</param>
        /// <returns>A value indicating whether the operation succeeded.</returns>
        public static bool Touch(IApplicationStatusInfo appStatus)
        {
            var configPath = Path.Combine(appStatus.ApplicationMapPath, "web.config");
            try
            {
                RetryableAction.Retry5TimesWith2SecondsDelay(
                    () => File.SetLastWriteTime(configPath, DateTime.Now), "Touching config file");
                return true;
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
                return false;
            }
        }

        /// <summary>Updates the database connection string.</summary>
        /// <param name="conn">The connection string value.</param>
        [DnnDeprecated(9, 11, 1, "Use overload taking an IApplicationStatusInfo")]
        public static partial void UpdateConnectionString(string conn)
        {
            UpdateConnectionString(
                Globals.GetCurrentServiceProvider().GetService<IApplicationStatusInfo>() ?? new ApplicationStatusInfo(new Application()),
                conn);
        }

        /// <summary>Updates the database connection string.</summary>
        /// <param name="appStatus">The application status info.</param>
        /// <param name="conn">The connection string value.</param>
        public static void UpdateConnectionString(IApplicationStatusInfo appStatus, string conn)
        {
            var xmlConfig = Load(appStatus);
            var name = GetDefaultProvider("data").Attributes["connectionStringName"];

            // Update ConnectionStrings
            var xmlConnection = xmlConfig.SelectSingleNode("configuration/connectionStrings/add[@name='" + name + "']");
            XmlUtils.UpdateAttribute(xmlConnection, "connectionString", conn);

            // Update AppSetting
            var xmlAppSetting = xmlConfig.SelectSingleNode("configuration/appSettings/add[@key='" + name + "']");
            XmlUtils.UpdateAttribute(xmlAppSetting, "value", conn);

            // Save changes
            Save(appStatus, xmlConfig);
        }

        /// <summary>Updates the data provider configuration.</summary>
        /// <param name="name">The data provider name.</param>
        /// <param name="databaseOwner">The database owner, usually dbo.</param>
        /// <param name="objectQualifier">The object qualifier if multiple Dnn instance run under the same database (not recommended).</param>
        [DnnDeprecated(9, 11, 1, "Use overload taking an IApplicationStatusInfo")]
        public static partial void UpdateDataProvider(string name, string databaseOwner, string objectQualifier)
        {
            UpdateDataProvider(
                Globals.GetCurrentServiceProvider().GetService<IApplicationStatusInfo>() ?? new ApplicationStatusInfo(new Application()),
                name,
                databaseOwner,
                objectQualifier);
        }

        /// <summary>Updates the data provider configuration.</summary>
        /// <param name="appStatus">The application status info.</param>
        /// <param name="name">The data provider name.</param>
        /// <param name="databaseOwner">The database owner, usually dbo.</param>
        /// <param name="objectQualifier">The object qualifier if multiple Dnn instance run under the same database (not recommended).</param>
        public static void UpdateDataProvider(IApplicationStatusInfo appStatus, string name, string databaseOwner, string objectQualifier)
        {
            var xmlConfig = Load(appStatus);

            // Update provider
            var xmlProvider = xmlConfig.SelectSingleNode($"configuration/dotnetnuke/data/providers/add[@name='{name}']");
            XmlUtils.UpdateAttribute(xmlProvider, "databaseOwner", databaseOwner);
            XmlUtils.UpdateAttribute(xmlProvider, "objectQualifier", objectQualifier);

            // Save changes
            Save(appStatus, xmlConfig);
        }

        /// <summary>Updates the specified upgrade connection string.</summary>
        /// <param name="name">The connection string name.</param>
        /// <param name="upgradeConnectionString">The new value for the connection string.</param>
        [DotNetNuke.Internal.SourceGenerators.DnnDeprecated(9, 11, 1, "Use overload taking an IApplicationStatusInfo")]
        public static partial void UpdateUpgradeConnectionString(string name, string upgradeConnectionString)
        {
            UpdateUpgradeConnectionString(
                Globals.GetCurrentServiceProvider().GetService<IApplicationStatusInfo>() ?? new ApplicationStatusInfo(new Application()),
                name,
                upgradeConnectionString);
        }

        /// <summary>Updates the specified upgrade connection string.</summary>
        /// <param name="appStatus">The application status info.</param>
        /// <param name="name">The connection string name.</param>
        /// <param name="upgradeConnectionString">The new value for the connection string.</param>
        public static void UpdateUpgradeConnectionString(IApplicationStatusInfo appStatus, string name, string upgradeConnectionString)
        {
            var xmlConfig = Load(appStatus);

            // Update provider
            var xmlProvider = xmlConfig.SelectSingleNode($"configuration/dotnetnuke/data/providers/add[@name='{name}']");
            XmlUtils.UpdateAttribute(xmlProvider, "upgradeConnectionString", upgradeConnectionString);

            // Save changes
            Save(appStatus, xmlConfig);
        }

        /// <summary>Updates the unique machine key. Warning: Do not change this after installation unless you know what your are doing.</summary>
        /// <returns>An empty string upon success or an error message upon failure.</returns>
        [DnnDeprecated(9, 11, 1, "Use overload taking an IApplicationStatusInfo")]
        public static partial string UpdateMachineKey()
        {
            return UpdateMachineKey(
                Globals.GetCurrentServiceProvider().GetService<IApplicationStatusInfo>() ?? new ApplicationStatusInfo(new Application()));
        }

        /// <summary>Updates the unique machine key. Warning: Do not change this after installation unless you know what your are doing.</summary>
        /// <param name="appStatus">The application status info.</param>
        /// <returns>An empty string upon success or an error message upon failure.</returns>
        public static string UpdateMachineKey(IApplicationStatusInfo appStatus)
        {
            var xmlConfig = new XmlDocument { XmlResolver = null };
            var strError = string.Empty;

            // save the current config files
            BackupConfig(appStatus);
            try
            {
                // open the web.config
                xmlConfig = Load(appStatus);

                // create random keys for the Membership machine keys
                xmlConfig = UpdateMachineKey(xmlConfig);
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                strError += ex.Message;
            }

            // save a copy of the web.config
            strError += Save(appStatus, xmlConfig, GetTimestampedBackupPath(appStatus, "web_.config"));

            // save the web.config
            strError += Save(appStatus, xmlConfig);

            return strError;
        }

        /// <summary>Updates the unique machine key. Warning: Do not change this after installation unless you know what your are doing.</summary>
        /// <param name="xmlConfig">The configuration XML document.</param>
        /// <returns>The newly modified XML document.</returns>
        public static XmlDocument UpdateMachineKey(XmlDocument xmlConfig)
        {
            var portalSecurity = PortalSecurity.Instance;
            var validationKey = portalSecurity.CreateKey(20);
            var decryptionKey = portalSecurity.CreateKey(24);

            var xmlMachineKey = xmlConfig.SelectSingleNode("configuration/system.web/machineKey");
            XmlUtils.UpdateAttribute(xmlMachineKey, "validationKey", validationKey);
            XmlUtils.UpdateAttribute(xmlMachineKey, "decryptionKey", decryptionKey);

            return AddAppSetting(xmlConfig, "InstallationDate", DateTime.Today.ToString("d", new CultureInfo("en-US")));
        }

        /// <summary>Updates the validation key. WARNING: Do not call this API unless you now what you are doing.</summary>
        /// <returns>An empty string upon success or an error message upon failure.</returns>
        [DnnDeprecated(9, 11, 1, "Use overload taking an IApplicationStatusInfo")]
        public static partial string UpdateValidationKey()
        {
            return UpdateValidationKey(
                Globals.GetCurrentServiceProvider().GetService<IApplicationStatusInfo>() ?? new ApplicationStatusInfo(new Application()));
        }

        /// <summary>Updates the validation key. WARNING: Do not call this API unless you now what you are doing.</summary>
        /// <param name="appStatus">The application status info.</param>
        /// <returns>An empty string upon success or an error message upon failure.</returns>
        public static string UpdateValidationKey(IApplicationStatusInfo appStatus)
        {
            var xmlConfig = new XmlDocument { XmlResolver = null };
            var strError = string.Empty;

            // save the current config files
            BackupConfig(appStatus);
            try
            {
                // open the web.config
                xmlConfig = Load(appStatus);

                // create random keys for the Membership machine keys
                xmlConfig = UpdateValidationKey(xmlConfig);
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                strError += ex.Message;
            }

            // save a copy of the web.config
            strError += Save(appStatus, xmlConfig, GetTimestampedBackupPath(appStatus, "web_.config"));

            // save the web.config
            strError += Save(appStatus, xmlConfig);
            return strError;
        }

        /// <summary>Updates the validation key. WARNING: Do not call this APi unless you now what you are doing.</summary>
        /// <param name="xmlConfig">The XML configuration document.</param>
        /// <returns>The newly modified XML configuration document.</returns>
        public static XmlDocument UpdateValidationKey(XmlDocument xmlConfig)
        {
            var xmlMachineKey = xmlConfig.SelectSingleNode("configuration/system.web/machineKey");
            if (xmlMachineKey.Attributes["validationKey"].Value == "F9D1A2D3E1D3E2F7B3D9F90FF3965ABDAC304902")
            {
                var objSecurity = PortalSecurity.Instance;
                var validationKey = objSecurity.CreateKey(20);
                XmlUtils.UpdateAttribute(xmlMachineKey, "validationKey", validationKey);
            }

            return xmlConfig;
        }

        /// <summary>Gets the path for the specified Config file.</summary>
        /// <param name="file">The config.file to get the path for.</param>
        /// <returns>fully qualified path to the file.</returns>
        /// <remarks>Will copy the file from the template directory as required.</remarks>
        [DnnDeprecated(9, 11, 1, "Use overload taking an IApplicationStatusInfo")]
        public static partial string GetPathToFile(ConfigFileType file)
        {
            return GetPathToFile(file, false);
        }

        /// <summary>Gets the path for the specified Config file.</summary>
        /// <param name="appStatus">The application status info.</param>
        /// <param name="file">The config.file to get the path for.</param>
        /// <returns>fully qualified path to the file.</returns>
        /// <remarks>Will copy the file from the template directory as required.</remarks>
        public static string GetPathToFile(IApplicationStatusInfo appStatus, ConfigFileType file)
        {
            return GetPathToFile(appStatus, file, false);
        }

        /// <summary>Gets the path for the specified Config file.</summary>
        /// <param name="file">The config.file to get the path for.</param>
        /// <param name="overwrite">force an overwrite of the config file.</param>
        /// <returns>fully qualified path to the file.</returns>
        /// <remarks>Will copy the file from the template directory as required.</remarks>
        [DnnDeprecated(9, 11, 1, "Use overload taking an IApplicationStatusInfo")]
        public static partial string GetPathToFile(ConfigFileType file, bool overwrite)
        {
            return GetPathToFile(
                Globals.GetCurrentServiceProvider().GetService<IApplicationStatusInfo>() ?? new ApplicationStatusInfo(new Application()),
                file,
                overwrite);
        }

        /// <summary>Gets the path for the specified Config file.</summary>
        /// <param name="appStatus">The application status info.</param>
        /// <param name="file">The config.file to get the path for.</param>
        /// <param name="overwrite">force an overwrite of the config file.</param>
        /// <returns>fully qualified path to the file.</returns>
        /// <remarks>Will copy the file from the template directory as required.</remarks>
        public static string GetPathToFile(IApplicationStatusInfo appStatus, ConfigFileType file, bool overwrite)
        {
            var fileName = EnumToFileName(file);
            var path = Path.Combine(appStatus.ApplicationMapPath, fileName);

            if (!File.Exists(path) || overwrite)
            {
                // Copy from \Config
                var pathToDefault = Path.Combine(appStatus.ApplicationMapPath, Globals.glbConfigFolder.TrimStart('\\'), fileName);
                if (File.Exists(pathToDefault))
                {
                    File.Copy(pathToDefault, path, true);
                }
            }

            return path;
        }

        /// <summary>UpdateInstallVersion, but only if the setting does not already exist.</summary>
        /// <param name="version">The version to update to.</param>
        /// <returns>An empty string upon success or an error message upon failure.</returns>
        [DnnDeprecated(9, 11, 1, "Use overload taking an IApplicationStatusInfo")]
        public static partial string UpdateInstallVersion(Version version)
        {
            return UpdateInstallVersion(
                Globals.GetCurrentServiceProvider().GetService<IApplicationStatusInfo>() ?? new ApplicationStatusInfo(new Application()),
                version);
        }

        /// <summary>UpdateInstallVersion, but only if the setting does not already exist.</summary>
        /// <param name="appStatus">The application status info.</param>
        /// <param name="version">The version to update to.</param>
        /// <returns>An empty string upon success or an error message upon failure.</returns>
        public static string UpdateInstallVersion(IApplicationStatusInfo appStatus, Version version)
        {
            var strError = string.Empty;

            var installVersion = GetSetting("InstallVersion");
            if (string.IsNullOrEmpty(installVersion))
            {
                // we need to add the InstallVersion
                var xmlConfig = new XmlDocument { XmlResolver = null };

                // save the current config files
                BackupConfig(appStatus);
                try
                {
                    // open the web.config
                    xmlConfig = Load(appStatus);

                    // Update the InstallVersion
                    xmlConfig = UpdateInstallVersion(xmlConfig, version);
                }
                catch (Exception ex)
                {
                    Logger.Error(ex);
                    strError += ex.Message;
                }

                // save a copy of the web.config
                strError += Save(appStatus, xmlConfig, GetTimestampedBackupPath(appStatus, "web_.config"));

                // save the web.config
                strError += Save(appStatus, xmlConfig);
            }

            return strError;
        }

        /// <summary>Checks if .Net Framework 4.5 or above is in use.</summary>
        /// <returns>A value indicating whether .Net Framework 4.5 or above is in use.</returns>
        public static bool IsNet45OrNewer()
        {
            // Class "ReflectionContext" exists from .NET 4.5 onwards.
            return Type.GetType("System.Reflection.ReflectionContext", false) != null;
        }

        /// <summary>Adds the File Change Notification (FCN) mode to the web.config if it does not yet exist.</summary>
        /// <param name="fcnMode">The file change notification (FNC) mode.</param>
        /// <returns>Always an empty string.</returns>
        [DnnDeprecated(9, 11, 1, "Use overload taking an IApplicationStatusInfo")]
        public static partial string AddFCNMode(FcnMode fcnMode)
        {
            return AddFCNMode(
                Globals.GetCurrentServiceProvider().GetService<IApplicationStatusInfo>() ?? new ApplicationStatusInfo(new Application()),
                fcnMode);
        }

        /// <summary>Adds the File Change Notification (FCN) mode to the web.config if it does not yet exist.</summary>
        /// <param name="appStatus">The application status info.</param>
        /// <param name="fcnMode">The file change notification (FNC) mode.</param>
        /// <returns>Always an empty string.</returns>
        public static string AddFCNMode(IApplicationStatusInfo appStatus, FcnMode fcnMode)
        {
            try
            {
                // check current .net version and if attribute has been added already
                if (IsNet45OrNewer() && GetFcnMode() != fcnMode.ToString())
                {
                    // open the web.config
                    var xmlConfig = Load(appStatus);

                    var xmlHttpRunTimeKey = xmlConfig.SelectSingleNode("configuration/system.web/httpRuntime") ??
                                                xmlConfig.SelectSingleNode("configuration/location/system.web/httpRuntime");
                    XmlUtils.CreateAttribute(xmlConfig, xmlHttpRunTimeKey, "fcnMode", fcnMode.ToString());

                    // save the web.config
                    Save(appStatus, xmlConfig);
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

        private static string GetTimestampedBackupPath(IApplicationStatusInfo appStatus, string fileName)
        {
            return Path.Combine(
                appStatus.ApplicationMapPath,
                Globals.glbConfigFolder.TrimStart('\\'),
                $"Backup_{DateTime.Now:yyyyMMddHHmm}",
                fileName);
        }
    }
}
