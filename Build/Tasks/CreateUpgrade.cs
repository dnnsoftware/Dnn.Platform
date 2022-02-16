// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Build.Tasks
{
    using System;
    using System.IO;
    using System.Linq;

    using Cake.Common.Diagnostics;
    using Cake.Common.IO;
    using Cake.Frosting;

    using Dnn.CakeUtils;

    /// <summary>A cake task to create the Upgrade package.</summary>
    [IsDependentOn(typeof(PreparePackaging))]
    [IsDependentOn(typeof(OtherPackages))]
    [IsDependentOn(typeof(CreateInstall))] // This is to ensure CreateUpgrade runs last and not in parallel, can be removed when we get to v10 where the telerik workaround is no longer needed
    [IsDependentOn(typeof(CreateSymbols))] // This is to ensure CreateUpgrade runs last and not in parallel, can be removed when we get to v10 where the telerik workaround is no longer needed
    [IsDependentOn(typeof(CreateDeploy))] // This is to ensure CreateUpgrade runs last and not in parallel, can be removed when we get to v10 where the telerik workaround is no longer needed
    public sealed class CreateUpgrade : FrostingTask<Context>
    {
        /// <inheritdoc/>
        public override void Run(Context context)
        {
            this.RenameResourcesFor98xUpgrades(context);
            context.CreateDirectory(context.ArtifactsFolder);
            var excludes = new string[context.PackagingPatterns.InstallExclude.Length + context.PackagingPatterns.UpgradeExclude.Length];
            context.PackagingPatterns.InstallExclude.CopyTo(excludes, 0);
            context.PackagingPatterns.UpgradeExclude.CopyTo(excludes, context.PackagingPatterns.InstallExclude.Length);
            var files = context.GetFilesByPatterns(context.WebsiteFolder, new[] { "**/*" }, excludes);
            files.Add(context.GetFiles("./Website/Install/Module/DNNCE_Website.Deprecated_*_Install.zip"));
            context.Information("Zipping {0} files for Upgrade zip", files.Count);

            var packageZip = $"{context.ArtifactsFolder}DNN_Platform_{context.GetBuildNumber()}_Upgrade.zip";
            context.Zip(context.WebsiteFolder, packageZip, files);
        }

        [Obsolete(
            "Workaround to support upgrades from 9.8.0 which may or may not still have Telerik installed."
            + "This method is to be removed in v10.0.0 and we should also implement a solution to remove these .resources files"
            + "from the available extensions to make sure people don't install them by mistake.")]
        private void RenameResourcesFor98xUpgrades(Context context)
        {
            var telerikPackages = new[]
                                  {
                                      $"{context.WebsiteFolder}Install/Module/DNNCE_DigitalAssetsManagement*.zip",
                                      $"{context.WebsiteFolder}Install/Module/Telerik*.zip",
                                      $"{context.WebsiteFolder}Install/Library/DNNCE_Web.Deprecated*.zip",
                                      $"{context.WebsiteFolder}Install/Library/DNNCE_Website.Deprecated*.zip",
                                  };

            var filesToRename = context.GetFilesByPatterns(telerikPackages);
            foreach (var fileToRename in filesToRename)
            {
                File.Move(
                    fileToRename.ToString(),
                    fileToRename.ChangeExtension("resources").ToString());
            }
        }
    }
}
