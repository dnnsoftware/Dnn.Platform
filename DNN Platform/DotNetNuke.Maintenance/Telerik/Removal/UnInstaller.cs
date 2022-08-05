namespace DotNetNuke.Maintenance.Telerik.Removal
{
    using System;
    using System.Collections.Generic;
    using System.IO;

    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Maintenance.Telerik.Steps;

    internal class UnInstaller
    {
        private readonly IServiceProvider serviceProvider;
        private readonly ILocalizer localizer;

        public UnInstaller(IServiceProvider serviceProvider, ILocalizer localizer)
        {
            this.serviceProvider = serviceProvider ??
                throw new ArgumentNullException(nameof(serviceProvider));

            this.localizer = localizer ??
                throw new ArgumentNullException(nameof(localizer));

            this.ProgressInternal = new List<UninstallSummaryItem>();
        }

        public IEnumerable<UninstallSummaryItem> Progress => this.ProgressInternal;

        private protected List<UninstallSummaryItem> ProgressInternal { get; set; }

        private protected IStep InstallAvailableExtension(string packageName, string fileNamePattern, string packageType)
        {
            var step = this.GetService<IInstallAvailablePackageStep>();
            step.PackageFileNamePattern = fileNamePattern;
            step.PackageName = packageName;
            step.PackageType = packageType;
            return step;
        }

        private protected IStep ReplaceModuleInPage(string pageName, string oldModuleName, string newModuleName)
        {
            var step = this.GetService<IReplaceTabModuleStep>();
            step.PageName = pageName;
            step.OldModuleName = oldModuleName;
            step.NewModuleName = newModuleName;
            return step;
        }

        private protected IStep ReplaceModuleInHostPage(string pageName, string oldModuleName, string newModuleName)
        {
            var parentStep = this.GetService<IReplaceTabModuleStep>();
            parentStep.PageName = pageName;
            parentStep.OldModuleName = oldModuleName;
            parentStep.NewModuleName = newModuleName;

            var step = this.GetService<IReplacePortalTabModuleStep>();
            step.ParentStep = parentStep;
            step.PortalId = Null.NullInteger;

            return step;
        }

        private protected IStep RemoveExtension(string packageName)
        {
            var step = this.GetService<IRemoveExtensionStep>();
            step.PackageName = packageName;
            return step;
        }

        private protected IStep UpdateDataTypeList(string value)
        {
            var commandFormat = string.Join(
                Environment.NewLine,
                "UPDATE {{databaseOwner}}[{{objectQualifier}}Lists]",
                "SET Text = 'DotNetNuke.Web.UI.WebControls.Internal.PropertyEditorControls.{0}EditControl, DotNetNuke.Web'",
                "WHERE ListName = 'DataType' AND Value = '{0}'");

            var step = this.GetService<IExecuteSqlStep>();
            step.Name = this.localizer.LocalizeFormat("UninstallStepUpdateDataTypeListFormat", value);
            step.CommandText = string.Format(commandFormat, value);
            return step;
        }

        private protected IStep UpdateSiteUrlsConfig()
        {
            return this.GetService<IRemoveTelerikRewriterRulesStep>();
        }

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

        private protected IStep RemoveTelerikBindingRedirects()
        {
            return this.GetService<IRemoveTelerikBindingRedirectsStep>();
        }

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

        private protected T GetService<T>()
            where T : class
        {
            return (T)this.serviceProvider.GetService(typeof(T));
        }
    }
}
