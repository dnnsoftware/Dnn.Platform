// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Maintenance.Telerik.Removal
{
    using System;

    /// <inheritdoc cref="ITelerikUninstaller" />
    internal sealed class TelerikUninstaller : UnInstaller, ITelerikUninstaller
    {
        /// <summary>Initializes a new instance of the <see cref="TelerikUninstaller"/> class.</summary>
        /// <param name="serviceProvider">An instance of <see cref="IServiceProvider"/>.</param>
        /// <param name="localizer">An instance of <see cref="ILocalizer"/>.</param>
        public TelerikUninstaller(IServiceProvider serviceProvider, ILocalizer localizer)
        : base(serviceProvider, localizer)
        {
        }

        /// <inheritdoc/>
        public void Execute()
        {
            var steps = new[]
            {
                this.RemoveExtension("DotNetNuke.Telerik.Web"),
                this.RemoveExtension("DotNetNuke.Web.Deprecated"),
                this.RemoveExtension("DotNetNuke.Website.Deprecated"),
                this.RemoveExtension("Admin.Messaging"),
                this.RemoveExtension("DNNSecurityHotFix20171", false),
                this.RemoveFile("bin", "DNNSecurityHotFix20171.dll"),
                this.RemoveExtension("DotNetNuke.RadEditorProvider"),
                this.UpdateDataTypeList("Date"),
                this.UpdateDataTypeList("DateTime"),
                this.UpdateSiteUrlsConfig(),
                this.UpdateWebConfig("/configuration/appSettings", "key"),
                this.UpdateWebConfig("/configuration/system.webServer/handlers", "name, type"),
                this.UpdateWebConfig("/configuration/system.webServer/modules", "name, type"),
                this.RemoveTelerikBindingRedirects(),
                this.RemoveUninstalledExtensionFiles("App_Data/ExtensionPackages", "Library_DotNetNuke.Telerik.Web_*"),
                this.RemoveUninstalledExtensionFiles("App_Data/ExtensionPackages", "Library_DotNetNuke.Web.Deprecated_*"),
                this.RemoveUninstalledExtensionFiles("App_Data/ExtensionPackages", "Library_DotNetNuke.Website.Deprecated_*"),
                this.RemoveUninstalledExtensionFiles("App_Data/ExtensionPackages", "Module_DNNSecurityHotFix*"),
                this.RemoveFile("bin", "Telerik.Web.UI.dll"),
                this.RemoveFile("bin", "Telerik.Web.UI.Skins.dll"),
                this.RemoveExtension("TelerikRemoval"),
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
