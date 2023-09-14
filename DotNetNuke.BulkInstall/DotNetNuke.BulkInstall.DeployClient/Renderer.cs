// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.BulkInstall.DeployClient;

using Spectre.Console;

/// <summary>The <see cref="IRenderer"/> implementation, using <see cref="IAnsiConsole"/> (i.e. Spectre.Console).</summary>
public class Renderer : IRenderer
{
    private readonly IAnsiConsole console;
    private readonly HashSet<string> succeededPackageFiles = new();
    private readonly HashSet<string> failedPackageFiles = new();

    /// <summary>Initializes a new instance of the <see cref="Renderer"/> class.</summary>
    /// <param name="console">The console.</param>
    public Renderer(IAnsiConsole console)
    {
        this.console = console;
    }

    /// <inheritdoc/>
    public void Welcome(LogLevel level)
    {
        var shouldLog = level <= LogLevel.Information;
        if (!shouldLog)
        {
            return;
        }

        this.console.Write(new FigletText("PolyDeploy").Color(Color.Orange1));
    }

    /// <inheritdoc/>
    public async Task RenderFileUploadsAsync(LogLevel level, IEnumerable<(string File, Task UploadTask)> uploads)
    {
        var shouldLog = level <= LogLevel.Information;
        if (this.console.Profile.Capabilities.Interactive && shouldLog)
        {
            // TODO: actually show upload progress
            await this.console.Progress()
                .StartAsync(async context =>
                {
                    await Task.WhenAll(uploads.Select(async upload =>
                    {
                        var progressTask = context.AddTask(upload.File);
                        await upload.UploadTask;
                        progressTask.Increment(100);
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
                    this.console.MarkupLineInterpolated($"{upload.File} upload complete");
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

        var tree = new Tree(new Markup(":file_folder: [yellow]Packages[/]"));
        foreach (var packageFile in packageFiles.Values)
        {
            if (packageFile == null)
            {
                continue;
            }

            var fileNode = tree.AddNode(Markup.FromInterpolated($":page_facing_up: [aqua]{packageFile.Name}[/]"));
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

                var packageNode = fileNode.AddNode(Markup.FromInterpolated($":wrapped_gift: [lime]{package.Name}[/] [grey]{package.VersionStr}[/]"));
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
                        packageNode.AddNode(Markup.FromInterpolated($"Depends on :radioactive: [lime]Platform Version[/] [grey]{dependency.PackageName}[/]"));
                    }
                    else
                    {
                        packageNode.AddNode(Markup.FromInterpolated($"Depends on :wrapped_gift: [lime]{dependency.PackageName}[/] [grey]{dependency.DependencyVersion}[/]"));
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

        var fileTree = new Tree(new Markup(":file_folder: [yellow]Packages[/]"));
        fileTree.AddNodes(MakeNode(separatedFiles));

        this.console.Write(fileTree);

        static TreeNode MakeNode(IEnumerable<string[]> files)
        {
            var filesList = files.ToList();
            if (filesList is[[var fileName,],])
            {
                return new TreeNode(Markup.FromInterpolated($":page_facing_up: [aqua]{fileName}[/]"));
            }

            var (joinedPath, groupedFiles) = GetGroupedFiles(filesList);

            var folderNode =
                new TreeNode(Markup.FromInterpolated($":file_folder: [yellow]{joinedPath}[/]"));

            folderNode.AddNodes(groupedFiles.Select(MakeNode));

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
                    this.console.MarkupLineInterpolated($":check_mark_button: [aqua]{file.Name}[/] [green]Succeeded[/]");
                }

                this.succeededPackageFiles.Add(file.Name);
            }

            if ((file.Failures?.Any() == true || !file.CanInstall) && !this.failedPackageFiles.Contains(file.Name))
            {
                if (level <= LogLevel.Error)
                {
                    var failureTree = new Tree(Markup.FromInterpolated($":cross_mark: [aqua]{file.Name}[/] [red]Failed[/]"));

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
}
