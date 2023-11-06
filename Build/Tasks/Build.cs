// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Build.Tasks
{
    using System;
    using System.Linq;

    using Cake.Common.Build;
    using Cake.Common.Build.AzurePipelines.Data;
    using Cake.Common.IO;
    using Cake.Common.Tools.MSBuild;
    using Cake.Core;
    using Cake.Core.IO;
    using Cake.Frosting;
    using Cake.Issues;
    using Cake.Issues.MsBuild;
    using DotNetNuke.Build;

    /// <summary>A cake task to compile the platform.</summary>
    [IsDependentOn(typeof(CleanWebsite))]
    [IsDependentOn(typeof(RestoreNuGetPackages))]
    [IsDependentOn(typeof(BuildNpmPackages))]
    public sealed class Build : FrostingTask<Context>
    {
        /// <inheritdoc/>
        public override void Run(Context context)
        {
            var cleanLog = context.ArtifactsDir.Path.CombineWithFilePath("clean.binlog");
            var buildLog = context.ArtifactsDir.Path.CombineWithFilePath("rebuild.binlog");
            try
            {
                var cleanSettings = CreateMsBuildSettings(context, cleanLog).WithTarget("Clean");
                context.MSBuild(context.DnnSolutionPath, cleanSettings);

                var buildSettings = CreateMsBuildSettings(context, buildLog)
                    .SetPlatformTarget(PlatformTarget.MSIL)
                    .WithTarget("Rebuild")
                    .WithProperty("SourceLinkCreate", "true");
                context.MSBuild(context.DnnSolutionPath, buildSettings);
            }
            finally
            {
                if (context.AzurePipelines().IsRunningOnAzurePipelines)
                {
                    ReportIssuesToAzurePipelines(context, cleanLog, buildLog);
                }
            }
        }

        private static MSBuildSettings CreateMsBuildSettings(Context context, FilePath binLogPath)
        {
            return new MSBuildSettings().SetConfiguration(context.BuildConfiguration)
                .SetMaxCpuCount(0)
                .EnableBinaryLogger(binLogPath.FullPath)
                .SetNoConsoleLogger(context.IsRunningInCI);
        }

        private static void ReportIssuesToAzurePipelines(ICakeContext context, FilePath cleanLog, FilePath buildLog)
        {
            var issueProviders =
                from logFilePath in new[] { cleanLog, buildLog, }
                where context.FileExists(logFilePath)
                let settings = new MsBuildIssuesSettings(logFilePath, context.MsBuildBinaryLogFileFormat())
                select new MsBuildIssuesProvider(context.Log, settings);
            var issues = context.ReadIssues(issueProviders, context.Directory("."));
            foreach (var issue in issues)
            {
                var messageData = new AzurePipelinesMessageData
                {
                    SourcePath = issue.AffectedFileRelativePath?.FullPath,
                    LineNumber = issue.Line,
                };

                if (string.Equals(issue.PriorityName, "Error", StringComparison.Ordinal))
                {
                    context.AzurePipelines().Commands.WriteError(issue.MessageText, messageData);
                }
                else
                {
                    context.AzurePipelines().Commands.WriteWarning(issue.MessageText, messageData);
                }
            }
        }
    }
}
