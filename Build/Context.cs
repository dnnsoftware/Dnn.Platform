// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Build
{
    using System;
    using System.Globalization;

    using Cake.Common;
    using Cake.Common.Build;
    using Cake.Common.Diagnostics;
    using Cake.Common.IO;
    using Cake.Common.IO.Paths;
    using Cake.Common.Tools.GitVersion;
    using Cake.Core;
    using Cake.Core.Diagnostics;
    using Cake.Core.IO;
    using Cake.Frosting;
    using Cake.Json;

    /// <inheritdoc />
    public class Context : FrostingContext
    {
        /// <summary>Initializes a new instance of the <see cref="Context"/> class.</summary>
        /// <param name="context">The base context.</param>
        public Context(ICakeContext context)
            : base(context)
        {
            try
            {
                //////////////////////////////////////////////////////////////////////
                // ARGUMENTS
                //////////////////////////////////////////////////////////////////////

                this.Target = context.Argument("target", "Default");
                context.Information($"Target: {this.Target}");
                this.BuildConfiguration = context.Argument("configuration", "Release");
                context.Information($"Configuration: {this.BuildConfiguration}");

                //////////////////////////////////////////////////////////////////////
                // PREPARATION
                //////////////////////////////////////////////////////////////////////

                // Define directories.
                this.RootDir = context.MakeAbsolute(context.Directory("./"));
                context.Verbose($"RootDir: {this.RootDir}");

                this.TempDir = context.Directory("./Temp/");
                context.Information($"TempDir: {this.TempDir}");

                this.ArtifactsDir = context.Directory("./Artifacts/");
                context.Information($"ArtifactsDir: {this.ArtifactsDir}");

                this.WebsiteDir = context.Directory("./Website/");
                context.Information($"WebsiteDir: {this.WebsiteDir}");

                // Global information variables
                this.IsRunningInCI = false;

                this.DnnSolutionPath = context.File("./DNN_Platform.sln");

                this.SqlDataProviderExists = false;

                var settingsFile = context.File("./settings.local.json");
                this.Settings = LoadSettings(context, settingsFile);
                this.WriteSettings(context, settingsFile);

                this.BuildId = context.AzurePipelines().IsRunningOnAzurePipelines
                    ? context.AzurePipelines().Environment.Build.Id.ToString(CultureInfo.InvariantCulture)
                    : context.GitHubActions().IsRunningOnGitHubActions
                        ? context.GitHubActions().Environment.Workflow.RunId
                        : "0";
                context.Information($"BuildId: {this.BuildId}");
                this.BuildNumber = string.Empty;
                this.ProductVersion = string.Empty;
            }
            catch (Exception exc)
            {
                this.Error(exc);
                throw;
            }
        }

        /// <summary>Gets or sets the DNN version.</summary>
        public string ProductVersion { get; set; }

        /// <summary>Gets or sets the DNN version in the build number format.</summary>
        public string BuildNumber { get; set; }

        /// <summary>Gets or sets the build ID from the CI environment.</summary>
        public string BuildId { get; set; }

        /// <summary>Gets or sets a value indicating whether the current version's SQL Data Provider file exists (e.g. <c>09.09.00.SqlDataProvider</c>).</summary>
        public bool SqlDataProviderExists { get; set; }

        /// <summary>Gets or sets the path to the DNN solution.</summary>
        public FilePath DnnSolutionPath { get; set; }

        /// <summary>Gets or sets a value indicating whether this build is running in a CI environment.</summary>
        public bool IsRunningInCI { get; set; }

        /// <summary>Gets or sets the path to the root of the repository.</summary>
        public DirectoryPath RootDir { get; set; }

        /// <summary>Gets or sets the path to the website directory.</summary>
        public ConvertableDirectoryPath WebsiteDir { get; set; }

        /// <summary>Gets or sets the path to the artifacts directory.</summary>
        public ConvertableDirectoryPath ArtifactsDir { get; set; }

        /// <summary>Gets or sets the path to the temp directory.</summary>
        public ConvertableDirectoryPath TempDir { get; set; }

        /// <summary>Gets or sets the build configuration, e.g. Debug or Release.</summary>
        public string BuildConfiguration { get; set; }

        /// <summary>Gets or sets the target.</summary>
        public string Target { get; set; }

        /// <summary>Gets or sets the collection of glob patterns to include and exclude when packaging.</summary>
        public PackagingPatterns PackagingPatterns { get; set; }

        /// <summary>Gets or sets the local dev site settings.</summary>
        public LocalSettings Settings { get; set; }

        /// <summary>Gets or sets the resolved version information.</summary>
        public GitVersion Version { get; set; }

        /// <summary>Gets the build number.</summary>
        /// <returns>The version of the build.</returns>
        public string GetBuildNumber()
        {
            return this.BuildNumber;
        }

        /// <summary>Gets the build number with leading zeroes.</summary>
        /// <returns>The version with leading zeroes.</returns>
        public string GetTwoDigitsVersionNumber()
        {
            var fullVer = this.GetBuildNumber().Split('-')[0]; // Gets rid of the -unstable, -beta, etc.
            var numbers = fullVer.Split('.');
            for (var i = 0; i < numbers.Length; i++)
            {
                if (numbers[i].Length < 2)
                {
                    numbers[i] = "0" + numbers[i];
                }
            }

            return string.Join(".", numbers);
        }

        /// <summary>Gets the product version.</summary>
        /// <returns>The version of DNN being built.</returns>
        public string GetProductVersion()
        {
            return this.ProductVersion;
        }

        private static LocalSettings LoadSettings(ICakeContext context, FilePath settingsFile)
        {
            if (context.FileExists(settingsFile))
            {
                context.Information((FormattableLogActionEntry log) => log($"Loading settings from {settingsFile.FullPath}"));
                return context.DeserializeJsonFromFile<LocalSettings>(settingsFile);
            }

            context.Information((FormattableLogActionEntry log) => log($"Did not find settings file {settingsFile.FullPath}"));
            return new LocalSettings();
        }

        private void WriteSettings(ICakeContext context, FilePath settingsFile)
        {
            context.SerializeJsonToPrettyFile(settingsFile, this.Settings);
            context.Information((FormattableLogActionEntry log) => log($"Saved settings to {settingsFile.FullPath}"));
            context.Debug(log => log("{0}", $"Settings: {context.SerializeJson(this.Settings)}"));
        }
    }
}
