// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Maintenance.Telerik.Steps
{
    using System;
    using System.Collections.Generic;

    using DotNetNuke.Instrumentation;
    using DotNetNuke.Maintenance.Telerik.Removal;

    /// <summary>A container for an array of steps.</summary>
    internal sealed class RemoveExtensionStep : StepArrayBase, IRemoveExtensionStep
    {
        private readonly IServiceProvider serviceProvider;

        /// <summary>Initializes a new instance of the <see cref="RemoveExtensionStep"/> class.</summary>
        /// <param name="loggerSource">An instance of <see cref="ILoggerSource"/>.</param>
        /// <param name="localizer">An instance of <see cref="ILocalizer"/>.</param>
        /// <param name="serviceProvider">An instance of <see cref="IServiceProvider"/>.</param>
        public RemoveExtensionStep(ILoggerSource loggerSource, ILocalizer localizer, IServiceProvider serviceProvider)
            : base(loggerSource, localizer)
        {
            this.serviceProvider = serviceProvider ??
                throw new ArgumentNullException(nameof(serviceProvider));
        }

        /// <inheritdoc/>
        public override string Name => this.LocalizeFormat("UninstallStepRemoveExtension", this.PackageName);

        /// <inheritdoc/>
        [Required]
        public string PackageName { get; set; }

        /// <inheritdoc/>
        [Required]
        public bool DeleteFiles { get; set; }

        /// <inheritdoc/>
        protected override void ExecuteInternal()
        {
            this.Steps = new List<IStep>(new[]
            {
                this.RemoveSystemAttributeFromPackage(),
                this.ClearCache(),
                this.UninstallExtension(),
                this.DeleteDependencyRecords(),
                this.ClearCache(),
            });

            base.ExecuteInternal();
        }

        private IStep RemoveSystemAttributeFromPackage()
        {
            var step = this.GetService<IExecuteSqlStep>();
            step.Name = this.LocalizeFormat("UninstallStepRemoveSystemAttribute", this.PackageName);
            step.CommandText = $$"""
                                 UPDATE {databaseOwner}{objectQualifier}Packages
                                 SET IsSystemPackage = 0
                                 WHERE [Name] = '{{this.PackageName}}'
                                 """;

            return step;
        }

        private IStep ClearCache()
        {
            return this.GetService<IClearCacheStep>();
        }

        private IStep UninstallExtension()
        {
            var step = this.GetService<IUninstallPackageStep>();
            step.PackageName = this.PackageName;
            step.DeleteFiles = this.DeleteFiles;

            return step;
        }

        private IStep DeleteDependencyRecords()
        {
            var step = this.GetService<IExecuteSqlStep>();
            step.Name = this.LocalizeFormat("UninstallStepCleanupDependencyRecords", this.PackageName);
            step.CommandText = $$"""
                                 DELETE FROM {databaseOwner}{objectQualifier}PackageDependencies
                                 WHERE PackageName = '{{this.PackageName}}'
                                 """;

            return step;
        }

        private T GetService<T>()
            where T : class
        {
            return (T)this.serviceProvider.GetService(typeof(T));
        }
    }
}
