namespace PolyDeploy.DeployClient.Tests
{
    using Spectre.Console;
    using Spectre.Console.Rendering;
    using Spectre.Console.Testing;

    public class RendererTests
    {
        [Fact]
        public void Welcome_DisplaysSomething()
        {
            var console = new TestConsole();

            var renderer = new Renderer(console);
            renderer.Welcome();

            console.Output.ShouldNotBeNullOrWhiteSpace();
        }

        [Fact]
        public void RenderListOfFiles_GivenFiles_RendersTreeOfFiles()
        {
            var console = new TestConsole().Interactive();

            var renderer = new Renderer(console);
            renderer.RenderListOfFiles(new[] { "OpenContent_4.5.0_Install.zip", "2sxc_12.4.4_Install.zip", });

            console.Output.ShouldContain("OpenContent_4.5.0_Install.zip");
            console.Output.ShouldContain("2sxc_12.4.4_Install.zip");
        }

        [Fact]
        public async Task RenderFileUploadsAsync_Interactive_RendersSomething()
        {
            var console = new TestConsole().Interactive();

            var renderer = new Renderer(console);
            await renderer.RenderFileUploadsAsync(new[] { ("OpenContent_4.5.0_Install.zip", Task.CompletedTask), });

            console.Output.ShouldContainStringsInOrder("OpenContent_4.5.0_Install.zip", "100%");
        }
        
        [Fact]
        public async Task RenderFileUploadsAsync_NonInteractive_RendersSomething()
        {
            var console = new TestConsole();

            var renderer = new Renderer(console);
            await renderer.RenderFileUploadsAsync(new[] { ("OpenContent_4.5.0_Install.zip", Task.CompletedTask), });

            
            console.Output.ShouldContainStringsInOrder("OpenContent_4.5.0_Install.zip", "upload", "complete");
        }
        
        [Fact]
        public async Task RenderFileUploadsAsync_UploadTaskIsAwaited()
        {
            var console = new TestConsole();

            var renderer = new Renderer(console);

            var exception = await Should.ThrowAsync<Exception>(() =>  renderer.RenderFileUploadsAsync(new[] { ("OpenContent_4.5.0_Install.zip", UploadFile()), }));

            exception.Message.ShouldBe("UploadFile() was called!");

            static Task UploadFile() {
                return Task.FromException(new Exception("UploadFile() was called!"));
            }
        }

        [Fact]
        public void RenderInstallationOverview_DisplaysTreeOfPackageDetails()
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
            renderer.RenderInstallationOverview(packages);

            // TODO: check the structure of the tree, maybe using reflection?
            packagesTree.ShouldNotBeNull();
            var treeSegments = ((IRenderable)packagesTree).Render(new RenderContext(new TestCapabilities()), 80);

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

        [Fact]
        public void RenderInstallationOverview_DisplaysDnnPlatformVersionDependency()
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

            renderer.RenderInstallationOverview(new SortedList<int, SessionResponse?> { { 1, sessionResponse }, });
            console.Output.ShouldContainStringsInOrder("Jamestown.zip", "Platform Version", "09.01.02");
        }

        [Fact]
        public void RenderInstallationOverview_WhenDependencyHasNoVersion_DisplaysDependencyName()
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

            renderer.RenderInstallationOverview(new SortedList<int, SessionResponse?> { { 1, sessionResponse }, });
            console.Output.ShouldContainStringsInOrder(new[] { "Jamestown.zip", "DNNJWT" });
        }

        [Fact]
        public void RenderInstallationStatus_OnlyOutputsAttemptedSessionResponses()
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


            renderer.RenderInstallationStatus(new SortedList<int, SessionResponse?> { { 1, jimmy }, { 2, james }, });
            console.Output.ShouldContainStringsInOrder(new[] { "✅", "Jimmy", "Succeeded" });
            console.Output.ShouldNotContain("James");
        }

        [Fact]
        public void RenderInstallationStatus_OutputsMultipleResponsesOnDifferentLines()
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


            renderer.RenderInstallationStatus(new SortedList<int, SessionResponse?> { { 1, jimmy }, { 2, james }, });
            console.Output.ShouldContainStringsInOrder("✅", "Jimmy", "Succeeded", "\n", "✅", "James", "Succeeded", "\n");
        }

        [Fact]
        public void RenderInstallationStatus_DoesNotOutputDuplicateInformation()
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

            renderer.RenderInstallationStatus(new SortedList<int, SessionResponse?> { { 2, james }, { 3, george }, });
            george = george with { Attempted = true, Failures = new List<string?> { "He hit the tree 🌴" }, };
            renderer.RenderInstallationStatus(new SortedList<int, SessionResponse?> { { 2, james }, { 3, george }, });
            renderer.RenderInstallationStatus(new SortedList<int, SessionResponse?> { { 2, james }, { 3, george }, });
            console.Output.ShouldContainStringsInOrder(onlyOnce: true, "✅", "James", "Succeeded", "❌", "George", "Failed", "He hit the tree 🌴");
        }

        [Fact]
        public void RenderInstallationStatus_OutputsMessageWhenPackageIsSuccessful()
        {
            var console = new TestConsole().Interactive();
            var renderer = new Renderer(console);
            
            var james = new SessionResponse
            {
                Name = "James",
                Attempted = false,
                Success = false
            };

            renderer.RenderInstallationStatus(new SortedList<int, SessionResponse?> { { 2, james }, });
            console.Output.ShouldNotContainStringsInOrder("James");
            james = james with { Success = true };
            renderer.RenderInstallationStatus(new SortedList<int, SessionResponse?> { { 2, james }, });
            console.Output.ShouldContainStringsInOrder(onlyOnce: true, "✅", "James", "Succeeded");
        }

        [Fact]
        public void RenderInstallationStatus_RendersFailures()
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

            renderer.RenderInstallationStatus(new SortedList<int, SessionResponse?> { { 1, jimmy }, { 2, james }, });
            console.Output.ShouldNotContainStringsInOrder("❌", "James", "Failed");
            console.Output.ShouldContainStringsInOrder("❌", "Jimmy", "Failed", "BAD ZIP", "REALLY FAILED");
        }
        
        [Fact]
        public void RenderError()
        {
            var console = new TestConsole().Interactive();
            var renderer = new Renderer(console);

            try
            {
                throw new Exception("Exception message");
            }
            catch (Exception exception)
            {
                renderer.RenderError("An error occurred while uploading the packages", exception);
                console.Output.ShouldContainStringsInOrder("An error occurred while uploading the packages",
                    "Exception message", nameof(RendererTests), nameof(RenderError));
            }
        }
    }
}
