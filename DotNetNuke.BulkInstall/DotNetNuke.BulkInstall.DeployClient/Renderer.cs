// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.BulkInstall.DeployClient;

using Spectre.Console;

/// <summary>The <see cref="IRenderer"/> implementation, using <see cref="IAnsiConsole"/> (i.e. Spectre.Console).</summary>
public class Renderer : IRenderer, ILogger
{
    private readonly IAnsiConsole console;
    private readonly bool useDecorativeIcons;
    private readonly bool unicodeSupported;
    private readonly int? outputEncodingCodePage;
    private readonly HashSet<string> succeededPackageFiles = new();
    private readonly HashSet<string> failedPackageFiles = new();

    /// <summary>Initializes a new instance of the <see cref="Renderer"/> class.</summary>
    /// <param name="console">The console.</param>
    public Renderer(IAnsiConsole console)
    {
        this.console = console;
        this.useDecorativeIcons = this.CanUseDecorativeIcons(out var unicodeSupported, out var outputEncodingCodePage);
        this.unicodeSupported = unicodeSupported;
        this.outputEncodingCodePage = outputEncodingCodePage;
    }

    /// <summary>Initializes a new instance of the <see cref="Renderer"/> class with explicit icon support control (for testing).</summary>
    /// <param name="console">The console.</param>
    /// <param name="useDecorativeIcons">Whether to use decorative unicode icons.</param>
    internal Renderer(IAnsiConsole console, bool useDecorativeIcons)
    {
        this.console = console;
        this.useDecorativeIcons = useDecorativeIcons;
        this.unicodeSupported = useDecorativeIcons;
        this.outputEncodingCodePage = useDecorativeIcons ? 65001 : null;
    }

    /// <inheritdoc/>
    public void Welcome(LogLevel level)
    {
        if (level <= LogLevel.Trace)
        {
            this.console.WriteLine($"Icon rendering enabled: {this.useDecorativeIcons} (Unicode: {this.unicodeSupported}, CodePage: {this.outputEncodingCodePage?.ToString() ?? "unknown"})");
        }

        var shouldLog = level <= LogLevel.Information;
        if (!shouldLog)
        {
            return;
        }

        this.console.Write(new FigletText("Bulk Install").Color(Color.Orange1));
    }

    /// <inheritdoc/>
    public async Task RenderFileUploadsAsync(LogLevel level, IEnumerable<UploadPackageResult> uploads)
    {
        var shouldLog = level <= LogLevel.Information;
        if (this.console.Profile.Capabilities.Interactive && shouldLog)
        {
            await this.console.Progress()
                .StartAsync(async context =>
                {
                    await Task.WhenAll(uploads.Select(async upload =>
                    {
                        var progressTask = context.AddTask(upload.PackageName);
                        progressTask.MaxValue = 1;
                        upload.OnProgress += (_, progress) => { progressTask.Value = progress; };
                        await upload.UploadTask;
                        progressTask.Value = progressTask.MaxValue;
                        progressTask.StopTask();
                    }));
                });
        }
        else
        {
            await Task.WhenAll(uploads.Select(async upload =>
            {
                await upload.UploadTask;
                if (shouldLog)
                {
                    this.console.MarkupLineInterpolated($"{upload.PackageName} upload complete");
                }
            }));
        }
    }

    /// <inheritdoc/>
    public void RenderInstallationOverview(LogLevel level, SortedList<int, SessionResponse?> packageFiles)
    {
        if (level > LogLevel.Information)
        {
            return;
        }

        var tree = new Tree(Markup.FromInterpolated($"{Icon(this.useDecorativeIcons, ":file_folder:", "DIR")} [yellow]Packages[/]"));
        foreach (var packageFile in packageFiles.Values)
        {
            if (packageFile == null)
            {
                continue;
            }

            var fileNode = tree.AddNode(Markup.FromInterpolated($"{Icon(this.useDecorativeIcons, ":page_facing_up:", "FILE")} [aqua]{packageFile.Name}[/]"));
            if (packageFile.Packages == null)
            {
                continue;
            }

            foreach (var package in packageFile.Packages)
            {
                if (package == null)
                {
                    continue;
                }

                var packageNode = fileNode.AddNode(Markup.FromInterpolated($"{Icon(this.useDecorativeIcons, ":wrapped_gift:", "PKG")} [lime]{package.Name}[/] [grey]{package.VersionStr}[/]"));
                if (package.Dependencies == null)
                {
                    continue;
                }

                foreach (var dependency in package.Dependencies)
                {
                    if (dependency == null)
                    {
                        continue;
                    }

                    if (string.IsNullOrEmpty(dependency.DependencyVersion) && !dependency.IsPackageDependency)
                    {
                        if (Version.TryParse(dependency.PackageName, out _))
                        {
                            packageNode.AddNode(Markup.FromInterpolated($"Depends on {Icon(this.useDecorativeIcons, ":radioactive:", "DNN")} [lime]Platform Version[/] [grey]{dependency.PackageName}[/]"));
                        }
                        else
                        {
                            packageNode.AddNode(Markup.FromInterpolated($"Depends on {Icon(this.useDecorativeIcons, ":gear:", "DEP")} [grey]{dependency.PackageName}[/]"));
                        }
                    }
                    else
                    {
                        packageNode.AddNode(Markup.FromInterpolated($"Depends on {Icon(this.useDecorativeIcons, ":wrapped_gift:", "PKG")} [lime]{dependency.PackageName}[/] [grey]{dependency.DependencyVersion}[/]"));
                    }
                }
            }
        }

        this.console.Write(tree);
    }

