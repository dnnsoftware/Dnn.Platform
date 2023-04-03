// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Tests.BulkInstall.DeployClient
{
    using DotNetNuke.BulkInstall.DeployClient;
    using Spectre.Console;
    using Spectre.Console.Rendering;
    using Spectre.Console.Testing;

    public class RendererTests
    {
        [MemberData(nameof(LogLevelsLessThanOrEqualTo), LogLevel.Information)]
        [Theory]
        public void Welcome_DisplaysSomething(LogLevel logLevel)
        {
            var console = new TestConsole();

            var renderer = new Renderer(console);
            renderer.Welcome(logLevel);

            console.Output.ShouldNotBeNullOrWhiteSpace();
        }

        [MemberData(nameof(LogLevelsGreaterThanOrEqualTo), LogLevel.Warning)]
        [Theory]
        public void Welcome_WithHighLogLevel_DisplaysNothing(LogLevel logLevel)
        {
            var console = new TestConsole();

            var renderer = new Renderer(console);
            renderer.Welcome(logLevel);

            console.Output.ShouldBeEmpty();
        }

        [MemberData(nameof(LogLevelsLessThanOrEqualTo), LogLevel.Information)]
        [Theory]
        public void RenderListOfFiles_DisplaysSomething(LogLevel logLevel)
        {
            var console = new TestConsole().Interactive();

            var renderer = new Renderer(console);
            renderer.RenderListOfFiles(logLevel, new[] { "OpenContent_4.5.0_Install.zip", "2sxc_12.4.4_Install.zip", });

            console.Output.ShouldContain("OpenContent_4.5.0_Install.zip");
            console.Output.ShouldContain("2sxc_12.4.4_Install.zip");
        }


        [MemberData(nameof(LogLevelsGreaterThanOrEqualTo), LogLevel.Warning)]
        [Theory]
        public void RenderListOfFiles_WithHighLogLevel_DisplaysNothing(LogLevel logLevel)
        {
            var console = new TestConsole().Interactive();

            var renderer = new Renderer(console);
            renderer.RenderListOfFiles(logLevel, new[] { "OpenContent_4.5.0_Install.zip", "2sxc_12.4.4_Install.zip", });

            console.Output.ShouldBeEmpty();
        }

        [Fact]
        public void RenderListOfFiles_DisplaysFilePath()
        {
            var console = new TestConsole().Interactive();

            var renderer = new Renderer(console);
            renderer.RenderListOfFiles(LogLevel.Information, new[] { @"E:\foo\bar\OpenContent_4.5.0_Install.zip", @"E:\foo\bar\2sxc_12.4.4_Install.zip", });

            console.Output.ShouldContainStringsInOrder(onlyOnce: true, @"E:\foo\bar", "OpenContent_4.5.0_Install.zip", "2sxc_12.4.4_Install.zip");
        }

        [Fact]
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
                @"E:\",
                "my-top-level.zip",
                "foo",
                "Modules",
                "the-module.zip",
                "the-templates.zip",
                "Skins",
                "themezilla.zip");
        }

        [Fact]
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


        [MemberData(nameof(LogLevelsLessThanOrEqualTo), LogLevel.Information)]
        [Theory]
        public async Task RenderFileUploadsAsync_InteractiveWithLogging_RendersSomething(LogLevel logLevel)
        {
            var console = new TestConsole().Interactive();

            var renderer = new Renderer(console);
            await renderer.RenderFileUploadsAsync(logLevel, new[] { ("OpenContent_4.5.0_Install.zip", Task.CompletedTask), });

            console.Output.ShouldContainStringsInOrder("OpenContent_4.5.0_Install.zip", "100%");
        }

        [MemberData(nameof(LogLevelsGreaterThanOrEqualTo), LogLevel.Warning)]
        [Theory]
        public async Task RenderFileUploadsAsync_InteractiveWithHighLogLevel_RendersNothing(LogLevel logLevel)
        {
            var console = new TestConsole().Interactive();

            var renderer = new Renderer(console);
            await renderer.RenderFileUploadsAsync(logLevel, new[] { ("OpenContent_4.5.0_Install.zip", Task.CompletedTask), });

            console.Output.ShouldBeEmpty();
        }

        [MemberData(nameof(LogLevelsLessThanOrEqualTo), LogLevel.Information)]
        [Theory]
        public async Task RenderFileUploadsAsync_NonInteractiveWIthLogging_RendersSomething(LogLevel logLevel)
        {
            var console = new TestConsole();

            var renderer = new Renderer(console);
            await renderer.RenderFileUploadsAsync(logLevel, new[] { ("OpenContent_4.5.0_Install.zip", Task.CompletedTask), });


            console.Output.ShouldContainStringsInOrder("OpenContent_4.5.0_Install.zip", "upload", "complete", "\n");
        }

        [MemberData(nameof(LogLevelsGreaterThanOrEqualTo), LogLevel.Warning)]
        [Theory]
        public async Task RenderFileUploadsAsync_NonInteractiveWithHighLogLevel_RendersNothing(LogLevel logLevel)
        {
            var console = new TestConsole();

            var renderer = new Renderer(console);
            await renderer.RenderFileUploadsAsync(logLevel, new[] { ("OpenContent_4.5.0_Install.zip", Task.CompletedTask), });


            console.Output.ShouldBeEmpty();
        }

        [MemberData(nameof(AllLogLevels))]
        [Theory]
        public async Task RenderFileUploadsAsync_Interactive_UploadTaskIsAwaited(LogLevel logLevel)
        {
            var console = new TestConsole().Interactive();

            var renderer = new Renderer(console);

            var exception = await Should.ThrowAsync<Exception>(() => renderer.RenderFileUploadsAsync(logLevel, new[] { ("OpenContent_4.5.0_Install.zip", UploadFile()), }));

            exception.Message.ShouldBe("UploadFile() was called!");

            static Task UploadFile()
            {
                return Task.FromException(new Exception("UploadFile() was called!"));
            }
        }

        [MemberData(nameof(AllLogLevels))]
        [Theory]
        public async Task RenderFileUploadsAsync_UploadTaskIsAwaited(LogLevel logLevel)
        {
            var console = new TestConsole();

            var renderer = new Renderer(console);

            var exception = await Should.ThrowAsync<Exception>(() => renderer.RenderFileUploadsAsync(logLevel, new[] { ("OpenContent_4.5.0_Install.zip", UploadFile()), }));

            exception.Message.ShouldBe("UploadFile() was called!");

            static Task UploadFile()
            {
                return Task.FromException(new Exception("UploadFile() was called!"));
            }
        }

        [MemberData(nameof(LogLevelsLessThanOrEqualTo), LogLevel.Information)]
        [Theory]
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
                }
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
                    "1.0.0"
                });
        }

        [MemberData(nameof(LogLevelsLessThanOrEqualTo), LogLevel.Information)]
        [Theory]
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
                        }
                    }
                },
            };

            renderer.RenderInstallationOverview(logLevel, new SortedList<int, SessionResponse?> { { 1, sessionResponse }, });
            console.Output.ShouldContainStringsInOrder("Jamestown.zip", "Platform Version", "09.01.02");
        }

        [MemberData(nameof(LogLevelsLessThanOrEqualTo), LogLevel.Information)]
        [Theory]
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
                        }
                    }
                },
            };

            renderer.RenderInstallationOverview(logLevel, new SortedList<int, SessionResponse?> { { 1, sessionResponse }, });
            console.Output.ShouldContainStringsInOrder(new[] { "Jamestown.zip", "DNNJWT" });
        }

        [MemberData(nameof(LogLevelsGreaterThanOrEqualTo), LogLevel.Warning)]
        [Theory]
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
                        }
                    }
                },
            };

            renderer.RenderInstallationOverview(logLevel, new SortedList<int, SessionResponse?> { { 1, sessionResponse }, });
            console.Output.ShouldBeEmpty();
        }

        [MemberData(nameof(LogLevelsLessThanOrEqualTo), LogLevel.Trace)]
        [Theory]
        public void RenderInstallationStatus_OnlyOutputsAttemptedSessionResponses(LogLevel logLevel)
        {
            var console = new TestConsole().Interactive();
            var renderer = new Renderer(console);

            var jimmy = new SessionResponse
            {
                Name = "Jimmy",
                Attempted = true,
                Success = true,
            };

            var james = new SessionResponse
            {
                Name = "James",
                Attempted = false,
                Success = false,
            };

            renderer.RenderInstallationStatus(logLevel, new SortedList<int, SessionResponse?> { { 1, jimmy }, { 2, james }, });
            console.Output.ShouldContainStringsInOrder(new[] { "✅", "Jimmy", "Succeeded" });
            console.Output.ShouldNotContain("James");
        }

        [MemberData(nameof(LogLevelsLessThanOrEqualTo), LogLevel.Information)]
        [Theory]
        public void RenderInstallationStatus_OutputsMultipleResponsesOnDifferentLines(LogLevel logLevel)
        {
            var console = new TestConsole().Interactive();
            var renderer = new Renderer(console);

            var jimmy = new SessionResponse
            {
                Name = "Jimmy",
                Attempted = true,
                Success = true,
            };

            var james = new SessionResponse
            {
                Name = "James",
                Attempted = true,
                Success = true,
            };


            renderer.RenderInstallationStatus(logLevel, new SortedList<int, SessionResponse?> { { 1, jimmy }, { 2, james }, });
            console.Output.ShouldContainStringsInOrder("✅", "Jimmy", "Succeeded", "\n", "✅", "James", "Succeeded", "\n");
        }

        [MemberData(nameof(LogLevelsGreaterThanOrEqualTo), LogLevel.Warning)]
        [Theory]
        public void RenderInstallationStatus_ShouldNotRenderWarningOrHigher(LogLevel logLevel)
        {
            var console = new TestConsole().Interactive();
            var renderer = new Renderer(console);

            var jimmy = new SessionResponse
            {
                Name = "Jimmy",
                Attempted = true,
                Success = true,
            };

            var james = new SessionResponse
            {
                Name = "James",
                Attempted = true,
                Success = true,
            };

            renderer.RenderInstallationStatus(logLevel, new SortedList<int, SessionResponse?> { { 1, jimmy }, { 2, james }, });
            console.Output.ShouldBeEmpty();
        }

        [MemberData(nameof(LogLevelsLessThanOrEqualTo), LogLevel.Trace)]
        [Theory]
        public void RenderInstallationStatus_DoesNotOutputDuplicateInformation(LogLevel logLevel)
        {
            var console = new TestConsole().Interactive();
            var renderer = new Renderer(console);

            var james = new SessionResponse
            {
                Name = "James",
                Attempted = true,
                Success = true,
            };

            var george = new SessionResponse
            {
                Name = "George",
                Attempted = false,
                Success = false,
            };

            renderer.RenderInstallationStatus(logLevel, new SortedList<int, SessionResponse?> { { 2, james }, { 3, george }, });
            george = george with { Attempted = true, Failures = new List<string?> { "He hit the tree 🌴" }, };
            renderer.RenderInstallationStatus(logLevel, new SortedList<int, SessionResponse?> { { 2, james }, { 3, george }, });
            renderer.RenderInstallationStatus(logLevel, new SortedList<int, SessionResponse?> { { 2, james }, { 3, george }, });
            console.Output.ShouldContainStringsInOrder(onlyOnce: true, "✅", "James", "Succeeded", "❌", "George", "Failed", "He hit the tree 🌴");
        }

        [Theory]
        [InlineData(LogLevel.Warning)]
        [InlineData(LogLevel.Error)]
        public void RenderInstallationStatus_WhenErrorLevel_DoesNotOutputInformationLevel(LogLevel logLevel)
        {
            var console = new TestConsole().Interactive();
            var renderer = new Renderer(console);

            var james = new SessionResponse
            {
                Name = "James",
                Attempted = true,
                Success = true,
            };

            var george = new SessionResponse
            {
                Name = "George",
                Attempted = false,
                Success = false,
            };

            renderer.RenderInstallationStatus(logLevel, new SortedList<int, SessionResponse?> { { 2, james }, { 3, george }, });
            george = george with { Attempted = true, Failures = new List<string?> { "He hit the tree 🌴" }, };
            renderer.RenderInstallationStatus(logLevel, new SortedList<int, SessionResponse?> { { 2, james }, { 3, george }, });
            renderer.RenderInstallationStatus(logLevel, new SortedList<int, SessionResponse?> { { 2, james }, { 3, george }, });
            console.Output.ShouldNotContainStringsInOrder("✅", "James", "Succeeded");
            console.Output.ShouldContainStringsInOrder(onlyOnce: true, "❌", "George", "Failed", "He hit the tree 🌴");
        }

        [MemberData(nameof(LogLevelsLessThanOrEqualTo), LogLevel.Information)]
        [Theory]
        public void RenderInstallationStatus_OutputsMessageWhenPackageIsSuccessful(LogLevel logLevel)
        {
            var console = new TestConsole().Interactive();
            var renderer = new Renderer(console);

            var james = new SessionResponse
            {
                Name = "James",
                Attempted = false,
                Success = false
            };

            renderer.RenderInstallationStatus(logLevel, new SortedList<int, SessionResponse?> { { 2, james }, });
            console.Output.ShouldNotContainStringsInOrder("James");
            james = james with { Success = true };
            renderer.RenderInstallationStatus(logLevel, new SortedList<int, SessionResponse?> { { 2, james }, });
            console.Output.ShouldContainStringsInOrder(onlyOnce: true, "✅", "James", "Succeeded");
        }

        [MemberData(nameof(LogLevelsLessThanOrEqualTo), LogLevel.Error)]
        [Theory]
        public void RenderInstallationStatus_RendersFailures(LogLevel logLevel)
        {
            var console = new TestConsole().Interactive();
            var renderer = new Renderer(console);

            var jimmy = new SessionResponse
            {
                Name = "Jimmy",
                Attempted = true,
                Failures = new List<string?> { "BAD ZIP", "REALLY FAILED" }
            };

            var james = new SessionResponse
            {
                Name = "James",
                Attempted = true,
                Success = false
            };

            renderer.RenderInstallationStatus(logLevel, new SortedList<int, SessionResponse?> { { 1, jimmy }, { 2, james }, });
            console.Output.ShouldNotContainStringsInOrder("❌", "James", "Failed");
            console.Output.ShouldContainStringsInOrder("❌", "Jimmy", "Failed", "BAD ZIP", "REALLY FAILED");
        }

        [MemberData(nameof(LogLevelsLessThanOrEqualTo), LogLevel.Error)]
        [Theory]
        public void RenderInstallationStatus_FailuresHaveSpectreMarkup_RendersFailures(LogLevel logLevel)
        {
            var console = new TestConsole().Interactive();
            var renderer = new Renderer(console);

            var jimmy = new SessionResponse
            {
                Name = "Jimmy",
                Attempted = true,
                Failures = new List<string?> { "Failed SQL Query", "SELECT [Name] FROM Place" }
            };
            
            renderer.RenderInstallationStatus(logLevel, new SortedList<int, SessionResponse?> { { 1, jimmy }, });
            console.Output.ShouldContainStringsInOrder("❌", "Jimmy", "Failed", "Failed SQL Query", "SELECT [Name] FROM Place");
        }

        [MemberData(nameof(LogLevelsGreaterThanOrEqualTo), LogLevel.None)]
        [Theory]
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

        [MemberData(nameof(LogLevelsLessThanOrEqualTo), LogLevel.Critical)]
        [Theory]
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
                console.Output.ShouldContainStringsInOrder("An error occurred while uploading the packages",
                    "Exception message", nameof(RendererTests), nameof(RenderCriticalError));
            }
        }

        public static IEnumerable<object[]> AllLogLevels()
        {
            return Enum.GetValues<LogLevel>().Select(logLevel => new object[] { logLevel });
        }

        public static IEnumerable<object[]> LogLevelsGreaterThanOrEqualTo(LogLevel minimumLevel)
        {
            return Enum.GetValues<LogLevel>().Where(logLevel => logLevel >= minimumLevel).Select(logLevel => new object[] { logLevel });
        }

        public static IEnumerable<object[]> LogLevelsLessThanOrEqualTo(LogLevel maximumLevel)
        {
            return Enum.GetValues<LogLevel>().Where(logLevel => logLevel <= maximumLevel).Select(logLevel => new object[] { logLevel });
        }
    }
}
