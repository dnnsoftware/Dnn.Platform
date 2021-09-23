namespace PolyDeploy.DeployClient.Tests
{
    using System.Threading.Tasks;

    using Shouldly;

    using Spectre.Console;

    using Xunit;

    public class RendererTests
    {
        [Fact]
        public void RenderListOfFiles_GivenFiles_RendersTreeOfFiles()
        {
            var consoleFactory = new AnsiConsoleFactory();
            var console = consoleFactory.Create(new AnsiConsoleSettings());
            var recorder = console.CreateRecorder();

            var renderer = new Renderer(recorder);
            renderer.RenderListOfFiles(new[] { "OpenContent_4.5.0_Install.zip", "2sxc_12.4.4_Install.zip", });

            var actual = recorder.ExportText();
            actual.ShouldContain("OpenContent_4.5.0_Install.zip");
            actual.ShouldContain("2sxc_12.4.4_Install.zip");
        }
    }
}