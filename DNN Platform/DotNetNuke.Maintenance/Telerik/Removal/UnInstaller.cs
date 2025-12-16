// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Maintenance.Telerik.Removal
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.IO;

    using DotNetNuke.Maintenance.Telerik.Steps;

    /// <summary>Base class for uninstalling Telerik and related components.</summary>
    internal class UnInstaller
    {
        private readonly IServiceProvider serviceProvider;
        private readonly ILocalizer localizer;

        /// <summary>Initializes a new instance of the <see cref="UnInstaller"/> class.</summary>
        /// <param name="serviceProvider">Instance of <see cref="IServiceProvider"/>.</param>
        /// <param name="localizer">Instance of <see cref="ILocalizer"/>.</param>
        public UnInstaller(IServiceProvider serviceProvider, ILocalizer localizer)
        {
            this.serviceProvider = serviceProvider ??
                throw new ArgumentNullException(nameof(serviceProvider));

            this.localizer = localizer ??
                throw new ArgumentNullException(nameof(localizer));

            this.ProgressInternal = new List<UninstallSummaryItem>();
        }

        /// <summary>
        /// Gets an <see cref="IEnumerable{T}"/> of type <see cref="UninstallSummaryItem"/>
        /// containing the overall execution progress.
        /// </summary>
        public IEnumerable<UninstallSummaryItem> Progress => this.ProgressInternal;

        /// <summary>
        /// Gets or sets an <see cref="List{T}"/> of type <see cref="UninstallSummaryItem"/>
        /// containing the overall execution progress.
        /// </summary>
        private protected List<UninstallSummaryItem> ProgressInternal { get; set; }

        /// <summary>Returns a step to replace one module by another.</summary>
        /// <param name="oldModuleName">Modulename of the module to be replaced.</param>
        /// <param name="newModuleName">Modulename of the module to be put in its place.</param>
        /// <param name="migrateSettings">Callback function to migrate settings.</param>
        /// <returns><see cref="IStep"/> of the module replacer.</returns>
        private protected IStep ReplaceModule(string oldModuleName, string newModuleName, Func<Hashtable, Hashtable> migrateSettings)
        {
            var step = this.GetService<IReplaceTabModuleStep>();
            step.OldModuleName = oldModuleName;
            step.NewModuleName = newModuleName;
            step.MigrateSettings = migrateSettings;
            return step;
        }

        /// <summary>Returns a step to remove a package from the DNN installation.</summary>
        /// <param name="packageName">Name of the package.</param>
        /// <param name="deleteFiles">Whether to delete installed files.</param>
        /// <returns><see cref="IStep"/> of the package remover.</returns>
        private protected IStep RemoveExtension(string packageName, bool deleteFiles = true)
        {
            var step = this.GetService<IRemoveExtensionStep>();
            step.DeleteFiles = deleteFiles;
            step.PackageName = packageName;
            return step;
        }

        /// <summary>Returns a step to remove a file from the DNN installation.</summary>
        /// <param name="relativePath">Path to the directory to search, relative to the application root path.</param>
        /// <param name="searchPattern">Search string to match against the names of files in path.</param>
        /// <returns><see cref="IStep"/> of the file remover.</returns>
        private protected IStep RemoveFile(string relativePath, string searchPattern)
        {
            var step = this.GetService<IDeleteFilesStep>();
            step.RelativePath = relativePath;
            step.SearchPattern = searchPattern;
            step.SearchOption = SearchOption.TopDirectoryOnly;
            return step;
        }

        /// <summary>Returns a step to replace a data type's editor.</summary>
        /// <param name="value">Data type's name.</param>
        /// <returns><see cref="IStep"/> of the data type updater.</returns>
        private protected IStep UpdateDataTypeList(string value)
        {
            var step = this.GetService<IExecuteSqlStep>();
            step.Name = this.localizer.LocalizeFormat("UninstallStepUpdateDataTypeListFormat", value);
            step.CommandText = $$"""
                                 UPDATE {databaseOwner}[{objectQualifier}Lists]
                                 SET Text = 'DotNetNuke.Web.UI.WebControls.Internal.PropertyEditorControls.{{value}}EditControl, DotNetNuke.Web'
                                 WHERE ListName = 'DataType' AND Value = '{{value}}'
                                 """;
            return step;
        }

        /// <summary>Returns a step to remove Telerik rewriter rules in the siteurls.config.</summary>
        /// <returns><see cref="IStep"/> of the Telerik rewriter remover.</returns>
        private protected IStep UpdateSiteUrlsConfig()
        {
            return this.GetService<IRemoveTelerikRewriterRulesStep>();
        }

        /// <summary>Returns step to remove items from the web.config.</summary>
        /// <param name="collectionPath">Collection path.</param>
        /// <param name="attributeNames">Attribute names.</param>
        /// <returns><see cref="IStep"/> of the web.config editor.</returns>
        private protected IStep UpdateWebConfig(string collectionPath, string attributeNames)
        {
            var step = this.GetService<IRemoveItemFromCollectionStep>();
            step.Name = this.localizer.LocalizeFormat("UninstallStepUpdateWebConfigFormat", collectionPath);
            step.RelativeFilePath = "Web.config";
            step.CollectionPath = collectionPath;
            step.AttributeNamesToIncludeInSearch = attributeNames;
            step.SearchTerm = "telerik";
            return step;
        }

        /// <summary>Returns a step to remove Telerik binding redirects from the web.config.</summary>
        /// <returns><see cref="IStep"/> of the Telerik binding redirect remover.</returns>
        private protected IStep RemoveTelerikBindingRedirects()
        {
            return this.GetService<IRemoveTelerikBindingRedirectsStep>();
        }

        /// <summary>Returns a step to remove uninstalled extensions from the install folder so they can't get installed by mistake.</summary>
        /// <param name="relativePath">Path to where the extension is.</param>
        /// <param name="searchPattern">Pattern to get the right files.</param>
        /// <param name="recurse">Whether to recurse folders.</param>
        /// <returns><see cref="IStep"/> of the uninstalled extension remover.</returns>
        private protected IStep RemoveUninstalledExtensionFiles(string relativePath, string searchPattern, bool recurse = false)
        {
            var step = this.GetService<IDeleteFilesStep>();
            step.Name = this.localizer.LocalizeFormat(
                "UninstallStepCleanupExtensionFilesFormat", $"{relativePath}/{searchPattern}");
            step.RelativePath = relativePath;
            step.SearchPattern = searchPattern;
            step.SearchOption = recurse ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
            return step;
        }

        /// <summary>Returns a service dependency.</summary>
        /// <typeparam name="T">Dependency type.</typeparam>
        /// <returns>Requested Service.</returns>
        private protected T GetService<T>()
            where T : class
        {
            return (T)this.serviceProvider.GetService(typeof(T));
        }
    }
}
