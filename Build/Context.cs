// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Build;

using System;
using System.IO;

using Cake.Common;
using Cake.Common.Diagnostics;
using Cake.Common.IO;
using Cake.Common.IO.Paths;
using Cake.Common.Tools.GitVersion;
using Cake.Core;
using Cake.Frosting;
using Cake.Json;

/// <inheritdoc/>
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
            this.TempFolder = "./Temp/";
            this.TempDir = context.Directory(this.TempFolder);
            context.Information($"TempDir: {this.TempDir}");

            this.ArtifactsFolder = "./Artifacts/";
            this.ArtifactsDir = context.Directory(this.ArtifactsFolder);
            context.Information($"ArtifactsDir: {this.ArtifactsDir}");

            this.WebsiteFolder = "./Website/";
            this.WebsiteDir = context.Directory(this.WebsiteFolder);
            context.Information($"WebsiteDir: {this.WebsiteDir}");

            // Global information variables
            this.IsRunningInCI = false;

            this.DnnSolutionPath = "./DNN_Platform.sln";

            this.SqlDataProviderExists = false;

            const string settingsFile = "./settings.local.json";
            this.Settings = LoadSettings(context, settingsFile);
            this.WriteSettings(context, settingsFile);

            this.BuildId = context.EnvironmentVariable("BUILD_BUILDID") ?? "0";
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
    public string DnnSolutionPath { get; set; }

    /// <summary>Gets or sets a value indicating whether this build is running in a CI environment.</summary>
    public bool IsRunningInCI { get; set; }

    /// <summary>Gets or sets the path to the website directory.</summary>
    public ConvertableDirectoryPath WebsiteDir { get; set; }

    /// <summary>Gets or sets the relative path to the website directory.</summary>
    public string WebsiteFolder { get; set; }

    /// <summary>Gets or sets the path to the artifacts directory.</summary>
    public ConvertableDirectoryPath ArtifactsDir { get; set; }

    /// <summary>Gets or sets the relative path to the artifacts directory.</summary>
    public string ArtifactsFolder { get; set; }

    /// <summary>Gets or sets the path to the temp directory.</summary>
    public ConvertableDirectoryPath TempDir { get; set; }

    /// <summary>Gets or sets the relative path to the temp directory.</summary>
    public string TempFolder { get; set; }

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

    private static LocalSettings LoadSettings(ICakeContext context, string settingsFile)
    {
        if (File.Exists(settingsFile))
        {
            context.Information(log => log($"Loading settings from {Path.GetFullPath(settingsFile)}"));
            return context.DeserializeJsonFromFile<LocalSettings>(settingsFile);
        }

        context.Information(log => log($"Did not find settings file {Path.GetFullPath(settingsFile)}"));
        return new LocalSettings();
    }

    private void WriteSettings(ICakeContext context, string settingsFile)
    {
        context.SerializeJsonToPrettyFile(settingsFile, this.Settings);
        context.Information(log => log($"Saved settings to {Path.GetFullPath(settingsFile)}"));
        context.Debug(log => log("{0}", $"Settings: {context.SerializeJson(this.Settings)}"));
    }
}
