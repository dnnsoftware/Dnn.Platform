namespace PolyDeploy.DeployClient.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using FakeItEasy;
    using Shouldly;
    using Spectre.Console;
    using Spectre.Console.Rendering;
    using Spectre.Console.Testing;
    using Xunit;

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
        public async Task RenderFileUploadsAsync_RendersSomething()
        {
            var console = new TestConsole().Interactive();

            var renderer = new Renderer(console);
            await renderer.RenderFileUploadsAsync(new[] { ("OpenContent_4.5.0_Install.zip", Task.CompletedTask), });

            console.Output.ShouldContain("OpenContent_4.5.0_Install.zip");
            console.Output.ShouldContain("100%");
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
                            new() { PackageName = null, DependencyVersion = "9.1.2" },
                            new() { PackageName = null, DependencyVersion = "9.1.2" },
                        }
                    }
                },
            };

            renderer.RenderInstallationOverview(new SortedList<int, SessionResponse?> { { 1, sessionResponse }, });
            console.Output.ShouldContainStringsInOrder(new[] { "Jamestown.zip", "Platform Version", "9.1.2" });
        }

        [Fact]
        public void RenderInstallationStatus_OutputsSessionResponse()
        {
            var console = new TestConsole().Interactive();
            var renderer = new Renderer(console);

            var sessionResponse = new SessionResponse
            {
                Name = "Jimmy",
                Attempted = true,
                Success = false
            };

            renderer.RenderInstallationStatus(new SortedList<int, SessionResponse?> { { 1, sessionResponse }, });
            console.Output.ShouldContainStringsInOrder(new[] { "Jimmy", "Started" });
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
                Success = false
            };

            var james = new SessionResponse
            {
                Name = "James",
                Attempted = false,
                Success = false
            };


            renderer.RenderInstallationStatus(new SortedList<int, SessionResponse?> { { 1, jimmy }, { 2, james }, });
            console.Output.ShouldContainStringsInOrder(new[] { "Jimmy", "Started" });
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
                Success = false
            };

            var james = new SessionResponse
            {
                Name = "James",
                Attempted = true,
                Success = false
            };


            renderer.RenderInstallationStatus(new SortedList<int, SessionResponse?> { { 1, jimmy }, { 2, james }, });
            console.Output.ShouldContainStringsInOrder("Jimmy", "Started", "\n", "James", "Started", "\n");
        }

        [Fact]
        public void RenderInstallationStatus_DoesNotOutputDuplicateInformation()
        {
            var console = new TestConsole().Interactive();
            var renderer = new Renderer(console);

            var jimmy = new SessionResponse
            {
                Name = "Jimmy",
                Attempted = true,
                Success = false
            };

            var james = new SessionResponse
            {
                Name = "James",
                Attempted = true,
                Success = false
            };

            var george = new SessionResponse
            {
                Name = "George",
                Attempted = false,
                Success = false,
            };

            renderer.RenderInstallationStatus(new SortedList<int, SessionResponse?> { { 1, jimmy }, { 2, james }, { 3, george }, });
            james = james with { Success = true };
            george = george with { Attempted = true, Failures = new List<string?> { "He hit the tree 🌴" }, };
            renderer.RenderInstallationStatus(new SortedList<int, SessionResponse?> { { 1, jimmy }, { 2, james }, { 3, george }, });
            renderer.RenderInstallationStatus(new SortedList<int, SessionResponse?> { { 1, jimmy }, { 2, james }, { 3, george }, });
            console.Output.ShouldContainStringsInOrder(onlyOnce: true, "Jimmy", "Started", "James", "Started", "James", "Succeeded", "George", "Started", "George", "Failed", "He hit the tree 🌴");
        }

        [Fact]
        public void RenderInstallationStatus_OutputsMessageWhenPackageIsSuccessful()
        {
            var console = new TestConsole().Interactive();
            var renderer = new Renderer(console);

            var jimmy = new SessionResponse
            {
                Name = "Jimmy",
                Attempted = true,
                Success = false
            };

            var james = new SessionResponse
            {
                Name = "James",
                Attempted = true,
                Success = false
            };

            renderer.RenderInstallationStatus(new SortedList<int, SessionResponse?> { { 1, jimmy }, { 2, james }, });
            console.Output.ShouldContainStringsInOrder(onlyOnce: true, "Jimmy", "Started", "James", "Started");
            james = james with { Success = true };
            renderer.RenderInstallationStatus(new SortedList<int, SessionResponse?> { { 1, jimmy }, { 2, james }, });
            console.Output.ShouldContainStringsInOrder(onlyOnce: true, "Jimmy", "Started", "James", "Started", "James", "Succeeded");
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
            console.Output.ShouldNotContainStringsInOrder("James", "Failed");
            console.Output.ShouldContainStringsInOrder("Jimmy", "Failed", "BAD ZIP", "REALLY FAILED");
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
