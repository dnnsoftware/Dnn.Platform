namespace PolyDeploy.DeployClient
{
    using System.Collections.Generic;
    using System.Linq;

    using Spectre.Console;

    public class Renderer : IRenderer
    {
        private readonly IAnsiConsole console;

        public Renderer(IAnsiConsole console)
        {
            this.console = console;
        }

        public void RenderFileUploadStarted(string file)
        {
            this.console.Write(new Markup($"Uploading [aqua]{file}[/]…"));
        }

        public void RenderFileUploadComplete(string file)
        {
            this.console.Write(new Markup($"Uploaded [aqua]{file}[/]!"));
        }

        public void RenderListOfFiles(IEnumerable<string> files)
        {
            var fileTree = new Tree(new Markup(":file_folder: [yellow]Packages[/]"));
            fileTree.AddNodes(files.Select(f => new Markup($":page_facing_up: [aqua]{f}[/]")));

            this.console.Write(fileTree);
        }
    }
}