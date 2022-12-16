namespace PolyDeploy.DeployClient
{
    using Spectre.Console;

    public class Renderer : IRenderer
    {
        private readonly IAnsiConsole console;

        public Renderer(IAnsiConsole console)
        {
            this.console = console;
        }

        public void Welcome(LogLevel level)
        {
            var shouldLog = level <= LogLevel.Information;
            if (shouldLog)
            {
                this.console.Write(new FigletText("PolyDeploy").Color(Color.Orange1));
            }
        }

        public async Task RenderFileUploadsAsync(LogLevel level, IEnumerable<(string file, Task uploadTask)> uploads)
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
                            var progressTask = context.AddTask(upload.file);
                            await upload.uploadTask;
                            progressTask.Increment(100);
                            progressTask.StopTask();
                        }));
                    });
            }
            else
            {
                await Task.WhenAll(uploads.Select(async upload =>
                {
                    await upload.uploadTask;
                    if (shouldLog)
                    {
                        this.console.MarkupLineInterpolated($"{upload.file} upload complete");
                    }
                }));
            }
        }

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
                            packageNode.AddNode(Markup.FromInterpolated($"Depends on :wrapped_gift: [lime]Platform Version[/] [grey]{dependency.PackageName}[/]"));
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

        public void RenderListOfFiles(LogLevel level, IEnumerable<string> files)
        {
            var shouldLog = level <= LogLevel.Information;
            if (shouldLog)
            {
                var fileTree = new Tree(new Markup(":file_folder: [yellow]Packages[/]"));
                fileTree.AddNodes(files.Select(f => Markup.FromInterpolated($":page_facing_up: [aqua]{f}[/]")));

                this.console.Write(fileTree);
            }
        }

        public void RenderInstallationStatus(LogLevel level, SortedList<int, SessionResponse?> packageFiles)
        {
            foreach (var file in packageFiles.Values)
            {
                if (file?.Name is null) continue;
                if (file.Success && !succeededPackageFiles.Contains(file.Name))
                {
                    if (level <= LogLevel.Information)
                    {
                        this.console.MarkupLineInterpolated($":check_mark_button: [aqua]{file.Name}[/] [green]Succeeded[/]");
                    }

                    succeededPackageFiles.Add(file.Name);
                }

                if (file.Failures?.Any() == true && !failedPackageFiles.Contains(file.Name))
                {
                    if (level <= LogLevel.Error)
                    {
                        var failureTree = new Tree(Markup.FromInterpolated($":cross_mark: [aqua]{file.Name}[/] [red]Failed[/]"));
                        failureTree.AddNodes(file.Failures.Where(f => f != null).Select(f => f!));
                        this.console.Write(failureTree);
                    }

                    failedPackageFiles.Add(file.Name);
                }
            }
        }

        public void RenderCriticalError(LogLevel level, string message, Exception exception)
        {
            if (level > LogLevel.Critical)
            {
                return;
            }
            
            this.console.WriteLine(message);
            this.console.WriteException(exception);
        }

        private readonly HashSet<string> succeededPackageFiles = new();
        private readonly HashSet<string> failedPackageFiles = new();
    }
}
