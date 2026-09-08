// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Tests.BulkInstall.DeployClient;

using DotNetNuke.BulkInstall.DeployClient;

using NUnit.Framework;

using Spectre.Console;
using Spectre.Console.Rendering;
using Spectre.Console.Testing;

[TestFixture]
public class RendererTests
{
    public static LogLevel[] AllLogLevels()
    {
        return Enum.GetValues<LogLevel>();
    }

    public static IEnumerable<LogLevel> LogLevelsGreaterThanOrEqualTo(LogLevel minimumLevel)
    {
        return Enum.GetValues<LogLevel>().Where(logLevel => logLevel >= minimumLevel);
    }

    public static IEnumerable<LogLevel> LogLevelsLessThanOrEqualTo(LogLevel maximumLevel)
    {
        return Enum.GetValues<LogLevel>().Where(logLevel => logLevel <= maximumLevel);
    }

    [TestCaseSource(nameof(LogLevelsLessThanOrEqualTo), new object[] { LogLevel.Information, })]
    public void Welcome_DisplaysSomething(LogLevel logLevel)
    {
        var console = new TestConsole();

        var renderer = new Renderer(console);
        renderer.Welcome(logLevel);

        console.Output.ShouldNotBeNullOrWhiteSpace();
    }

    [TestCaseSource(nameof(LogLevelsGreaterThanOrEqualTo), new object[] { LogLevel.Warning, })]
    public void Welcome_WithHighLogLevel_DisplaysNothing(LogLevel logLevel)
    {
        var console = new TestConsole();

        var renderer = new Renderer(console);
        renderer.Welcome(logLevel);

        console.Output.ShouldBeEmpty();
    }

    [TestCaseSource(nameof(LogLevelsLessThanOrEqualTo), new object[] { LogLevel.Information, })]
    public void RenderListOfFiles_DisplaysSomething(LogLevel logLevel)
    {
        var console = new TestConsole().Interactive();

        var renderer = new Renderer(console);
        renderer.RenderListOfFiles(logLevel, new[] { "OpenContent_4.5.0_Install.zip", "2sxc_12.4.4_Install.zip", });

        console.Output.ShouldContain("OpenContent_4.5.0_Install.zip");
        console.Output.ShouldContain("2sxc_12.4.4_Install.zip");
    }

    [TestCaseSource(nameof(LogLevelsGreaterThanOrEqualTo), new object[] { LogLevel.Warning, })]
    public void RenderListOfFiles_WithHighLogLevel_DisplaysNothing(LogLevel logLevel)
    {
        var console = new TestConsole().Interactive();

        var renderer = new Renderer(console);
        renderer.RenderListOfFiles(logLevel, new[] { "OpenContent_4.5.0_Install.zip", "2sxc_12.4.4_Install.zip", });

        console.Output.ShouldBeEmpty();
    }

    [Test]
    public void RenderListOfFiles_DisplaysFilePath()
    {
        var console = new TestConsole().Interactive();

        var renderer = new Renderer(console);
        renderer.RenderListOfFiles(LogLevel.Information, new[] { @"E:\foo\bar\OpenContent_4.5.0_Install.zip", @"E:\foo\bar\2sxc_12.4.4_Install.zip", });

        console.Output.ShouldContainStringsInOrder(onlyOnce: true, @"E:\foo\bar", "OpenContent_4.5.0_Install.zip", "2sxc_12.4.4_Install.zip");
    }

    [Test]
    public void RenderListOfFiles_WithTopLevelMultiplePaths_DisplaysFilePathsInTree()
    {
        var console = new TestConsole().Interactive();

        var renderer = new Renderer(console);
        renderer.RenderListOfFiles(LogLevel.Information, new[]
        {
            @"E:\my-top-level.zip",
            @"E:\foo\Modules\the-module.zip",
            @"E:\foo\Modules\the-templates.zip",
            @"E:\foo\Skins\themezilla.zip",
        });

        console.Output.ShouldContainStringsInOrder(
            onlyOnce: true,
            "Packages",
            "\n",
            @"E:\",
            "\n",
            "my-top-level.zip",
            "\n",
            "foo",
            "\n",
            "Modules",
            "\n",
            "the-module.zip",
            "\n",
            "the-templates.zip",
            "\n",
            "Skins",
            "\n",
            "themezilla.zip",
            "\n");
    }

