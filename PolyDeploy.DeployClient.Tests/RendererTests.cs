namespace PolyDeploy.DeployClient.Tests
{
    using System.Threading.Tasks;

    using Shouldly;

    using Spectre.Console.Testing;
    using Xunit;

    public class RendererTests
    {
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
    }
}