namespace PolyDeploy.DeployClient.Tests
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    using Spectre.Console;

    public class Renderer : IRenderer
    {
        private readonly IAnsiConsole console;

        public Renderer(IAnsiConsole console)
        {
            this.console = console;
        }

        public async Task RenderListOfFiles(IEnumerable<string> files)
        {
            var fileTree = new Tree(new Markup(":file_folder: [yellow]Packages[/]"));
            fileTree.AddNodes(files.Select(f => new Markup($":page_facing_up: [aqua]{f}[/]")));

            this.console.Write(fileTree);
        }
    }
}