    [Test]
    public void RenderListOfFiles_WithMultiplePaths_DisplaysFilePathsInTree()
    {
        var console = new TestConsole().Interactive();

        var renderer = new Renderer(console);
        renderer.RenderListOfFiles(LogLevel.Information, new[]
        {
            @"E:\foo\Containers\my-containers.zip",
            @"E:\foo\Modules\the-module.zip",
            @"E:\foo\Modules\the-templates.zip",
            @"E:\foo\Skins\themezilla.zip",
        });

        console.Output.ShouldContainStringsInOrder(
            onlyOnce: true,
            @"E:\foo",
            "Containers",
            "my-containers.zip",
            "Modules",
            "the-module.zip",
            "the-templates.zip",
            "Skins",
            "themezilla.zip");
    }

    [TestCaseSource(nameof(LogLevelsLessThanOrEqualTo), new object[] { LogLevel.Information, })]
    public async Task RenderFileUploadsAsync_InteractiveWithLogging_RendersSomething(LogLevel logLevel)
    {
        var console = new TestConsole().Interactive();

        var renderer = new Renderer(console);
        var result = new UploadPackageResult(Task.CompletedTask, "OpenContent_4.5.0_Install.zip", new MemoryStream());
        await renderer.RenderFileUploadsAsync(logLevel, new[] { result });

        console.Output.ShouldContainStringsInOrder("OpenContent_4.5.0_Install.zip", "100%");
    }

    [TestCaseSource(nameof(LogLevelsGreaterThanOrEqualTo), new object[] { LogLevel.Warning, })]
    public async Task RenderFileUploadsAsync_InteractiveWithHighLogLevel_RendersNothing(LogLevel logLevel)
    {
        var console = new TestConsole().Interactive();

        var renderer = new Renderer(console);
        var result = new UploadPackageResult(Task.CompletedTask, "OpenContent_4.5.0_Install.zip", new MemoryStream());
        await renderer.RenderFileUploadsAsync(logLevel, new[] { result });

        console.Output.ShouldBeEmpty();
    }

    [TestCaseSource(nameof(LogLevelsLessThanOrEqualTo), new object[] { LogLevel.Information, })]
    public async Task RenderFileUploadsAsync_NonInteractiveWIthLogging_RendersSomething(LogLevel logLevel)
    {
        var console = new TestConsole();

        var renderer = new Renderer(console);
        var result = new UploadPackageResult(Task.CompletedTask, "OpenContent_4.5.0_Install.zip", new MemoryStream());
        await renderer.RenderFileUploadsAsync(logLevel, new[] { result });

        console.Output.ShouldContainStringsInOrder("OpenContent_4.5.0_Install.zip", "upload", "complete", "\n");
    }

    [TestCaseSource(nameof(LogLevelsGreaterThanOrEqualTo), new object[] { LogLevel.Warning, })]
    public async Task RenderFileUploadsAsync_NonInteractiveWithHighLogLevel_RendersNothing(LogLevel logLevel)
    {
        var console = new TestConsole();

        var renderer = new Renderer(console);
        var result = new UploadPackageResult(Task.CompletedTask, "OpenContent_4.5.0_Install.zip", new MemoryStream());
        await renderer.RenderFileUploadsAsync(logLevel, new[] { result });

        console.Output.ShouldBeEmpty();
    }

    [TestCaseSource(nameof(AllLogLevels))]
    public async Task RenderFileUploadsAsync_Interactive_UploadTaskIsAwaited(LogLevel logLevel)
    {
        var console = new TestConsole().Interactive();

        var renderer = new Renderer(console);

        var result = new UploadPackageResult(UploadFile(), "OpenContent_4.5.0_Install.zip", new MemoryStream());

        var exception = await Should.ThrowAsync<Exception>(() => renderer.RenderFileUploadsAsync(logLevel, new[] { result }));

        exception.Message.ShouldBe("UploadFile() was called!");

        static Task UploadFile()
        {
            return Task.FromException(new Exception("UploadFile() was called!"));
        }
    }

