// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Build.Tasks
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Security.Cryptography;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;

    using Cake.Common;
    using Cake.Common.Build;
    using Cake.Common.Diagnostics;
    using Cake.Common.IO;
    using Cake.Core;
    using Cake.Core.IO;
    using Cake.FileHelpers;
    using Cake.Frosting;

    using Dnn.CakeUtils;

    using Microsoft.IdentityModel.JsonWebTokens;
    using Microsoft.IdentityModel.Tokens;

    using Newtonsoft.Json;

    using Octokit;

    using YamlDotNet.RepresentationModel;

    using ProductHeaderValue = Octokit.ProductHeaderValue;

    /// <summary>A cake task to create a GitHub pull request from CI.</summary>
    /// <remarks>
    /// This task is invoked as a standalone target from the pipeline after the build and tests succeed.
    /// It authenticates as a GitHub App, fetches releases to update the bug report template,
    /// then checks whether any uncommitted changes exist.
    /// If changes exist it commits them to a new branch, pushes, and opens a draft PR.
    /// It requires the following environment variables:
    /// <list type="bullet">
    ///   <item><c>GITHUB_APP_ID</c> – The numeric GitHub App ID.</item>
    ///   <item><c>GITHUB_APP_PRIVATE_KEY</c> – The PEM-encoded private key for the GitHub App.</item>
    ///   <item><c>BUILD_REPOSITORY_NAME</c> – The <c>owner/repo</c> slug (set automatically by Azure Pipelines).</item>
    ///   <item><c>BUILD_SOURCEBRANCH</c> – The full ref of the source branch (set automatically by Azure Pipelines).</item>
    /// </list>
    /// </remarks>
    public sealed class CreateGitHubPullRequest : AsyncFrostingTask<Context>
    {
        private const string TargetBranch = "develop";
        private const string GitUserName = "DNN Platform CI Bot";
        private const string GitUserEmail = "noreply@dnncommunity.org";

        /// <inheritdoc/>
        public override async Task RunAsync(Context context)
        {
            if (!context.IsRunningInCI)
            {
                context.Information("Skipping CreateGitHubPullRequest because the build is not running in CI.");
                return;
            }

            var solutionInfoPath = context.File("SolutionInfo.cs");
            var bugReportPath = context.File(".github/ISSUE_TEMPLATE/bug-report.yml");

            var sourceBranch = context.AzurePipelines().IsRunningOnAzurePipelines
                ? context.AzurePipelines().Environment.Repository.SourceBranch
                : context.GitHubActions().IsRunningOnGitHubActions
                    ? context.GitHubActions().Environment.Workflow.Ref
                    : string.Empty;
            context.Information("CreateGitHubPullRequest: source branch is '{0}'.", sourceBranch);
            if (!IsTargetedBranch(sourceBranch))
            {
                context.Information("Skipping CreateGitHubPullRequest because branch '{0}' is not develop, main, or release/*.", sourceBranch);
                return;
            }

            var appId = context.EnvironmentVariable("GITHUB_APP_ID");
            var privateKey = context.EnvironmentVariable("GITHUB_APP_PRIVATE_KEY");
            if (string.IsNullOrEmpty(appId) || string.IsNullOrEmpty(privateKey))
            {
                context.Warning("Skipping CreateGitHubPullRequest because GITHUB_APP_ID or GITHUB_APP_PRIVATE_KEY is not set.");
                return;
            }

            // owner/repo – e.g. "dnnsoftware/Dnn.Platform"
            var repoSlug = context.AzurePipelines().IsRunningOnAzurePipelines
                ? context.AzurePipelines().Environment.Repository.RepoName
                : context.GitHubActions().IsRunningOnGitHubActions
                    ? context.GitHubActions().Environment.Workflow.Repository
                    : throw new CakeException("Repository name environment variable is not set.");

            var parts = repoSlug.Split('/');
            if (parts.Length != 2)
            {
                throw new CakeException($"Repository name '{repoSlug}' is not in the expected 'owner/repo' format.");
            }

            var owner = parts[0];
            var repo = parts[1];

            // Generate a short-lived installation token from the GitHub App credentials
            var token = await GenerateInstallationToken(context);

            var client = new GitHubClient(new ProductHeaderValue("DnnPlatformCakeBuild"))
            {
                Credentials = new Credentials(token),
            };

            context.Information("Authenticated as GitHub App installation.");

            // Update bug-report.yml with version info from GitHub releases
            await UpdateBugReportVersions(context, client, owner, repo, bugReportPath);

            // Reset SolutionInfo.cs if only the commit count/SHA changed (not the major.minor.patch)
            // to avoid creating a PR for every single commit.
            ResetSolutionInfoIfVersionUnchanged(context, solutionInfoPath);

            // Only proceed with the PR if there are actual changes
            if (!HasUncommittedChanges(context))
            {
                context.Information("No uncommitted changes found after updates. Skipping PR creation.");
                return;
            }

            var headBranch = $"automated/ci-{context.BuildId}";

            // Configure git identity for CI (agents don't have one by default)
            Git(context, $"config user.name \"{GitUserName}\"");
            Git(context, $"config user.email \"{GitUserEmail}\"");

            // Commit all changes to a new branch
            Git(context, $"checkout -b {headBranch}");
            Git(context, "add .");

            // Verify there are actually staged changes after git add
            // (git status --porcelain can report changes that don't result in staged content)
            if (!HasStagedChanges(context))
            {
                context.Information("No staged changes after git add. Skipping PR creation.");
                return;
            }

            Git(context, $"commit -m \"[Automated] CI build {context.BuildId} changes\"");

            // Push using token via HTTP header so it never appears in logs
            Git(context, $"remote set-url origin https://github.com/{repoSlug}.git");
            var encodedToken = Convert.ToBase64String(Encoding.UTF8.GetBytes($"x-access-token:{token}"));
            Git(context, $"-c http.extraHeader=\"Authorization: Basic {encodedToken}\" push origin {headBranch}", redactOutput: true);

            const string title = $"[Automated] Merge CI changes into {TargetBranch}";
            var body = $"Automated pull request created by CI build {context.BuildId}.";

            context.Information("Creating GitHub PR: {0} → {1} in {2}/{3}", headBranch, TargetBranch, owner, repo);

            var newPr = new NewPullRequest(title, headBranch, TargetBranch)
            {
                Body = body,
                Draft = true,
            };

            var pr = await client.PullRequest.Create(owner, repo, newPr);
            context.Information("Pull request #{0} created: {1}", pr.Number, pr.HtmlUrl);
        }

        private static bool IsTargetedBranch(string sourceBranch)
        {
            const string refsHeads = "refs/heads/";
            var branch = sourceBranch.StartsWith(refsHeads, StringComparison.OrdinalIgnoreCase)
                ? sourceBranch.Substring(refsHeads.Length)
                : sourceBranch;

            return string.Equals(branch, "develop", StringComparison.OrdinalIgnoreCase)
                || string.Equals(branch, "main", StringComparison.OrdinalIgnoreCase)
                || branch.StartsWith("release/", StringComparison.OrdinalIgnoreCase);
        }

        private static async Task<string> GenerateInstallationToken(Context context)
        {
            var appId = context.EnvironmentVariable("GITHUB_APP_ID");
            var privateKeyPem = context.EnvironmentVariable("GITHUB_APP_PRIVATE_KEY");

            // Azure DevOps collapses multi-line secrets into a single line,
            // so we need to normalize the PEM before importing it.
            var normalized = NormalizePem(privateKeyPem);

            var rsa = RSA.Create();
            rsa.ImportFromPem(normalized);

            var now = DateTimeOffset.UtcNow;
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Issuer = appId,
                IssuedAt = now.AddSeconds(-60).UtcDateTime,
                Expires = now.AddMinutes(9).UtcDateTime,
                SigningCredentials = new SigningCredentials(
                    new RsaSecurityKey(rsa),
                    SecurityAlgorithms.RsaSha256),
            };

            var tokenHandler = new JsonWebTokenHandler();
            var jwt = tokenHandler.CreateToken(tokenDescriptor);

            context.Information("Generated JWT for GitHub App ID {0}.", appId);

            // Exchange the JWT for a short-lived installation access token
            using var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwt);
            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/vnd.github+json"));
            httpClient.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("DnnPlatformCakeBuild", "1.0"));

            // Get the installation ID
            var installationsResponse = await httpClient.GetAsync("https://api.github.com/app/installations");
            installationsResponse.EnsureSuccessStatusCode();
            var installationsJson = await installationsResponse.Content.ReadAsStringAsync();
            var installations = JsonConvert.DeserializeObject<List<GitHubInstallation>>(installationsJson);

            if (installations == null || installations.Count == 0)
            {
                throw new CakeException("No GitHub App installations found. Install the app on the target repository first.");
            }

            var installationId = installations[0].Id;
            context.Information("Found GitHub App installation ID: {0}.", installationId);

            // Create an installation access token
            var tokenResponse = await httpClient.PostAsync(
                $"https://api.github.com/app/installations/{installationId}/access_tokens",
                new StringContent(string.Empty));
            tokenResponse.EnsureSuccessStatusCode();
            var tokenJson = await tokenResponse.Content.ReadAsStringAsync();
            var accessToken = JsonConvert.DeserializeObject<GitHubAccessToken>(tokenJson);

            if (string.IsNullOrEmpty(accessToken?.Token))
            {
                throw new CakeException("Failed to obtain a GitHub App installation access token.");
            }

            context.Information("GitHub App installation token generated successfully.");
            return accessToken.Token;
        }

        private static string NormalizePem(string pem)
        {
            // Azure DevOps may replace newlines with literal \r\n or \n sequences
            pem = pem.Replace("\\r\\n", "\n").Replace("\\n", "\n").Trim();

            if (pem.Contains('\n'))
            {
                return pem;
            }

            // The PEM was collapsed to a single line — extract the base64 and re-wrap
            string begin, end;
            if (pem.Contains("BEGIN RSA PRIVATE KEY", StringComparison.Ordinal))
            {
                begin = "-----BEGIN RSA PRIVATE KEY-----";
                end = "-----END RSA PRIVATE KEY-----";
            }
            else if (pem.Contains("BEGIN PRIVATE KEY", StringComparison.Ordinal))
            {
                begin = "-----BEGIN PRIVATE KEY-----";
                end = "-----END PRIVATE KEY-----";
            }
            else
            {
                return pem;
            }

            var base64 = pem
                .Replace(begin, string.Empty)
                .Replace(end, string.Empty)
                .Replace(" ", string.Empty);

            var sb = new StringBuilder();
            sb.AppendLine(begin);
            for (var i = 0; i < base64.Length; i += 64)
            {
                sb.AppendLine(base64.Substring(i, Math.Min(64, base64.Length - i)));
            }

            sb.Append(end);
            return sb.ToString();
        }

        private static async Task UpdateBugReportVersions(Context context, GitHubClient client, string owner, string repo, FilePath bugReportPath)
        {
            context.Information("Fetching GitHub releases to update bug report template…");
            var releases =
                from release in await client.Repository.Release.GetAll(owner, repo)
                where !release.Draft
                let versionWithoutPrefix = release.TagName[1..]
                let versionWithoutSuffix = versionWithoutPrefix.Contains('-') ? versionWithoutPrefix[..versionWithoutPrefix.IndexOf('-')] : versionWithoutPrefix
                let version = Version.Parse(versionWithoutSuffix)
                let isReleaseCandidate = release.TagName.Contains("rc", StringComparison.OrdinalIgnoreCase)
                orderby version descending
                select new { release, version, isReleaseCandidate, };
            releases = releases.ToList();

            var latestStable = releases.FirstOrDefault(r => !r.isReleaseCandidate);

            context.Information(
                "Latest stable release: {0}",
                latestStable != null ? latestStable.release.TagName : "(none)");

            var currentRcs = releases
                .Where(r => r.isReleaseCandidate)
                .Where(r => latestStable == null || r.release.PublishedAt > latestStable.release.PublishedAt)
                .OrderByDescending(r => r.version)
                .ToList();

            context.Information("Found {0} currently published RC release(s).", currentRcs.Count);

            // Build the new options list
            var options = new List<string>();
            if (latestStable != null)
            {
                options.Add($"{latestStable.version} (latest release)");
            }

            options.AddRange(currentRcs.Select(rc => $"{rc.version} (release candidate)"));
            options.Add("develop build (unreleased)");

            // Parse the YAML template and update the affected-versions options
            var yaml = new YamlStream();
            using (var reader = new StreamReader(bugReportPath.FullPath))
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
                context.Warning("Could not locate affected-versions options in {0}, skipping update.", bugReportPath);
                return;
            }

            optionsNode.Children.Clear();
            foreach (var option in options)
            {
                optionsNode.Children.Add(new YamlScalarNode(option));
            }

            await using var stringWriter = new StringWriter();
            yaml.Save(stringWriter, false);

            // YamlStream.Save wraps output in document markers (--- / ...) that the original file doesn't use
            context.FileWriteText(bugReportPath, StripDocumentMarkers(stringWriter.ToString()));
            context.Information("Updated {0} with {1} version option(s).", bugReportPath, options.Count);
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

        private static bool HasStagedChanges(ICakeContext context)
        {
            var process = context.StartAndReturnProcess(
                "git",
                new ProcessSettings
                {
                    Arguments = "diff --cached --quiet",
                    RedirectStandardOutput = true,
                });
            process.WaitForExit();

            // git diff --cached --quiet exits with 1 if there are staged changes, 0 if clean
            return process.GetExitCode() != 0;
        }

        private static void ResetSolutionInfoIfVersionUnchanged(Context context, FilePath solutionInfoPath)
        {
            var committedProcess = context.StartAndReturnProcess(
                "git",
                new ProcessSettings
                {
                    Arguments = $"show HEAD:{solutionInfoPath}",
                    RedirectStandardOutput = true,
                });
            committedProcess.WaitForExit();

            if (committedProcess.GetExitCode() != 0)
            {
                context.Information("Could not read committed {0}, skipping reset check.", solutionInfoPath);
                return;
            }

            var committedContent = string.Join("\n", committedProcess.GetStandardOutput());
            var currentContent = context.ReadFile(solutionInfoPath);

            var committedVersion = ExtractAssemblyVersion(committedContent);
            var currentVersion = ExtractAssemblyVersion(currentContent);

            context.Information("SolutionInfo.cs AssemblyVersion — committed: '{0}', current: '{1}'.", committedVersion, currentVersion);

            if (string.Equals(committedVersion, currentVersion, StringComparison.Ordinal))
            {
                context.Information("Major.Minor.Patch has not changed. Resetting {0} to avoid a noisy PR.", solutionInfoPath);
                Git(context, $"checkout -- {solutionInfoPath}");
            }
            else
            {
                context.Information("Major.Minor.Patch changed ({0} → {1}). Keeping {2} modifications.", committedVersion, currentVersion, solutionInfoPath);
            }
        }

        private static string ExtractAssemblyVersion(string content)
        {
            var match = Regex.Match(content, @"\[assembly:\s*AssemblyVersion\(""([^""]+)""\)\]");
            return match.Success ? match.Groups[1].Value : string.Empty;
        }

        private static void Git(ICakeContext context, string arguments, bool redactOutput = false)
        {
            context.Information("git {0}", redactOutput ? "[redacted]" : arguments);
            using var process = context.StartAndReturnProcess("git", new ProcessSettings { Arguments = arguments, });
            process.WaitForExit();
            if (process.GetExitCode() != 0)
            {
                throw new CakeException($"git {(redactOutput ? "[redacted]" : arguments)} failed with exit code {process.GetExitCode()}.");
            }
        }

        /// <summary>Minimal model for deserializing a GitHub App installation response.</summary>
        private sealed class GitHubInstallation
        {
            /// <summary>Gets or sets the installation ID.</summary>
            [JsonProperty("id")]
            public long Id { get; set; }
        }

        /// <summary>Minimal model for deserializing a GitHub installation access token response.</summary>
        private sealed class GitHubAccessToken
        {
            /// <summary>Gets or sets the access token.</summary>
            [JsonProperty("token")]
            public string Token { get; set; }
        }
    }
}
