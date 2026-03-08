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

    using Cake.Common;
    using Cake.Common.Diagnostics;
    using Cake.Core;
    using Cake.Core.IO;
    using Cake.Frosting;

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
    public sealed class CreateGitHubPullRequest : FrostingTask<Context>
    {
        private const string TargetBranch = "develop";
        private const string BugReportPath = ".github/ISSUE_TEMPLATE/bug-report.yml";
        private const string SolutionInfoPath = "SolutionInfo.cs";
        private const string GitUserName = "DNN Platform CI Bot";
        private const string GitUserEmail = "noreply@dnncommunity.org";

        /// <inheritdoc/>
        public override void Run(Context context)
        {
            if (!context.IsRunningInCI)
            {
                context.Information("Skipping CreateGitHubPullRequest because the build is not running in CI.");
                return;
            }

            var sourceBranch = context.EnvironmentVariable("BUILD_SOURCEBRANCH") ?? string.Empty;
            context.Information("CreateGitHubPullRequest: BUILD_SOURCEBRANCH is '{0}'.", sourceBranch);
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
            var repoSlug = context.EnvironmentVariable("BUILD_REPOSITORY_NAME")
                           ?? throw new CakeException("BUILD_REPOSITORY_NAME environment variable is not set.");

            var parts = repoSlug.Split('/');
            if (parts.Length != 2)
            {
                throw new CakeException($"BUILD_REPOSITORY_NAME '{repoSlug}' is not in the expected 'owner/repo' format.");
            }

            var owner = parts[0];
            var repo = parts[1];

            // Generate a short-lived installation token from the GitHub App credentials
            var token = GenerateInstallationToken(context);

            var client = new GitHubClient(new ProductHeaderValue("DnnPlatformCakeBuild"))
            {
                Credentials = new Credentials(token),
            };

            context.Information("Authenticated as GitHub App installation.");

            // Update bug-report.yml with version info from GitHub releases
            UpdateBugReportVersions(context, client, owner, repo);

            // Reset SolutionInfo.cs if only the commit count/SHA changed (not the major.minor.patch)
            // to avoid creating a PR for every single commit.
            ResetSolutionInfoIfVersionUnchanged(context);

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
            Git(context, $"commit -m \"[Automated] CI build {context.BuildId} changes\"");

            // Push using token via HTTP header so it never appears in logs
            Git(context, $"remote set-url origin https://github.com/{repoSlug}.git");
            var encodedToken = Convert.ToBase64String(Encoding.UTF8.GetBytes($"x-access-token:{token}"));
            Git(context, $"-c http.extraHeader=\"Authorization: Basic {encodedToken}\" push origin {headBranch}", redactOutput: true);

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

        private static string GenerateInstallationToken(Context context)
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
            var installationsResponse = httpClient.GetAsync("https://api.github.com/app/installations").GetAwaiter().GetResult();
            installationsResponse.EnsureSuccessStatusCode();
            var installationsJson = installationsResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            var installations = JsonConvert.DeserializeObject<List<GitHubInstallation>>(installationsJson);

            if (installations == null || installations.Count == 0)
            {
                throw new CakeException("No GitHub App installations found. Install the app on the target repository first.");
            }

            var installationId = installations[0].Id;
            context.Information("Found GitHub App installation ID: {0}.", installationId);

            // Create an installation access token
            var tokenResponse = httpClient.PostAsync(
                $"https://api.github.com/app/installations/{installationId}/access_tokens",
                new StringContent(string.Empty)).GetAwaiter().GetResult();
            tokenResponse.EnsureSuccessStatusCode();
            var tokenJson = tokenResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
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

        private static void ResetSolutionInfoIfVersionUnchanged(Context context)
        {
            var committedProcess = context.StartAndReturnProcess(
                "git",
                new ProcessSettings
                {
                    Arguments = $"show HEAD:{SolutionInfoPath}",
                    RedirectStandardOutput = true,
                });
            committedProcess.WaitForExit();

            if (committedProcess.GetExitCode() != 0)
            {
                context.Information("Could not read committed {0}, skipping reset check.", SolutionInfoPath);
                return;
            }

            var committedContent = string.Join("\n", committedProcess.GetStandardOutput());
            var currentContent = File.ReadAllText(SolutionInfoPath);

            var committedVersion = ExtractAssemblyVersion(committedContent);
            var currentVersion = ExtractAssemblyVersion(currentContent);

            context.Information("SolutionInfo.cs AssemblyVersion — committed: '{0}', current: '{1}'.", committedVersion, currentVersion);

            if (string.Equals(committedVersion, currentVersion, StringComparison.Ordinal))
            {
                context.Information("Major.Minor.Patch has not changed. Resetting {0} to avoid a noisy PR.", SolutionInfoPath);
                Git(context, $"checkout -- {SolutionInfoPath}");
            }
            else
            {
                context.Information("Major.Minor.Patch changed ({0} → {1}). Keeping {2} modifications.", committedVersion, currentVersion, SolutionInfoPath);
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