    /// <inheritdoc/>
    public void RenderListOfFiles(LogLevel level, IEnumerable<string> files)
    {
        var shouldLog = level <= LogLevel.Information;
        if (!shouldLog)
        {
            return;
        }

        var separatedFiles = files.Select(GetFileParts).Select(fileParts => fileParts.ToArray());

        var fileTree = new Tree(Markup.FromInterpolated($"{Icon(this.useDecorativeIcons, ":file_folder:", "DIR")} [yellow]Packages[/]"));
        fileTree.AddNodes(MakeNode(separatedFiles, this.useDecorativeIcons));

        this.console.Write(fileTree);

        static TreeNode MakeNode(IEnumerable<string[]> files, bool useDecorativeIcons)
        {
            var filesList = files.ToList();
            if (filesList is [[var fileName,],])
            {
                return new TreeNode(Markup.FromInterpolated($"{Icon(useDecorativeIcons, ":page_facing_up:", "FILE")} [aqua]{fileName}[/]"));
            }

            var (joinedPath, groupedFiles) = GetGroupedFiles(filesList);

            var folderNode =
                new TreeNode(Markup.FromInterpolated($"{Icon(useDecorativeIcons, ":file_folder:", "DIR")} [yellow]{joinedPath}[/]"));

            folderNode.AddNodes(groupedFiles.Select(f => MakeNode(f, useDecorativeIcons)));

            return folderNode;
        }

        static (string JoinedPath, IEnumerable<IEnumerable<string[]>> GroupedParts) GetGroupedFiles(IReadOnlyList<string[]> files)
        {
            string joinedPath;
            IEnumerable<IEnumerable<string[]>> groupedParts;

            var firstFile = files[0];
            var minParts = files.Min(parts => parts.Length);
            for (var i = 0; i < minParts; i++)
            {
                if (!files.Any(parts => parts[i] != firstFile[i]))
                {
                    continue;
                }

                joinedPath = string.Concat(firstFile.Take(i));
                groupedParts = files.GroupBy(
                    parts => string.Concat(parts.Take(i + 1)),
                    parts => parts[i..]);
                return (joinedPath, groupedParts);
            }

            joinedPath = string.Concat(firstFile.Take(minParts - 1));
            groupedParts = new[] { new[] { new[] { firstFile.Last() } } };
            return (joinedPath, groupedParts);
        }

        static IEnumerable<string> GetFileParts(string filePath)
        {
            var directory = Path.GetDirectoryName(filePath);
            if (directory == null)
            {
                yield return EnsureEndsWithSlash(filePath);
                yield break;
            }

            foreach (var part in GetFileParts(directory))
            {
                yield return EnsureEndsWithSlash(part);
            }

            yield return Path.GetFileName(filePath);
        }

        static string EnsureEndsWithSlash(string str)
        {
            if (str.EndsWith(Path.DirectorySeparatorChar))
            {
                return str;
            }

            return str + Path.DirectorySeparatorChar;
        }
    }

    /// <inheritdoc/>
    public void RenderInstallationStatus(LogLevel level, SortedList<int, SessionResponse?> packageFiles)
    {
        foreach (var file in packageFiles.Values)
        {
            if (file?.Name is null)
            {
                continue;
            }

            if (file.Success && !this.succeededPackageFiles.Contains(file.Name))
            {
                if (level <= LogLevel.Information)
                {
                    this.console.MarkupLineInterpolated($"{Icon(this.useDecorativeIcons, ":check_mark_button:", "OK")} [aqua]{file.Name}[/] [green]Succeeded[/]");
                }

                this.succeededPackageFiles.Add(file.Name);
            }

            if ((file.Failures?.Any() == true || !file.CanInstall) && !this.failedPackageFiles.Contains(file.Name))
            {
                if (level <= LogLevel.Error)
                {
                    var failureTree = new Tree(Markup.FromInterpolated($"{Icon(this.useDecorativeIcons, ":cross_mark:", "ERR")} [aqua]{file.Name}[/] [red]Failed[/]"));

                    if (file.Failures?.Any() == true)
                    {
                        failureTree.AddNodes(file.Failures.Where(f => f != null).Select(f => new Text(f!)));
                    }
                    else
                    {
                        failureTree.AddNode("Can't install some packages, check their dependencies");
                        if (file.Packages != null)
                        {
                            failureTree.AddNodes(file.Packages.Where(p => p?.CanInstall == false).Select(p => Markup.FromInterpolated($"Unable to install [aqua]{p!.Name}[/]")));
                        }
                    }

                    this.console.Write(failureTree);
                }

                this.failedPackageFiles.Add(file.Name);
            }
        }
    }

    /// <inheritdoc/>
    public void RenderCriticalError(LogLevel level, string message, Exception exception)
    {
        if (level > LogLevel.Critical)
        {
            return;
        }

        this.console.WriteLine(message);
        this.console.WriteException(exception);
    }

    /// <inheritdoc/>
    public void LogTrace(LogLevel level, string message)
    {
        if (level > LogLevel.Trace)
        {
            return;
        }

        this.console.WriteLine(message);
    }

    private static string Icon(bool useDecorativeIcons, string decorativeIcon, string fallbackText)
    {
        _ = fallbackText;
        return useDecorativeIcons ? $"{decorativeIcon} " : string.Empty;
    }

    private bool CanUseDecorativeIcons(out bool unicodeSupported, out int? outputEncodingCodePage)
    {
        unicodeSupported = this.console.Profile.Capabilities.Unicode;
        outputEncodingCodePage = null;

        if (!unicodeSupported)
        {
            return false;
        }

        try
        {
            outputEncodingCodePage = Console.OutputEncoding.CodePage;
        }
        catch
        {
            return false;
        }

        return outputEncodingCodePage == 65001;
    }
}