    [TestCaseSource(nameof(AllLogLevels))]
    public async Task RenderFileUploadsAsync_UploadTaskIsAwaited(LogLevel logLevel)
    {
        var console = new TestConsole();

        var renderer = new Renderer(console);

        var result = new UploadPackageResult(UploadFile(), "OpenContent_4.5.0_Install.zip", new MemoryStream());

        var exception = await Should.ThrowAsync<Exception>(() => renderer.RenderFileUploadsAsync(logLevel, new[] { result }));

        exception.Message.ShouldBe("UploadFile() was called!");

        static Task UploadFile()
        {
            return Task.FromException(new Exception("UploadFile() was called!"));
        }
    }

    [TestCaseSource(nameof(LogLevelsLessThanOrEqualTo), new object[] { LogLevel.Information, })]
    public void RenderInstallationOverview_DisplaysTreeOfPackageDetails(LogLevel logLevel)
    {
        var packages = new SortedList<int, SessionResponse?>
        {
            {
                0,
                new SessionResponse
                {
                    CanInstall = true,
                    Name = "Jamestown.zip",
                    Packages = new List<PackageResponse?>
                    {
                        new PackageResponse { Name = "James: Town", CanInstall = true, VersionStr = "1.2.3", Dependencies = new List<DependencyResponse?>(0), },
                        new PackageResponse { Name = "Jack: Village", CanInstall = true, VersionStr = "1.2.4", Dependencies = new List<DependencyResponse?>(0), },
                    },
                }
            },
            {
                1,
                new SessionResponse
                {
                    CanInstall = true,
                    Name = "Beanville.zip",
                    Packages = new List<PackageResponse?>
                    {
                        new PackageResponse
                        {
                            Name = "Bean: Ville",
                            CanInstall = true,
                            VersionStr = "2.4.1",
                            Dependencies = new List<DependencyResponse?> { new DependencyResponse { IsPackageDependency = true, PackageName = "James: Town", DependencyVersion = "1.0.0", }, },
                        },
                    },
                }
            },
        };

        Tree? packagesTree = null;
        var console = A.Fake<IAnsiConsole>();
        A.CallTo(() => console.Write(A<Tree>._)).Invokes((IRenderable? tree) => packagesTree = tree as Tree);
        var renderer = new Renderer(console);
        renderer.RenderInstallationOverview(logLevel, packages);

        // TODO: check the structure of the tree, maybe using reflection?
        packagesTree.ShouldNotBeNull();
        var treeSegments = ((IRenderable)packagesTree).Render(RenderOptions.Create(console, new TestCapabilities()), 80);

        var joinedSegments = string.Join(string.Empty, treeSegments.Select(s => s.Text));
        joinedSegments.ShouldContainStringsInOrder(
            new[]
            {
                "Jamestown.zip",
                "James: Town",
                "1.2.3",
                "Jack: Village",
                "1.2.4",
                "Beanville.zip",
                "Bean: Ville",
                "2.4.1",
                "James: Town",
                "1.0.0",
            });
    }

    [TestCaseSource(nameof(LogLevelsLessThanOrEqualTo), new object[] { LogLevel.Information, })]
    public void RenderInstallationOverview_DisplaysDnnPlatformVersionDependency(LogLevel logLevel)
    {
        var console = new TestConsole().Interactive();

        var renderer = new Renderer(console);
        var sessionResponse = new SessionResponse
        {
            Packages = new List<PackageResponse?>
            {
                new()
                {
                    Name = "Jamestown.zip",
                    Dependencies = new List<DependencyResponse?>
                    {
                        new() { PackageName = "09.01.02", DependencyVersion = string.Empty, IsPackageDependency = false, },
                    },
                    CanInstall = true,
                },
            },
        };

        renderer.RenderInstallationOverview(logLevel, new SortedList<int, SessionResponse?> { { 1, sessionResponse }, });
        console.Output.ShouldContainStringsInOrder("Jamestown.zip", "Platform Version", "09.01.02");
    }

