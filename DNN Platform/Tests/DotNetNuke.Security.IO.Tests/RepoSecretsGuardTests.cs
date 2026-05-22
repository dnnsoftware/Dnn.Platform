// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Security.IO.Tests;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

using FluentAssertions;

using Xunit;

/// <summary>
/// Repository-wide guard tests against the SonarQube findings this feature remediates.
/// These tests fail intentionally before the fix is applied and pass after.
/// </summary>
public class RepoSecretsGuardTests
{
    private static readonly string RepoRoot = LocateRepoRoot();

    private static readonly string[] ExcludedTopLevelDirs =
    {
        ".git", ".specify", "specs", ".claude", "bin", "obj", "Packages", "node_modules",
    };

    [Fact]
    public void Repo_Contains_No_Password_Equals_Sa_Literal()
    {
        var pattern = new Regex(@"Password\s*=\s*sa\b", RegexOptions.IgnoreCase);
        var hits = FindHits("*.cs", pattern);

        hits.Should().BeEmpty(
            because: "SonarQube rule secrets:S6703 flags real-looking DB password literals " +
                     "(including XML-doc examples) as disclosed credentials.");
    }

    private static IReadOnlyList<string> FindHits(string searchPattern, Regex pattern)
    {
        var hits = new List<string>();
        foreach (var file in EnumerateTrackedFiles(searchPattern))
        {
            string text;
            try
            {
                text = File.ReadAllText(file);
            }
            catch (IOException)
            {
                continue;
            }

            if (pattern.IsMatch(text))
            {
                hits.Add(file.Length > RepoRoot.Length + 1
                    ? file.Substring(RepoRoot.Length + 1)
                    : file);
            }
        }

        return hits;
    }

    private static IEnumerable<string> EnumerateTrackedFiles(string searchPattern)
    {
        return EnumerateAcceptedDirectories(RepoRoot)
            .SelectMany(dir => SafeEnumerate(dir, searchPattern));
    }

    private static IEnumerable<string> EnumerateAcceptedDirectories(string root)
    {
        yield return root;
        foreach (var top in SafeEnumerateDirs(root))
        {
            if (ExcludedTopLevelDirs.Any(ex =>
                string.Equals(Path.GetFileName(top), ex, StringComparison.OrdinalIgnoreCase)))
            {
                continue;
            }

            foreach (var sub in WalkDirectories(top))
            {
                yield return sub;
            }
        }
    }

    private static IEnumerable<string> WalkDirectories(string start)
    {
        yield return start;
        foreach (var dir in SafeEnumerateDirs(start))
        {
            var name = Path.GetFileName(dir);
            if (string.Equals(name, "bin", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(name, "obj", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(name, ".vs", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            foreach (var sub in WalkDirectories(dir))
            {
                yield return sub;
            }
        }
    }

    private static IEnumerable<string> SafeEnumerateDirs(string path)
    {
        try
        {
            return Directory.EnumerateDirectories(path);
        }
        catch (UnauthorizedAccessException)
        {
            return Array.Empty<string>();
        }
    }

    private static IEnumerable<string> SafeEnumerate(string path, string searchPattern)
    {
        try
        {
            return Directory.EnumerateFiles(path, searchPattern, SearchOption.TopDirectoryOnly);
        }
        catch (UnauthorizedAccessException)
        {
            return Array.Empty<string>();
        }
    }

    private static string LocateRepoRoot()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null && !File.Exists(Path.Combine(dir.FullName, "DNN_Platform.sln")))
        {
            dir = dir.Parent;
        }

        if (dir is null)
        {
            throw new InvalidOperationException(
                "Could not locate repository root (DNN_Platform.sln) from " +
                AppContext.BaseDirectory);
        }

        return dir.FullName;
    }
}
