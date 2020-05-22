using System.Collections.Generic;

namespace DotNetNuke.Abstractions.Prompt
{
    public interface ICommandHelp
    {
        string Description { get; set; }
        string Error { get; set; }
        string Name { get; set; }
        IEnumerable<ICommandOption> Options { get; set; }
        string ResultHtml { get; set; }
    }
}
