// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Maintenance.Telerik.Removal
{
    using System;
    using System.Collections;

    /// <inheritdoc />
    internal class DamUninstaller : UnInstaller, IDamUninstaller
    {
        /// <summary>Initializes a new instance of the <see cref="DamUninstaller"/> class.</summary>
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
                this.ReplaceModule("Digital Asset Management", "ResourceManager", this.MigrateSettings),
                this.RemoveExtension("DigitalAssetsManagement"),
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

        private Hashtable MigrateSettings(Hashtable oldSettings)
        {
            var settings = new Hashtable();
            var mode = oldSettings["Mode"];
            var newMode = 0;
            if (mode != null)
            {
                switch ((string)mode)
                {
                    case "User":
                        newMode = 1;
                        break;
                    case "Group":
                        newMode = 2;
                        break;
                    default:
                        break;
                }
            }

            settings.Add("RM_Mode", newMode.ToString());

            var rootFolder = oldSettings["RootFolderId"];
            if (rootFolder != null)
            {
                settings.Add("RM_HomeFolder", rootFolder);
            }

            return settings;
        }
    }
}
