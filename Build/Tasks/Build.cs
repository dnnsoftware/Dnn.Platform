// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Build.Tasks
{
    using System;
    using System.Linq;

    using Cake.Common.Build;
    using Cake.Common.Build.AzurePipelines.Data;
    using Cake.Common.Tools.MSBuild;
    using Cake.Frosting;
    using Cake.Issues;
    using Cake.Issues.MsBuild;

    using DotNetNuke.Build;

    /// <summary>A cake task to compile the platform.</summary>
    [Dependency(typeof(CleanWebsite))]
    [Dependency(typeof(RestoreNuGetPackages))]
    public sealed class Build : FrostingTask<Context>
    {
        /// <inheritdoc/>
        public override void Run(Context context)
        {
            // TODO: when Cake.Issues.MsBuild is updated to support Binary Log version 9, can use .EnableBinaryLogger() instead of .WithLogger(…)
            // TODO: also can remove the .InstallTool(…) call for Cake.Issues.MsBuild in Program.cs at that point
            var cleanLog = context.ArtifactsDir.Path.CombineWithFilePath("clean.binlog");
            var cleanSettings = new MSBuildSettings().SetConfiguration(context.BuildConfiguration)
                .WithTarget("Clean")
                .SetMaxCpuCount(0)
                .WithLogger(context.Tools.Resolve("Cake.Issues.MsBuild*/**/StructuredLogger.dll").FullPath, "BinaryLogger", cleanLog.FullPath)
                .SetNoConsoleLogger(context.IsRunningInCI);
            context.MSBuild(context.DnnSolutionPath, cleanSettings);

            var buildLog = context.ArtifactsDir.Path.CombineWithFilePath("rebuild.binlog");
            var buildSettings = new MSBuildSettings().SetConfiguration(context.BuildConfiguration)
                .SetPlatformTarget(PlatformTarget.MSIL)
                .WithTarget("Rebuild")
                .SetMaxCpuCount(0)
                .WithProperty("SourceLinkCreate", "true")
                .WithLogger(context.Tools.Resolve("Cake.Issues.MsBuild*/**/StructuredLogger.dll").FullPath, "BinaryLogger", buildLog.FullPath)
                .SetNoConsoleLogger(context.IsRunningInCI);
            context.MSBuild(context.DnnSolutionPath, buildSettings);

            // TODO: when Cake.Issues.Recipe is updated to support Frosting, we can switch to their more robust issue processing and reporting features
            if (!context.IsRunningInCI)
            {
                return;
            }

            var issueProviders = new[]
            {
                context.MsBuildIssuesFromFilePath(cleanLog, context.MsBuildBinaryLogFileFormat()),
                context.MsBuildIssuesFromFilePath(buildLog, context.MsBuildBinaryLogFileFormat()),
            };
            foreach (var issue in context.ReadIssues(issueProviders, context.Environment.WorkingDirectory))
            {
                var messageData = new AzurePipelinesMessageData { SourcePath = issue.AffectedFileRelativePath?.FullPath, LineNumber = issue.Line, };
                if (issue.Priority == (int)IssuePriority.Error)
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
