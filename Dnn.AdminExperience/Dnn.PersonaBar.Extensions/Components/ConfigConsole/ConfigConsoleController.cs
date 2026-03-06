// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.PersonaBar.ConfigConsole.Components
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Xml;
    using System.Xml.Schema;

    using DotNetNuke.Abstractions.Application;
    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Instrumentation;
    using DotNetNuke.Internal.SourceGenerators;
    using DotNetNuke.Services.FileSystem.Internal;
    using DotNetNuke.Services.Installer;

    using Microsoft.Extensions.DependencyInjection;

    /// <summary>Contains business logic for the Config Console component in the Persona Bar.</summary>
    public partial class ConfigConsoleController
    {
        /// <summary>Name of the Web configuration file.</summary>
        internal const string WebConfig = "Web.config";

        private const string CONFIGEXT = ".config";
        private const string ROBOTSEXT = "robots.txt";  // in multi-portal instances, there may be multiple robots.txt files (e.g., site1.com.robots.txt, site2.com.robots.txt, etc.)
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(ConfigConsoleController));

        private readonly IApplicationStatusInfo appStatus;
        private readonly IApplicationInfo appInfo;
        private readonly IDirectory directoryApi;
        private readonly IFile fileApi;

        /// <summary>Initializes a new instance of the <see cref="ConfigConsoleController"/> class.</summary>
        [Obsolete("Deprecated in DotNetNuke 10.2.2. Please use overload with IPermissionDefinitionService. Scheduled removal in v12.0.0.")]
        public ConfigConsoleController()
            : this(null, null, null, null)
        {
        }

        /// <summary>Initializes a new instance of the <see cref="ConfigConsoleController"/> class.</summary>
        /// <param name="appStatus">The application status.</param>
        /// <param name="appInfo">The application info.</param>
        /// <param name="directoryApi">The directory API.</param>
        /// <param name="fileApi">The file API.</param>
        public ConfigConsoleController(IApplicationStatusInfo appStatus, IApplicationInfo appInfo, IDirectory directoryApi, IFile fileApi)
        {
            this.appStatus = appStatus ?? Globals.GetCurrentServiceProvider().GetRequiredService<IApplicationStatusInfo>();
            this.appInfo = appInfo ?? Globals.GetCurrentServiceProvider().GetRequiredService<IApplicationInfo>();
            this.directoryApi = directoryApi ?? Globals.GetCurrentServiceProvider().GetRequiredService<IDirectory>();
            this.fileApi = fileApi ?? Globals.GetCurrentServiceProvider().GetRequiredService<IFile>();
        }

        /// <summary>Gets the config file list.</summary>
        /// <returns>A sequence of file names.</returns>
        [DnnDeprecated(10, 2, 2, "Use GetConfigFilesListAsync")]
        public partial IEnumerable<string> GetConfigFilesList()
        {
            return
                from file in this.directoryApi.GetFiles(this.appStatus.ApplicationMapPath)
                where file.EndsWith(CONFIGEXT, StringComparison.OrdinalIgnoreCase) || file.EndsWith(ROBOTSEXT, StringComparison.OrdinalIgnoreCase)
                select Path.GetFileName(file);
        }

        /// <summary>Gets the config file list.</summary>
        /// <returns>A sequence of file names.</returns>
        public async Task<IEnumerable<string>> GetConfigFilesListAsync()
        {
            return
                from file in await this.directoryApi.GetFilesAsync(this.appStatus.ApplicationMapPath)
                where file.EndsWith(CONFIGEXT, StringComparison.OrdinalIgnoreCase) || file.EndsWith(ROBOTSEXT, StringComparison.OrdinalIgnoreCase)
                select Path.GetFileName(file);
        }

        /// <summary>Get the contents of the config file.</summary>
        /// <param name="configFile">The file name.</param>
        /// <returns>The file contents.</returns>
        [DnnDeprecated(10, 2, 2, "Use GetConfigFileAsync")]
        public partial string GetConfigFile(string configFile)
        {
            this.ValidateFilePath(configFile);

            if (!configFile.EndsWith(CONFIGEXT, StringComparison.InvariantCultureIgnoreCase))
            {
                return File.ReadAllText(Path.Combine(this.appStatus.ApplicationMapPath, configFile));
            }

            var configDoc = Config.Load(this.appStatus, configFile);
            using var txtWriter = new StringWriter();
            using (var writer = new XmlTextWriter(txtWriter))
            {
                writer.Formatting = Formatting.Indented;
                configDoc.WriteTo(writer);
            }

            return txtWriter.ToString();
        }

        /// <summary>Get the contents of the config file.</summary>
        /// <param name="configFile">The file name.</param>
        /// <returns>The file contents.</returns>
        public async Task<string> GetConfigFileAsync(string configFile)
        {
            await this.ValidateFilePathAsync(configFile);

            if (!configFile.EndsWith(CONFIGEXT, StringComparison.InvariantCultureIgnoreCase))
            {
                var configFilePath = Path.Combine(this.appStatus.ApplicationMapPath, configFile);
                using var streamReader = new StreamReader(this.fileApi.OpenRead(configFilePath));
                return await streamReader.ReadToEndAsync();
            }

            var configDoc = Config.Load(this.appStatus, configFile);
            using var txtWriter = new StringWriter();
            using (var writer = new XmlTextWriter(txtWriter))
            {
                writer.Formatting = Formatting.Indented;
                configDoc.WriteTo(writer);
            }

            return txtWriter.ToString();
        }

        /// <summary>Update the contents of a config file.</summary>
        /// <param name="fileName">The file name.</param>
        /// <param name="fileContent">The file contents.</param>
        [DnnDeprecated(10, 2, 2, "Use UpdateConfigFileAsync")]
        public partial void UpdateConfigFile(string fileName, string fileContent)
        {
            this.ValidateFilePath(fileName);

            if (fileName.EndsWith(CONFIGEXT, StringComparison.InvariantCultureIgnoreCase))
            {
                var configDoc = new XmlDocument { XmlResolver = null, };
                using (var configReader = XmlReader.Create(new StringReader(fileContent), new XmlReaderSettings { XmlResolver = null, }))
                {
                    configDoc.Load(configReader);
                }

                Config.Save(this.appStatus, configDoc, fileName);
            }
            else
            {
                this.SaveNonConfig(fileContent, fileName);
            }
        }

        /// <summary>Update the contents of a config file.</summary>
        /// <param name="fileName">The file name.</param>
        /// <param name="fileContent">The file contents.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task UpdateConfigFileAsync(string fileName, string fileContent)
        {
            await this.ValidateFilePathAsync(fileName);

            if (fileName.EndsWith(CONFIGEXT, StringComparison.InvariantCultureIgnoreCase))
            {
                var configDoc = new XmlDocument { XmlResolver = null, };
                using (var configReader = XmlReader.Create(new StringReader(fileContent), new XmlReaderSettings { XmlResolver = null, }))
                {
                    configDoc.Load(configReader);
                }

                Config.Save(this.appStatus, configDoc, fileName);
            }
            else
            {
                await this.SaveNonConfigAsync(fileContent, fileName);
            }
        }

        /// <summary>Validates a config file against a well known schema.</summary>
        /// <param name="fileName">The config file name.</param>
        /// <param name="fileContent">The contents of the config file.</param>
        /// <returns>A list of validation errors.</returns>
        [DnnDeprecated(10, 2, 2, "Use ValidateConfigFileAsync")]
        public partial IEnumerable<string> ValidateConfigFile(string fileName, string fileContent)
        {
            this.ValidateFilePath(fileName);

            if (!fileName.EndsWith(CONFIGEXT, StringComparison.InvariantCultureIgnoreCase))
            {
                return [];
            }

            if (!fileName.EndsWith(WebConfig, StringComparison.InvariantCultureIgnoreCase))
            {
                return [];
            }

            var configDoc = new XmlDocument { XmlResolver = null, };
            using (var configReader = XmlReader.Create(new StringReader(fileContent), new XmlReaderSettings { XmlResolver = null, }))
            {
                configDoc.Load(configReader);
            }

            return ValidateSchema(configDoc, "Schemas/DotNetConfig.xsd");
        }

        /// <summary>Validates a config file against a well known schema.</summary>
        /// <param name="fileName">The config file name.</param>
        /// <param name="fileContent">The contents of the config file.</param>
        /// <returns>A list of validation errors.</returns>
        public async Task<IEnumerable<string>> ValidateConfigFileAsync(string fileName, string fileContent)
        {
            await this.ValidateFilePathAsync(fileName);

            if (!fileName.EndsWith(CONFIGEXT, StringComparison.InvariantCultureIgnoreCase))
            {
                return [];
            }

            if (!fileName.EndsWith(WebConfig, StringComparison.InvariantCultureIgnoreCase))
            {
                return [];
            }

            var configDoc = new XmlDocument { XmlResolver = null, };
            using (var configReader = XmlReader.Create(new StringReader(fileContent), new XmlReaderSettings { XmlResolver = null, }))
            {
                configDoc.Load(configReader);
            }

            return await ValidateSchemaAsync(configDoc, "Schemas/DotNetConfig.xsd");
        }

        /// <summary>Applies an XML Merge document.</summary>
        /// <param name="fileContent">The XML Merge document contents.</param>
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Breaking change")]
        public void MergeConfigFile(string fileContent)
        {
            if (!IsValidXmlMergeDocument(fileContent))
            {
                return;
            }

            var doc = new XmlDocument { XmlResolver = null, };
            using (var mergeReader = XmlReader.Create(new StringReader(fileContent), new XmlReaderSettings { XmlResolver = null, }))
            {
                doc.Load(mergeReader);
            }

            var merge = new XmlMerge(doc, Globals.FormatVersion(this.appInfo.Version), this.appInfo.Description);
            merge.UpdateConfigs();
        }

        [DnnDeprecated(10, 2, 2, "Use ValidateSchemaAsync")]
        private static partial List<string> ValidateSchema(XmlDocument configDoc, string schemaRelPath)
        {
            var errors = new List<string>();

            configDoc.Schemas.Add(LoadSchema(schemaRelPath));
            configDoc.Validate((_, e) => errors.Add(e.Message));

            return errors;

            static XmlSchema LoadSchema(string schemaRelPath)
            {
                var xsd = LoadResource(schemaRelPath);

                using var reader = new StringReader(xsd);
                return XmlSchema.Read(XmlReader.Create(reader), (_, _) => { });
            }

            static string LoadResource(string relativePath)
            {
                var segments = relativePath.Split(['/',], StringSplitOptions.RemoveEmptyEntries);
                var relativeName = string.Join(".", segments);
                var name = $"Dnn.PersonaBar.Extensions.Components.ConfigConsole.{relativeName}";

                using var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(name);
                using var reader = new StreamReader(stream);
                return reader.ReadToEnd();
            }
        }

        private static async Task<List<string>> ValidateSchemaAsync(XmlDocument configDoc, string schemaRelPath)
        {
            var errors = new List<string>();

            configDoc.Schemas.Add(await LoadSchema(schemaRelPath));
            configDoc.Validate((_, e) => errors.Add(e.Message));

            return errors;

            static async Task<XmlSchema> LoadSchema(string schemaRelPath)
            {
                var xsd = await LoadResource(schemaRelPath);

                using var reader = new StringReader(xsd);
                return XmlSchema.Read(XmlReader.Create(reader), (_, _) => { });
            }

            static async Task<string> LoadResource(string relativePath)
            {
                var segments = relativePath.Split(['/',], StringSplitOptions.RemoveEmptyEntries);
                var relativeName = string.Join(".", segments);
                var name = $"Dnn.PersonaBar.Extensions.Components.ConfigConsole.{relativeName}";

                using var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(name);
                using var reader = new StreamReader(stream);
                return await reader.ReadToEndAsync();
            }
        }

        private static bool IsValidXmlMergeDocument(string mergeDocText)
        {
            if (string.IsNullOrEmpty(mergeDocText.Trim()))
            {
                return false;
            }

            // TODO: Add more checks here
            return true;
        }

        [DnnDeprecated(10, 2, 2, "Use SaveNonConfigAsync")]
        private partial string SaveNonConfig(string document, string filename)
        {
            var retMsg = string.Empty;
            try
            {
                var strFilePath = Path.Combine(this.appStatus.ApplicationMapPath, filename);
                var existingFileAttributes = FileAttributes.Normal;
                if (this.fileApi.Exists(strFilePath))
                {
                    // save current file attributes
                    existingFileAttributes = this.fileApi.GetAttributes(strFilePath);

                    // change to normal ( in case it is flagged as read-only )
                    this.fileApi.SetAttributes(strFilePath, FileAttributes.Normal);
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
                        this.fileApi.WriteAllText(strFilePath, document);

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
                this.fileApi.SetAttributes(strFilePath, existingFileAttributes);
            }
            catch (Exception exc)
            {
                // the file permissions may not be set properly
                Logger.Error(exc);
                retMsg = exc.Message;
            }

            return retMsg;
        }

        private async Task<string> SaveNonConfigAsync(string document, string filename)
        {
            var retMsg = string.Empty;
            try
            {
                var strFilePath = Path.Combine(this.appStatus.ApplicationMapPath, filename);
                var existingFileAttributes = FileAttributes.Normal;
                if (await this.fileApi.ExistsAsync(strFilePath))
                {
                    // save current file attributes
                    existingFileAttributes = await this.fileApi.GetAttributesAsync(strFilePath);

                    // change to normal ( in case it is flagged as read-only )
                    await this.fileApi.SetAttributesAsync(strFilePath, FileAttributes.Normal);
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
                        await this.fileApi.WriteAllTextAsync(strFilePath, document);

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
                await this.fileApi.SetAttributesAsync(strFilePath, existingFileAttributes);
            }
            catch (Exception exc)
            {
                // the file permissions may not be set properly
                Logger.Error(exc);
                retMsg = exc.Message;
            }

            return retMsg;
        }

        [DnnDeprecated(10, 2, 2, "Use ValidateFilePathAsync")]
        private partial void ValidateFilePath(string filePath)
        {
            var configFileNames = this.GetConfigFilesList();
            if (!configFileNames.ToHashSet(StringComparer.OrdinalIgnoreCase).Contains(filePath))
            {
                throw new ArgumentException("Invalid File Path");
            }
        }

        private async Task ValidateFilePathAsync(string filePath)
        {
            var configFileNames = await this.GetConfigFilesListAsync();
            if (!configFileNames.ToHashSet(StringComparer.OrdinalIgnoreCase).Contains(filePath))
            {
                throw new ArgumentException("Invalid File Path");
            }
        }
    }
}
