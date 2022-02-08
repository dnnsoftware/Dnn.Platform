// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Build.Tasks
{
    using Cake.Frosting;

    /// <summary>A cake task to compile the platform and create all of the packages.</summary>
    /// <remarks>This is the task run during CI.</remarks>
    [Dependency(typeof(CleanArtifacts))]
    [Dependency(typeof(UpdateDnnManifests))]
    [Dependency(typeof(GenerateSecurityAnalyzerChecksums))]
    [Dependency(typeof(SetPackageVersions))]
    [Dependency(typeof(CreateInstall))]
    [Dependency(typeof(CreateUpgrade))]
    [Dependency(typeof(CreateDeploy))]
    [Dependency(typeof(CreateSymbols))]
    [Dependency(typeof(CreateNugetPackages))]
    [Dependency(typeof(GeneratePackagesChecksums))]
    public sealed class BuildAll : FrostingTask<Context>
    {
        /// <inheritdoc/>
        public override void Run(Context context)
        {
            this.RevertSqlDataProvider(context);
        }

        private void RevertSqlDataProvider(Context context)
        {
            var fileName = context.GetTwoDigitsVersionNumber() + ".SqlDataProvider";
            var filePath = "./Dnn Platform/Website/Providers/DataProviders/SqlDataProvider/" + fileName;
            if (!context.SqlDataProviderExists && System.IO.File.Exists(filePath))
            {
                System.IO.File.Delete(filePath);
            }
        }
    }
}
