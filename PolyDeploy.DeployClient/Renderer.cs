namespace PolyDeploy.DeployClient
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

        public async Task RenderFileUploadsAsync(IEnumerable<(string file, Task uploadTask)> uploads)
        {
            await this.console.Progress()
            //.Columns(new ProgressColumn[] { new TaskDescriptionColumn(), new ProgressBarColumn() })
            .StartAsync(async context =>
            {
                await Task.WhenAll(uploads.Select(async upload =>
                {
                    var progressTask = context.AddTask(upload.file);
                    await upload.uploadTask;
                    progressTask.StopTask();
                }));
            });
        }

        public void RenderListOfFiles(IEnumerable<string> files)
        {
            var fileTree = new Tree(new Markup(":file_folder: [yellow]Packages[/]"));
            fileTree.AddNodes(files.Select(f => new Markup($":page_facing_up: [aqua]{f}[/]")));

            this.console.Write(fileTree);
        }
    }
}