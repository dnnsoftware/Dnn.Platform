// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Maintenance.Telerik.Removal
{
    using System;

    /// <inheritdoc />
    internal class DamUninstaller : UnInstaller, IDamUninstaller
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DamUninstaller"/> class.
        /// </summary>
        /// <param name="serviceProvider">An instance of <see cref="IServiceProvider"/>.</param>
        /// <param name="localizer">An instance of <see cref="ILocalizer"/>.</param>
        public DamUninstaller(IServiceProvider serviceProvider, ILocalizer localizer)
        : base(serviceProvider, localizer)
        {
        }

        /// <inheritdoc/>
        public void Execute()
        {
            var steps = new[]
            {
                this.ReplaceModuleInPage("File Management", "Digital Asset Management", "ResourceManager"),
                this.RemoveExtension("DigitalAssetsManagement"),
                this.UpdateDataTypeList("Date"),
                this.UpdateDataTypeList("DateTime"),
            };

            var skip = false;

            foreach (var step in steps)
            {
                if (!skip)
                {
                    step.Execute();

                    var nullable = step.Success;
                    if (nullable.HasValue && nullable.Value == false)
                    {
                        skip = true;
                    }
                }

                this.ProgressInternal.AddRange(UninstallSummaryItem.FromStep(step));
            }
        }
    }
}
