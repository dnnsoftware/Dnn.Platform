// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.Modules.TelerikRemovalLibrary
{
    using System;
    using System.Collections.Generic;

    using DotNetNuke.Instrumentation;

    /// <summary>
    /// A container for an array of steps.
    /// </summary>
    internal class RemoveExtensionStep : StepArrayBase, IRemoveExtensionStep
    {
        private readonly IServiceProvider serviceProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="RemoveExtensionStep"/> class.
        /// </summary>
        /// <param name="loggerSource">An instance of <see cref="ILoggerSource"/>.</param>
        /// <param name="serviceProvider">An instance of <see cref="IServiceProvider"/>.</param>
        public RemoveExtensionStep(ILoggerSource loggerSource, IServiceProvider serviceProvider)
            : base(loggerSource)
        {
            this.serviceProvider = serviceProvider ??
                throw new ArgumentNullException(nameof(serviceProvider));
        }

        /// <inheritdoc/>
        public override string Name => $"Remove extension '{this.PackageName}'";

        /// <inheritdoc/>
        [Required]
        public string PackageName { get; set; }

        /// <inheritdoc/>
        protected override void ExecuteInternal()
        {
            this.Steps = new List<IStep>(new[]
            {
                this.RemoveSystemAttributeFromPackage(),
                this.ClearCache(),
                this.UninstallExtension(),
            });

            base.ExecuteInternal();
        }

        private IStep RemoveSystemAttributeFromPackage()
        {
            var commandFormat = string.Join(
                Environment.NewLine,
                "UPDATE {{databaseOwner}}{{objectQualifier}}Packages",
                "SET IsSystemPackage = 0",
                "WHERE [Name] = '{0}'");

            var step = this.GetService<IExecuteSqlStep>();
            step.InternalName = $"Remove the 'System' attribute from package '{this.PackageName}'";
            step.CommandText = string.Format(commandFormat, this.PackageName);

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
            step.DeleteFiles = true;

            return step;
        }

        private T GetService<T>()
            where T : class
        {
            return (T)this.serviceProvider.GetService(typeof(T));
        }
    }
}