    [TestCaseSource(nameof(LogLevelsLessThanOrEqualTo), new object[] { LogLevel.Information, })]
    public void RenderInstallationOverview_DisplaysUnknownDependency(LogLevel logLevel)
    {
        var console = new TestConsole().Interactive();

        var renderer = new Renderer(console, useDecorativeIcons: true);
        var sessionResponse = new SessionResponse
        {
            Packages = new List<PackageResponse?>
            {
                new()
                {
                    Name = "Jamestown.zip",
                    Dependencies = new List<DependencyResponse?>
                    {
                        new() { PackageName = "Unknown Package", DependencyVersion = string.Empty, IsPackageDependency = false, },
                    },
                    CanInstall = true,
                },
            },
        };

        renderer.RenderInstallationOverview(logLevel, new SortedList<int, SessionResponse?> { { 1, sessionResponse }, });
        console.Output.ShouldContainStringsInOrder("Jamestown.zip", "⚙", "Unknown Package");
    }

    [TestCaseSource(nameof(LogLevelsLessThanOrEqualTo), new object[] { LogLevel.Information, })]
    public void RenderInstallationOverview_WhenDependencyHasNoVersion_DisplaysDependencyName(LogLevel logLevel)
    {
        var console = new TestConsole().Interactive();

        var renderer = new Renderer(console);
        var sessionResponse = new SessionResponse
        {
            Packages = new List<PackageResponse?>
            {
                new()
                {
                    Name = "Jamestown.zip",
                    Dependencies = new List<DependencyResponse?>
                    {
                        new() { PackageName = "DNNJWT", DependencyVersion = string.Empty, IsPackageDependency = true, },
                    },
                    CanInstall = true,
                },
            },
        };

        renderer.RenderInstallationOverview(logLevel, new SortedList<int, SessionResponse?> { { 1, sessionResponse }, });
        console.Output.ShouldContainStringsInOrder(new[] { "Jamestown.zip", "DNNJWT" });
    }

    [TestCaseSource(nameof(LogLevelsGreaterThanOrEqualTo), new object[] { LogLevel.Warning, })]
    public void RenderInstallationOverview_LogLevelAboveInformation_DoesNotRender(LogLevel logLevel)
    {
        var console = new TestConsole().Interactive();

        var renderer = new Renderer(console);
        var sessionResponse = new SessionResponse
        {
            Packages = new List<PackageResponse?>
            {
                new()
                {
                    Name = "Jamestown.zip",
                    Dependencies = new List<DependencyResponse?>
                    {
                        new() { PackageName = "DNNJWT", DependencyVersion = string.Empty, IsPackageDependency = true, },
                    },
                    CanInstall = true,
                },
            },
        };

        renderer.RenderInstallationOverview(logLevel, new SortedList<int, SessionResponse?> { { 1, sessionResponse }, });
        console.Output.ShouldBeEmpty();
    }

    [TestCaseSource(nameof(LogLevelsLessThanOrEqualTo), new object[] { LogLevel.Trace, })]
    public void RenderInstallationStatus_OnlyOutputsAttemptedSessionResponses(LogLevel logLevel)
    {
        var console = new TestConsole().Interactive();
        var renderer = new Renderer(console, useDecorativeIcons: true);

        var jimmy = new SessionResponse
        {
            Name = "Jimmy",
            Attempted = true,
            Success = true,
            CanInstall = true,
        };

        var james = new SessionResponse
        {
            Name = "James",
            Attempted = false,
            Success = false,
            CanInstall = true,
        };

        renderer.RenderInstallationStatus(logLevel, new SortedList<int, SessionResponse?> { { 1, jimmy }, { 2, james }, });
        console.Output.ShouldContainStringsInOrder(new[] { "✅", "Jimmy", "Succeeded" });
        console.Output.ShouldNotContain("James");
    }

