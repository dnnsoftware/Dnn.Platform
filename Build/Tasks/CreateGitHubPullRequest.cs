// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Build.Tasks
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;

    using Cake.Common;
    using Cake.Common.Diagnostics;
    using Cake.Core;
    using Cake.Core.IO;
    using Cake.Frosting;

    using Octokit;

    using YamlDotNet.RepresentationModel;

    /// <summary>A cake task to create a GitHub pull request from CI.</summary>
    /// <remarks>
    /// This task runs after <see cref="BuildAll"/> and only in CI.
    /// It fetches GitHub releases to update the bug report template with current version options,
    /// then checks whether any uncommitted changes exist.
    /// If changes exist it commits them to a new branch, pushes, and opens a draft PR.
    /// It requires the following environment variables:
    /// <list type="bullet">
    ///   <item><c>GITHUB_TOKEN</c> – A GitHub token with <c>repo</c> scope.</item>
    ///   <item><c>BUILD_REPOSITORY_NAME</c> – The <c>owner/repo</c> slug (set automatically by Azure Pipelines).</item>
    ///   <item><c>BUILD_SOURCEBRANCH</c> – The full ref of the source branch (set automatically by Azure Pipelines).</item>
    /// </list>
    /// </remarks>
    public sealed class CreateGitHubPullRequest : FrostingTask<Context>
    {
        private const string TargetBranch = "develop";
        private const string BugReportPath = ".github/ISSUE_TEMPLATE/bug-report.yml";

        /// <inheritdoc/>
        public override bool ShouldRun(Context context)
        {
            var sourceBranch = context.EnvironmentVariable("BUILD_SOURCEBRANCH") ?? string.Empty;
            context.Information("CreateGitHubPullRequest: BUILD_SOURCEBRANCH is '{0}'.", sourceBranch);

            if (!context.IsRunningInCI)
            {
                context.Information("Skipping CreateGitHubPullRequest because the build is not running in CI.");
                return false;
            }

            var token = context.EnvironmentVariable("GITHUB_TOKEN");
            if (string.IsNullOrEmpty(token))
            {
                context.Warning("Skipping CreateGitHubPullRequest because GITHUB_TOKEN is not set.");
                return false;
            }

            return true;
        }

        /// <inheritdoc/>
        public override void Run(Context context)
        {
            var token = context.EnvironmentVariable("GITHUB_TOKEN");

            // owner/repo – e.g. "dnnsoftware/Dnn.Platform"
            var repoSlug = context.EnvironmentVariable("BUILD_REPOSITORY_NAME")
                           ?? throw new CakeException("BUILD_REPOSITORY_NAME environment variable is not set.");

            var parts = repoSlug.Split('/');
            if (parts.Length != 2)
            {
                throw new CakeException($"BUILD_REPOSITORY_NAME '{repoSlug}' is not in the expected 'owner/repo' format.");
            }

            var owner = parts[0];
            var repo = parts[1];

            var client = new GitHubClient(new ProductHeaderValue("DnnPlatformCakeBuild"))
            {
                Credentials = new Credentials(token),
            };

            // Update bug-report.yml with version info from GitHub releases
            UpdateBugReportVersions(context, client, owner, repo);

            // Only proceed with the PR if there are actual changes
            if (!HasUncommittedChanges(context))
            {
                context.Information("No uncommitted changes found after updates. Skipping PR creation.");
                return;
            }

            var headBranch = $"automated/ci-{context.BuildId}";

            // Commit all changes to a new branch and push
            Git(context, $"checkout -b {headBranch}");
            Git(context, "add .");
            Git(context, $"commit -m \"[Automated] CI build {context.BuildId} changes\"");
            Git(context, $"push https://{token}@github.com/{repoSlug}.git {headBranch}");

            var title = $"[Automated] Merge CI changes into {TargetBranch}";
            var body = $"Automated pull request created by CI build {context.BuildId}.";

            context.Information("Creating GitHub PR: {0} → {1} in {2}/{3}", headBranch, TargetBranch, owner, repo);

            var newPr = new NewPullRequest(title, headBranch, TargetBranch)
            {
                Body = body,
                Draft = true,
            };

            var pr = client.PullRequest.Create(owner, repo, newPr).GetAwaiter().GetResult();
            context.Information("Pull request #{0} created: {1}", pr.Number, pr.HtmlUrl);
        }

        private static void UpdateBugReportVersions(Context context, GitHubClient client, string owner, string repo)
        {
            context.Information("Fetching GitHub releases to update bug report template...");
            var releases = client.Repository.Release.GetAll(owner, repo).GetAwaiter().GetResult();

            var latestStable = releases
                .Where(r => !r.Draft && !r.TagName.Contains("rc", StringComparison.OrdinalIgnoreCase))
                .OrderByDescending(r => r.PublishedAt)
                .FirstOrDefault();

            context.Information(
                "Latest stable release: {0}",
                latestStable != null ? latestStable.TagName : "(none)");

            var currentRcs = releases
                .Where(r => !r.Draft && r.TagName.Contains("rc", StringComparison.OrdinalIgnoreCase))
                .Where(r => latestStable == null || r.PublishedAt > latestStable.PublishedAt)
                .OrderByDescending(r => r.PublishedAt)
                .ToList();

            context.Information("Found {0} currently published RC release(s).", currentRcs.Count);

            // Build the new options list
            var options = new List<string>();
            if (latestStable != null)
            {
                var version = latestStable.TagName.TrimStart('v');
                options.Add($"{version} (latest release)");
            }

            foreach (var rc in currentRcs)
            {
                var version = rc.TagName.TrimStart('v');
                options.Add($"{version} (release candidate)");
            }

            options.Add("develop build (unreleased)");

            // Parse the YAML template and update the affected-versions options
            var yaml = new YamlStream();
            using (var reader = new StreamReader(BugReportPath))
            {
                yaml.Load(reader);
            }

            var root = (YamlMappingNode)yaml.Documents[0].RootNode;
            var body = (YamlSequenceNode)root.Children[new YamlScalarNode("body")];

            var optionsNode = body.Children
                .OfType<YamlMappingNode>()
                .Where(item =>
                    item.Children.TryGetValue(new YamlScalarNode("id"), out var idNode) &&
                    idNode is YamlScalarNode { Value: "affected-versions", })
                .Select(item => (YamlMappingNode)item.Children[new YamlScalarNode("attributes")])
                .Select(attrs => (YamlSequenceNode)attrs.Children[new YamlScalarNode("options")])
                .FirstOrDefault();

            if (optionsNode == null)
            {
                context.Warning("Could not locate affected-versions options in {0}, skipping update.", BugReportPath);
                return;
            }

            optionsNode.Children.Clear();
            foreach (var option in options)
            {
                optionsNode.Children.Add(new YamlScalarNode(option));
            }

            using var stringWriter = new StringWriter();
            yaml.Save(stringWriter, false);

            // YamlStream.Save wraps output in document markers (--- / ...) that the original file doesn't use
            File.WriteAllText(BugReportPath, StripDocumentMarkers(stringWriter.ToString()));
            context.Information("Updated {0} with {1} version option(s).", BugReportPath, options.Count);
        }

        private static string StripDocumentMarkers(string yaml)
        {
            using var reader = new StringReader(yaml);
            var lines = new List<string>();
            while (reader.ReadLine() is { } line)
            {
                lines.Add(line);
            }

            if (lines.Count > 0 && lines[0] == "---")
            {
                lines.RemoveAt(0);
            }

            while (lines.Count > 0 && string.IsNullOrWhiteSpace(lines[^1]))
            {
                lines.RemoveAt(lines.Count - 1);
            }

            if (lines.Count > 0 && lines[^1] == "...")
            {
                lines.RemoveAt(lines.Count - 1);
            }

            return string.Join("\n", lines) + "\n";
        }

        private static bool HasUncommittedChanges(ICakeContext context)
        {
            var process = context.StartAndReturnProcess(
                "git",
                new ProcessSettings
                {
                    Arguments = "status --porcelain",
                    RedirectStandardOutput = true,
                });
            process.WaitForExit();
            var output = process.GetStandardOutput().ToList();
            return output.Count > 0;
        }

        private static void Git(ICakeContext context, string arguments)
        {
            context.Information("git {0}", arguments);
            using (var process = context.StartAndReturnProcess("git", new ProcessSettings { Arguments = arguments, }))
            {
                process.WaitForExit();
            }
        }
    }
}
