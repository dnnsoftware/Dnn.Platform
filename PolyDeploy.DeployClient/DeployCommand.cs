namespace PolyDeploy.DeployClient
{
    using System;
    using System.Collections.Generic;
    using PolyDeploy.Encryption;
    using Spectre.Cli;
    using Spectre.Console;
    using Spectre.Console.Rendering;
    using System.ComponentModel;
    using System.IO;
    using System.Linq;
    using System.Net.Http;
    using System.Net.Http.Json;
    using System.Text.Json;
    using System.Threading.Tasks;

    public class DeployCommand : AsyncCommand<DeployCommand.DeployInput>
    {
        public override async Task<int> ExecuteAsync(CommandContext context, DeployInput input)
        {
            try
            {
                var packageCrawler = new PackageCrawler(input.PackagesDirectory);
                var zipFiles = packageCrawler.GetPackagesFullPaths().ToList();
                if (!zipFiles.Any())
                {
                    AnsiConsole.MarkupLine($"No package zip files found in [yellow]{packageCrawler.PackageDirectoryPath}[/]");
                    return 2;
                }

                AnsiConsole.Render(GetPackagesTree(packageCrawler.PackageDirectoryPath, zipFiles));
                AnsiConsole.WriteLine();

                if (!input.NoPrompt)
                {
                    if (!AnsiConsole.Confirm("Would you like to continue?", false))
                    {
                        AnsiConsole.WriteLine("Exiting");
                        return 3;
                    }
                    AnsiConsole.WriteLine();
                }

                var httpClient = new HttpClient
                {
                    BaseAddress = new Uri(new Uri(input.TargetUri), "DesktopModules/PolyDeploy/API/"),
                    DefaultRequestHeaders = { { "x-api-key", input.ApiKey }, },
                };

                var session = await AnsiConsole.Status().StartAsync<Session?>("Creating session…", async context => 
                    await httpClient.GetFromJsonAsync<Session>("Remote/CreateSession"));

                if (session == null || string.IsNullOrWhiteSpace(session.Guid))
                {
                    AnsiConsole.MarkupLine("[white on red]Unable to create session via API[/]");
                    return 1;
                }

                AnsiConsole.MarkupLine($"Got session: [aqua]{session.Guid}[/].");
                AnsiConsole.WriteLine();

                await AnsiConsole.Status().StartAsync("Encrypting and uploading…", async context => 
                    await Task.WhenAll(zipFiles.Select(zipFile => AddPackageAsync(httpClient, input, session, zipFile, context))));
                AnsiConsole.WriteLine();

                _ = httpClient.GetAsync($"Remote/Install?sessionGuid={session.Guid}");

                return await GetInstallStatus(input, httpClient, session);
            }
            catch (Exception exception)
            {
                AnsiConsole.WriteException(exception);
                throw;
            }
        }

        private static async Task<int> GetInstallStatus(DeployInput input, HttpClient httpClient, Session? session)
        {
            var table = new Table().Centered();
            table.Title = new TableTitle("Installation Jobs");
            return await AnsiConsole.Live(table)
                                    .AutoClear(false)
                                    .StartAsync(
                                        async context =>
                                        {
                                            DateTime attemptStart = DateTime.Now;
                                            bool isComplete = false;
                                            while (!isComplete)
                                            {
                                                SessionProgress? progress = null;
                                                try
                                                {
                                                    progress = await httpClient.GetFromJsonAsync<SessionProgress>(
                                                        $"Remote/GetSession?sessionGuid={session.Guid}");
                                                }
                                                catch (Exception exception)
                                                {
                                                    AnsiConsole.WriteException(exception, ExceptionFormats.ShortenEverything);
                                                }

                                                var timeUntilTimeout =
                                                    TimeSpan.FromSeconds(input.InstallationStatusTimeout)
                                                    - (DateTime.Now - attemptStart);
                                                if (timeUntilTimeout < TimeSpan.Zero)
                                                {
                                                    AnsiConsole.MarkupLine(
                                                        "[white on red]Unable to access API to get session details[/]");
                                                    return 1;
                                                }

                                                if (string.IsNullOrWhiteSpace(progress?.Response))
                                                {
                                                    AnsiConsole.MarkupLine(
                                                        $"[white on red]Retrying session request, timeout in {timeUntilTimeout}[/]");
                                                    await Task.Delay(TimeSpan.FromSeconds(2));
                                                    continue;
                                                }

                                                var jobs = JsonSerializer.Deserialize<SortedList<string, InstallJob?>>(
                                                    progress.Response);
                                                if (!table.Columns.Any())
                                                {
                                                    table.AddColumns("Name", "Attempted", "Success", "Packages", "Failures");
                                                }

                                                while (table.Rows.Any())
                                                {
                                                    table.RemoveRow(0);
                                                }

                                                foreach (var job in jobs
                                                                    ?? Enumerable.Empty<KeyValuePair<string, InstallJob?>>())
                                                {
                                                    if (job.Value == null)
                                                    {
                                                        continue;
                                                    }

                                                    table.AddRow(
                                                        new IRenderable[]
                                                        {
                                                            new Markup(job.Value.Name ?? string.Empty),
                                                            new Markup(
                                                                job.Value.Attempted
                                                                    ? "[green]:check_mark_button:[/]"
                                                                    : string.Empty),
                                                            new Markup(
                                                                job.Value.Success
                                                                    ? "[green]:check_mark_button:[/]"
                                                                    : job.Value.Attempted
                                                                        ? "[red]:cross_mark_button:[/]"
                                                                        : string.Empty),
                                                            GetPackageTable(job.Value.Packages),
                                                            GetFailuresTree(job.Value.Failures),
                                                        });
                                                }

                                                context.Refresh();

                                                isComplete = progress.Status == SessionStatus.Complete;
                                                attemptStart = DateTime.Now;
                                            }

                                            return 0;
                                        });
        }

        private static IRenderable GetFailuresTree(IReadOnlyCollection<string?>? failures)
        {
            if (failures == null || !failures.Any(f => !string.IsNullOrWhiteSpace(f)))
            { 
                return new Markup(string.Empty);
            }

            var tree = new Tree("Failures");
            tree.AddNodes(failures.Where(f => !string.IsNullOrWhiteSpace(f)).Cast<string>());
            return tree;
        }

        private static Table GetPackageTable(IReadOnlyCollection<PackageJob?>? packages)
        {
            var table = new Table().AddColumns("Name", "Version");
            foreach (var package in packages ?? Enumerable.Empty<PackageJob?>())
            {
                if (package == null) { 
                    continue;
                }
                table.AddRow(package.Name ?? string.Empty, package.VersionStr ?? string.Empty);
            }

            return table;
        }

        private static async Task AddPackageAsync(HttpClient httpClient, DeployInput input, Session session, string zipFile, StatusContext context)
        {
            using var fileStream = File.OpenRead(zipFile);
            using var encrypted = Crypto.Encrypt(fileStream, input.EncryptionKey);
            string fileName = Path.GetFileName(zipFile);
            var content = new MultipartFormDataContent { { new StreamContent(encrypted), "none", fileName }, };
            await httpClient.PostAsync($"Remote/AddPackages?sessionGuid={session.Guid}", content);
            
            AnsiConsole.MarkupLine($"Encrypted and uploaded [yellow]{fileName}[/]");
        }

        private static Tree GetPackagesTree(string packageDirectoryPath, IEnumerable<string> zipFiles)
        {
            var tree = new Tree($"[yellow]{packageDirectoryPath}[/]");
            var files = zipFiles.Select(f => $"[aqua]{Path.GetRelativePath(packageDirectoryPath, f)}[/]");
            tree.AddNodes(files);
            return tree;
        }

        private sealed class Session
        { 
            public string? Guid { get; set; }
        }

        private sealed class SessionProgress
        { 
            public SessionStatus Status { get; set; }

            public string? Response { get; set; }
        }

        private sealed class InstallJob
        {
            public string? Name { get; set; }
            public IReadOnlyCollection<PackageJob?>? Packages { get; set; }
            public IReadOnlyCollection<string?>? Failures { get; set; }
            public bool Attempted { get; set; }
            public bool Success { get; set; }
        }

        private sealed class PackageJob
        {
            public string? Name { get; set; }
            public IReadOnlyCollection<PackageDependency?>? Packages { get; set; }
            public string? VersionStr { get; set; }
        }

        private sealed class PackageDependency
        {
            public bool IsPackageDependency { get; set; }
            public string? PackageName { get; set; }
            public string? DependencyVersion { get; set; }
        }

        private enum SessionStatus
        { 
            NotStarted = 0,
            InProgess = 1,
            Complete = 2
        }

        public sealed class DeployInput : CommandSettings
        {
            [CommandOption("--encryption-key|-e")]
            [Description("The encryption key from the PolyDeploy API")]
            public string EncryptionKey { get; init; } = null!;

            [CommandOption("--api-key|-a")]
            [Description("The API key from the PolyDeploy API")]
            public string ApiKey { get; init; } = null!;

            [CommandOption("--target-uri|-u")]
            [Description("The URL of the site with the PolyDeploy API")]
            public string TargetUri { get; init; } = null!;

            [CommandOption("--packages-directory|-d")]
            [Description("Defines the directory that contains the to install packages.")]
            public string PackagesDirectory{ get; init; } = string.Empty;

            [CommandOption("--installation-status-timeout|-t")]
            [Description("The number of seconds to ignore 404 errors when checking installation status")]
            [DefaultValue(60d)]
            public double InstallationStatusTimeout { get; init; }

            [CommandOption("--silent|-s")]
            [Description("Whether to not print any output")]
            public bool IsSilent { get; init; }

            [CommandOption("--no-prompt|-n")]
            [Description("Whether to prompt the user for confirmation")]
            public bool NoPrompt { get; init; }

            public override Spectre.Cli.ValidationResult Validate()
            {
                if (string.IsNullOrWhiteSpace(this.EncryptionKey))
                { 
                    return Spectre.Cli.ValidationResult.Error("--encryption-key is required");
                }

                if (string.IsNullOrWhiteSpace(this.ApiKey))
                { 
                    return Spectre.Cli.ValidationResult.Error("--api-key is required");
                }

                if (string.IsNullOrWhiteSpace(this.TargetUri))
                { 
                    return Spectre.Cli.ValidationResult.Error("--target-uri is required");
                }

                return base.Validate();
            }
        }
    }
}