    [TestCaseSource(nameof(LogLevelsLessThanOrEqualTo), new object[] { LogLevel.Information, })]
    public void RenderInstallationStatus_OutputsMultipleResponsesOnDifferentLines(LogLevel logLevel)
    {
        var console = new TestConsole().Interactive();
        var renderer = new Renderer(console, useDecorativeIcons: true);

        var jimmy = new SessionResponse
        {
            Name = "Jimmy",
            Attempted = true,
            Success = true,
            CanInstall = true,
        };

        var james = new SessionResponse
        {
            Name = "James",
            Attempted = true,
            Success = true,
            CanInstall = true,
        };

        renderer.RenderInstallationStatus(logLevel, new SortedList<int, SessionResponse?> { { 1, jimmy }, { 2, james }, });
        console.Output.ShouldContainStringsInOrder("✅", "Jimmy", "Succeeded", "\n", "✅", "James", "Succeeded", "\n");
    }

    [TestCaseSource(nameof(LogLevelsLessThanOrEqualTo), new object[] { LogLevel.Information, })]
    public void RenderInstallationStatus_WhenNotSuccessfulButNoFailures_ShowsThatPackageFailed(LogLevel logLevel)
    {
        var packages = new SortedList<int, SessionResponse?>
        {
            {
                0,
                new SessionResponse
                {
                    CanInstall = false,
                    Name = "Jamestown.zip",
                    Failures = new List<string?>(0),
                    Attempted = true,
                    Success = false,
                    Packages = new List<PackageResponse?>
                    {
                        new PackageResponse { Name = "James: Town", CanInstall = false, VersionStr = "1.2.3", Dependencies = new List<DependencyResponse?>(1) { new DependencyResponse { DependencyVersion = string.Empty, IsPackageDependency = true, PackageName = "Miss Sing" }, }, },
                    },
                }
            },
        };

        var console = new TestConsole().Interactive();
        var renderer = new Renderer(console, useDecorativeIcons: true);
        renderer.RenderInstallationStatus(logLevel, packages);

        console.Output.ShouldContainStringsInOrder(
            "❌",
            "Jamestown.zip",
            "Failed");
    }

    [TestCaseSource(nameof(LogLevelsGreaterThanOrEqualTo), new object[] { LogLevel.Warning, })]
    public void RenderInstallationStatus_ShouldNotRenderWarningOrHigher(LogLevel logLevel)
    {
        var console = new TestConsole().Interactive();
        var renderer = new Renderer(console);

        var jimmy = new SessionResponse
        {
            Name = "Jimmy",
            Attempted = true,
            Success = true,
            CanInstall = true,
        };

        var james = new SessionResponse
        {
            Name = "James",
            Attempted = true,
            Success = true,
            CanInstall = true,
        };

        renderer.RenderInstallationStatus(logLevel, new SortedList<int, SessionResponse?> { { 1, jimmy }, { 2, james }, });
        console.Output.ShouldBeEmpty();
    }

    [TestCaseSource(nameof(LogLevelsLessThanOrEqualTo), new object[] { LogLevel.Trace, })]
    public void RenderInstallationStatus_DoesNotOutputDuplicateInformation(LogLevel logLevel)
    {
        var console = new TestConsole().Interactive();
        var renderer = new Renderer(console, useDecorativeIcons: true);

        var james = new SessionResponse
        {
            Name = "James",
            Attempted = true,
            Success = true,
            CanInstall = true,
        };

        var george = new SessionResponse
        {
            Name = "George",
            Attempted = false,
            Success = false,
            CanInstall = true,
        };

        renderer.RenderInstallationStatus(logLevel, new SortedList<int, SessionResponse?> { { 2, james }, { 3, george }, });
        george = george with { Attempted = true, Failures = new List<string?> { "He hit the tree 🌴" }, };
        renderer.RenderInstallationStatus(logLevel, new SortedList<int, SessionResponse?> { { 2, james }, { 3, george }, });
        renderer.RenderInstallationStatus(logLevel, new SortedList<int, SessionResponse?> { { 2, james }, { 3, george }, });
        console.Output.ShouldContainStringsInOrder(onlyOnce: true, "✅", "James", "Succeeded", "❌", "George", "Failed", "He hit the tree 🌴");
    }

    [TestCase(LogLevel.Warning)]
    [TestCase(LogLevel.Error)]
    public void RenderInstallationStatus_WhenErrorLevel_DoesNotOutputInformationLevel(LogLevel logLevel)
    {
        var console = new TestConsole().Interactive();
        var renderer = new Renderer(console, useDecorativeIcons: true);

        var james = new SessionResponse
        {
            Name = "James",
            Attempted = true,
            Success = true,
            CanInstall = true,
        };

        var george = new SessionResponse
        {
            Name = "George",
            Attempted = false,
            Success = false,
            CanInstall = true,
        };

        renderer.RenderInstallationStatus(logLevel, new SortedList<int, SessionResponse?> { { 2, james }, { 3, george }, });
        george = george with { Attempted = true, Failures = new List<string?> { "He hit the tree 🌴" }, };
        renderer.RenderInstallationStatus(logLevel, new SortedList<int, SessionResponse?> { { 2, james }, { 3, george }, });
        renderer.RenderInstallationStatus(logLevel, new SortedList<int, SessionResponse?> { { 2, james }, { 3, george }, });
        console.Output.ShouldNotContainStringsInOrder("✅", "James", "Succeeded");
        console.Output.ShouldContainStringsInOrder(onlyOnce: true, "❌", "George", "Failed", "He hit the tree 🌴");
    }

    [TestCaseSource(nameof(LogLevelsLessThanOrEqualTo), new object[] { LogLevel.Information, })]
    public void RenderInstallationStatus_OutputsMessageWhenPackageIsSuccessful(LogLevel logLevel)
    {
        var console = new TestConsole().Interactive();
        var renderer = new Renderer(console, useDecorativeIcons: true);

        var james = new SessionResponse
        {
            Name = "James",
            Attempted = false,
            Success = false,
            CanInstall = true,
        };

        renderer.RenderInstallationStatus(logLevel, new SortedList<int, SessionResponse?> { { 2, james }, });
        console.Output.ShouldNotContainStringsInOrder("James");
        james = james with { Success = true };
        renderer.RenderInstallationStatus(logLevel, new SortedList<int, SessionResponse?> { { 2, james }, });
        console.Output.ShouldContainStringsInOrder(onlyOnce: true, "✅", "James", "Succeeded");
    }

    [TestCaseSource(nameof(LogLevelsLessThanOrEqualTo), new object[] { LogLevel.Error, })]
    public void RenderInstallationStatus_RendersFailures(LogLevel logLevel)
    {
        var console = new TestConsole().Interactive();
        var renderer = new Renderer(console, useDecorativeIcons: true);

        var jimmy = new SessionResponse
        {
            Name = "Jimmy",
            Attempted = true,
            Failures = new List<string?> { "BAD ZIP", "REALLY FAILED", },
            CanInstall = true,
        };

        var james = new SessionResponse
        {
            Name = "James",
            Attempted = true,
            Success = false,
            CanInstall = true,
        };

        renderer.RenderInstallationStatus(logLevel, new SortedList<int, SessionResponse?> { { 1, jimmy }, { 2, james }, });
        console.Output.ShouldNotContainStringsInOrder("❌", "James", "Failed");
        console.Output.ShouldContainStringsInOrder("❌", "Jimmy", "Failed", "BAD ZIP", "REALLY FAILED");
    }

    [TestCaseSource(nameof(LogLevelsLessThanOrEqualTo), new object[] { LogLevel.Error, })]
    public void RenderInstallationStatus_FailuresHaveSpectreMarkup_RendersFailures(LogLevel logLevel)
    {
        var console = new TestConsole().Interactive();
        var renderer = new Renderer(console, useDecorativeIcons: true);

        var jimmy = new SessionResponse
        {
            Name = "Jimmy",
            Attempted = true,
            Failures = new List<string?> { "Failed SQL Query", "SELECT [Name] FROM Place", },
            CanInstall = true,
        };

        renderer.RenderInstallationStatus(logLevel, new SortedList<int, SessionResponse?> { { 1, jimmy }, });
        console.Output.ShouldContainStringsInOrder("❌", "Jimmy", "Failed", "Failed SQL Query", "SELECT [Name] FROM Place");
    }

    [TestCaseSource(nameof(LogLevelsGreaterThanOrEqualTo), new object[] { LogLevel.None, })]
    public void RenderCriticalError_WithHighLogLevel_DisplaysNothing(LogLevel logLevel)
    {
        var console = new TestConsole().Interactive();
        var renderer = new Renderer(console);

        try
        {
            throw new Exception("Exception message");
        }
        catch (Exception exception)
        {
            renderer.RenderCriticalError(logLevel, "An error occurred while uploading the packages", exception);
            console.Output.ShouldBeEmpty();
        }
    }

    [TestCaseSource(nameof(LogLevelsLessThanOrEqualTo), new object[] { LogLevel.Critical, })]
    public void RenderCriticalError(LogLevel logLevel)
    {
        var console = new TestConsole().Interactive();
        var renderer = new Renderer(console);

        try
        {
            throw new Exception("Exception message");
        }
        catch (Exception exception)
        {
            renderer.RenderCriticalError(logLevel, "An error occurred while uploading the packages", exception);
            console.Output.ShouldContainStringsInOrder(
                "An error occurred while uploading the packages",
                "Exception message",
                nameof(RendererTests),
                nameof(this.RenderCriticalError));
        }
    }

    [Test]
    public void RenderInstallationStatus_WhenUnicodeNotSupported_SuccessRendersWithoutIcon()
    {
        var console = new TestConsole().Interactive();
        var renderer = new Renderer(console, useDecorativeIcons: false);

        var jimmy = new SessionResponse
        {
            Name = "Jimmy",
            Attempted = true,
            Success = true,
            CanInstall = true,
        };

        renderer.RenderInstallationStatus(LogLevel.Information, new SortedList<int, SessionResponse?> { { 1, jimmy }, });
        console.Output.ShouldContain("Jimmy");
        console.Output.ShouldContain("Succeeded");
        console.Output.ShouldNotContain("✅");
    }

    [Test]
    public void RenderInstallationStatus_WhenUnicodeNotSupported_FailureRendersWithoutIcon()
    {
        var console = new TestConsole().Interactive();
        var renderer = new Renderer(console, useDecorativeIcons: false);

        var jimmy = new SessionResponse
        {
            Name = "Jimmy",
            Attempted = true,
            Failures = new List<string?> { "BAD ZIP" },
            CanInstall = true,
        };

        renderer.RenderInstallationStatus(LogLevel.Error, new SortedList<int, SessionResponse?> { { 1, jimmy }, });
        console.Output.ShouldContain("Jimmy");
        console.Output.ShouldContain("Failed");
        console.Output.ShouldContain("BAD ZIP");
        console.Output.ShouldNotContain("❌");
    }

    [Test]
    public void RenderInstallationOverview_WhenUnicodeNotSupported_UnknownDependencyRendersWithoutIcon()
    {
        var console = new TestConsole().Interactive();
        var renderer = new Renderer(console, useDecorativeIcons: false);

        var sessionResponse = new SessionResponse
        {
            Packages = new List<PackageResponse?>
            {
                new()
                {
                    Name = "Jamestown.zip",
                    Dependencies = new List<DependencyResponse?>
                    {
                        new() { PackageName = "Unknown Package", DependencyVersion = string.Empty, IsPackageDependency = false, },
                    },
                    CanInstall = true,
                },
            },
        };

        renderer.RenderInstallationOverview(LogLevel.Information, new SortedList<int, SessionResponse?> { { 1, sessionResponse }, });
        console.Output.ShouldContain("Jamestown.zip");
        console.Output.ShouldContain("Unknown Package");
        console.Output.ShouldNotContain("⚙");
    }
}
